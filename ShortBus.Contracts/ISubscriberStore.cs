using System;
namespace ShortBus.Contracts
{
    public interface ISubscriberStore
    {
        Subscriber AddSubscriber(Guid id, string endpoint);
        void ClearEndpoint(string endpoint);
        Subscriber GetSubscriber(Guid id);
        System.Collections.Generic.List<Subscriber> GetSubscribers();
        System.Collections.Generic.List<Subscriber> GetUnseenSubscribersSince(DateTime limit);
        void RemoveSubscriber(Guid id);
        void SubscriberSeen(Guid id);
    }
}
