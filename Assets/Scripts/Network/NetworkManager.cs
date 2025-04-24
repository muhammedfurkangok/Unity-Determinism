using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Networking/Network Manager")]
public class NetworkManager : MonoBehaviour
{
    public NetworkConfig config;
    Peer peer;

    // Kayıtlı networklenmiş objeler
    List<NetworkedTransform> networkedObjects = new List<NetworkedTransform>();

    void Awake()
    {
        config = GetComponent<NetworkConfig>();
        peer   = new Peer(config.remoteIP, config.remotePort, config.listenPort);
        peer.onDataReceived += OnReceiveData;
    }

    void OnDestroy()
    {
        peer.Close();
    }

    // Her NetworkedTransform, buradan kaydolur:
    public void Register(NetworkedTransform nt)  
        => networkedObjects.Add(nt);

    // NetworkedTransform'dan gelen data'yı yay:
    public void Broadcast(byte[] bytes)
        => peer.Send(bytes);

    void OnReceiveData(byte[] data)
    {
        // Gelen paket JSON ise parse et ve ilgili objeye yolla
        var msg = JsonUtility.FromJson<SyncMessage>(System.Text.Encoding.UTF8.GetString(data));
        foreach (var nt in networkedObjects)
            if (nt.objectId == msg.id)
                nt.ApplyRemote(msg);
    }

    // Basit mesaj yapısı
    [System.Serializable]
    public struct SyncMessage
    {
        public int    id;
        public float  px, py, pz;
        public float  rx, ry, rz, rw;
    }
}