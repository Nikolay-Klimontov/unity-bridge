using Messaging;

namespace Stateful
{
    public enum HandleEvents
    {
        No = 0x0,
        Enter = 0x1,
        Exit = 0x2,
        Both = 0x3
    };
    public interface IState
    {
        HandleEvents HandleTransitionEvents { get; }
        StateMachine Parent { get; set; }
        void OnEnter();
        void OnEvent<TEvent>(TEvent e) where TEvent : BasicEvent;
        void OnExit();
    }
}