using PurrNet;
using UnityEngine;
using UnityEngine.UI;

public class LocalPlayerUI : MonoBehaviour
{
    [SerializeField]
    private Button readyButton;

    private PlayerReady localPlayer;

    private void Start()
    {
        readyButton.onClick.AddListener(OnReadyClicked);
    }

    private void Update()
    {
        if (localPlayer != null)
            return;

        PlayerReady[] players =
            FindObjectsByType<PlayerReady>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (PlayerReady player in players)
        {
            if (player.isOwner)
            {
                localPlayer = player;
                break;
            }
        }
    }

    private void OnReadyClicked()
    {
        if (localPlayer == null)
            return;

        localPlayer.ToggleReady();
    }

    private void OnDestroy()
    {
        readyButton.onClick.RemoveListener(OnReadyClicked);
    }
}