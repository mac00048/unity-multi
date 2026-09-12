using PurrNet;
using UnityEngine;

public class PlayerReady : NetworkBehaviour
{
    private SyncVar<bool> isReady = new(false);

    public bool IsReady => isReady.value;

    protected override void OnSpawned(bool asServer)
    {
        if (!asServer)
            return;

        isReady.value = false;

        WaitingRoomManager manager =
            FindFirstObjectByType<WaitingRoomManager>();

        if (manager != null)
        {
            manager.RegisterPlayer(this);
        }
    }

    public void ToggleReady()
    {
        if (!isOwner)
            return;

        SetReadyServer(!isReady.value);
    }

    [ServerRpc]
    private void SetReadyServer(bool value)
    {
        isReady.value = value;
    }
}