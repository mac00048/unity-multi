using PurrNet;
using UnityEngine;

public class NetworkDebug : NetworkBehaviour
{
    protected override void OnSpawned(bool asServer)
    {
        Debug.Log(
            $"WAITING ROOM NETWORK SPAWNED | Server: {asServer}"
        );
    }

    private void Update()
    {
        if (isServer)
        {
            Debug.Log(
                $"Server players: {networkManager.players.Count}"
            );
        }
    }
}