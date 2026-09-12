using PurrNet.StateMachine;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using PurrNet;
public class RoundEndState : StateNode
{
    [SerializeField] StateNode spawningState;
    [SerializeField] private int amountOfRounds = 3;


    private int _roundCount = 0;
    private WaitForSeconds _delay = new(3f);


    public override void Enter(bool asServer)
    {


        base.Enter(asServer);

        if (!asServer)
            return;


        CheckForGameEnd();
       
    }

    private void CheckForGameEnd()
    {
        _roundCount++;
        if (_roundCount > amountOfRounds)
        {
            machine.Next();
            return;
        }
        StartCoroutine(DelayNextState());
    }

    private IEnumerator DelayNextState()
    {
        yield return _delay;
        machine.SetState(spawningState);
    }
}
