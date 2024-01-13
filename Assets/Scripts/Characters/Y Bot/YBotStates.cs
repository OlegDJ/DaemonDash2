using UnityEngine;
using UnityEngine.AI;

public class YBotIdleState : State
{
    private float curTime, timeToWait;
    private bool idleStarted;

    private readonly YBotController c;
    private readonly YBotStateMachine sM;

    public YBotIdleState(YBotStateMachine _sM) : base("Idle State", _sM)
    {
        sM = _sM;
        c = sM.controller;
    }

    public override void OnStateEnter()
    {
        timeToWait = Random.Range(c.minTimeIdle, c.maxTimeIdle);
    }

    public override void StateUpdate()
    {
        if (c.playerInSightRange) sM.ChangeState(sM.chase);
        else if (!c.agent.hasPath)
        {
            if (!idleStarted)
            {
                c.movePos = c.modPos;
                c.SetAgent(false);
                c.agent.speed = c.runSpeed;
                idleStarted = true;
            }
            else
            {
                if (curTime < timeToWait) curTime += Time.deltaTime;
                else sM.ChangeState(sM.patrol);
            }
        }
    }

    public override void OnStateExit()
    {
        curTime = 0f;
        idleStarted = false;
    }
}

public class YBotPatrolState : State
{
    private bool patrolPointSet;
    private Vector3 patrolPointPos;
    private float patrolPointDist;

    private readonly YBotController c;
    private readonly YBotStateMachine sM;

    public YBotPatrolState(YBotStateMachine _sM) : base("Patrol State", _sM)
    {
        sM = _sM;
        c = sM.controller;
    }

    public override void OnStateEnter()
    {
        patrolPointSet = false;
        patrolPointPos = GetPatrolPointPosition();
        if (NavMesh.SamplePosition(patrolPointPos, out NavMeshHit navMeshHit, c.maxPatrolRange, -1))
        {
            patrolPointSet = true;
            patrolPointPos = new(navMeshHit.position.x, navMeshHit.position.y + c.centerOffset.y, navMeshHit.position.z);
            c.movePos = patrolPointPos;
            c.SetAgent(false);
            c.agent.speed = c.walkSpeed;
        }
        else sM.ChangeState(sM.idle);
    }

    public override void StateUpdate()
    {
        if (c.playerInSightRange) sM.ChangeState(sM.chase);
        else if (patrolPointSet)
        {
            patrolPointDist = Vector3.Distance(c.modPos, patrolPointPos);
            if (patrolPointDist <= c.minDistance || c.agent.desiredVelocity.magnitude <= c.minVelToStop)
            {
                sM.ChangeState(sM.idle);
            }
        }
    }

    public override void OnStateExit() { patrolPointSet = false; }

    private Vector3 GetPatrolPointPosition()
    {
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        return c.modPos + randomDirection * Random.Range(c.minPatrolRange, c.maxPatrolRange);
    }
}

public class YBotChaseState : State
{
    private readonly YBotController c;
    private readonly YBotStateMachine sM;

    public YBotChaseState(YBotStateMachine _sM) : base("Chase State", _sM)
    {
        sM = _sM;
        c = sM.controller;
    }

    public override void OnStateEnter()
    {
        c.SetAgent(true);
        c.agent.speed = c.runSpeed;
    }

    public override void StateUpdate()
    {
        if (c.playerInSightRange)
        {
            if (c.playerDist <= c.battleRad) sM.ChangeState(sM.battle);
            else c.movePos = c.modPlayerPos;
        }
        else sM.ChangeState(sM.idle);
    }
}

public class YBotBattleState : State
{
    private Vector3 circlePos;
    private float curTime, timeToWait;

    private readonly YBotController c;
    private readonly YBotStateMachine sM;

    public YBotBattleState(YBotStateMachine _sM) : base("Battle State", _sM)
    {
        sM = _sM;
        c = sM.controller;
    }

