using UnityEngine;

public class NetworkTestGame : MonoBehaviour
{
    public SimpleNetworkManager networkManager;

    private ulong currentFrame = 0;
    private ulong inputValue = 0;

    void Start()
    {
        networkManager.OnDataReceived += OnNetworkData;
    }

    void Update()
    {
        if (networkManager.mode == SimpleNetworkManager.Mode.None) return;

        currentFrame++;

        // Simulated input (örnek: space tuşuna basınca 1)
        inputValue = (ulong)(Input.GetKey(KeyCode.Space) ? 1 : 0);

        // Her frame veri gönder
        networkManager.SendData(inputValue, currentFrame);
    }

    void OnNetworkData(ulong receivedInput, ulong receivedFrame)
    {
        Debug.Log($"[Received] Frame: {receivedFrame} | Input: {receivedInput}");
        // Burada rollback sisteminle entegre edebilirsin
    }
}