using PurrNet;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoardView : View
{

    [SerializeField] private Transform scoreboardEntriesParent;
    [SerializeField] private ScoreboardEntry scoreboardEntryPrefab;
    private GameViewManager _gameViewManager;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);


    }

    private void Start()
    {
        _gameViewManager = InstanceHandler.GetInstance<GameViewManager>();
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<ScoreBoardView>();
    }

    public void SetData(Dictionary<PlayerID, ScoreManager.ScoreData> data)
    {

        foreach(Transform child in scoreboardEntriesParent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach(var playerScore in data)
        {
            var entry = Instantiate(scoreboardEntryPrefab, scoreboardEntriesParent);
            entry.SetData(playerScore.Key.id.ToString(), playerScore.Value.kills, playerScore.Value.deaths);
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            _gameViewManager.ShowView<ScoreBoardView>(false);

            if (Input.GetKeyUp(KeyCode.Tab))
                _gameViewManager.HideView<ScoreBoardView>();
        
    }


    public override void OnHide()
    {

    }

    public override void OnShow()
    {

    }

   
}
