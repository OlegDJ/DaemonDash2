using UnityEngine;

[RequireComponent(typeof(YBotController))]
public class YBotStateMachine : StateMachine
{
    [HideInInspector] public YBotController controller;

    [HideInInspector] public YBotIdleState idle;
    [HideInInspector] public YBotPatrolState patrol;
    [HideInInspector] public YBotChaseState chase;
    [HideInInspector] public YBotBattleState battle;
    [HideInInspector] public YBotAttackState attack;
    [HideInInspector] public YBotRelocateState relocate;

    private void Awake()
    {
        controller = GetComponent<YBotController>();

        idle = new(this);
        patrol = new(this);
        chase = new(this);
        battle = new(this);
        relocate = new(this);
        attack = new(this);
    }

    protected override State GetInitialState() { return idle; }
}
