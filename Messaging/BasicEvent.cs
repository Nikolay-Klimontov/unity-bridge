using System;

namespace Paper.Core.Messaging
{
    public class BasicEvent
    {
        public Guid ID { get; internal set; } 
        public object Sender { get; internal set; } = null;

        public BasicEvent(object sender)
        {
            ID = Guid.NewGuid();
            Sender = sender;
        }
    }

    public class DataEvent<TPayload> : BasicEvent
    {
        public TPayload Payload { get; internal set; }
        public DataEvent(object sender, TPayload payload) : base(sender)
        {
            Payload = payload;
        }
    }
}