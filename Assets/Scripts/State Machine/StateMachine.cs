using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    [Header("State Machine")]
    public string curStateName;

    public State curState;

    private void Start()
    {
        curState = GetInitialState();
        curStateName = GetInitialState().name;
        if (curState != null) curState.OnStateEnter();
    }

    protected virtual State GetInitialState() { return null; }

    private void Update() { if (curState != null) curState.StateUpdate(); }

    public void ChangeState(State newState)
    {
        curState.OnStateExit();
        curState = newState;
        curStateName = curState.name;
        curState.OnStateEnter();
    }
}
