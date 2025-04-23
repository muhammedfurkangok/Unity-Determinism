using PhysicSystem;
using UnityEngine;
using SoftFloat;
using System.Collections.Generic;

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

    private BallData ballData;
    private RollbackManager rollbackManager;

    private float leftBoundary;
    private float rightBoundary;
    private float topBoundary;
    private float bottomBoundary;

    private float accumulatedTime = 0f;
    private bool isRollbackMode = false;

    void Start()
    {
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

        // Rollback manager'ı başlat
        rollbackManager = new RollbackManager(
            (sfloat)gravity,
            (sfloat)leftBoundary,
            (sfloat)rightBoundary,
            (sfloat)bottomBoundary,
            (sfloat)topBoundary,
            (sfloat)fixedTimeStep
        );

        // İlk durumu kaydet
        rollbackManager.SaveGameState(ballData);
    }

    void Update()
    {
        // Rollback tuşuna basıldığında
        if (Input.GetKeyDown(rollbackKey) && !isRollbackMode)
        {
            PerformRollback();
            return;
        }

        // Fixed timestep mantığı için
        accumulatedTime += Time.deltaTime;
        
        while (accumulatedTime >= fixedTimeStep)
        {
            FixedUpdate();
            accumulatedTime -= fixedTimeStep;
        }

        // Top pozisyonunu güncelle
        transform.position = new Vector3((float)ballData.PositionX, (float)ballData.PositionY, transform.position.z);
    }

    void FixedUpdate()
    {
        // Yeni frame'e geç
        rollbackManager.AdvanceFrame();
        
        // Oyuncu girdilerini oku
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
            if (Input.GetKeyDown(KeyCode.Space))
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

        // Girdileri kaydet
        rollbackManager.SavePlayerInput(horizontalInput, jumpInput);

        // Girdileri uygula
        ballData.ApplyHorizontalInput(horizontalInput);
        ballData.ApplyJump(jumpInput, (sfloat)bottomBoundary);

        // Fizik simülasyonunu çalıştır
        ballData.UpdatePosition(
            (sfloat)fixedTimeStep,
            (sfloat)gravity,
            (sfloat)leftBoundary,
            (sfloat)rightBoundary,
            (sfloat)bottomBoundary,
            (sfloat)topBoundary
        );

        // Oyun durumunu kaydet
        rollbackManager.SaveGameState(ballData);
    }

    // Rollback işlemini gerçekleştir
    private void PerformRollback()
    {
        isRollbackMode = true;
        Debug.Log("Rollback başlatılıyor...");

        int currentFrame = rollbackManager.GetCurrentFrame();
        int targetRollbackFrame = Mathf.Max(0, currentFrame - rollbackFrames);
        
        // Rollback yap ve simülasyonu yeniden çalıştır
        BallData newBallData = rollbackManager.RollbackAndResimulate(targetRollbackFrame, currentFrame);
        
        if (newBallData != null)
        {
            ballData = newBallData;
            Debug.Log($"Rollback tamamlandı. {targetRollbackFrame} frame'inden {currentFrame} frame'ine yeniden simüle edildi.");
        }
        
        isRollbackMode = false;
    }

    // Test için bir metot
    public void TestRollback(int framesToRollback)
    {
        int currentFrame = rollbackManager.GetCurrentFrame();
        int targetFrame = Mathf.Max(0, currentFrame - framesToRollback);
        
        BallData rollbackedBallData = rollbackManager.Rollback(targetFrame);
        if (rollbackedBallData != null)
        {
            ballData = rollbackedBallData;
            Debug.Log($"{targetFrame} frame'ine rollback yapıldı!");
        }
    }
    
    // Oyun durumunu döndüren metot - online implementasyon için kullanışlı olabilir
    public BallData GetBallData()
    {
        return ballData;
    }
    
    // Dışarıdan oyun durumunu ayarlayan metot - online implementasyon için kullanışlı olabilir
    public void SetBallData(BallData newBallData)
    {
        ballData = newBallData;
    }
}