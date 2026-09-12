using PurrNet.StateMachine;
using UnityEngine;
using System.Collections.Generic;
using System;
using PurrNet;

public class RoundRunningState : StateNode<List<PlayerHealth>>
{

    private List<PlayerID> _players = new();
    public override void Enter(List<PlayerHealth> data, bool asServer)
    {
        base.Enter(data, asServer);


        if (!asServer)
            return;

        _players.Clear();


        foreach(var player in data)
        {
            if (player.owner.HasValue)
            {
                _players.Add(player.owner.Value);
                player.onDeath_server += OnPlayerDeath;
            }
        }
    }

    private void OnPlayerDeath(PlayerID deadPlayer)
    {

        _players.Remove(deadPlayer);



        if (_players.Count <= 1)
        {
                machine.Next();
        }


    }
}
