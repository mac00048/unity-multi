using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SafeZone : MonoBehaviour
{
    public static readonly List<SafeZone> AllZones = new();

    [Header("UI")]
    [SerializeField] private CanvasGroup uiCanvasGroup;
    [SerializeField] private TMP_Text zoneText;

    [Header("Text")]
    [SerializeField] private string safeZoneText = "> SAFE ZONE";
    [SerializeField] private string combatZoneText = "> COMBAT ZONE";

    [SerializeField] private bool showCombatZoneText = true;

    private Collider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();

        if (zoneCollider == null)
        {
            Debug.LogError(
                "SafeZone requires a Collider.",
                this
            );
        }

        HideUI();
    }

    private void OnEnable()
    {
        if (!AllZones.Contains(this))
            AllZones.Add(this);
    }

    private void OnDisable()
    {
        AllZones.Remove(this);
    }

    public static bool IsPositionInSafeZone(Vector3 position)
    {
        foreach (SafeZone zone in AllZones)
        {
            if (zone == null)
                continue;

            if (zone.zoneCollider == null)
                continue;

            if (zone.IsInside(position))
                return true;
        }

        return false;
    }

    public static SafeZone GetZoneAtPosition(Vector3 position)
    {
        foreach (SafeZone zone in AllZones)
        {
            if (zone == null)
                continue;

            if (zone.zoneCollider == null)
                continue;

            if (zone.IsInside(position))
                return zone;
        }

        return null;
    }

    public static SafeZone GetFirstZone()
    {
        foreach (SafeZone zone in AllZones)
        {
            if (zone != null)
                return zone;
        }

        return null;
    }

    private bool IsInside(Vector3 position)
    {
        Vector3 closestPoint =
            zoneCollider.ClosestPoint(position);

        float distance =
            Vector3.Distance(
                position,
                closestPoint
            );

        return distance <= 0.01f;
    }

    public void ShowSafeZoneUI()
    {
        if (zoneText != null)
            zoneText.text = safeZoneText;

        ShowUI();

        Debug.Log("SAFE ZONE UI");
    }

    public void ShowCombatZoneUI()
    {
        if (!showCombatZoneText)
        {
            HideUI();
            return;
        }

        if (zoneText != null)
            zoneText.text = combatZoneText;

        ShowUI();

        Debug.Log("COMBAT ZONE UI");
    }

    public void HideZoneUI()
    {
        HideUI();
    }

    private void ShowUI()
    {
        if (uiCanvasGroup == null)
            return;

        uiCanvasGroup.alpha = 1f;
        uiCanvasGroup.interactable = false;
        uiCanvasGroup.blocksRaycasts = false;
    }

    private void HideUI()
    {
        if (uiCanvasGroup == null)
            return;

        uiCanvasGroup.alpha = 0f;
        uiCanvasGroup.interactable = false;
        uiCanvasGroup.blocksRaycasts = false;
    }

    private void OnDrawGizmos()
    {
        Collider collider =
            GetComponent<Collider>();

        if (collider == null)
            return;

        Gizmos.matrix =
            transform.localToWorldMatrix;

        if (collider is BoxCollider box)
        {
            Gizmos.DrawWireCube(
                box.center,
                box.size
            );
        }
        else if (collider is SphereCollider sphere)
        {
            Gizmos.DrawWireSphere(
                sphere.center,
                sphere.radius
            );
        }
    }
}