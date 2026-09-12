using PurrLobby;
using UnityEngine;

public class MatchStartTerminal : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Optional UI")]
    [SerializeField] private GameObject interactionHint;

    private PlayerController localPlayer;
    private PlayerReady localPlayerReady;

    private WaitingRoomManager waitingRoomManager;
    private LobbyDataHolder lobbyDataHolder;

    private bool playerInside;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        waitingRoomManager =
            FindFirstObjectByType<WaitingRoomManager>();

        lobbyDataHolder =
            FindFirstObjectByType<LobbyDataHolder>();

        if (waitingRoomManager == null)
        {
            Debug.LogError(
                "MatchStartTerminal: WaitingRoomManager not found!",
                this
            );
        }

        if (lobbyDataHolder == null)
        {
            Debug.LogError(
                "MatchStartTerminal: LobbyDataHolder not found!",
                this
            );
        }

        if (interactionHint != null)
        {
            interactionHint.SetActive(false);
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!playerInside)
            return;

        if (localPlayer == null)
            return;

        if (!localPlayer.isOwner)
            return;

        if (Input.GetKeyDown(interactionKey))
        {
            Interact();
        }
    }

    // =========================================================
    // INTERACT
    // =========================================================

    private void Interact()
    {
        if (localPlayerReady == null)
        {
            localPlayerReady =
                localPlayer.GetComponent<PlayerReady>();
        }

        if (localPlayerReady == null)
        {
            Debug.LogError(
                "MatchStartTerminal: PlayerReady not found on player!",
                localPlayer
            );

            return;
        }

        // =====================================================
        // FIRST E → READY
        // =====================================================

        if (!localPlayerReady.IsReady)
        {
            localPlayerReady.ToggleReady();

            Debug.Log(
                "WAITING ROOM | Player READY"
            );

            return;
        }

        // =====================================================
        // SECOND E
        // =====================================================

        // Если это не host,
        // повторное нажатие ничего не делает.
        if (!IsHost())
        {
            Debug.Log(
                "WAITING ROOM | Only HOST can start the game."
            );

            return;
        }

        // =====================================================
        // HOST → START
        // =====================================================

        if (waitingRoomManager == null)
        {
            Debug.LogError(
                "MatchStartTerminal: WaitingRoomManager missing!",
                this
            );

            return;
        }

        if (!waitingRoomManager.CanStartGame())
        {
            Debug.Log(
                $"WAITING ROOM | Cannot start. " +
                $"Ready: {waitingRoomManager.ReadyPlayerCount}/" +
                $"{waitingRoomManager.PlayerCount}"
            );

            return;
        }

        Debug.Log(
            "WAITING ROOM | HOST PRESSED E → START GAME"
        );

        waitingRoomManager.ForceStartGame();
    }

    // =========================================================
    // HOST CHECK
    // =========================================================

    private bool IsHost()
    {
        if (lobbyDataHolder == null)
        {
            lobbyDataHolder =
                FindFirstObjectByType<LobbyDataHolder>();
        }

        if (lobbyDataHolder == null)
        {
            Debug.LogError(
                "MatchStartTerminal: LobbyDataHolder not found!"
            );

            return false;
        }

        if (!lobbyDataHolder.CurrentLobby.IsValid)
        {
            Debug.LogWarning(
                "MatchStartTerminal: Current lobby is invalid."
            );

            return false;
        }

        return lobbyDataHolder.CurrentLobby.IsOwner;
    }

    // =========================================================
    // TRIGGER ENTER
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<PlayerController>(
                out PlayerController player))
        {
            return;
        }

        // Нас интересует только локальный игрок.
        if (!player.isOwner)
            return;

        localPlayer = player;

        localPlayerReady =
            player.GetComponent<PlayerReady>();

        playerInside = true;

        if (interactionHint != null)
        {
            interactionHint.SetActive(true);
        }

        Debug.Log(
            "WAITING ROOM | Player entered start terminal"
        );
    }

    // =========================================================
    // TRIGGER EXIT
    // =========================================================

    private void OnTriggerExit(Collider other)
    {
        if (localPlayer == null)
            return;

        if (other.gameObject !=
            localPlayer.gameObject)
        {
            return;
        }

        playerInside = false;

        localPlayer = null;
        localPlayerReady = null;

        if (interactionHint != null)
        {
            interactionHint.SetActive(false);
        }

        Debug.Log(
            "WAITING ROOM | Player left start terminal"
        );
    }
}