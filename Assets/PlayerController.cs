using PurrLobby;
using PurrNet;
using PurrNet.StateMachine;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float sprintSpeed = 12f;

    [Header("Ground Movement")]
    [SerializeField] private float groundAcceleration = 70f;
    [SerializeField] private float groundDeceleration = 150f;

    [Header("Air Movement")]
    [SerializeField] private float airAcceleration = 12f;
    [SerializeField] private float airSpeed = 12f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 1.2f;
    [SerializeField] private float gravity = -24f;
    [SerializeField] private float groundedAnimLockDuration = 0.15f;

    [Header("Slide")]
    [SerializeField] private float slideSpeed = 14f;
    [SerializeField] private float slideDuration = 0.65f;
    [SerializeField] private float slideDeceleration = 18f;
    [SerializeField] private float slideCooldown = 0.8f;

    [Tooltip("Минимальная скорость движения для начала слайда.")]
    [SerializeField] private float slideMinSpeed = 7f;

    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("References")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private NetworkAnimator animator;
    [SerializeField] private List<Renderer> renderers = new();

    [Header("Weapon State")]
    [SerializeField] private StateMachine stateMachine;
    [SerializeField] private List<StateNode> weaponStates = new();

    private CharacterController characterController;
    private PlayerRagdoll ragdoll;

    private Vector3 velocity;
    private float verticalRotation;

    private bool weaponsDisabled;

    // =========================================================
    // DEATH
    // =========================================================

    private bool isDead;

    // =========================================================
    // ANIMATION
    // =========================================================

    private float groundedAnimLockTimer;

    // =========================================================
    // PAUSE
    // =========================================================

    private PauseController pauseController;

    // =========================================================
    // SLIDE
    // =========================================================

    private bool isSliding;
    private float slideTimer;
    private float slideCooldownTimer;
    private Vector3 slideDirection;

    // =========================================================
    // NETWORK SPAWN
    // =========================================================

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(isOwner);
        }

        if (isOwner)
        {
            foreach (var rend in renderers)
            {
                if (rend != null)
                {
                    rend.shadowCastingMode =
                        ShadowCastingMode.ShadowsOnly;
                }
            }
        }
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        characterController =
            GetComponent<CharacterController>();

        // ---------------------------------------------------------
        // RAGDOLL
        // ---------------------------------------------------------

        ragdoll = GetComponent<PlayerRagdoll>();

        if (ragdoll != null)
        {
            ragdoll.OnRagdollStateChanged +=
                HandleRagdollStateChanged;
        }
        else
        {
            Debug.LogWarning(
                "PlayerController: PlayerRagdoll not found on this GameObject.",
                this
            );
        }

        if (!isOwner)
            return;

        // ---------------------------------------------------------
        // PAUSE
        // ---------------------------------------------------------

        pauseController =
            FindFirstObjectByType<PauseController>();

        if (pauseController == null)
        {
            Debug.LogWarning(
                "PlayerController: PauseController not found in scene.",
                this
            );
        }

        // ---------------------------------------------------------
        // CAMERA
        // ---------------------------------------------------------

        if (playerCamera == null)
        {
            Debug.LogError(
                "PlayerController: Player Camera is not assigned.",
                this
            );

            enabled = false;
            return;
        }

        // ---------------------------------------------------------
        // CURSOR
        // ---------------------------------------------------------

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        if (!isOwner)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (ragdoll != null)
        {
            ragdoll.OnRagdollStateChanged -=
                HandleRagdollStateChanged;
        }
    }

    // =========================================================
    // RAGDOLL / DEATH
    // =========================================================

    private void HandleRagdollStateChanged(bool active)
    {
        isDead = active;

        Debug.Log(
            $"PLAYER CONTROLLER: RagdollStateChanged → isDead={isDead}",
            this
        );

        if (!isOwner)
            return;

        if (isDead)
        {
            // Если умер во время слайда,
            // полностью сбрасываем его.

            isSliding = false;
            slideTimer = 0f;
            slideCooldownTimer = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // После возрождения.

            groundedAnimLockTimer = 0f;

            isSliding = false;
            slideTimer = 0f;
            slideCooldownTimer = 0f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // =========================================================
    // RESET VELOCITY
    // =========================================================

    public void ResetVelocity()
    {
        velocity = Vector3.zero;

        isSliding = false;
        slideTimer = 0f;
        slideCooldownTimer = 0f;

        Debug.Log(
            "PLAYER CONTROLLER: Velocity reset on respawn",
            this
        );
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!isOwner)
            return;

        if (isDead)
            return;

        if (pauseController != null &&
            pauseController.IsInputBlocked())
        {
            return;
        }

        // ---------------------------------------------------------
        // SLIDE COOLDOWN
        // ---------------------------------------------------------

        if (slideCooldownTimer > 0f)
        {
            slideCooldownTimer -= Time.deltaTime;
        }

        // ---------------------------------------------------------
        // CAMERA
        // ---------------------------------------------------------

        HandleRotation();

        // ---------------------------------------------------------
        // MOVEMENT / SLIDE
        // ---------------------------------------------------------

        if (isSliding)
        {
            HandleSlide();
        }
        else
        {
            HandleMovement();

            // Left Ctrl = slide
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                TryStartSlide();
            }
        }

        // ---------------------------------------------------------
        // WEAPON SWITCHING
        // ---------------------------------------------------------

        if (!weaponsDisabled)
        {
            HandleWeaponSwitching();
        }
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void HandleMovement()
    {
        if (isDead || !characterController.enabled)
        {
            return;
        }

        bool isGrounded =
            characterController.isGrounded;

        float horizontal =
            Input.GetAxisRaw("Horizontal");

        float vertical =
            Input.GetAxisRaw("Vertical");

        Vector3 wishDirection =
            transform.right * horizontal +
            transform.forward * vertical;

        wishDirection =
            Vector3.ClampMagnitude(
                wishDirection,
                1f
            );

        float maxSpeed =
            Input.GetKey(KeyCode.LeftShift)
                ? sprintSpeed
                : moveSpeed;

        // ---------------------------------------------------------
        // GROUND
        // ---------------------------------------------------------

        if (isGrounded)
        {
            if (velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            HandleGroundMovement(
                wishDirection,
                maxSpeed
            );

            // -----------------------------------------------------
            // JUMP
            // -----------------------------------------------------

            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }
        }
        else
        {
            // -----------------------------------------------------
            // AIR
            // -----------------------------------------------------

            HandleAirMovement(wishDirection);
        }

        // ---------------------------------------------------------
        // GRAVITY
        // ---------------------------------------------------------

        velocity.y +=
            gravity * Time.deltaTime;

        if (!characterController.enabled)
        {
            return;
        }

        // ---------------------------------------------------------
        // MOVE
        // ---------------------------------------------------------

        characterController.Move(
            velocity * Time.deltaTime
        );

        // ---------------------------------------------------------
        // ANIMATION
        // ---------------------------------------------------------

        UpdateAnimator(
            horizontal,
            vertical,
            isGrounded
        );
    }

    // =========================================================
    // GROUND MOVEMENT
    // =========================================================

    private void HandleGroundMovement(
        Vector3 wishDirection,
        float wishSpeed)
    {
        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

        // ---------------------------------------------------------
        // STOP
        // ---------------------------------------------------------

        if (wishDirection.sqrMagnitude < 0.001f)
        {
            horizontalVelocity =
                Vector3.MoveTowards(
                    horizontalVelocity,
                    Vector3.zero,
                    groundDeceleration *
                    Time.deltaTime
                );
        }
        else
        {
            // -----------------------------------------------------
            // ACCELERATION
            // -----------------------------------------------------

            float currentSpeed =
                Vector3.Dot(
                    horizontalVelocity,
                    wishDirection
                );

            float speedToAdd =
                wishSpeed - currentSpeed;

            if (speedToAdd > 0f)
            {
                float accelerationSpeed =
                    groundAcceleration *
                    Time.deltaTime;

                accelerationSpeed =
                    Mathf.Min(
                        accelerationSpeed,
                        speedToAdd
                    );

                horizontalVelocity +=
                    wishDirection *
                    accelerationSpeed;
            }

            // -----------------------------------------------------
            // MAX SPEED
            // -----------------------------------------------------

            if (horizontalVelocity.magnitude >
                wishSpeed)
            {
                horizontalVelocity =
                    horizontalVelocity.normalized *
                    wishSpeed;
            }
        }

        velocity.x =
            horizontalVelocity.x;

        velocity.z =
            horizontalVelocity.z;
    }

    // =========================================================
    // AIR MOVEMENT
    // =========================================================

    private void HandleAirMovement(
        Vector3 wishDirection)
    {
        if (wishDirection.sqrMagnitude < 0.001f)
            return;

        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

        float currentSpeed =
            Vector3.Dot(
                horizontalVelocity,
                wishDirection
            );

        float speedToAdd =
            airSpeed - currentSpeed;

        if (speedToAdd <= 0f)
            return;

        float accelerationSpeed =
            airAcceleration *
            airSpeed *
            Time.deltaTime;

        accelerationSpeed =
            Mathf.Min(
                accelerationSpeed,
                speedToAdd
            );

        horizontalVelocity +=
            wishDirection *
            accelerationSpeed;

        velocity.x =
            horizontalVelocity.x;

        velocity.z =
            horizontalVelocity.z;
    }

    // =========================================================
    // JUMP
    // =========================================================

    private void Jump()
    {
        if (isSliding)
            return;

        velocity.y =
            Mathf.Sqrt(
                jumpForce *
                -2f *
                gravity
            );

        groundedAnimLockTimer =
            groundedAnimLockDuration;

        if (animator != null)
        {
            animator.ResetTrigger("Jump");
            animator.SetTrigger("Jump");

            Debug.Log(
                "JUMP TRIGGERED",
                this
            );
        }
    }

    // =========================================================
    // TRY START SLIDE
    // =========================================================

    private void TryStartSlide()
    {
        if (isDead)
            return;

        if (isSliding)
            return;

        if (slideCooldownTimer > 0f)
            return;

        if (!characterController.enabled)
            return;

        if (!characterController.isGrounded)
            return;

        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

        float currentSpeed =
            horizontalVelocity.magnitude;

        // С места слайд невозможен.
        if (currentSpeed < slideMinSpeed)
            return;

        if (horizontalVelocity.sqrMagnitude < 0.001f)
            return;

        slideDirection =
            horizontalVelocity.normalized;

        isSliding = true;

        slideTimer = slideDuration;
        slideCooldownTimer = slideCooldown;

        velocity.x =
            slideDirection.x * slideSpeed;

        velocity.z =
            slideDirection.z * slideSpeed;

        velocity.y = -2f;

        // Одна slide-анимация.
        if (animator != null)
        {
            animator.SetBool("IsSliding", true);
        }

        Debug.Log(
            $"SLIDE STARTED | Speed: {currentSpeed:F2}",
            this
        );
    }

    // =========================================================
    // HANDLE SLIDE
    // =========================================================

    private void HandleSlide()
    {
        if (!characterController.enabled)
        {
            StopSlide();
            return;
        }

        slideTimer -=
            Time.deltaTime;

        // ---------------------------------------------------------
        // CURRENT HORIZONTAL SPEED
        // ---------------------------------------------------------

        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

        float currentSpeed =
            horizontalVelocity.magnitude;

        // ---------------------------------------------------------
        // DECELERATION
        // ---------------------------------------------------------

        currentSpeed =
            Mathf.MoveTowards(
                currentSpeed,
                0f,
                slideDeceleration *
                Time.deltaTime
            );

        velocity.x =
            slideDirection.x *
            currentSpeed;

        velocity.z =
            slideDirection.z *
            currentSpeed;

        // ---------------------------------------------------------
        // GRAVITY
        // ---------------------------------------------------------

        velocity.y +=
            gravity *
            Time.deltaTime;

        // ---------------------------------------------------------
        // MOVE
        // ---------------------------------------------------------

        characterController.Move(
            velocity *
            Time.deltaTime
        );

        // ---------------------------------------------------------
        // STOP CONDITIONS
        // ---------------------------------------------------------

        if (slideTimer <= 0f)
        {
            StopSlide();
            return;
        }

        if (currentSpeed <= 0.1f)
        {
            StopSlide();
            return;
        }

        if (!characterController.isGrounded)
        {
            StopSlide();
            return;
        }

        // ---------------------------------------------------------
        // SLIDE ANIMATION PARAMETERS
        // ---------------------------------------------------------

        if (animator != null)
        {
            animator.SetFloat(
                "Forward",
                0f
            );

            animator.SetFloat(
                "Sideways",
                0f
            );

            animator.SetBool(
                "Grounded",
                true
            );

            animator.SetFloat(
                "VerticalVelocity",
                velocity.y
            );
        }
    }

    // =========================================================
    // STOP SLIDE
    // =========================================================

    private void StopSlide()
    {
        if (!isSliding)
            return;

        isSliding = false;

        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

        horizontalVelocity *= 0.25f;

        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        // Выключаем slide-анимацию.
        if (animator != null)
        {
            animator.SetBool("IsSliding", false);
        }

        Debug.Log(
            "SLIDE ENDED",
            this
        );
    }

    // =========================================================
    // ANIMATOR
    // =========================================================

    private void UpdateAnimator(
        float horizontal,
        float vertical,
        bool isGrounded)
    {
        if (animator == null)
            return;

        // Во время слайда не даём locomotion
        // перебивать slide-анимации.

        if (isSliding)
            return;

        animator.SetFloat(
            "Forward",
            vertical,
            0.1f,
            Time.deltaTime
        );

        animator.SetFloat(
            "Sideways",
            horizontal,
            0.1f,
            Time.deltaTime
        );

        bool groundedForAnim =
            isGrounded;

        // ---------------------------------------------------------
        // GROUNDED LOCK
        // ---------------------------------------------------------

        if (groundedAnimLockTimer > 0f)
        {
            groundedAnimLockTimer -=
                Time.deltaTime;

            groundedForAnim = false;
        }

        animator.SetBool(
            "Grounded",
            groundedForAnim
        );

        animator.SetFloat(
            "VerticalVelocity",
            velocity.y
        );
    }

    // =========================================================
    // CAMERA / LOOK
    // =========================================================

    private void HandleRotation()
    {
        float mouseX =
            Input.GetAxis("Mouse X") *
            lookSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y") *
            lookSensitivity;

        verticalRotation -=
            mouseY;

        verticalRotation =
            Mathf.Clamp(
                verticalRotation,
                -maxLookAngle,
                maxLookAngle
            );

        playerCamera.transform.localRotation =
            Quaternion.Euler(
                verticalRotation,
                0f,
                0f
            );

        transform.Rotate(
            Vector3.up *
            mouseX
        );
    }

    // =========================================================
    // WEAPON SWITCHING
    // =========================================================

    private void HandleWeaponSwitching()
    {
        if (stateMachine == null)
            return;

        if (weaponStates == null ||
            weaponStates.Count < 2)
            return;

        if (Input.GetKeyDown(
            KeyCode.Alpha1))
        {
            stateMachine.SetState(
                weaponStates[0]
            );
        }

        if (Input.GetKeyDown(
            KeyCode.Alpha2))
        {
            stateMachine.SetState(
                weaponStates[1]
            );
        }
    }

    // =========================================================
    // DISABLE WEAPONS
    // =========================================================

    public void DisableWeapons()
    {
        if (weaponsDisabled)
            return;

        weaponsDisabled = true;

        Debug.Log(
            "WEAPONS DISABLED",
            this
        );

        if (stateMachine != null)
        {
            stateMachine.enabled = false;

            Debug.Log(
                "WEAPON STATE MACHINE DISABLED",
                stateMachine
            );
        }
    }

    // =========================================================
    // GIZMOS
    // =========================================================

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            transform.position +
            Vector3.up * 0.03f,
            Vector3.down * 0.2f
        );
    }

#endif
}