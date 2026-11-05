using System;
using System.Collections.Generic;
using UnityArchitectureShowcase.UI;
using UnityEngine;

namespace UnityArchitectureShowcase
{
    public class MainMono : MonoBehaviour
    {
        public static MainMono instance { get; private set; } = null;

        public Action OnQuit;
        public Action OnUpdate;

        private Queue<Action> updateQueue = new Queue<Action>();

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);
        }

        private void Start()
        {
            StateMachine.instance.ChangeState(nameof(State.InitialState));
        }

        private void Update()
        {
            OnUpdate?.Invoke();

            if (updateQueue.Count > 0)
                updateQueue.Dequeue()?.Invoke();
        }
    }
}