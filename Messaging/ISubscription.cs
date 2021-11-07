namespace Game.Core.Messaging
{
    internal interface ISubscription
    {
        SubscriptionToken Token { get; }
        bool Active { get; }
        void Invalidate();
        void Publish(BasicEvent e);
    }
}