using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class OldEnemyController : MonoBehaviour
{
    #region Health
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [HideInInspector] public int health;
    #endregion

    #region Check Player In Ranges
    [Header("Check Player In Ranges")]
    [SerializeField] private float sightRange = 25f;
    [SerializeField] private float attackRange = 1.5f;
    [HideInInspector] public bool fullVision;
    [SerializeField, Range(0f, 360f)] private float viewAngle = 190f;
    public Vector3 offsetFromObjectCenter = new(0f, 1f, 0f);
    [HideInInspector] public float distanceToPlayer;
    private Vector3 directionToPlayer;
    [HideInInspector] public bool playerInSightRange, playerInAttackRange;
    [HideInInspector] public Vector3 modifiedPlayerPosition, modifiedPosition;
    private Vector3 destinationPosition;
    #endregion

    #region AI Behavior
    [Header("AI Behavior")]
    [SerializeField] private float updateDestinationDelay = 0.2f;
    private bool updateDestination;
    private float movePower, desirableMovePower;
    public float minDistance = 0.2f;

    [Space(7.5f)] // Idle State
    public float minTimeIdle = 5f;
    public float maxTimeIdle = 10f;

    [Space(7.5f)] // Patrol State
    public float minPatrolRange = 5f;
    public float maxPatrolRange = 10f;
    public float minVelocityToStop = 0.01f;

    [Space(7.5f)] // Battle State
    [SerializeField] private float minBattleRadius = 4f;
    [SerializeField] private float maxBattleRadius = 7f;
    [HideInInspector] public float battleRadius, maxDistanceFromPlayer = 15f;
    public float minTimeBetweenActions = 3f, maxTimeBetweenActions = 6f;
    [Range(0f, 1f)] public float actionChance = 0.6f;

    [Space(7.5f)] // Change Position State
    public float pointMoveSpeed = 0.5f;

    [Space(7.5f)] // Attack State
    public float minDistanceToAttack = 2.5f;
    #endregion

    #region Movement
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    #endregion

    #region Rotation
    [Header("Rotation")]
    [SerializeField, Range(0f, 1f)] private float rotationSpeed = 0.1f;
    private Vector3 lookPos;
    private Quaternion desiredRotation;
    #endregion

    #region Attack
    [Header("Attack")]
    [SerializeField] private float minTimeBetweenAttacks = 3f, maxTimeBetweenAttacks = 5f;
    public bool canAttack;
    #endregion

    #region Block
    [Header("Block")]
    [SerializeField] private float minTimeBlock = 2f;
    [SerializeField] private float maxTimeBlock = 5f;
    [SerializeField, Range(0f, 1f)] private float blockChance = 0.6f;
    private bool isBlocking;
    #endregion

    #region Stuff
    [Header("Stuff")]
    [SerializeField, Tooltip("" +
        "Offset from object's center where pop-up text spawns." +
        "")]
    private Vector3 popUpTextOffset = new(0f, 1.25f, 0f);
    [SerializeField] private float animationSpeed = 0.1f;
    #endregion

    #region References
    [Header("References")]
    [SerializeField] private GameObject ragdollPrefab;
    [SerializeField] private LayerMask playerLayerMask, obstacleLayerMask;
    [HideInInspector] public NavMeshAgent navMeshAgent;
    private GameObject player;
    [HideInInspector] public Animator anim;
    private PopUpTextManager popUpTextManager;
    #endregion

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        anim = GetComponentInChildren<Animator>();
        GameObject mainManager = GameObject.FindGameObjectWithTag("Manager");
        popUpTextManager = mainManager.GetComponent<PopUpTextManager>();
    }

    private void Start()
    {
        health = maxHealth;

        UpdateBattleRadius();

        destinationPosition = transform.position + offsetFromObjectCenter;
        StartCoroutine(UpdateDestinationForAgentRoutine());

        StartCoroutine(AttackRoutine());
    }

    private void Update()
    {
        if (health <= 0f) Death();

        modifiedPlayerPosition = player.transform.position + offsetFromObjectCenter;
        modifiedPosition = transform.position + offsetFromObjectCenter;

        Rotation();
        Animation();
    }

    private void FixedUpdate() { CheckPlayerInRanges(); }

    public void TakeDamage(int damage, bool willDodge)
    {
        anim.SetTrigger("Hit");
        if (!isBlocking || isBlocking && !willDodge)
        {
            health -= damage;
            popUpTextManager.DisplayDamagePopUpText(damage, transform.position + popUpTextOffset);
            if (Random.value >= blockChance) StartCoroutine(BlockCycle());
        }
        else popUpTextManager.DisplayDodgedPopUpText(transform.position + popUpTextOffset);
    }

    private void Death()
    {
        Instantiate(ragdollPrefab, transform.position, transform.rotation, transform.parent);
        popUpTextManager.DisplayDeathPopUpText(transform.position + popUpTextOffset);
        Destroy(gameObject);
    }

    private IEnumerator UpdateDestinationForAgentRoutine()
    {
        while (true)
        {
            if (navMeshAgent != null && updateDestination) navMeshAgent.SetDestination(destinationPosition);
            yield return new WaitForSeconds(updateDestinationDelay);
        }
    }

    private void CheckPlayerInRanges()
    {
        if (player != null)
        {
            distanceToPlayer = Vector3.Distance(modifiedPosition, modifiedPlayerPosition);
            directionToPlayer = (modifiedPlayerPosition - modifiedPosition).normalized;

            if (!Physics.Raycast(modifiedPosition, directionToPlayer, distanceToPlayer, obstacleLayerMask))
            {
                if (!fullVision)
                {
                    if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2)
                    {
                        if (distanceToPlayer <= sightRange) playerInSightRange = true;
                        else playerInSightRange = false;

                        if (distanceToPlayer <= attackRange) playerInAttackRange = true;
                        else playerInAttackRange = false;
                    }
                    else { playerInSightRange = false; playerInAttackRange = false; }
                }
                else
                {
                    if (distanceToPlayer <= sightRange) playerInSightRange = true;
                    else playerInSightRange = false;

                    if (distanceToPlayer <= attackRange) playerInAttackRange = true;
                    else playerInAttackRange = false;
                }
            }
            else { playerInSightRange = false; playerInAttackRange = false; }
        }
        else { playerInSightRange = false; playerInAttackRange = false; }
    }

    private void Rotation()
    {
        if (playerInSightRange)
        {
            navMeshAgent.updateRotation = false;

            lookPos = modifiedPlayerPosition - modifiedPosition;
            lookPos.y = modifiedPosition.y;

            desiredRotation = Quaternion.LookRotation(lookPos);

            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed);
        }
        else navMeshAgent.updateRotation = true;
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (playerInAttackRange && canAttack) MakeOneAttack();
            yield return new WaitForSeconds(Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks));
        }
    }

    private IEnumerator BlockCycle()
    {
        isBlocking = true;
        navMeshAgent.isStopped = true;
        yield return new WaitForSeconds(Random.Range(minTimeBlock, maxTimeBlock));
        navMeshAgent.isStopped = false;
        isBlocking = false;
    }

    private void Animation()
    {
        if (navMeshAgent.velocity.magnitude > 0f)
        {
            if (navMeshAgent.speed == walkSpeed) desirableMovePower = 0.5f;
            else desirableMovePower = 1f;
        }
        else desirableMovePower = 0f;
        if (movePower != desirableMovePower) movePower = Mathf.MoveTowards(movePower, desirableMovePower, animationSpeed);
        anim.SetFloat("Move Power", Mathf.Clamp01(movePower));

        anim.SetBool("Is Blocking", isBlocking);
    }

    public void SetNavMeshAgent(Vector3 desirableDestinationPosition,
        float desirableSpeed, bool shouldUpdateDestination)
    {
        destinationPosition = desirableDestinationPosition;
        navMeshAgent.speed = desirableSpeed;
        updateDestination = shouldUpdateDestination;

        if (navMeshAgent != null && !updateDestination) navMeshAgent.SetDestination(destinationPosition);
    }

    public void UpdateBattleRadius()
    {
        battleRadius = Mathf.Round(Random.Range(minBattleRadius, maxBattleRadius) * 10f) * 0.1f;
    }

    public Vector3 GetConnectedDirection()
    {
        Vector3 heading = modifiedPosition - modifiedPlayerPosition;
        return (heading / heading.magnitude).normalized;
    }

    public Vector3 GetRandomDirection()
    {
        Vector2 rawDirection = Random.insideUnitCircle.normalized;
        return new(rawDirection.x, 0f, rawDirection.y);
    }

    public void MakeOneAttack() { anim.SetTrigger("Attack"); }
}
