using System;

namespace Game.Core.Messaging
{
    public interface IEventBus
    {
        SubscriptionToken Subscribe<TEvent>(Action<TEvent> action) where TEvent : BasicEvent;

        void Unsubscribe(SubscriptionToken subscription);

        void Publish<TEvent>(TEvent e) where TEvent : BasicEvent;
    }
}