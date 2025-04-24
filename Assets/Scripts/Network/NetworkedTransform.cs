using UnityEngine;

[RequireComponent(typeof(Transform))]
[AddComponentMenu("Networking/Networked Transform")]
public class NetworkedTransform : MonoBehaviour
{
    static int nextId = 1;
    public int objectId;

    NetworkManager mgr;
    Transform     tr;

    void Awake()
    {
        objectId = nextId++;
        tr       = transform;
    }

    void Start()
    {
        mgr = FindObjectOfType<NetworkManager>();
        mgr.Register(this);
    }

    void Update()
    {
        // Her kare JSON’a çevirip manager aracılığıyla gönder
        var msg = new NetworkManager.SyncMessage {
            id = objectId,
            px = tr.position.x, py = tr.position.y, pz = tr.position.z,
            rx = tr.rotation.x, ry = tr.rotation.y, rz = tr.rotation.z, rw = tr.rotation.w
        };
        var json = JsonUtility.ToJson(msg);
        mgr.Broadcast(System.Text.Encoding.UTF8.GetBytes(json));
    }

    // Karşı taraftan gelen pozisyonu uygula
    public void ApplyRemote(NetworkManager.SyncMessage msg)
    {
        tr.position = new Vector3(msg.px, msg.py, msg.pz);
        tr.rotation = new Quaternion(msg.rx, msg.ry, msg.rz, msg.rw);
    }
}