using System;
using System.Collections.Generic;
using Messaging;

namespace Stateful
{
    public class StateMachine
    {
        #region NESTED

        public class StateConfig
        {
            public StateToken Token { get; }
            private StateMachine owner { get; }

            internal StateConfig(StateToken token, StateMachine owner)
            {
                Token = token;
                this.owner = owner;
            }

            public StateConfig AddTransition<TEvent>(StateToken to) where TEvent : BasicEvent
            {
                owner.AddTransition<TEvent>(Token, to);
                return this;
            }
        }

        #endregion

        protected Dictionary<StateToken, IState> states = new Dictionary<StateToken, IState>();
        private Dictionary<StateToken, Dictionary<Type, StateToken>> transitionMap = new Dictionary<StateToken, Dictionary<Type, StateToken>>();
        
        private StateToken start = null;
        protected StateToken current = null;

        public bool Active { get; private set; } = false;

        #region PUBLIC_INTERFACE

        public StateToken StartState
        {
            set => start = states.ContainsKey(value) ? value : null;
        }
        
        public StateMachine()
        {
            
        }

        public void Start()
        {
            if (Active || start == null) return;

            Active = true;
                
            if (current == null)
                current = start;

            states[current].OnEnter();
        }

        public void Stop()
        {
            if (!Active) return;
            
            Active = false;
            states[current].OnExit();
        }

        public void Reset()
        {
            if (Active) return;

            current = null;
        }
        
        public StateToken AddState(IState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException();
            }

            if (state.Parent != null)
            {
                throw new Exception("Attempting to add state that already registered somewhere else");
            }
            
            state.Parent = this;
            
            var token = new StateToken();
            states.Add(token, state);
            transitionMap.Add(token, new Dictionary<Type, StateToken>());
            return token;
        }

        public StateConfig Configure(StateToken token)
        {
            return new StateConfig(token, this);
        }

        public void AddTransition<TEvent>(StateToken from, StateToken to) where TEvent : BasicEvent
        {
            if (!states.ContainsKey(from) || !states.ContainsKey(to))
            {
                return;
            }
            
            transitionMap[from].Add(typeof(TEvent), to);
        }

        public void Push<TEvent>(TEvent e) where TEvent : BasicEvent
        {
            if (!Active) return;
            
            if (HasTransition<TEvent>())
            {
                var transitionPolitics = states[current].HandleTransitionEvents;
                if (transitionPolitics == HandleEvents.Exit || transitionPolitics == HandleEvents.Both)
                {
                    states[current].OnEvent<TEvent>(e);
                }
                
                SwitchState(transitionMap[current][typeof(TEvent)]);

                transitionPolitics = states[current].HandleTransitionEvents;
                if (transitionPolitics == HandleEvents.Enter || transitionPolitics == HandleEvents.Both)
                {
                    states[current].OnEvent<TEvent>(e);
                }
                return;
            }
            states[current].OnEvent<TEvent>(e);
        }
        
        #endregion

        #region PRIVATE

        bool HasTransition<TEvent>() where TEvent : BasicEvent
        {
            return transitionMap[current].ContainsKey(typeof(TEvent));
        }
        
        void SwitchState(StateToken destination)
        {
            states[current].OnExit();
            current = destination;
            states[destination].OnEnter();
        }

        #endregion
    }
}