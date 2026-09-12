using PurrNet;
using System.Collections;
using TMPro;
using UnityEngine;

public class EndGameView : View
{

    [SerializeField] private TMP_Text winnertext;
    [SerializeField] private float fadeDuration = 1f;


    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<EndGameView>();
    }

    public void SetWinner(PlayerID winner)
    {
        winnertext.text = $"Player {winner.id} Wins the round";

    }


    public override void OnHide()
    {

    }

    public override void OnShow()
    {

    }
}
