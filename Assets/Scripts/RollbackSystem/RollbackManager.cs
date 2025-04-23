using System.Collections.Generic;
using SoftFloat;
using UnityEngine;

namespace PhysicSystem
{
    public class RollbackManager
    {
        private Dictionary<int, GameState> gameStates = new Dictionary<int, GameState>();
        private Dictionary<int, PlayerInput> playerInputs = new Dictionary<int, PlayerInput>();
        
        private int currentFrame = 0;
        private int lastSavedFrame = -1;
        private int maxHistoryFrames = 300; // 5 saniye @ 60 FPS
        
        private sfloat gravity;
        private sfloat leftBoundary;
        private sfloat rightBoundary;
        private sfloat bottomBoundary;
        private sfloat topBoundary;
        private sfloat deltaTime;

        // Pozisyon tarihçesi (görsel rollback için)
        public List<Vector2> PositionHistory { get; private set; } = new List<Vector2>();

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

            // Pozisyon tarihçesini kaydet
            PositionHistory.Add(new Vector2((float)ballData.PositionX, (float)ballData.PositionY));
            if (PositionHistory.Count > maxHistoryFrames)
            {
                PositionHistory.RemoveAt(0);
            }

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
            
            // Şu anki frame'i güncelle
            currentFrame = targetFrame;
            
            // Derin kopya yaparak döndür
            return targetState.BallData.Clone();
        }

        // Belirli bir frame'den başlayarak simülasyonu yeniden çalıştır
        public BallData ResimulateToFrame(int targetFrame, BallData initialBallData)
        {
            int startFrame = currentFrame;
            BallData currentBallData = initialBallData.Clone();
            
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

        // Mock input eklemek için metot
        public void AddMockInput(int frame, sfloat horizontalInput, bool jumpInput)
        {
            PlayerInput input = new PlayerInput(frame, horizontalInput, jumpInput);
            playerInputs[frame] = input;
        }

        // Belirli bir frame aralığı için mock inputlar ekle
        public void AddMockInputRange(int startFrame, int endFrame, sfloat horizontalInput, bool jumpInput)
        {
            for (int frame = startFrame; frame <= endFrame; frame++)
            {
                AddMockInput(frame, horizontalInput, jumpInput);
            }
        }

        // Pozisyon tarihçesini temizle
        public void ClearPositionHistory()
        {
            PositionHistory.Clear();
        }

        public bool GetPlayerInput(int i, out PlayerInput playerInput)
        {
            if (playerInputs.TryGetValue(i, out playerInput))
            {
                return true;
            }
            else
            {
                playerInput = new PlayerInput();
                return false;
            }
            
            
        }
    }
}