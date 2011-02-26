using System;
using System.Collections.Generic;

namespace ShortBus.Contracts
{
    public class Subscriber
    {
        public Guid SubscriberId
        {
            get;
            set;
        }

        public string Endpoint
        {
            get;
            set;
        }

        public DateTime LastSeen
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            if (obj is Subscriber)
                return SubscriberId.Equals((obj as Subscriber).SubscriberId);
            return SubscriberId.Equals(obj);
        }

        public override int GetHashCode()
        {
            return SubscriberId.GetHashCode();
        }

        public override string ToString()
        {
            return SubscriberId.ToString();
        }
    }
}
