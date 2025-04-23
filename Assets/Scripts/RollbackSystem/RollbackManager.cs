using System.Collections.Generic;
using SoftFloat;
using UnityEngine;

namespace PhysicSystem
{
    // Oyun durumunu saklamak için kullanılacak veri yapısı
    public struct GameState
    {
        public int Frame;
        public BallData BallData;

        // Deep copy oluşturmak için constructor
        public GameState(int frame, BallData ballData)
        {
            Frame = frame;
            BallData = new BallData
            {
                PositionX = ballData.PositionX,
                PositionY = ballData.PositionY,
                VelocityX = ballData.VelocityX,
                VelocityY = ballData.VelocityY,
                Radius = ballData.Radius,
                JumpForce = ballData.JumpForce,
                MoveSpeed = ballData.MoveSpeed
            };
        }
    }

    // Oyuncu girdilerini saklamak için kullanılacak veri yapısı
    public struct PlayerInput
    {
        public int Frame;
        public sfloat HorizontalInput;
        public bool JumpInput;

        public PlayerInput(int frame, sfloat horizontalInput, bool jumpInput)
        {
            Frame = frame;
            HorizontalInput = horizontalInput;
            JumpInput = jumpInput;
        }
    }

    // Rollback yönetici sınıfı
    public class RollbackManager
    {
        private Dictionary<int, GameState> gameStates = new Dictionary<int, GameState>();
        private Dictionary<int, PlayerInput> playerInputs = new Dictionary<int, PlayerInput>();
        
        private int currentFrame = 0;
        private int lastSavedFrame = -1;
        private int maxHistoryFrames = 120; // 2 saniye @ 60 FPS
        
        private sfloat gravity;
        private sfloat leftBoundary;
        private sfloat rightBoundary;
        private sfloat bottomBoundary;
        private sfloat topBoundary;
        private sfloat deltaTime;

        public RollbackManager(sfloat gravity, sfloat leftBoundary, sfloat rightBoundary, sfloat bottomBoundary, sfloat topBoundary, sfloat fixedDeltaTime)
        {
            this.gravity = gravity;
            this.leftBoundary = leftBoundary;
            this.rightBoundary = rightBoundary;
            this.bottomBoundary = bottomBoundary;
            this.topBoundary = topBoundary;
            this.deltaTime = fixedDeltaTime;
        }

        // Oyun durumunu kaydet
        public void SaveGameState(BallData ballData)
        {
            GameState state = new GameState(currentFrame, ballData);
            gameStates[currentFrame] = state;
            lastSavedFrame = currentFrame;

            // Eski durumları temizle (bellek optimizasyonu için)
            if (currentFrame > maxHistoryFrames)
            {
                int frameToRemove = currentFrame - maxHistoryFrames;
                if (gameStates.ContainsKey(frameToRemove))
                {
                    gameStates.Remove(frameToRemove);
                }
            }
        }

        // Oyuncu girdisini kaydet
        public void SavePlayerInput(sfloat horizontalInput, bool jumpInput)
        {
            PlayerInput input = new PlayerInput(currentFrame, horizontalInput, jumpInput);
            playerInputs[currentFrame] = input;
        }

        // Belirli bir frame'e rollback yap
        public BallData Rollback(int targetFrame)
        {
            // Hedef frame'i sınırla
            targetFrame = Mathf.Max(0, Mathf.Min(targetFrame, lastSavedFrame));
            
            if (!gameStates.ContainsKey(targetFrame))
            {
                Debug.LogError($"Frame {targetFrame} bulunamadı!");
                return null;
            }

            // Hedef frame'deki durumu al
            GameState targetState = gameStates[targetFrame];
            BallData ballData = targetState.BallData;
            
            // Şu anki frame'i güncelle
            currentFrame = targetFrame;
            
            return ballData;
        }

        // Belirli bir frame'den başlayarak simülasyonu yeniden çalıştır
        public BallData ResimulateToFrame(int targetFrame, BallData initialBallData)
        {
            int startFrame = currentFrame;
            BallData currentBallData = initialBallData;
            
            for (int frame = startFrame; frame <= targetFrame; frame++)
            {
                if (playerInputs.TryGetValue(frame, out PlayerInput input))
                {
                    // Oyuncu girdisini uygula
                    currentBallData.ApplyHorizontalInput(input.HorizontalInput);
                    currentBallData.ApplyJump(input.JumpInput, bottomBoundary);
                }
                
                // Fizik simülasyonunu çalıştır
                currentBallData.UpdatePosition(deltaTime, gravity, leftBoundary, rightBoundary, bottomBoundary, topBoundary);
                
                // Frame'i ilerlet
                currentFrame = frame;
            }
            
            return currentBallData;
        }

        // Bir sonraki frame'e geç
        public void AdvanceFrame()
        {
            currentFrame++;
        }

        // Şu anki frame'i döndür
        public int GetCurrentFrame()
        {
            return currentFrame;
        }

        // Rollback sonrası yeniden simülasyon yapmak için
        public BallData RollbackAndResimulate(int rollbackFrame, int targetFrame)
        {
            BallData rollbackData = Rollback(rollbackFrame);
            if (rollbackData != null)
            {
                return ResimulateToFrame(targetFrame, rollbackData);
            }
            return null;
        }
    }
}
