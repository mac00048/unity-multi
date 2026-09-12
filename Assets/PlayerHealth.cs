
using PurrLobby;
using PurrNet;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField] private SyncVar<int> health = new(100);
    [SerializeField] private int maxHealth = 100;

    [Header("Layers")]
    [SerializeField] private int selfLayer;
    [SerializeField] private int otherLayer;

    [Header("Ragdoll")]
    [SerializeField] private PlayerRagdoll ragdoll;
    [SerializeField] private float deathForce = 5f;
    [SerializeField] private PlayerController playerController;

    [Header("Weapon")]
    [SerializeField] private GameObject weaponRoot;

    [Header("Respawn")]
    [SerializeField] private RespawnConfig respawnConfig;

    [Header("Respawn Console")]
    [SerializeField] private bool showRespawnConsole = true;
    [SerializeField] private float consoleStartDelay = 0.2f;
    [SerializeField] private float consoleHideDelay = 0.7f;

    public Action<PlayerID> onDeath_server;

    private bool isDead;
    private Coroutine respawnRoutine;

    private RespawnConsoleUI respawnConsoleUI;

    // =========================================================
    // SAFE ZONE
    // =========================================================

    private bool wasInSafeZone;


    // =========================================================
    // SPAWN
    // =========================================================

    protected override void OnSpawned()
    {
        base.OnSpawned();

        if (ragdoll == null)
            ragdoll = GetComponent<PlayerRagdoll>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        respawnConsoleUI =
            FindFirstObjectByType<RespawnConsoleUI>();

        var actualLayer = isOwner
            ? selfLayer
            : otherLayer;

        SetLayerRecursive(
            gameObject,
            actualLayer
        );

        if (isOwner)
        {
            InstanceHandler
                .GetInstance<MainGameView>()
                .UpdateHealth(health.value);

            health.onChanged += OnHealthChanged;

            if (respawnConsoleUI != null)
                respawnConsoleUI.Hide();

            // SAFE ZONE UI
            wasInSafeZone = IsInSafeZone();

            UpdateSafeZoneUI(true);
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    protected override void OnDestroy()
    {
        base.OnDestroy();

        health.onChanged -= OnHealthChanged;
    }


    // =========================================================
    // HEALTH CHANGED
    // =========================================================

    private void OnHealthChanged(int newHealth)
    {
        if (!isOwner)
            return;

        InstanceHandler
            .GetInstance<MainGameView>()
            .UpdateHealth(newHealth);
    }


    // =========================================================
    // LAYER
    // =========================================================

    private void SetLayerRecursive(
        GameObject obj,
        int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(
                child.gameObject,
                layer
            );
        }
    }


    // =========================================================
    // HEALTH
    // =========================================================

    public int Health => health.value;


    // =========================================================
    // SAFE ZONE
    // =========================================================

    public bool IsInSafeZone()
    {
        return SafeZone.IsPositionInSafeZone(
            transform.position
        );
    }


    private void UpdateSafeZoneUI(
     bool forceUpdate = false)
    {
        if (!isOwner)
            return;

        bool inSafeZone =
            IsInSafeZone();

        if (!forceUpdate &&
            inSafeZone == wasInSafeZone)
        {
            return;
        }

        wasInSafeZone = inSafeZone;

        SafeZone zone =
            SafeZone.GetFirstZone();

        if (zone == null)
            return;

        if (inSafeZone)
        {
            zone.ShowSafeZoneUI();
        }
        else
        {
            zone.ShowCombatZoneUI();
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!isOwner)
            return;

        // SAFE ZONE UI
        UpdateSafeZoneUI();

        // TEST DEATH
        if (Input.GetKeyDown(KeyCode.K))
        {
            ChangeHealth(-100);
        }
    }


    // =========================================================
    // TEST DEATH
    // =========================================================

    public void TestDeath()
    {
        if (isDead)
            return;

        Die(default);
    }


    // =========================================================
    // NETWORK DAMAGE
    // =========================================================

    [ServerRpc(requireOwnership: false)]
    public void ChangeHealth(
        int amount,
        RPCInfo info = default)
    {
        if (isDead)
            return;


        // =====================================================
        // SAFE ZONE
        // =====================================================

        // Если это урон и ЦЕЛЬ находится в Safe Zone,
        // урон полностью блокируется.

        if (amount < 0)
        {
            if (IsInSafeZone())
            {
                Debug.Log(
                    $"SAFE ZONE: damage blocked on {name}",
                    this
                );

                return;
            }
        }


        // =====================================================
        // APPLY HEALTH
        // =====================================================

        health.value += amount;


        // =====================================================
        // DEATH
        // =====================================================

        if (health.value <= 0)
        {
            Die(info);
        }
    }


    // =========================================================
    // DEATH
    // =========================================================

    private void Die(RPCInfo info)
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log(
            $"PLAYER DIED | Owner: {owner}",
            this
        );


        // =====================================================
        // SCORE
        // =====================================================

        if (InstanceHandler.TryGetInstance(
                out ScoreManager scoreManager))
        {
            scoreManager.AddKIll(info.sender);

            if (owner.HasValue)
            {
                scoreManager.AddDeath(
                    owner.Value
                );
            }
        }


        if (owner.HasValue)
        {
            onDeath_server?.Invoke(
                owner.Value
            );
        }


        // =====================================================
        // DISABLE PLAYER CONTROL
        // =====================================================

        if (playerController != null)
        {
            playerController.DisableWeapons();
        }


        // =====================================================
        // DISABLE WEAPON
        // =====================================================

        if (weaponRoot != null)
        {
            weaponRoot.SetActive(false);
        }

        DisableWeaponObservers();


        // =====================================================
        // RAGDOLL
        // =====================================================

        ActivateRagdollObservers();


        // =====================================================
        // RESPAWN CONSOLE
        // =====================================================

        if (showRespawnConsole)
        {
            ShowRespawnConsoleObservers();
        }


        // =====================================================
        // START RESPAWN
        // =====================================================

        if (respawnRoutine != null)
        {
            StopCoroutine(respawnRoutine);
        }

        respawnRoutine =
            StartCoroutine(
                RespawnAfterDelay()
            );
    }


    // =========================================================
    // WEAPON DISABLE
    // =========================================================

    [ObserversRpc]
    private void DisableWeaponObservers()
    {
        if (weaponRoot != null)
        {
            weaponRoot.SetActive(false);
        }
    }


    // =========================================================
    // RAGDOLL
    // =========================================================

    [ObserversRpc]
    private void ActivateRagdollObservers()
    {
        if (ragdoll == null)
            return;

        Vector3 forceDirection =
            -transform.forward;

        Vector3 hitPoint =
            transform.position +
            Vector3.up;

        ragdoll.ActivateRagdoll(
            forceDirection * deathForce,
            hitPoint
        );
    }


    // =========================================================
    // RESPAWN
    // =========================================================

    private IEnumerator RespawnAfterDelay()
    {
        float delay =
            respawnConfig != null
                ? respawnConfig.respawnDelay
                : 5f;

        Debug.Log(
            $"PLAYER RESPAWN: scheduled in {delay}s | Owner: {owner}",
            this
        );


        // =====================================================
        // CONSOLE
        // =====================================================

        if (showRespawnConsole)
        {
            yield return new WaitForSeconds(
                consoleStartDelay
            );

            SendRespawnConsoleLine(
                "> PLAYER ELIMINATED"
            );

            yield return new WaitForSeconds(
                consoleStartDelay
            );

            SendRespawnConsoleLine(
                "> RESPAWN SEQUENCE INITIATED"
            );

            yield return new WaitForSeconds(
                consoleStartDelay
            );
        }


        // =====================================================
        // COUNTDOWN
        // =====================================================

        int secondsLeft =
            Mathf.CeilToInt(delay);

        while (secondsLeft > 0)
        {
            if (showRespawnConsole)
            {
                SendRespawnConsoleLine(
                    $"> RESPAWN IN: {secondsLeft}"
                );
            }

            yield return new WaitForSeconds(1f);

            secondsLeft--;
        }


        // =====================================================
        // FIND SPAWN POINT
        // =====================================================

        SpawnPointManager spawnManager =
            FindFirstObjectByType<SpawnPointManager>();

        Transform spawnPoint =
            spawnManager != null
                ? spawnManager.GetRandomSpawnPoint()
                : null;


        Vector3 position =
            spawnPoint != null
                ? spawnPoint.position
                : transform.position;


        Quaternion rotation =
            spawnPoint != null
                ? spawnPoint.rotation
                : transform.rotation;


        // =====================================================
        // SPAWN ERROR
        // =====================================================

        if (spawnManager == null)
        {
            Debug.LogWarning(
                "PlayerHealth: SpawnPointManager not found.",
                this
            );

            if (showRespawnConsole)
            {
                SendRespawnConsoleLine(
                    "> ERROR: SPAWN MANAGER NOT FOUND."
                );

                yield return new WaitForSeconds(1f);
            }
        }
        else if (spawnPoint == null)
        {
            Debug.LogWarning(
                "PlayerHealth: Spawn point not found.",
                this
            );

            if (showRespawnConsole)
            {
                SendRespawnConsoleLine(
                    "> ERROR: SPAWN POINT NOT FOUND."
                );

                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            if (showRespawnConsole)
            {
                SendRespawnConsoleLine(
                    "> LOCATING SPAWN POINT..."
                );

                yield return new WaitForSeconds(0.2f);

                SendRespawnConsoleLine(
                    "> SPAWN POINT FOUND."
                );

                yield return new WaitForSeconds(0.2f);
            }
        }


        // =====================================================
        // SERVER RESPAWN
        // =====================================================

        Debug.Log(
            $"PLAYER RESPAWN: executing now | Owner: {owner} | Position: {position}",
            this
        );


        // =====================================================
        // RESET STATE
        // =====================================================

        isDead = false;

        health.value =
            maxHealth;


        // =====================================================
        // RESPAWN
        // =====================================================

        RespawnObservers(
            position,
            rotation
        );


        // =====================================================
        // COMPLETE
        // =====================================================

        if (showRespawnConsole)
        {
            SendRespawnConsoleLine(
                "> RESPAWN COMPLETE."
            );

            yield return new WaitForSeconds(
                consoleHideDelay
            );

            HideRespawnConsoleObservers();
        }


        respawnRoutine = null;
    }


    // =========================================================
    // RESPAWN OBSERVERS
    // =========================================================

    [ObserversRpc]
    private void RespawnObservers(
        Vector3 position,
        Quaternion rotation)
    {
        Debug.Log(
            "PLAYER RESPAWN: applying on client",
            this
        );


        // =====================================================
        // RAGDOLL RESET
        // =====================================================

        if (ragdoll != null)
        {
            ragdoll.ResetRagdollPhysics();
        }


        // =====================================================
        // CHARACTER CONTROLLER
        // =====================================================

        CharacterController characterController =
            GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }


        // =====================================================
        // TELEPORT
        // =====================================================

        transform.position =
            position;

        transform.rotation =
            rotation;


        // =====================================================
        // RESET VELOCITY
        // =====================================================

        if (playerController != null)
        {
            playerController.ResetVelocity();
        }


        // =====================================================
        // CHARACTER CONTROLLER ENABLE
        // =====================================================

        if (characterController != null)
        {
            characterController.enabled = true;
        }


        // =====================================================
        // WEAPON
        // =====================================================

        if (weaponRoot != null)
        {
            weaponRoot.SetActive(true);
        }


        // =====================================================
        // RAGDOLL OFF
        // =====================================================

        if (ragdoll != null)
        {
            ragdoll.SetRagdoll(false);
        }
    }


    // =========================================================
    // SHOW RESPAWN CONSOLE
    // =========================================================

    [ObserversRpc]
    private void ShowRespawnConsoleObservers()
    {
        if (!isOwner)
            return;

        if (!showRespawnConsole)
            return;

        if (respawnConsoleUI == null)
        {
            respawnConsoleUI =
                FindFirstObjectByType<RespawnConsoleUI>();
        }

        if (respawnConsoleUI == null)
        {
            Debug.LogWarning(
                "PlayerHealth: RespawnConsoleUI not found.",
                this
            );

            return;
        }

        respawnConsoleUI.Show();

        respawnConsoleUI.Clear();
    }


    // =========================================================
    // SEND CONSOLE LINE
    // =========================================================

    [ObserversRpc]
    private void SendRespawnConsoleLine(
        string text)
    {
        if (!isOwner)
            return;

        if (!showRespawnConsole)
            return;

        if (respawnConsoleUI == null)
        {
            respawnConsoleUI =
                FindFirstObjectByType<RespawnConsoleUI>();
        }

        if (respawnConsoleUI == null)
            return;

        respawnConsoleUI.AddLine(text);
    }


    // =========================================================
    // HIDE RESPAWN CONSOLE
    // =========================================================

    [ObserversRpc]
    private void HideRespawnConsoleObservers()
    {
        if (!isOwner)
            return;

        HideRespawnConsole();
    }


    private void HideRespawnConsole()
    {
        if (respawnConsoleUI == null)
        {
            respawnConsoleUI =
                FindFirstObjectByType<RespawnConsoleUI>();
        }

        if (respawnConsoleUI == null)
            return;

        respawnConsoleUI.Hide();
    }
}


