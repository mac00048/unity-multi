using UnityEngine;
using System.Collections;
using PurrLobby;
using PurrNet;
using PurrNet.Logging;
using PurrNet.Steam;
using PurrNet.Transports;
using System;
using Steamworks;


public class ConnectionStarter : MonoBehaviour
{
    private SteamTransport _steamTransport;

    private NetworkManager _networkManager;
    private LobbyDataHolder _lobbyDataHolder;

    private bool _isFromLobby;

    private void Awake()
    {
        Debug.Log($"[ConnectionStarter] GameObject: {gameObject.name}");

        var components = GetComponents<Component>();

        foreach (var component in components)
        {
            Debug.Log($"[ConnectionStarter] Component: {component.GetType().FullName}");
        }

        _networkManager = GetComponent<NetworkManager>();
        _steamTransport = GetComponent<SteamTransport>();

        _lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();

        if (_networkManager == null)
            Debug.LogError("[ConnectionStarter] NetworkManager NOT FOUND on this GameObject!");

        if (_steamTransport == null)
            Debug.LogError("[ConnectionStarter] SteamTransport NOT FOUND on this GameObject!");

        if (_lobbyDataHolder == null)
            Debug.LogError("[ConnectionStarter] LobbyDataHolder NOT FOUND!");

        _isFromLobby = _lobbyDataHolder != null;
    }

    private void Start()
    {
        if (!_networkManager)
        {
            PurrLogger.LogError($"Failed to start connection {nameof(NetworkManager)} is null", this);
            return;
        }

        if (!_steamTransport)
        {
            PurrLogger.LogError($"Failed to start connection {nameof(SteamTransport)} is null", this);
            return;
        }

        if (_isFromLobby)
        {
            StartFromLobby();
        }
        else
        {
            StartNormal();
        }


    }

    private void StartNormal()
    {

    }

    private void StartFromLobby()
    {


        if (!_lobbyDataHolder)
        {
            PurrLogger.LogError(
                $"Failed to start connection {nameof(LobbyDataHolder)} is null",
                this
            );
            return;
        }

        if (!_lobbyDataHolder.CurrentLobby.IsValid)
        {
            PurrLogger.LogError(
                "Failed to start connection. Lobby is invalid",
                this
            );
            return;
        }

        if (!ulong.TryParse(
            _lobbyDataHolder.CurrentLobby.LobbyId,
            out ulong ulongId))
        {
            Debug.LogError("Failed to parse lobby id into ulong");
            return;
        }

        var lobbyOwner =
            SteamMatchmaking.GetLobbyOwner(new CSteamID(ulongId));

        if (!lobbyOwner.IsValid())
        {
            Debug.LogError(
                "Failed to get lobby owner from parsed lobby id"
            );
            return;
        }

        _steamTransport.address = lobbyOwner.ToString();

        if (_lobbyDataHolder.CurrentLobby.IsOwner)
        {
            // Создатель Lobby
            _networkManager.StartServer();
            StartCoroutine(StartClient());
        }
        else
        {
            // Обычный игрок
            StartCoroutine(StartClient());
        }
    }

    private IEnumerator StartClient()
    {
        yield return new WaitForSeconds(1f);
        _networkManager.StartClient();
    }
}
