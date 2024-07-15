using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace StateMachineSystem
{
    public class StateMachine
    {
        IState currentState;
        readonly List<StateTransition> stateTransitions;
        readonly List<StateTransition> anyTransitions;
        public StateMachine()
        {
            stateTransitions = new List<StateTransition>();
            anyTransitions = new List<StateTransition>();
        }
        public void SetState(IState _state)
        {
            currentState?.Exit();
            currentState = _state;
            currentState.Enter();
        }
        public void Tick()
        {
            IState state = CheckState();

            if (state != null)
            {
                SetState(state);
            }
        }
        IState CheckState()
        {
            foreach (var stateTransition in anyTransitions)
            {
                if (stateTransition.Contition.Invoke())
                {
                    return stateTransition.To;
                }
            }
            foreach (var stateTransition in stateTransitions)
            {
                if (stateTransition.Contition.Invoke() && stateTransition.From == currentState)
                {
                    return stateTransition.To;
                }                    
            }
            return null;
        }
        public void SetStates(StateTransition _stateTransition)
        {
            stateTransitions.Add(_stateTransition);
        }
        public void SetAnyStates(StateTransition _stateTransition)
        {
            anyTransitions.Add(_stateTransition);
        }
    }
    public class StateTransition
    {
        public IState From { get;}
        public IState To { get;}
        public System.Func<bool> Contition { get; }

        public StateTransition(IState _from, IState _to, System.Func<bool> _condition)
        {
            From = _from;
            To = _to;
            Contition = _condition;
        }
    }
    public interface IState
    {
        void Enter();
        void Exit();
        void Tick();
    }

    //------------------- STATES ------------------------
    public class IdleState : IState
    {
        public void Enter()
        {
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
        {
            throw new System.NotImplementedException();
        }
    }
    public class WalkState : IState
    {
        public void Enter()
        {
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
        {
            throw new System.NotImplementedException();
        }
    }
    public class RunState : IState
    {
        public void Enter()
        {
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
        {
            throw new System.NotImplementedException();
        }
    }
    public class AttackState : IState
    {
        public void Enter()
        {
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
        {
            throw new System.NotImplementedException();
        }
    }
    public class DyingState : IState
    {
        public void Enter()
        {
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
        {
            throw new System.NotImplementedException();
        }
    }
}