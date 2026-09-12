using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class PlayerRagdoll : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Behaviour playerController;

    [Header("Ragdoll")]
    [SerializeField] private Rigidbody[] ragdollBodies;
    [SerializeField] private Collider[] ragdollColliders;

    private bool isRagdoll;

    // Локальное событие — срабатывает на КАЖДОЙ машине
    // (сервер, владелец, наблюдатели) в момент, когда именно
    // на этой машине применился SetRagdoll. Подписчики:
    // PlayerController (чтобы остановить движение/анимацию).
    public event Action<bool> OnRagdollStateChanged;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        ragdollBodies = GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = GetComponentsInChildren<Collider>(true);

        Debug.Log(
            $"RAGDOLL INIT | Bodies: {ragdollBodies.Length} | Colliders: {ragdollColliders.Length}",
            this
        );

        SetRagdoll(false);
    }

    // =========================================================
    // ВАЖНО: сеть уже реализована СНАРУЖИ, в PlayerHealth
    // (Die() → [ObserversRpc] ActivateRagdollObservers()).
    // Поэтому здесь НЕТ собственных RPC — иначе будет
    // дублирование вызовов на клиентах.
    // =========================================================

    public void SetRagdoll(bool enabled)
    {
        isRagdoll = enabled;

        Debug.Log(
            $"RAGDOLL SET: {enabled} | Bodies: {ragdollBodies.Length}",
            this
        );

        OnRagdollStateChanged?.Invoke(enabled);

        if (animator != null)
            animator.enabled = !enabled;

        if (characterController != null)
            characterController.enabled = !enabled;

        if (playerController != null)
            playerController.enabled = !enabled;

        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb == null) continue;
            if (rb.gameObject == gameObject) continue;

            rb.isKinematic = !enabled;
            rb.useGravity = enabled;

            Debug.Log(
                $"RAGDOLL BODY | {rb.name} | Kinematic: {rb.isKinematic} | Gravity: {rb.useGravity}",
                rb
            );
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col == null) continue;
            if (col.gameObject == gameObject) continue;
            if (col is CharacterController) continue;

            col.enabled = enabled;
        }
    }

    public void ActivateRagdoll(Vector3 force, Vector3 hitPoint)
    {
        if (isRagdoll)
            return;

        Debug.Log("ACTIVATING RAGDOLL", this);

        SetRagdoll(true);

        Rigidbody closestBody = null;
        float closestDistance = float.MaxValue;

        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb == null) continue;
            if (rb.gameObject == gameObject) continue;

            float distance = Vector3.Distance(
                rb.worldCenterOfMass,
                hitPoint
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBody = rb;
            }
        }

        if (closestBody != null)
        {
            Debug.Log($"RAGDOLL FORCE → {closestBody.name}", closestBody);

            closestBody.AddForceAtPosition(
                force,
                hitPoint,
                ForceMode.Impulse
            );
        }
        else
        {
            Debug.LogError("RAGDOLL ERROR: No Rigidbody found!");
        }
    }

    // Обнуляет скорости тел рагдолла. Вызывать перед возрождением,
    // чтобы тело не "улетело" при повторной активации в будущем.
    public void ResetRagdollPhysics()
    {
        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb == null) continue;
            if (rb.gameObject == gameObject) continue;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("RAGDOLL PHYSICS RESET", this);
    }

    public bool IsRagdoll => isRagdoll;
}