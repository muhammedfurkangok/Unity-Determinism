using System.Collections.Generic;
using PhysicSystem;
using SoftFloat;
using UnityEngine;

namespace RollbackSystem
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

        public void SaveGameState(BallData ballData)
        {
            GameState state = new GameState(currentFrame, ballData);
            gameStates[currentFrame] = state;
            lastSavedFrame = currentFrame;

            PositionHistory.Add(new Vector2((float)ballData.PositionX, (float)ballData.PositionY));
            if (PositionHistory.Count > maxHistoryFrames)
            {
                PositionHistory.RemoveAt(0);
            }

            if (currentFrame > maxHistoryFrames)
            {
                int frameToRemove = currentFrame - maxHistoryFrames;
                if (gameStates.ContainsKey(frameToRemove))
                {
                    gameStates.Remove(frameToRemove);
                }
            }
        }

        public void SavePlayerInput(sfloat horizontalInput, bool jumpInput)
        {
            PlayerInput input = new PlayerInput(currentFrame, horizontalInput, jumpInput);
            playerInputs[currentFrame] = input;
        }

        public BallData Rollback(int targetFrame)
        {
            targetFrame = Mathf.Max(0, Mathf.Min(targetFrame, lastSavedFrame));
            
            if (!gameStates.ContainsKey(targetFrame))
            {
                Debug.LogError($"Frame {targetFrame} bulunamadı!");
                return null;
            }

            GameState targetState = gameStates[targetFrame];
            
            // Şu anki frame'i güncelle
            currentFrame = targetFrame;
            
            return targetState.BallData.Clone();
        }

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
                
                currentBallData.UpdatePosition(deltaTime, gravity, leftBoundary, rightBoundary, bottomBoundary, topBoundary);
                
                currentFrame = frame;
            }
            
            return currentBallData;
        }

        public void AdvanceFrame()
        {
            currentFrame++;
        }

        public int GetCurrentFrame()
        {
            return currentFrame;
        }

        public BallData RollbackAndResimulate(int rollbackFrame, int targetFrame)
        {
            BallData rollbackData = Rollback(rollbackFrame);
            if (rollbackData != null)
            {
                return ResimulateToFrame(targetFrame, rollbackData);
            }
            return null;
        }

        public void AddMockInput(int frame, sfloat horizontalInput, bool jumpInput)
        {
            PlayerInput input = new PlayerInput(frame, horizontalInput, jumpInput);
            playerInputs[frame] = input;
        }

        public void AddMockInputRange(int startFrame, int endFrame, sfloat horizontalInput, bool jumpInput)
        {
            for (int frame = startFrame; frame <= endFrame; frame++)
            {
                AddMockInput(frame, horizontalInput, jumpInput);
            }
        }

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