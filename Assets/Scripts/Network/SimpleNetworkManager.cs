using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SimpleNetworkManager : MonoBehaviour
{
    public enum Mode { None, Server, Client }
    public Mode mode = Mode.None;

    UdpClient udp;
    IPEndPoint remoteEP;

    Thread receiveThread;
    bool isRunning = false;

    public event Action<ulong, ulong> OnDataReceived;

    public string ipAddress = "127.0.0.1"; // Server IP (client kullanır)
    public int port = 9050;                // Same port for both ends

    void OnDestroy()
    {
        isRunning = false;
        udp?.Close();
        receiveThread?.Abort();
    }

    public void StartAsServer()
    {
        mode = Mode.Server;
        udp = new UdpClient(port);
        remoteEP = new IPEndPoint(IPAddress.Any, port);
        Debug.Log("Server started. Waiting for client...");
        StartReceiving();
    }

    public void StartAsClient()
    {
        mode = Mode.Client;
        udp = new UdpClient();
        remoteEP = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        StartReceiving();
        Debug.Log("Client started. Trying to connect...");
    }

    void StartReceiving()
    {
        isRunning = true;
        receiveThread = new Thread(() =>
        {
            while (isRunning)
            {
                try
                {
                    var receivedData = udp.Receive(ref remoteEP);
                    ulong input = BitConverter.ToUInt64(receivedData, 0);
                    ulong frame = BitConverter.ToUInt64(receivedData, 8);
                    OnDataReceived?.Invoke(input, frame);
                }
                catch (Exception ex)
                {
                    Debug.Log("Receive error: " + ex.Message);
                }
            }
        });
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    public void SendData(ulong input, ulong frame)
    {
        byte[] buffer = new byte[16];
        Array.Copy(BitConverter.GetBytes(input), 0, buffer, 0, 8);
        Array.Copy(BitConverter.GetBytes(frame), 0, buffer, 8, 8);

        if (mode == Mode.Server && remoteEP != null)
            udp.Send(buffer, buffer.Length, remoteEP);
        else if (mode == Mode.Client)
            udp.Send(buffer, buffer.Length, remoteEP);
    }
}
