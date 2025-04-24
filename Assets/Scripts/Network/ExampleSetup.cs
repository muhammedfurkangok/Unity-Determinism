using UnityEngine;

// Sahnede örnek objeleri otomatik yaratmak için (isteğe bağlı)
public class ExampleSetup : MonoBehaviour
{
    public GameObject prefab;
    public int spawnCount = 2;
    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            var go = Instantiate(prefab, new Vector3(i * 2f, 0, 0), Quaternion.identity);
            go.name = $"NetObj_{i+1}";
            go.AddComponent<NetworkedTransform>();
        }
    }
}