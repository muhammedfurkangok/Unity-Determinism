using System.Net;
using System.Net.Sockets;
using UnityEngine;

[AddComponentMenu("Networking/Network Config")]
public class NetworkConfig : MonoBehaviour
{
    [Header("Remote Settings")]
    public string remoteIP   = "127.0.0.1";
    public int    remotePort = 7777;

    [Header("Local Settings")]
    public int    listenPort = 7778;

    public string localIP { get; private set; }

    void Reset()
    {
        localIP    = GetLocalIPv4();
        remoteIP   = localIP;
        remotePort = 7777;
        listenPort = 7778;
    }

    void Awake()
    {
        localIP = GetLocalIPv4();
    }

    string GetLocalIPv4()
    {
        foreach (var ni in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            if (ni.AddressFamily == AddressFamily.InterNetwork)
                return ni.ToString();
        return "127.0.0.1";
    }
}