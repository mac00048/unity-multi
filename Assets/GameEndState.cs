using PurrNet.StateMachine;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using PurrNet;
using System.Linq;

public class GameEndState : StateNode
{
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if(!InstanceHandler.TryGetInstance(out ScoreManager scoreManager))
        {
            Debug.LogError("GameEndState failed to get ScoreManager", this);
            return;
        }

        var winner = scoreManager.GetWinner();
        if(winner == default)
        {
            Debug.LogError("GameEndState failed to get winner", this);
            return;
        }

        if(!InstanceHandler.TryGetInstance(out EndGameView endGameView))
        {
            Debug.Log($"Failed to get end game view", this);
            return;
        }

        if(!InstanceHandler.TryGetInstance(out GameViewManager gameVewManager))
        {
            Debug.Log($"Failed to get game view manager", this);
            return;
        }

        endGameView.SetWinner(winner);
        gameVewManager.ShowView<EndGameView>();
        Debug.Log($"Game ended with winner {winner}");


    }
}
