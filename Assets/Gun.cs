using UnityEngine;
using PurrNet;
using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;

public class Gun : StateNode
{
    [Header("Stats")]
    [SerializeField] private float range = 20f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private bool automatic;

    [Header("Recoil")]
    [SerializeField] private float recoilStrength = 1f;
    [SerializeField] private float recoilDuration = 0.1f;
    [SerializeField] private AnimationCurve recoilCurve;
    [SerializeField] private float rotationAmount = 25f;
    [SerializeField] private AnimationCurve rotationCurve;

    [Header("Weapon Bob")]
    [SerializeField] private bool enableBob = true;
    [SerializeField] private float bobFrequency = 8f;
    [SerializeField] private float bobHorizontalAmount = 0.025f;
    [SerializeField] private float bobVerticalAmount = 0.035f;
    [SerializeField] private float bobSmooth = 10f;
    [SerializeField] private float bobSpeedThreshold = 0.1f;

    [Header("Weapon Sway")]
    [SerializeField] private bool enableSway = true;
    [SerializeField] private float swayAmount = 2f;
    [SerializeField] private float swayRotationAmount = 2f;
    [SerializeField] private float swaySmooth = 10f;
    [SerializeField] private float maxSway = 4f;

    [Header("References")]
    [SerializeField] private Transform camTransform;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightIKTarget;
    [SerializeField] private Transform leftIKTarget;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem environmentHitEffect;
    [SerializeField] private ParticleSystem playerHitEffect;
    [SerializeField] private List<Renderer> renderers = new();

    // PauseController находится в сцене,
    // а Gun находится внутри Player Prefab.
    private PurrLobby.PauseController pauseController;

    private float _lastFireTime;

    private Vector3 _originalPos;
    private Quaternion _originalRotation;

    private Coroutine _recoilCoroutine;

    // =========================================================
    // WEAPON BOB / SWAY STATE
    // =========================================================

    private float _bobTimer;

    private Vector3 _bobOffset;
    private Quaternion _bobRotation;

    private Vector3 _swayOffset;
    private Quaternion _swayRotation;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        ToggleVisuals(false);
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        _originalPos = transform.localPosition;
        _originalRotation = transform.localRotation;

        // Ищем PauseController в сцене.
        //
        // НЕ нужно добавлять его в Player Prefab.
        pauseController =
            FindFirstObjectByType<PurrLobby.PauseController>();