    public override void OnStateEnter()
    {
        timeToWait = Random.Range(c.minActionsInterval, c.maxActionsInterval);
        c.fullVision = true;
        c.SetAgent(true);
        c.agent.speed = c.runSpeed;
    }

    public override void StateUpdate()
    {
        if (c.playerDist < c.maxPlayerDist)
        {
            if (curTime < timeToWait)
            {
                curTime += Time.deltaTime;
                circlePos = c.modPlayerPos + c.GetConnectedDirection() * c.battleRad;
                circlePos.y = c.modPos.y;
                c.movePos = circlePos;
            }
            else
            {
                if (Random.value > c.actionChance) sM.ChangeState(sM.relocate);
                else sM.ChangeState(sM.attack);
            }
        }
        else sM.ChangeState(sM.chase);
    }

    public override void OnStateExit()
    {
        curTime = 0f;
        c.fullVision = false;
    }
}

public class YBotAttackState : State
{
    private bool attacked;

    private readonly YBotController c;
    private readonly YBotStateMachine sM;

    public YBotAttackState(YBotStateMachine _sM) : base("Attack State", _sM)
    {
        sM = _sM;
        c = sM.controller;
    }

    public override void OnStateEnter()
    {
        c.SetAgent(true);
        c.agent.speed = c.runSpeed;
        c.canAttack = false;
        c.agent.stoppingDistance = c.minDistanceToAttack;
    }

    public override void StateUpdate()
    {
        if (c.playerDist < c.maxPlayerDist)
        {
            if (!attacked)
            {
                if (c.playerDist > c.minDistanceToAttack) c.movePos = c.modPlayerPos;
                else
                {
                    attacked = true;
                    c.MakeOneAttack();
                    c.UpdateBattleRadius();
                }
            }
            else sM.ChangeState(sM.battle);
        }
        else sM.ChangeState(sM.chase);
    }

    public override void OnStateExit()
    {
        attacked = false;
        c.canAttack = true;
        c.agent.stoppingDistance = 0f;
    }
}

public class YBotRelocateState : State
{
    private float curRelTime, curTime;
    private bool leftOrRight;

    private readonly YBotController c;
    private readonly YBotStateMachine sM;

    public YBotRelocateState(YBotStateMachine _sM) : base("Relocate State", _sM)
    {
        sM = _sM;
        c = sM.controller;
    }

    public override void OnStateEnter()
    {
        c.SetAgent(true);
        c.agent.speed = c.walkSpeed;
        c.fullVision = true;
        curRelTime = Random.Range(c.minRelocateTime, c.maxRelocateTime);
        leftOrRight = Random.value >= 0.5f;
    }

    public override void StateUpdate()
    {
        if (c.playerDist < c.maxPlayerDist)
        {
            curTime += Time.deltaTime;
            c.movePos = c.GetRelocateDirection(leftOrRight);
        }
        else sM.ChangeState(sM.chase);

        if (curTime >= curRelTime) sM.ChangeState(sM.battle);
    }

    public override void OnStateExit()
    {
        c.fullVision = false;
        curTime = 0f;
    }
}

//public class YBotBlockState : State
//{
//    private readonly YBotController c;
//    private readonly YBotStateMachine sM;

//    public YBotBlockState(YBotStateMachine _sM) : base("Block State", _sM)
//    {
//        sM = _sM;
//        c = sM.controller;
//    }

//    public override void OnStateEnter()
//    {
//        base.OnStateEnter();
//    }

//    public override void StateUpdate()
//    {
//        base.StateUpdate();
//    }

//    public override void OnStateExit()
//    {
//        base.OnStateExit();
//    }
//}

/*

||||||||||||||||||||
|| STATE TEMPLATE ||
||||||||||||||||||||

using UnityEngine;

public class TemplateState : State
{
    private readonly TemplateController c;
    private readonly TemplateStateMachine sM;

    public TemplateState(TemplateStateMachine _sM) : base("Template State", _sM)
    {
        sM = _sM;
        c = sM.controller;
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
    }
}

*/