using PurrNet;
using UnityEngine;

public class WaitingRoomPlayerSpawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private Transform[] spawnPoints;

    [SerializeField]
    private WaitingRoomManager waitingRoomManager;

    private int nextSpawnIndex;

    protected override void OnSpawned(bool asServer)
    {
        if (!asServer)
            return;

        // Здесь можно вызвать регистрацию уже существующих игроков,
        // если они были заспавнены до менеджера.
    }

    public void SpawnPlayer()
    {
        if (!isServer)
            return;

        Transform spawnPoint = GetNextSpawnPoint();

        GameObject player = Instantiate(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        PlayerReady ready = player.GetComponent<PlayerReady>();

        if (ready != null)
        {
            waitingRoomManager.RegisterPlayer(ready);
        }
    }

    private Transform GetNextSpawnPoint()
    {
        if (spawnPoints.Length == 0)
            return transform;

        Transform point = spawnPoints[nextSpawnIndex];

        nextSpawnIndex++;

        if (nextSpawnIndex >= spawnPoints.Length)
            nextSpawnIndex = 0;

        return point;
    }
}