        if (pauseController == null)
        {
            Debug.LogWarning(
                "Gun: PauseController not found in scene.",
                this
            );
        }
    }

    // =========================================================
    // STATE ENTER
    // =========================================================

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        ToggleVisuals(true);

        ResetWeaponMotion();
    }

    // =========================================================
    // STATE EXIT
    // =========================================================

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);

        ToggleVisuals(false);

        ResetWeaponMotion();
    }

    // =========================================================
    // VISUALS
    // =========================================================

    private void ToggleVisuals(bool toggle)
    {
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = toggle;
            }
        }
    }

    // =========================================================
    // STATE UPDATE
    // =========================================================

    public override void StateUpdate(bool asServer)
    {
        base.StateUpdate(asServer);

        // IK может продолжать работать.
        SetIKTargets();

        // Оружие работает только у владельца.
        if (!isOwner)
            return;

        // =====================================================
        // PAUSE / SETTINGS BLOCK
        // =====================================================

        if (pauseController != null &&
            pauseController.IsInputBlocked())
        {
            ResetWeaponMotion();
            return;
        }

        // =====================================================
        // WEAPON BOB / SWAY
        // =====================================================

        UpdateWeaponMotion();

        // =====================================================
        // SHOOT INPUT
        // =====================================================

        bool shootInput;

        if (automatic)
        {
            // Автоматическое оружие:
            // держим ЛКМ.
            shootInput =
                Input.GetKey(KeyCode.Mouse0);
        }
        else
        {
            // Полуавтоматическое оружие:
            // одно нажатие ЛКМ.
            shootInput =
                Input.GetKeyDown(KeyCode.Mouse0);
        }

        if (!shootInput)
            return;

        // =====================================================
        // FIRE RATE
        // =====================================================

        if (_lastFireTime + fireRate >
            Time.unscaledTime)
        {
            return;
        }

        // =====================================================
        // SHOT
        // =====================================================

        PlayShotEffect();

        _lastFireTime =
            Time.unscaledTime;

        // =====================================================
        // RAYCAST
        // =====================================================

        if (!Physics.Raycast(
                camTransform.position,
                camTransform.forward,
                out var hit,
                range,
                hitLayer))
        {
            return;
        }

        // =====================================================
        // PLAYER HIT
        // =====================================================

        if (!hit.transform.TryGetComponent(
                out PlayerHealth playerHealth))
        {
            // Попали в окружение.
            EnvironmentHit(
                hit.point,
                hit.normal
            );

            return;
        }

        // =====================================================
        // PLAYER DAMAGE
        // =====================================================

        PlayerHit(
            playerHealth,
            playerHealth.transform.InverseTransformPoint(
                hit.point
            ),
            hit.normal
        );

        playerHealth.ChangeHealth(-damage);
    }

    // =========================================================
    // WEAPON MOTION
    // =========================================================

    private void UpdateWeaponMotion()
    {
        if (!enableBob && !enableSway)
            return;

        UpdateBob();
        UpdateSway();

        // =====================================================
        // FINAL POSITION
        // =====================================================

        Vector3 targetPosition =
            _originalPos +
            _bobOffset +
            _swayOffset;

        transform.localPosition =
            Vector3.Lerp(
                transform.localPosition,
                targetPosition,
                bobSmooth * Time.deltaTime
            );

        // =====================================================
        // FINAL ROTATION
        // =====================================================

        Quaternion targetRotation =
            _originalRotation *
            _bobRotation *
            _swayRotation;

        transform.localRotation =
            Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                swaySmooth * Time.deltaTime
            );
    }

    // =========================================================
    // WEAPON BOB
    // =========================================================

    private void UpdateBob()
    {
        if (!enableBob)
        {
            _bobOffset =
                Vector3.Lerp(
                    _bobOffset,
                    Vector3.zero,
                    bobSmooth * Time.deltaTime
                );

            _bobRotation =
                Quaternion.Slerp(
                    _bobRotation,
                    Quaternion.identity,
                    bobSmooth * Time.deltaTime
                );

            return;
        }

        // =====================================================
        // PLAYER MOVEMENT
        // =====================================================

        float horizontal =
            Input.GetAxisRaw("Horizontal");

        float vertical =
            Input.GetAxisRaw("Vertical");

        Vector2 movementInput =
            new Vector2(
                horizontal,
                vertical
            );

        float movementAmount =
            Mathf.Clamp01(
                movementInput.magnitude
            );

        // =====================================================
        // NO MOVEMENT
        // =====================================================

        if (movementAmount < bobSpeedThreshold)
        {
            _bobTimer = 0f;

            _bobOffset =
                Vector3.Lerp(
                    _bobOffset,
                    Vector3.zero,
                    bobSmooth * Time.deltaTime
                );

            _bobRotation =
                Quaternion.Slerp(
                    _bobRotation,
                    Quaternion.identity,
                    bobSmooth * Time.deltaTime
                );

            return;
        }

        // =====================================================
        // BOB TIMER
        // =====================================================

        float speedMultiplier =
            Input.GetKey(KeyCode.LeftShift)
                ? 1.25f
                : 1f;

        _bobTimer +=
            Time.deltaTime *
            bobFrequency *
            speedMultiplier;

        // =====================================================
        // BOB POSITION
        // =====================================================

        float verticalBob =
            Mathf.Sin(_bobTimer) *
            bobVerticalAmount *
            movementAmount;

        float horizontalBob =
            Mathf.Cos(_bobTimer * 0.5f) *
            bobHorizontalAmount *
            movementAmount;

        _bobOffset =
            new Vector3(
                horizontalBob,
                Mathf.Abs(verticalBob),
                0f
            );

        // =====================================================
        // BOB ROTATION
        // =====================================================

        float rotationZ =
            Mathf.Sin(_bobTimer * 0.5f) *
            1.5f *
            movementAmount;

        float rotationX =
            Mathf.Sin(_bobTimer) *
            1f *
            movementAmount;

        _bobRotation =
            Quaternion.Euler(
                rotationX,
                0f,
                rotationZ
            );
    }

    // =========================================================
    // WEAPON SWAY
    // =========================================================

    private void UpdateSway()
    {
        if (!enableSway)
        {
            _swayOffset =
                Vector3.Lerp(
                    _swayOffset,
                    Vector3.zero,
                    swaySmooth * Time.deltaTime
                );

            _swayRotation =
                Quaternion.Slerp(
                    _swayRotation,
                    Quaternion.identity,
                    swaySmooth * Time.deltaTime
                );

            return;
        }

        // =====================================================
        // MOUSE INPUT
        // =====================================================

        float mouseX =
            Input.GetAxisRaw("Mouse X");

        float mouseY =
            Input.GetAxisRaw("Mouse Y");

        mouseX =
            Mathf.Clamp(
                mouseX,
                -maxSway,
                maxSway
            );

        mouseY =
            Mathf.Clamp(
                mouseY,
                -maxSway,
                maxSway
            );

        // =====================================================
        // POSITION SWAY
        // =====================================================

        Vector3 targetOffset =
            new Vector3(
                -mouseX * swayAmount * 0.01f,
                -mouseY * swayAmount * 0.01f,
                0f
            );

        _swayOffset =
            Vector3.Lerp(
                _swayOffset,
                targetOffset,
                swaySmooth * Time.deltaTime
            );

        // =====================================================
        // ROTATION SWAY
        // =====================================================

        Quaternion targetRotation =
            Quaternion.Euler(
                mouseY * swayRotationAmount,
                -mouseX * swayRotationAmount,
                -mouseX * swayRotationAmount * 0.5f
            );

        _swayRotation =
            Quaternion.Slerp(
                _swayRotation,
                targetRotation,
                swaySmooth * Time.deltaTime
            );
    }

    // =========================================================
    // RESET WEAPON MOTION
    // =========================================================

    private void ResetWeaponMotion()
    {
        _bobTimer = 0f;

        _bobOffset = Vector3.zero;
        _swayOffset = Vector3.zero;

        _bobRotation = Quaternion.identity;
        _swayRotation = Quaternion.identity;

        transform.localPosition =
            _originalPos;

        transform.localRotation =
            _originalRotation;
    }

    // =========================================================
    // PLAYER HIT EFFECT
    // =========================================================

    [ObserversRpc(runLocally: true)]
    private void PlayerHit(
        PlayerHealth player,
        Vector3 localPos,
        Vector3 normal)
    {
        if (playerHitEffect &&
            player &&
            player.transform)
        {
            Instantiate(
                playerHitEffect,
                player.transform.TransformPoint(localPos),
                Quaternion.LookRotation(normal)
            );
        }
    }

    // =========================================================
    // ENVIRONMENT HIT EFFECT
    // =========================================================

    [ObserversRpc(runLocally: true)]
    private void EnvironmentHit(
        Vector3 position,
        Vector3 normal)
    {
        if (environmentHitEffect)
        {
            Instantiate(
                environmentHitEffect,
                position,
                Quaternion.LookRotation(normal)
            );
        }
    }

    // =========================================================
    // IK
    // =========================================================

    private void SetIKTargets()
    {
        if (rightIKTarget != null &&
            rightHandTarget != null)
        {
            rightIKTarget.SetPositionAndRotation(
                rightHandTarget.position,
                rightHandTarget.rotation
            );
        }

        if (leftIKTarget != null &&
            leftHandTarget != null)
        {
            leftIKTarget.SetPositionAndRotation(
                leftHandTarget.position,
                leftHandTarget.rotation
            );
        }
    }

    // =========================================================
    // SHOT EFFECT
    // =========================================================

    [ObserversRpc(runLocally: true)]
    private void PlayShotEffect()
    {
        if (muzzleFlash)
        {
            muzzleFlash.Play();
        }

        if (_recoilCoroutine != null)
        {
            StopCoroutine(_recoilCoroutine);
        }

        _recoilCoroutine =
            StartCoroutine(PlayRecoil());
    }

    // =========================================================
    // RECOIL
    // =========================================================

    private IEnumerator PlayRecoil()
    {
        float elapsed = 0f;

        while (elapsed < recoilDuration)
        {
            elapsed += Time.deltaTime;

            float curveTime =
                elapsed / recoilDuration;

            // Position recoil
            float recoilValue =
                recoilCurve != null
                    ? recoilCurve.Evaluate(curveTime)
                    : 0f;

            Vector3 recoilOffset =
                Vector3.back *
                (recoilValue * recoilStrength);

            // Rotation recoil
            float rotationValue =
                rotationCurve != null
                    ? rotationCurve.Evaluate(curveTime)
                    : 0f;

            Vector3 rotationOffset =
                new Vector3(
                    rotationValue *
                    rotationAmount,
                    0f,
                    0f
                );

            // Recoil поверх текущего bob/sway.
            transform.localPosition =
                _originalPos +
                _bobOffset +
                _swayOffset +
                recoilOffset;

            transform.localRotation =
                _originalRotation *
                _bobRotation *
                _swayRotation *
                Quaternion.Euler(
                    rotationOffset
                );

            yield return null;
        }

        // После recoil не сбрасываем bob/sway.
        // Возвращаемся к текущему состоянию оружия.

        transform.localPosition =
            _originalPos +
            _bobOffset +
            _swayOffset;

        transform.localRotation =
            _originalRotation *
            _bobRotation *
            _swayRotation;
    }
}