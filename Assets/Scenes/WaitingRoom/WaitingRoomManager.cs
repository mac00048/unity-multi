using PurrNet;
using PurrNet.Modules;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PurrLobby;

public class WaitingRoomManager : NetworkBehaviour
{
    [Header("Lobby")]
    [SerializeField] private int maxPlayers = 8;

    [Header("Game")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Players")]
    [SerializeField] private int minPlayers = 2;

    [Header("UI")]
    [SerializeField] private WaitingRoomUI waitingRoomUI;

    private readonly List<PlayerReady> players = new();

    private float uiUpdateTimer;

    // =========================================================
    // SPAWN
    // =========================================================

    protected override void OnSpawned(bool asServer)
    {
        if (!asServer)
            return;

        UpdatePlayersUI();

        Debug.Log(
            $"WAITING ROOM STARTED | " +
            $"Players: {players.Count}/{maxPlayers}"
        );
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!isServer)
            return;

        CleanupPlayers();

        // Обновляем UI 4 раза в секунду.
        uiUpdateTimer -= Time.deltaTime;

        if (uiUpdateTimer <= 0f)
        {
            uiUpdateTimer = 0.25f;
            UpdatePlayersUI();
        }
    }

    // =========================================================
    // REGISTER PLAYER
    // =========================================================

    public void RegisterPlayer(PlayerReady player)
    {
        if (!isServer)
            return;

        if (player == null)
            return;

        if (players.Contains(player))
            return;

        if (players.Count >= maxPlayers)
        {
            Debug.LogWarning(
                $"WAITING ROOM | Maximum players reached: {maxPlayers}"
            );

            return;
        }

        players.Add(player);

        Debug.Log(
            $"WAITING ROOM | Player registered: " +
            $"{players.Count}/{maxPlayers}"
        );

        UpdatePlayersUI();
    }

    // =========================================================
    // UNREGISTER PLAYER
    // =========================================================

    public void UnregisterPlayer(PlayerReady player)
    {
        if (!isServer)
            return;

        if (player == null)
            return;

        players.Remove(player);

        Debug.Log(
            $"WAITING ROOM | Player removed: " +
            $"{players.Count}/{maxPlayers}"
        );

        UpdatePlayersUI();
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void CleanupPlayers()
    {
        int oldCount = players.Count;

        players.RemoveAll(
            player => player == null
        );

        if (oldCount != players.Count)
        {
            UpdatePlayersUI();
        }
    }

    // =========================================================
    // PLAYER COUNT
    // =========================================================

    public int PlayerCount
    {
        get
        {
            return players.Count;
        }
    }

    // =========================================================
    // READY COUNT
    // =========================================================

    public int ReadyPlayerCount
    {
        get
        {
            int readyCount = 0;

            foreach (PlayerReady player in players)
            {
                if (player == null)
                    continue;

                if (player.IsReady)
                {
                    readyCount++;
                }
            }

            return readyCount;
        }
    }

    // =========================================================
    // CAN START GAME
    // =========================================================

    public bool CanStartGame()
    {
        // Не начинаем без минимального количества игроков.
        if (players.Count < minPlayers)
            return false;

        // Все подключенные игроки должны быть готовы.
        foreach (PlayerReady player in players)
        {
            if (player == null)
                return false;

            if (!player.IsReady)
                return false;
        }

        return true;
    }

    // =========================================================
    // FORCE START
    // =========================================================
    //
    // Вызывается ТОЛЬКО host-терминалом.
    //
    // Host является server, поэтому RPC здесь не нужен.
    //
    // =========================================================

    public void ForceStartGame()
    {
        if (!isServer)
        {
            Debug.LogWarning(
                "WAITING ROOM | ForceStartGame ignored: not server."
            );

            return;
        }

        if (!CanStartGame())
        {
            Debug.Log(
                $"WAITING ROOM | Cannot start game. " +
                $"Ready: {ReadyPlayerCount}/{PlayerCount}"
            );

            return;
        }

        Debug.Log(
            $"WAITING ROOM | HOST STARTED GAME | " +
            $"Ready: {ReadyPlayerCount}/{PlayerCount}"
        );

        StartGame();
    }

    // =========================================================
    // GAME START
    // =========================================================

    private void StartGame()
    {
        if (!isServer)
            return;

        Debug.Log(
            $"WAITING ROOM | Loading game scene: {gameSceneName}"
        );

        var settings = new PurrSceneSettings
        {
            isPublic = true,
            mode = LoadSceneMode.Single
        };

        networkManager.sceneModule.LoadSceneAsync(
            gameSceneName,
            settings
        );
    }

    // =========================================================
    // UI
    // =========================================================

    private void UpdatePlayersUI()
    {
        SendPlayersToClients(
            players.Count,
            maxPlayers
        );
    }

    [ObserversRpc]
    private void SendPlayersToClients(
        int current,
        int max)
    {
        WaitingRoomUI.Instance?.SetPlayers(
            current,
            max
        );
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        // Просто визуальный маркер объекта в Editor.
    }

#endif
}