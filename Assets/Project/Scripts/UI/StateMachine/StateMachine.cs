using System.Collections.Generic;
using UnityEngine;

namespace UnityArchitectureShowcase.UI
{
    public enum State
    {
        InitialState,
        HomeState
    }

    public class StateMachine
    {
        public static StateMachine instance { get; } = new StateMachine();

        private IState currentState;
        private Dictionary<string, IState> states;

        public StateMachine()
        {
            states = new Dictionary<string, IState>
            {
                { nameof(State.InitialState), new InitialState() },
                { nameof(State.HomeState), new HomeState() }
            };
        }

        public void ChangeState(string stateName)
        {
            if (!states.TryGetValue(stateName, out IState nextState))
            {
                Debug.LogError($"{stateName} not found");
                return;
            }

            if (currentState != null)
                currentState.Dispose();

            currentState = nextState;
            currentState.Setup();
        }
    }
}