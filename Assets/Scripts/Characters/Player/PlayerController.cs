using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour, IDamageable
{
    #region States
    [Header("States")]
    public bool isDead;

    [Space(7.5f)] // Movement
    public bool canMove = true;
    public bool canRotateBody = true;
    public bool canRun = true, isRunning;

    [Space(7.5f)] // Actions
    public bool canAttack = true;
    public bool canDash = true, isDashing;
    public bool canBlock = true, isBlocking;
    public bool canFocus = true, isFocusing, isFocusChanging;

    [Space(7.5f)] // Orientation
    public bool isGrounded;
    public bool isSloped;
    #endregion

    #region Health
    [Header("Health")]
    [SerializeField] private int maxHealth = 400;
    private int health;
    #endregion

    #region Energy
    [Header("Energy")]
    [SerializeField] private float maxEnergy = 200f;
    [SerializeField] private float energyReviveSpeed = 0.5f, energyReduceRun = 0.2f;
    [SerializeField] private float energyReduceAttack = 25f, energyReduceDash = 0.6f, energyReduceDodge = 5f;
    private float energy, desirableDashEnergy;
    #endregion

    #region Movement
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    private float moveSpeed;
    private Vector3 moveDirection, targetVelocity;
    [SerializeField] private Vector3 checkRayOffset = new(0f, 0.1f, 0f);

    [Space(7.5f)] // Ground Check
    [SerializeField] private float groundDrag = 5f;

    [Space(7.5f)] // Slope Check
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private float slopeCheckRayLength = 0.3f;
    private RaycastHit slopeHit;
    #endregion

    #region Rotation
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.15f;
    private float curBodyRot, targetBodyRot, bodyRotVel;

    //private Vector3 rotationDirection;
    //private Vector3 smoothedRotationDirection;
    #endregion

    #region Focus
    [Header("Focus")]
    [SerializeField] private float focusChangeRange = 45f;
    private RaycastHit focusChangeHit;
    #endregion

    #region Dashing
    [Header("Dashing")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 1f, dashCooldown = 10f;
    [SerializeField] private Vector3 dashStartOffset = new(0f, 20f, 0f);
    private float dashCooldownTimer;
    #endregion

    #region Stuff
    [Header("Stuff")]
    [SerializeField] private Vector3 popUpTextOffset = new(0f, 1.25f, 0f);
    [SerializeField] private float animationSpeed = 0.05f;
    [HideInInspector] public float movePower, desirableMovePower;
    #endregion

    #region References
    [Header("References")]
    [SerializeField] private CameraController camControl;
    public Transform playerBody;
    [SerializeField] private GameObject ragdollPrefab;
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private Animator anim;
    [SerializeField] private Collider blockTrigger;
    private Transform trnsfrm;
    private Rigidbody rb;
    private Collider col;
    private Manager mngr;
    [HideInInspector] public Focusable focusTarget;
    private Transform focusPoint;
    #endregion

    private void Awake()
    {
        trnsfrm = transform;
        mngr = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        focusPoint = camControl.focusPoint;
    }

    private void Start()
    {
        canMove = true;
        mngr.ui.SetHealthBarMaxValue(maxHealth);
        health = maxHealth;
        mngr.ui.SetEnergyBarMaxValue(maxEnergy);
        energy = maxEnergy;
        mngr.ui.SetDashCooldownBarMaxValue(dashCooldown);
    }

    private void HandlePermits()
    {
        if (!isDead)
        {
            canMove = canRotateBody = true;
            canRun = energy > 0f && mngr.input.movement.magnitude > 0f;
            canAttack = energy >= energyReduceAttack;
            canDash = energy >= energyReduceDash &&
                mngr.input.movement.magnitude > 0f && dashCooldownTimer >= dashCooldown;
            canBlock = energy >= energyReduceDodge;
            canFocus = true;

            if (isDashing) canRun = canAttack = canBlock =
                    isRunning = isBlocking = isFocusChanging = false;
            if (isBlocking) canDash = canRun =
                    isDashing = isRunning = false;
            if (isFocusChanging) canMove = canRotateBody = canRun = canAttack = canDash = canBlock =
                    isRunning = isDashing = isBlocking = isFocusing = false;

            if (!canFocus) CancelFocusChange();
        }
        else
        {
            canMove = false;
            canRotateBody = false;
            canRun = false;
            canAttack = false;
            canDash = false;
            canBlock = false;
            canFocus = false;
        }
    }

    public void Action(ActType actionType)
    {
        if (isDead) return;
        switch (actionType)
        {
            case ActType.RunPerf:
                if (!canRun) return;
                isRunning = true;
                camControl.SetRunning(true);
                camControl.RunFOV();
                break;
            case ActType.RunCanc:
                isRunning = false;
                camControl.SetRunning(false);
                break;
            case ActType.AttackStrtd:
                if (canAttack) Attack();
                break;
            case ActType.FocusChangePerf:
                if (canFocus) PerformFocusChange();
                break;
            case ActType.FocusChangeCanc:
                if (canFocus) CancelFocusChange();
                break;
            case ActType.DashStrtd:
                if (!canDash) return;
                Dash();
                camControl.SetDashing(true);
                break;
            case ActType.BlockStrtd:
                if (!canBlock) return;
                isBlocking = true;
                blockTrigger.enabled = true;
                canMove = false;
                break;
            case ActType.BlockCanc:
                isBlocking = false;
                blockTrigger.enabled = false;
                canMove = true;
                break;
        }
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            GroundCheck();
            SlopeCheck();
            if (isFocusChanging) FocusChangeFixedUpdate();
            Movement();
        }
    }

    private void Update()
    {
        HandlePermits();
        HandleHealth();
        if (!isDead)
        {
            HandleEnergy();
            if (canRotateBody) Rotation();
            DashUpdate();
            Animation();
        }
        mngr.ui.SetDashCooldownBarValue(dashCooldownTimer);
    }

    private void HandleHealth()
    {
        if (health <= 0f && !isDead) Death();
        health = Mathf.Clamp(health, 0, maxHealth);
        mngr.ui.SetHealthBarValue(health);
    }

    private void HandleEnergy()
    {
        if (energy < maxEnergy && !isBlocking && !isRunning && !isDashing)
            energy += energyReviveSpeed * mngr.GetDeltaTime();
        else if (isRunning)
            energy -= energyReduceRun * mngr.GetDeltaTime();
        else if (isDashing)
            energy -= energyReduceDash * mngr.GetDeltaTime();
        energy = Mathf.Clamp(energy, 0, maxEnergy);
        mngr.ui.SetEnergyBarValue(energy);
    }

    public void Heal(int _heal) { health += _heal; }

    public void GetAttacked(int _damage, bool _willBeDodged)
    {
        if (isDead) return;
        anim.SetInteger("Hit Index", Random.Range(1, 4));
        anim.SetTrigger("Hit");
        camControl.SetHit();
        if (!_willBeDodged)
        {
            health -= _damage;
            mngr.popUpTxt.DisplayDamagePopUpText(_damage, transform.position + popUpTextOffset);
        }
        else
        {
            energy -= energyReduceDodge;
            mngr.popUpTxt.DisplayDodgedPopUpText(transform.position + popUpTextOffset);
        }
    }

    private void Death()
    {
        isDead = true;
        Instantiate(ragdollPrefab, transform.position, transform.rotation, transform);
        col.enabled = false;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        mngr.popUpTxt.DisplayDeathPopUpText(transform.position + popUpTextOffset);
        mngr.OnPlayerDeath();
        Destroy(playerBody.gameObject);
    }

    private void GroundCheck()
    {
        if (!isSloped) isGrounded = Physics.Raycast(transform.position + checkRayOffset, Vector3.down, 0.2f);
        else isGrounded = false;

        if ((isGrounded || isSloped) && !isDashing) rb.drag = groundDrag;
        else rb.drag = 0f;
    }

    private void SlopeCheck()
    {
        if (Physics.Raycast(transform.position + checkRayOffset,
            Vector3.down, out slopeHit, slopeCheckRayLength))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            if (angle <= maxSlopeAngle && angle != 0f) isSloped = true;
        }
        else isSloped = false;
    }

    private void Movement()
    {
        if (canMove && mngr.input.smoothedMovement.magnitude > 0f)
        {
            if (!isDashing)
            {
                if (isRunning) moveSpeed = runSpeed;
                else moveSpeed = walkSpeed;
            }
            else moveSpeed = dashSpeed;

            moveDirection = camControl.camHandleTrnsfrm.right * mngr.input.smoothedMovement.x +
                camControl.camHandleTrnsfrm.forward * mngr.input.smoothedMovement.y;
            moveDirection.y = 0f;
            moveDirection.Normalize();

            rb.useGravity = true;

            if (isSloped) targetVelocity = GetSlopeMoveDirection() * moveSpeed;
            else
            {
                targetVelocity = moveDirection * moveSpeed;

                if (isDashing) targetVelocity.y = 0f;
                else targetVelocity.y = rb.velocity.y;
            }
        }
        else
        {
            targetVelocity = Vector3.zero;

            if (isSloped) rb.useGravity = false;
            else
            {
                targetVelocity.y = rb.velocity.y;
                rb.useGravity = true;
            }
        }

        rb.velocity = targetVelocity;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
    
    private void Rotation()
    {
        if (isFocusing)
        {
            Vector3 desRotDir = focusPoint.position - playerBody.position;
            targetBodyRot = Quaternion.LookRotation(desRotDir).eulerAngles.y;
            curBodyRot = Mathf.SmoothDampAngle(curBodyRot, targetBodyRot, ref bodyRotVel, rotationSpeed);
            playerBody.eulerAngles = new Vector3(0f, curBodyRot, 0f);
        }
        else if (mngr.input.movement.magnitude > 0f)
        {
            targetBodyRot = Mathf.Atan2(mngr.input.movement.x, mngr.input.movement.y) * Mathf.Rad2Deg;
            targetBodyRot -= camControl.yAngleOffset;
            curBodyRot = Mathf.SmoothDampAngle(curBodyRot, targetBodyRot, ref bodyRotVel, rotationSpeed);
            playerBody.eulerAngles = new Vector3(0f, curBodyRot, 0f);
        }

        //rotationDirection = camControl.camPivotTrnsfrm.right * mngr.input.movement.x +
        //    camControl.camPivotTrnsfrm.forward * mngr.input.movement.y;
        //rotationDirection.y = 0f;
        //rotationDirection.Normalize();

        //if (mngr.input.movement.magnitude > 0f)
        //{
        //    smoothedRotationDirection = Vector3.Lerp(playerBody.forward,
        //        rotationDirection, rotSpeed * mngr.GetDeltaTime());
        //    playerBody.forward = smoothedRotationDirection;
        //}
    }

    private void Attack()
    {
        energy -= energyReduceAttack;
        anim.SetInteger("Attack Index", Random.Range(1, 4));
        anim.SetTrigger("Attack");
    }

    private void TurnFocusChange(bool on)
    {
        if (on) mngr.time.SlowDownTime();
        else mngr.time.SpeedUpTime();

        mngr.postProc.SetColorAdjustmentsSaturation(on);
    }

    private void PerformFocusChange()
    {
        if (isFocusing) camControl.StartFocusTransition();
        else camControl.StartFChangeTransition();
        isFocusChanging = true;
        mngr.ui.TurnCrosshair(true);
        TurnFocusChange(true);
    }

    private void CancelFocusChange()
    {
        if (focusTarget != null && canFocus)
        {
            isFocusing = true;
            camControl.BeforeFocus(focusTarget.transform,
                focusTarget.startPointIndex,
                focusTarget.focusPoints);
        }
        else
        {
            camControl.StartFChangeTransition();
            mngr.ui.TurnCrosshair(false);
        }
        isFocusChanging = false;
        TurnFocusChange(false);
    }

    private void FocusChangeFixedUpdate()
    {
        if (Physics.Raycast(camControl.camHandleTrnsfrm.position, camControl.camHandleTrnsfrm.forward,
            out focusChangeHit, focusChangeRange, ~playerLayerMask))
        {
            if (!focusChangeHit.transform.GetComponent<Focusable>())
            {
                mngr.ui.ChangeCrosshairAlpha(false);
                focusTarget = null;
                mngr.ui.SetFocusInfoText("Nothing");
            }
            else
            {
                mngr.ui.ChangeCrosshairAlpha(true);
                focusTarget = focusChangeHit.transform.GetComponent<Focusable>();
                mngr.ui.SetFocusInfoText(focusChangeHit.transform.name);
            }
        }
    }

    private void DashUpdate()
    {
        if (dashCooldownTimer < dashCooldown && !isDashing) dashCooldownTimer += Time.deltaTime;
        else if (dashCooldownTimer > dashCooldown) dashCooldownTimer = dashCooldown;

        if (isDashing)
        {
            if (mngr.input.movement.magnitude == 0f || energy <= 0f) ResetDash();
            camControl.DashFOV();
        }
        else if (!isRunning) camControl.NormalFOV();
    }

    private void Dash()
    {
        isRunning = isBlocking = false;

        dashCooldownTimer = 0f;

        rb.AddForce(dashStartOffset, ForceMode.VelocityChange);

        isDashing = true;

        desirableDashEnergy = energy - energyReduceDash;

        Invoke(nameof(ResetDash), dashDuration);
    }

    private void ResetDash()
    {
        isDashing = false;
        camControl.SetDashing(false);
        desirableDashEnergy = 0f;
    }

    private void Animation()
    {
        if (mngr.input.movement.magnitude > 0f && canMove)
        {
            if (isRunning) desirableMovePower = 1f;
            else desirableMovePower = 0.5f;
        }
        else desirableMovePower = 0f;
        if (movePower != desirableMovePower)
            movePower = Mathf.MoveTowards(movePower, desirableMovePower, animationSpeed * mngr.GetDeltaTime());
        anim.SetFloat("Move Power", Mathf.Clamp01(movePower));

        anim.SetBool("Is Dashing", isDashing);

        anim.SetBool("Is Blocking", isBlocking);
    }
}
