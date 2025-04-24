using System.Collections;
using System.Collections.Generic;
using PhysicSystem;
using RollbackSystem;
using SoftFloat;
using UnityEngine;

namespace Physic
{
    public class BallVisualSoftFloat : MonoBehaviour
    {
        [Header("Control Settings")]
        [SerializeField] private bool autoJump = false;

        [Header("Physics Settings")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float fixedTimeStep = 0.016667f; // 60 FPS için fixed timestep

        [Header("Ball Initial Values")]
        [SerializeField] private float initialPositionX = 0f;
        [SerializeField] private float initialPositionY = 0f;
        [SerializeField] private float initialVelocityX = 0f;
        [SerializeField] private float initialVelocityY = 0f;
        [SerializeField] private float ballRadius = 0.5f;
        [SerializeField] private float ballJumpForce = 15f;
        [SerializeField] private float ballMoveSpeed = 5f;

        [Header("Rollback Settings")]
        [SerializeField] private KeyCode rollbackKey = KeyCode.R;
        [SerializeField] private int rollbackFrames = 60; // Kaç frame geri gidileceği

        [Header("Visual Rollback Settings")]
        [SerializeField] private bool showVisualRollback = true;
        [SerializeField] private float visualRollbackSpeed = 3f; // Geri sarma hızı
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color rollbackColor = Color.red;
        [SerializeField] private Color resimulateColor = Color.blue;

        [Header("Testing")]
        [SerializeField] private bool useMockInputs = false;
        [SerializeField] private bool recordPlayerMovement = true;

        private SpriteRenderer spriteRenderer;
        private BallData ballData;
        private RollbackManager rollbackManager;

        private float leftBoundary;
        private float rightBoundary;
        private float topBoundary;
        private float bottomBoundary;

        private float accumulatedTime = 0f;
        private bool isRollbackMode = false;
        private bool isPlayingRollbackAnimation = false;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            Camera cam = Camera.main;
            Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
            Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane));

            leftBoundary = bottomLeft.x;
            bottomBoundary = bottomLeft.y;
            rightBoundary = topRight.x;
            topBoundary = topRight.y;

            ballData = new BallData
            {
                PositionX = (sfloat)initialPositionX,
                PositionY = (sfloat)initialPositionY,
                VelocityX = (sfloat)initialVelocityX,
                VelocityY = (sfloat)initialVelocityY,
                Radius = (sfloat)ballRadius,
                JumpForce = (sfloat)ballJumpForce,
                MoveSpeed = (sfloat)ballMoveSpeed
            };

            rollbackManager = new RollbackManager(
                (sfloat)gravity,
                (sfloat)leftBoundary,
                (sfloat)rightBoundary,
                (sfloat)bottomBoundary,
                (sfloat)topBoundary,
                (sfloat)fixedTimeStep
            );

            rollbackManager.SaveGameState(ballData);

