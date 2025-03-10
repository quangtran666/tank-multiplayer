using UnityEngine;

public class SpawnOnDestroy : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    void OnDestroy()
    {
        Instantiate(prefab, transform.position, Quaternion.identity);
    }
}
