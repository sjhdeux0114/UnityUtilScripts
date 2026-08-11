using System;
using System.Collections.Generic;

public interface IState
{
    void Enter();
    void Tick();
    void Exit();
}

// 추상 클래스를 하나 두어, 캐릭터 컴포넌트에 쉽게 접근할 수 있도록 만듭니다.
public abstract class BaseState<T> : IState where T : class
{
    protected T Owner; // 이 상태를 소유한 캐릭터 (예: Player, Enemy 등)
    protected StateMachine<T> StateMachine;

    protected BaseState(T owner, StateMachine<T> stateMachine)
    {
        Owner = owner;
        StateMachine = stateMachine;
    }

    public abstract void Enter();
    public abstract void Tick();
    public abstract void Exit();
}
public enum CharacterStateType
{
    Ready,
    Move,
    Idle,
    Attack
}


public class StateMachine<T> where T : class
{
    public IState CurrentState { get; private set; }

    // 상태들을 미리 등록해두고 캐싱하여 GC 발생을 방지합니다.
    private Dictionary<CharacterStateType, IState> _states = new();

    public void AddState(CharacterStateType type, IState state)
    {
        if (!_states.ContainsKey(type))
        {
            _states.Add(type, state);
        }
    }

    public void Initialize(CharacterStateType startStateType)
    {
        if (_states.TryGetValue(startStateType, out IState startState))
        {
            CurrentState = startState;
            CurrentState.Enter();
        }
    }

    public void ChangeState(CharacterStateType _nextStateType)
    {
        if (!_states.TryGetValue(_nextStateType, out IState newState)) return;
        if (newState == CurrentState) return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Tick()
    {
        CurrentState?.Tick();
    }
}