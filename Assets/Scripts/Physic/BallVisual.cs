using System.Collections;
using System.Collections.Generic;
using Network;
using PhysicSystem;
using UnityEngine;
using SoftFloat;

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
    [SerializeField] private GameObject ghostTrailPrefab; // İsteğe bağlı: geçmiş pozisyonları göstermek için hayalet prefab
    [SerializeField] private LineRenderer positionHistoryTrail; // Pozisyon geçmişini görselleştirmek için

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

        // Mock inputları ekle (test için)
        if (useMockInputs)
        {
            SetupMockInputs();
        }
        
        // Line renderer varsa ayarla
        if (positionHistoryTrail != null)
        {
            positionHistoryTrail.positionCount = 0;
        }
    }

    void Update()
    {
        // Eğer rollback animasyonu oynatılıyorsa, normal update akışını atlıyoruz
        if (isPlayingRollbackAnimation)
        {
            return;
        }

        // Rollback tuşuna basıldığında
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

        // Fixed timestep mantığı için
        accumulatedTime += Time.deltaTime;
        
        while (accumulatedTime >= fixedTimeStep)
        {
            FixedUpdate();
            accumulatedTime -= fixedTimeStep;
        }

        // Top pozisyonunu güncelle
        transform.position = new Vector3((float)ballData.PositionX, (float)ballData.PositionY, transform.position.z);
        
        // Pozisyon geçmişini görselleştir
        UpdatePositionTrail();
    }

    void FixedUpdate()
    {
        // Yeni frame'e geç
        rollbackManager.AdvanceFrame();
        
        // Mock input kullanıyorsak, gerçek girdi okumayı atla
        if (!useMockInputs && recordPlayerMovement)
        {
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

            // Girdileri kaydet
            rollbackManager.SavePlayerInput(horizontalInput, jumpInput);

            // Girdileri uygula
            ballData.ApplyHorizontalInput(horizontalInput);
            ballData.ApplyJump(jumpInput, (sfloat)bottomBoundary);
        }
        else if (useMockInputs)
        {
            // Mock inputları kullan (playerInputs dictionary'sinden çekiliyor)
            int currentFrame = rollbackManager.GetCurrentFrame();
            
            // RollbackManager'daki mevcut frame için playerInputs dictionary'sinde bir entry varsa kullan
            if (rollbackManager.GetPlayerInput(currentFrame, out PlayerInput input))
            {
                ballData.ApplyHorizontalInput(input.HorizontalInput);
                ballData.ApplyJump(input.JumpInput, (sfloat)bottomBoundary);
            }
        }

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

    // Pozisyon geçmişini görselleştir
    private void UpdatePositionTrail()
    {
        if (positionHistoryTrail != null)
        {
            List<Vector2> history = rollbackManager.PositionHistory;
            positionHistoryTrail.positionCount = history.Count;
            
            for (int i = 0; i < history.Count; i++)
            {
                positionHistoryTrail.SetPosition(i, new Vector3(history[i].x, history[i].y, 0));
            }
        }
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

    // Görsel geri sarma coroutine'i
    private IEnumerator VisualRollbackCoroutine()
    {
        isPlayingRollbackAnimation = true;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = rollbackColor; // Rollback rengine geç
        }

        // Geri sarma animasyonu için gerekli değerleri hesapla
        int currentFrame = rollbackManager.GetCurrentFrame();
        int targetRollbackFrame = Mathf.Max(0, currentFrame - rollbackFrames);
        
        // Pozisyon geçmişi alalım
        List<Vector2> history = new List<Vector2>(rollbackManager.PositionHistory);
        
        // Rollback yapılacak pozisyon sayısını hesapla
        int rollbackSteps = Mathf.Min(history.Count, rollbackFrames);
        
        Debug.Log($"Görsel rollback başlatılıyor: {rollbackSteps} adım geri gidilecek.");
        
        // 1. Aşama: Görsel olarak geri sarma (pozisyon geçmişini ters yönde oynat)
        for (int i = 0; i < rollbackSteps; i++)
        {
            int index = history.Count - 1 - i;
            if (index >= 0 && index < history.Count)
            {
                // Top pozisyonunu güncelle - BURASI ÖNEMLİ!
                transform.position = new Vector3(history[index].x, history[index].y, transform.position.z);
                
                // İsteğe bağlı: Her geçmiş pozisyonda bir hayalet iz bırak
                if (ghostTrailPrefab != null && i % 5 == 0) // Her 5 adımda bir iz
                {
                    GameObject ghost = Instantiate(ghostTrailPrefab, transform.position, Quaternion.identity);
                    ghost.transform.localScale = transform.localScale * 0.5f; // Hayaletler daha küçük
                    Destroy(ghost, 0.5f); // Kısa süre sonra yok ol
                }
                
                yield return new WaitForSeconds(1f / (visualRollbackSpeed * 60f)); // Hareket hızı
            }
        }
        
        // Gerçek rollback işlemini yap
        BallData rollbackedBallData = rollbackManager.Rollback(targetRollbackFrame);
        if (rollbackedBallData != null)
        {
            ballData = rollbackedBallData;
            // Rollback sonrası topun pozisyonunu güncelle
            transform.position = new Vector3((float)ballData.PositionX, (float)ballData.PositionY, transform.position.z);
        }
        
        // Line renderer'ı temizle
        if (positionHistoryTrail != null)
        {
            positionHistoryTrail.positionCount = 0;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = resimulateColor; // Yeniden simülasyon rengine geç
        }
        
        yield return new WaitForSeconds(0.5f); // Kısa bir bekleme
        
        // 2. Aşama: Yeniden simülasyon
        BallData newBallData = rollbackManager.ResimulateToFrame(currentFrame, ballData);
        if (newBallData != null)
        {
            ballData = newBallData;
            // Simülasyon sonrası topun pozisyonunu güncelle
            transform.position = new Vector3((float)ballData.PositionX, (float)ballData.PositionY, transform.position.z);
        }
        
        // Normale dön
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
    
    // Rollback test
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
    
    // Oyun durumunu döndüren metot - online implementasyon için
    public BallData GetBallData()
    {
        return ballData;
    }
    
    // Dışarıdan oyun durumunu ayarlayan metot - online implementasyon için
    public void SetBallData(BallData newBallData)
    {
        ballData = newBallData.Clone();
        transform.position = new Vector3((float)ballData.PositionX, (float)ballData.PositionY, transform.position.z);
    }

    // Editor görselleştirmesi
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