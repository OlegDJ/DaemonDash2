using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class YBotController : MonoBehaviour, IDamageable
{
    #region States
    [Header("States")]
    public bool playerInSightRange;
    public bool playerInAttackRange;
    public bool fullVision, canMove = true;
    public bool isBlocking;
    public bool canAttack = true;
    #endregion

    #region Health
    [Header("Health")]
    [SerializeField] private int maxHealth = 400;
    [HideInInspector] public int health;
    #endregion

    #region Field of View
    [Header("Field of View")]
    [SerializeField] private float sightRange = 50f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField, Range(0f, 360f)] private float viewAngle = 150f;
    [HideInInspector] public float playerDist;
    private Vector3 dirToPlayer;
    #endregion

    #region State Machine Behavior
    [Header("State Machine Behavior")]
    [SerializeField] private float destUpdateInterval = 0.5f;
    private float movePower, desMovePower;
    public float minDistance = 0.2f;

    [Space(7.5f)] // Idle State
    public float minTimeIdle = 5f;
    public float maxTimeIdle = 10f;

    [Space(7.5f)] // Patrol State
    public float minPatrolRange = 5f;
    public float maxPatrolRange = 10f, minVelToStop = 0.01f;

    [Space(7.5f)] // Battle State
    public float maxPlayerDist = 10f;
    [SerializeField] private float minBattleRad = 4f, maxBattleRad = 7f;
    public float minActionsInterval = 3f, maxActionsInterval = 6f;
    [Range(0f, 1f)] public float actionChance = 0.6f;
    [HideInInspector] public float battleRad;

    [Space(7.5f)] // Relocate State
    public float minRelocateTime = 2f;
    public float maxRelocateTime = 15f;

    [Space(7.5f)] // Attack State
    public float minDistanceToAttack = 2.5f;
    #endregion

    #region Movement
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    [HideInInspector] public Vector3 movePos;
    #endregion

    #region Rotation
    [Header("Rotation")]
    [SerializeField, Range(0f, 1f)] private float rotSpeed = 0.1f;
    private Vector3 lookPos;
    private Quaternion desRot;
    #endregion

    #region Attack
    [Header("Attack")]
    [SerializeField] private float minAttackInterval = 0.5f;
    [SerializeField] private float maxAttackInterval = 3f;
    #endregion

    #region Block
    [Header("Block")]
    [SerializeField] private float minTimeBlock = 2f;
    [SerializeField] private float maxTimeBlock = 5f;
    [SerializeField, Range(0f, 1f)] private float blockChance = 0.6f;
    #endregion

    #region Stuff
    [Header("Stuff")]
    public Vector3 centerOffset = new(0f, 1f, 0f);
    [SerializeField] private Vector3 popUpTextOffset = new(0f, 1.25f, 0f);
    [SerializeField] private float animationSpeed = 0.1f;
    [HideInInspector] public Vector3 modPlayerPos, modPos;
    #endregion

    #region References
    [Header("References")]
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private GameObject ragdollPrefab;
    [SerializeField] private Collider blockTrigger;
    public Animator anim;
    [HideInInspector] public NavMeshAgent agent;
    private Manager mngr;
    private GameObject player;
    private PlayerController playerController;
    #endregion

    private void Awake()
    {
        mngr = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        player = mngr.player;
        playerController = mngr.playerController;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        health = maxHealth;
        UpdateBattleRadius();
        movePos = transform.position + centerOffset;
        StartCoroutine(AttackRoutine());
    }

    private void Update()
    {
        if (health <= 0f) Death();

        modPlayerPos = player.transform.position + centerOffset;
        modPos = transform.position + centerOffset;

        Rotation();
        Animation();
    }

    private void FixedUpdate() { CheckFOV(); }

    public void GetAttacked(int _damage, bool _willBeDodged)
    {
        anim.SetTrigger("Hit");
        if (!isBlocking || isBlocking && !_willBeDodged)
        {
            health -= _damage;
            mngr.popUpTxt.DisplayDamagePopUpText(_damage, transform.position + popUpTextOffset);
            if (Random.value >= blockChance) StartCoroutine(BlockCycle());
        }
        else mngr.popUpTxt.DisplayDodgedPopUpText(transform.position + popUpTextOffset);
    }

    private void Death()
    {
        Instantiate(ragdollPrefab, transform.position, transform.rotation, transform.parent);
        mngr.popUpTxt.DisplayDeathPopUpText(transform.position + popUpTextOffset);
        Destroy(gameObject);
    }

    private void CheckFOV()
    {
        if (!playerController.isDead)
        {
            playerDist = Vector3.Distance(modPos, modPlayerPos);
            dirToPlayer = (modPlayerPos - modPos).normalized;

            if (!Physics.Raycast(modPos, dirToPlayer, playerDist, obstacleLayerMask))
            {
                if (!fullVision)
                {
                    if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
                    {
                        if (playerDist <= sightRange) playerInSightRange = true;
                        else playerInSightRange = false;

                        if (playerDist <= attackRange) playerInAttackRange = true;
                        else playerInAttackRange = false;
                    }
                    else { playerInSightRange = false; playerInAttackRange = false; }
                }
                else
                {
                    if (playerDist <= sightRange) playerInSightRange = true;
                    else playerInSightRange = false;

                    if (playerDist <= attackRange) playerInAttackRange = true;
                    else playerInAttackRange = false;
                }
            }
            else { playerInSightRange = false; playerInAttackRange = false; }
        }
        else { playerInSightRange = false; playerInAttackRange = false; }
    }

    private IEnumerator UpdateDestinationRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(destUpdateInterval);
            if (agent != null && canMove) agent.SetDestination(movePos);
        }
    }

    public void SetAgent(bool _updateDest)
    {
        if (_updateDest) StartCoroutine(UpdateDestinationRoutine());
        else
        {
            StopCoroutine(UpdateDestinationRoutine());
            if (agent != null && canMove) agent.SetDestination(movePos);
        }
    }

    public void UpdateBattleRadius()
    {
        battleRad = Mathf.Round(Random.Range(minBattleRad, maxBattleRad) * 10f) * 0.1f;
    }

    public Vector3 GetConnectedDirection()
    {
        Vector3 heading = modPos - modPlayerPos;
        return (heading / heading.magnitude).normalized;
    }

    public Vector3 GetRelocateDirection(bool _leftOrRight)
    {
        return modPlayerPos + GetConnectedDirection() * battleRad +
            (_leftOrRight? -transform.right : transform.right);
    }

    private void Rotation()
    {
        if (playerInSightRange)
        {
            agent.updateRotation = false;
            lookPos = modPlayerPos - modPos;
            lookPos.y = 0f;
            desRot = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, desRot, rotSpeed);
        }
        else agent.updateRotation = true;
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (playerInAttackRange && canAttack) MakeOneAttack();
            yield return new WaitForSeconds(Random.Range(minAttackInterval, maxAttackInterval));
        }
    }

    public void MakeOneAttack()
    {
        anim.SetInteger("Attack Index", Random.Range(1, 4));
        anim.SetTrigger("Attack");
    }

    private IEnumerator BlockCycle()
    {
        isBlocking = true;
        blockTrigger.enabled = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(Random.Range(minTimeBlock, maxTimeBlock));
        agent.isStopped = false;
        blockTrigger.enabled = false;
        isBlocking = false;
    }

    private void Animation()
    {
        if (agent.velocity.magnitude > 0f)
        {
            if (agent.speed == walkSpeed) desMovePower = 0.5f;
            else desMovePower = 1f;
        }
        else desMovePower = 0f;
        if (movePower != desMovePower) movePower = Mathf.MoveTowards(movePower, desMovePower, animationSpeed);
        anim.SetFloat("Move Power", Mathf.Clamp01(movePower));
        anim.SetBool("Is Blocking", isBlocking);
    }
}
