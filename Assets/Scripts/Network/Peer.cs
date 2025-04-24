using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Peer
{
    UdpClient udp;
    IPEndPoint remoteEP;
    Thread     listenThread;
    public Action<byte[]> onDataReceived;

    public Peer(string remoteIP, int remotePort, int listenPort)
    {
        remoteEP = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
        udp      = new UdpClient(listenPort);
        listenThread = new Thread(ListenLoop) { IsBackground = true };
        listenThread.Start();
    }

    void ListenLoop()
    {
        var ep = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            try
            {
                var data = udp.Receive(ref ep);
                onDataReceived?.Invoke(data);
            }
            catch { break; }
        }
    }

    public void Send(byte[] data)
    {
        udp.Send(data, data.Length, remoteEP);
    }

    public void Close()
    {
        listenThread?.Abort();
        udp.Close();
    }
}