            if (useMockInputs)
            {
                SetupMockInputs();
            }
        
        }

        void Update()
        {
            if (isPlayingRollbackAnimation)
            {
                return;
            }

            if (Input.GetKeyDown(rollbackKey) && !isRollbackMode)
            {
                if (showVisualRollback)
                {
                    StartCoroutine(VisualRollbackCoroutine());
                }
                else
                {
                    PerformRollback();
                }
                return;
            }

            accumulatedTime += Time.deltaTime;
        
            while (accumulatedTime >= fixedTimeStep)
            {
                FixedUpdate();
                accumulatedTime -= fixedTimeStep;
            }

            transform.position = new Vector3((float)ballData.PositionX, (float)ballData.PositionY, transform.position.z);
        
        }

        void FixedUpdate()
        {
            rollbackManager.AdvanceFrame();
        
            if (!useMockInputs && recordPlayerMovement)
            {
                sfloat horizontalInput = (sfloat)0;
                bool jumpInput = false;

                if (Input.GetKey(KeyCode.A))
                {
                    horizontalInput = (sfloat)(-1f);
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    horizontalInput = (sfloat)(1f);
                }

                if (!autoJump)
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        jumpInput = true;
                    }
                }
                else
                {
                    if (ballData.IsOnGround((sfloat)bottomBoundary))
                    {
                        jumpInput = true;
                    }
                }

                rollbackManager.SavePlayerInput(horizontalInput, jumpInput);

                ballData.ApplyHorizontalInput(horizontalInput);
                ballData.ApplyJump(jumpInput, (sfloat)bottomBoundary);
            }
            else if (useMockInputs)
            {
                int currentFrame = rollbackManager.GetCurrentFrame();
            
                if (rollbackManager.GetPlayerInput(currentFrame, out PlayerInput input))
                {
                    ballData.ApplyHorizontalInput(input.HorizontalInput);
                    ballData.ApplyJump(input.JumpInput, (sfloat)bottomBoundary);
                }
            }

            ballData.UpdatePosition(
                (sfloat)fixedTimeStep,
                (sfloat)gravity,
                (sfloat)leftBoundary,
                (sfloat)rightBoundary,
                (sfloat)bottomBoundary,
                (sfloat)topBoundary
            );

            rollbackManager.SaveGameState(ballData);
        }

   

        private void PerformRollback()
        {
            isRollbackMode = true;
            Debug.Log("Rollback başlatılıyor...");

            int currentFrame = rollbackManager.GetCurrentFrame();
            int targetRollbackFrame = Mathf.Max(0, currentFrame - rollbackFrames);
        
            BallData newBallData = rollbackManager.RollbackAndResimulate(targetRollbackFrame, currentFrame);
        
            if (newBallData != null)
            {
                ballData = newBallData;
                Debug.Log($"Rollback tamamlandı. {targetRollbackFrame} frame'inden {currentFrame} frame'ine yeniden simüle edildi.");
            }
        
            isRollbackMode = false;
        }

        private IEnumerator VisualRollbackCoroutine()
        {
            isPlayingRollbackAnimation = true;
        
            if (spriteRenderer != null)
            {
                spriteRenderer.color = rollbackColor; 
            }

            int currentFrame = rollbackManager.GetCurrentFrame();
            int targetRollbackFrame = Mathf.Max(0, currentFrame - rollbackFrames);
        
            List<Vector2> history = new List<Vector2>(rollbackManager.PositionHistory);
        
            int rollbackSteps = Mathf.Min(history.Count, rollbackFrames);
        
            Debug.Log($"Görsel rollback başlatılıyor: {rollbackSteps} adım geri gidilecek.");
        
            for (int i = 0; i < rollbackSteps; i++)
            {
                int index = history.Count - 1 - i;
                if (index >= 0 && index < history.Count)
                {
                    transform.position = new Vector3(history[index].x, history[index].y, transform.position.z);
                
                
                
                    yield return new WaitForSeconds(1f / (visualRollbackSpeed * 60f)); // Hareket hızı
                }
            }
        
            BallData rollbackedBallData = rollbackManager.Rollback(targetRollbackFrame);
            if (rollbackedBallData != null)
            {
                ballData = rollbackedBallData;
                transform.position = new Vector3((float)ballData.PositionX, (float)ballData.PositionY, transform.position.z);
            }
        
            if (spriteRenderer != null)
            {
                spriteRenderer.color = resimulateColor; 
            }
        
            yield return new WaitForSeconds(0.5f); 
        
            BallData newBallData = rollbackManager.ResimulateToFrame(currentFrame, ballData);
            if (newBallData != null)
            {
                ballData = newBallData;
                // Simülasyon sonrası topun pozisyonunu güncelle
                transform.position = new Vector3((float)ballData.PositionX, (float)ballData.PositionY, transform.position.z);
            }
        
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        
            isPlayingRollbackAnimation = false;
            Debug.Log($"Görsel rollback tamamlandı. {targetRollbackFrame} frame'inden {currentFrame} frame'ine yeniden simüle edildi.");
        }
    
        // Test için mock inputlar ekle
        private void SetupMockInputs()
        {
            // Örnek olarak: İlk 60 frame sağa git
            rollbackManager.AddMockInputRange(0, 60, (sfloat)1.0f, false);
        
            // 61-120 frame arası sola git
            rollbackManager.AddMockInputRange(61, 120, (sfloat)(-1.0f), false);
        
            // 121. frame'de zıpla
            rollbackManager.AddMockInput(121, (sfloat)0.0f, true);
        
            // 122-180 frame arası sağa git
            rollbackManager.AddMockInputRange(122, 180, (sfloat)1.0f, false);
        
            // 181-240 frame arası sola git
            rollbackManager.AddMockInputRange(181, 240, (sfloat)(-1.0f), false);
        }
    
        public void TestRollback(int framesToRollback)
        {
            if (showVisualRollback)
            {
                StartCoroutine(VisualRollbackCoroutine());
            }
            else
            {
                PerformRollback();
            }
        }
    
        public BallData GetBallData()
        {
            return ballData;
        }
    
        public void SetBallData(BallData newBallData)
        {
            ballData = newBallData.Clone();
            transform.position = new Vector3((float)ballData.PositionX, (float)ballData.PositionY, transform.position.z);
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && rollbackManager != null)
            {
                List<Vector2> history = rollbackManager.PositionHistory;
            
                if (history.Count > 1)
                {
                    Gizmos.color = Color.yellow;
                    for (int i = 0; i < history.Count - 1; i++)
                    {
                        Gizmos.DrawLine(history[i], history[i + 1]);
                    }
                }
            }
        }
    }
}