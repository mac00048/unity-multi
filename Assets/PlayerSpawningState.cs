using PurrNet.StateMachine;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawningState : StateNode
{
    [SerializeField] private PlayerHealth playerPrefab;

    private SpawnPointManager spawnPointManager;

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer)
            return;

        spawnPointManager = FindFirstObjectByType<SpawnPointManager>();

        if (spawnPointManager == null)
        {
            Debug.LogError(
                "PlayerSpawningState: SpawnPointManager not found in scene!",
                this
            );
            return;
        }

        DespawnPlayers();

        var spawnedPlayers = SpawnPlayers();

        machine.Next(spawnedPlayers);
    }

    private List<PlayerHealth> SpawnPlayers()
    {
        var spawnedPlayers = new List<PlayerHealth>();

        int currentSpawnIndex = 0;

        foreach (var player in networkManager.players)
        {
            var spawnPoint = spawnPointManager.GetSpawnPointByIndex(currentSpawnIndex);

            var newPlayer = Instantiate(
                playerPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            Debug.Log(
                $"SPAWN PLAYER | player={player} | owner={newPlayer.owner} | isOwner={newPlayer.isOwner}"
            );

            newPlayer.GiveOwnership(player);
            spawnedPlayers.Add(newPlayer);

            currentSpawnIndex++;

            if (currentSpawnIndex >= spawnPointManager.SpawnPointCount)
                currentSpawnIndex = 0;
        }

        return spawnedPlayers;
    }

    private void DespawnPlayers()
    {
        var allPlayers = FindObjectsByType<PlayerHealth>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (var player in allPlayers)
        {
            Destroy(player.gameObject);
        }
    }
}