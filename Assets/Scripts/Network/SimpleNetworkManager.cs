using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class SimpleNetworkManager : MonoBehaviour
{
    public enum Mode
    {
        None,
        Server,
        Client
    }

    public Mode mode = Mode.None;

    private UdpClient udp;
    private IPEndPoint remoteEP;
    private Thread receiveThread;

    public int port = 7777;
    public string serverIP = "127.0.0.1";

    public GameObject connectionPanel;
    public GameObject ballObject;

    public Action<ulong, ulong> OnDataReceived;

    private bool gameStarted = false;

    public void StartAsServer()
    {
        mode = Mode.Server;
        udp = new UdpClient(port);
        Debug.Log(3"Server started on port " + port);
        StartReceiving();

        // Bu satırı ekle:
        UnityMainThreadDispatcher.Instance().Enqueue(StartGame);
    }


    public void StartAsClient()
    {
        mode = Mode.Client;
        udp = new UdpClient();
        remoteEP = new IPEndPoint(IPAddress.Parse(serverIP), port);
        Debug.Log("Client started, sending to " + serverIP + ":" + port);
        StartReceiving();

        // Server'a kendini tanıtmak için boş veri gönder
        SendData(0, 0);
    }

    public void SendData(ulong input, ulong frame)
    {
        if (mode == Mode.None)
        {
            Debug.LogWarning("Network mode not set.");
            return;
        }

        if (mode == Mode.Server && remoteEP == null)
        {
            Debug.LogWarning("Client bağlantısı bekleniyor...");
            return;
        }

        byte[] data = new byte[16];
        BitConverter.GetBytes(input).CopyTo(data, 0);
        BitConverter.GetBytes(frame).CopyTo(data, 8);

        try
        {
            udp.Send(data, data.Length, remoteEP);
        }
        catch (Exception e)
        {
            Debug.LogError("SendData hatası: " + e.Message);
        }
    }

    private void StartReceiving()
    {
        receiveThread = new Thread(() =>
        {
            while (true)
            {
                try
                {
                    var senderEP = new IPEndPoint(IPAddress.Any, 0);
                    var data = udp.Receive(ref senderEP);

                    // Server ilk client'i tanır
                    if (mode == Mode.Server && remoteEP == null)
                    {
                        remoteEP = senderEP;
                        Debug.Log("Client connected: " + remoteEP);
                        UnityMainThreadDispatcher.Instance().Enqueue(StartGame);
                    }

                    if (data.Length == 16)
                    {
                        ulong input = BitConverter.ToUInt64(data, 0);
                        ulong frame = BitConverter.ToUInt64(data, 8);

                        OnDataReceived?.Invoke(input, frame);

                        // Client ilk veri aldığında başlatır
                        if (mode == Mode.Client && !gameStarted)
                        {
                            gameStarted = true;
                            UnityMainThreadDispatcher.Instance().Enqueue(StartGame);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Receive hatası: " + e.Message);
                }
            }
        });

        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void StartGame()
    {
        Debug.Log("Oyun başlatılıyor.");

        if (connectionPanel != null)
            connectionPanel.SetActive(false);

        if (ballObject != null)
            ballObject.SetActive(true);
    }

    private void OnApplicationQuit()
    {
        receiveThread?.Abort();
        udp?.Close();
    }
}