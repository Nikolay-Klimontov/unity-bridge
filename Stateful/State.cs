using Messaging;

namespace Stateful
{
    public class State : IState
    {
        public HandleEvents HandleTransitionEvents { get; protected set; } = HandleEvents.No;
        public StateMachine Parent { get; set; } = null;
        public virtual void OnEnter()
        {
        }

        public virtual void OnEvent<TEvent>(TEvent e) where TEvent : BasicEvent
        {
           
        }

        public virtual void OnExit()
        {
          
        }
    }
}