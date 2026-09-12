using PurrNet.StateMachine;
using System.Collections;
using UnityEngine;

public class WaitForPlayers : StateNode
{
    [SerializeField] private int minPlayers = 2;

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer)
            return;

        StartCoroutine(WaitUntilPlayersReady());
    }

    private IEnumerator WaitUntilPlayersReady()
    {
        while (networkManager.players.Count < minPlayers)
        {
            Debug.Log($"Players connected: {networkManager.players.Count}");
            yield return null;
        }

        Debug.Log("Minimum players reached!");
        machine.Next();
    }
}