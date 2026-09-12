using UnityEngine;

public class DeathCameraController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera deathCamera;

    [Header("Position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.5f, -6f);

    [Header("Look")]
    [SerializeField] private float lookHeight = 1.0f;

    [Header("Movement")]
    [SerializeField] private float followSpeed = 5f;

    private Transform target;
    private bool active;

    private void Awake()
    {
        if (deathCamera == null)
            deathCamera = GetComponent<Camera>();

        deathCamera.enabled = false;
    }

    private void LateUpdate()
    {
        if (!active || target == null)
            return;

        Vector3 targetPosition =
            target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        Vector3 lookPosition =
            target.position +
            Vector3.up * lookHeight;

        transform.LookAt(lookPosition);
    }

    public void Activate(Transform targetTransform)
    {
        Debug.Log(
            $"DEATH CAMERA ACTIVATE | Target: {(targetTransform != null ? targetTransform.name : "NULL")}",
            this
        );

        if (targetTransform == null)
        {
            Debug.LogError("DEATH CAMERA ERROR | Target is NULL", this);
            return;
        }

        target = targetTransform;
        active = true;

        deathCamera.enabled = true;

        transform.position =
            target.position + offset;

        transform.LookAt(
            target.position +
            Vector3.up * lookHeight
        );

        Debug.Log(
            $"DEATH CAMERA ENABLED | Position: {transform.position}",
            this
        );
    }
}