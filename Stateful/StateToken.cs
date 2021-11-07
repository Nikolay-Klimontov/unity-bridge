using System;

namespace Stateful
{
    public class StateToken : IComparable<StateToken>
    {
        public Guid UID { get; internal set; }
        
        public StateToken()
        {
            UID = Guid.NewGuid();
        }

        public int CompareTo(StateToken other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return UID.CompareTo(other.UID);
        }
    }
}