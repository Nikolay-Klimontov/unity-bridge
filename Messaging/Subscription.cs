using System;

namespace Game.Core.Messaging
{
    internal class Subscription<TEvent> : ISubscription
    where TEvent : BasicEvent
    {
        public SubscriptionToken Token { get; internal set; }

        private Action<TEvent> action = null;
        public bool Active { get; private set; } = true;

        public Subscription(Action<TEvent> action, IEventBus parent)
        {
            Token = new SubscriptionToken(typeof(TEvent), parent);

            this.action = action;
        }

        public void Invalidate()
        {
            Active = false;
        }

        public void Publish(BasicEvent e)
        {
            if(!Active) 
                return;
            
            switch (e)
            {
                case null:
                    return;
                
                case TEvent @event:
                    action.Invoke(@event);
                    break;
            }
        }
    }
}