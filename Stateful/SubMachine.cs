using Messaging;

namespace Stateful
{
    public class SubMachine: StateMachine, IState
    {
        public HandleEvents HandleTransitionEvents { get; protected set; } = HandleEvents.No;
        public StateMachine Parent { get; set; }
        public virtual void OnEnter()
        {
            Start();
        }

        public void OnEvent<TEvent>(TEvent e) where TEvent : BasicEvent
        {
            Push(e);
        }

        public virtual void OnExit()
        {
            Stop();
            Reset();
        }
    }
}