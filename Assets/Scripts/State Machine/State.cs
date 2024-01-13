public abstract class State
{
    public string name;
    protected StateMachine stateMachine;

    public State(string name, StateMachine stateMachine)
    {
        this.name = name;
        this.stateMachine = stateMachine;
    }

    public virtual void OnStateEnter() { }

    public virtual void StateUpdate() { }

    public virtual void OnStateExit() { }
}
