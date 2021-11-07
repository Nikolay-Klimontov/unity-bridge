using System;

namespace Game.Core.Messaging
{
    public class SubscriptionToken
    {
        public Guid UID { get; internal set; }
        public Type EventType { get; internal set; }

        protected IEventBus Parent = null;
        
        public SubscriptionToken(Type eventType, IEventBus parent)
        {
            UID = Guid.NewGuid();
            EventType = eventType;
            Parent = parent;
        }

        ~SubscriptionToken()
        {
            Parent.Unsubscribe(this);
        }
    }
}