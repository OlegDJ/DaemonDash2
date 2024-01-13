using UnityEngine;

public class OldEnemyAIBehavior : StateMachine
{
    [HideInInspector] public OldEnemyController enemyController;

    [HideInInspector] public OldIdleState idleState;
    [HideInInspector] public OldPatrolState patrolState;
    [HideInInspector] public OldChaseState chaseState;
    [HideInInspector] public OldBattleState battleState;
    [HideInInspector] public OldChangePositionState changePositionState;
    [HideInInspector] public OldAttackState attackState;
    [HideInInspector] public OldEscapeState escapeState;

    private void Awake()
    {
        enemyController = GetComponent<OldEnemyController>();

        idleState = new(this);
        patrolState = new(this);
        chaseState = new(this);
        battleState = new(this);
        changePositionState = new(this);
        attackState = new(this);
        escapeState = new(this);
    }

    protected override State GetInitialState() { return idleState; }
}
