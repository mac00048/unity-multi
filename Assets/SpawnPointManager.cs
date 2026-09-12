using System.Collections.Generic;
using UnityEngine;

// ≈динственный источник точек спавна в сцене.
// »спользуетс€ как при первом спавне (PlayerSpawningState),
// так и при респавне после смерти (PlayerHealth).
public class SpawnPointManager : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints = new();

    public int SpawnPointCount => spawnPoints.Count;

    // —лучайна€ точка Ч используетс€ дл€ респавна после смерти.
    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError(
                "SpawnPointManager: No spawn points assigned!",
                this
            );
            return null;
        }

        int index = Random.Range(0, spawnPoints.Count);
        return spawnPoints[index];
    }

    // “очка по индексу с зацикливанием Ч используетс€
    // дл€ равномерного распределени€ игроков при старте матча.
    public Transform GetSpawnPointByIndex(int index)
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError(
                "SpawnPointManager: No spawn points assigned!",
                this
            );
            return null;
        }

        int safeIndex = index % spawnPoints.Count;
        return spawnPoints[safeIndex];
    }
}