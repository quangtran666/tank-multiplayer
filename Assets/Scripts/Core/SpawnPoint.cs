using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private static readonly List<SpawnPoint> spawnPoints = new();

    void OnEnable()
    {
        spawnPoints.Add(this);
    }

    void OnDisable()
    {
        spawnPoints.Remove(this);
    }

    public static Vector3 GetRandomSpawnPos()
    {
        if (spawnPoints.Count == 0) return Vector3.zero;

        return spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
