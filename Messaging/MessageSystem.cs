using System;

namespace Game.Core.Messaging
{
    public class MessageSystem
    {
        //TODO: Simplify access
        public EventBus Bus { get; } = new EventBus();
        public EventBus UnityBridge { get; } = new EventBus();
        private static readonly Lazy<MessageSystem> instance = new Lazy<MessageSystem>(() => new MessageSystem());
        public static MessageSystem GetInstance()
        {
            return instance.Value;
        }
        //TODO: Add mailbox by GUID
        //TODO: public static void RegisterMailBox(GUID subscriber)
        //TODO: public static bool SendTo<TEvent>(GUID target, TEvent e);
    }
}