using PurrNet;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
   [SerializeField] private SyncDictionary<PlayerID, ScoreData> scores = new();


    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
        scores.onChanged += OnScoresChanged;
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<ScoreManager>();
        scores.onChanged -= OnScoresChanged;
    }

    private void OnScoresChanged(SyncDictionaryChange<PlayerID, ScoreData> change)
    {
        if(InstanceHandler.TryGetInstance(out ScoreBoardView scoreBoardView))
        {
            scoreBoardView.SetData(scores.ToDictionary());
        }
    }

    public void AddKIll(PlayerID playerID)
    {
        CheckForDictionaryEntry(playerID);

        var scoreData = scores[playerID];
        scoreData.kills++;
        scores[playerID] = scoreData;
    }

    public void AddDeath(PlayerID playerID)
    {
        CheckForDictionaryEntry(playerID);

        var scoreData = scores[playerID];
        scoreData.deaths++;
        scores[playerID] = scoreData;
    }

    public PlayerID GetWinner()
    {
        PlayerID winner = default;
        var highestKills = 0;

       foreach( var score in scores)
        {
            if(score.Value.kills > highestKills)
            {
                highestKills = score.Value.kills;
                winner = score.Key;
            }
        }

        return winner;
    }


    private void CheckForDictionaryEntry(PlayerID playerID)
    {
        if(!scores.ContainsKey(playerID))
            scores.Add(playerID, new ScoreData());
    }


    public struct ScoreData
    {
        public int kills;
        public int deaths;


        public override string ToString()
        {
            return $"{kills}/{deaths}";
        }
    }

    
}
