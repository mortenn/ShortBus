using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShortBus.SQLStore
{
    public class SubscriberStore : ShortBus.Contracts.ISubscriberStore
    {
        public ShortBus.Contracts.Subscriber GetSubscriber(Guid id)
        {
            using (ServiceBusModelContainer entities = new ServiceBusModelContainer())
            {
                return DBOtoModel(entities.Subscribers.FirstOrDefault(sub => sub.subscriber == id));
            }
        }

        public List<ShortBus.Contracts.Subscriber> GetSubscribers()
        {
            using (ServiceBusModelContainer entities = new ServiceBusModelContainer())
            {
                return entities.Subscribers.ToList().ConvertAll<ShortBus.Contracts.Subscriber>(DBOtoModel);
            }
        }

        public ShortBus.Contracts.Subscriber AddSubscriber(Guid id, string endpoint)
        {
            using (ServiceBusModelContainer entities = new ServiceBusModelContainer())
            {
                Subscriber sub = entities.Subscribers.CreateObject();
                sub.subscriber = id;
                sub.endpoint = endpoint;
                sub.last_seen = DateTime.Now;
                entities.Subscribers.AddObject(sub);
                if (entities.SaveChanges() > 0)
                    return DBOtoModel(sub);

                return null;
            }
        }

        public void RemoveSubscriber(Guid id)
        {
            using (ServiceBusModelContainer entities = new ServiceBusModelContainer())
            {
                Subscriber subscription = entities.Subscribers.FirstOrDefault(sub => sub.subscriber == id);
                if (subscription != null)
                {
                    entities.DeleteObject(subscription);
                    entities.SaveChanges();
                }
            }
        }

        public void SubscriberSeen(Guid id)
        {
            using (ServiceBusModelContainer entities = new ServiceBusModelContainer())
            {
                (from sub in entities.Subscribers
                 where sub.subscriber == id
                 select sub
                ).ToList().ForEach(sub => sub.last_seen = DateTime.Now);
                entities.SaveChanges();
            }
        }

        public List<ShortBus.Contracts.Subscriber> GetUnseenSubscribersSince(DateTime limit)
        {
            using (ServiceBusModelContainer entities = new ServiceBusModelContainer())
            {
                return (from sub in entities.Subscribers
                        where sub.last_seen < limit
                        select sub
                       ).ToList().ConvertAll<ShortBus.Contracts.Subscriber>(DBOtoModel);
            }
        }

        public void ClearEndpoint(string endpoint)
        {
            using (ServiceBusModelContainer entities = new ServiceBusModelContainer())
            {
                foreach (Subscriber sub in entities.Subscribers.Where(sub => sub.endpoint == endpoint))
                    entities.DeleteObject(sub);
                entities.SaveChanges();
            }
        }

        private ShortBus.Contracts.Subscriber DBOtoModel(Subscriber sub)
        {
            return new ShortBus.Contracts.Subscriber
            {
                SubscriberId = sub.subscriber,
                Endpoint = sub.endpoint,
                LastSeen = sub.last_seen
            };
        }

        private Subscriber ModelToDBO(ServiceBusModelContainer entities, ShortBus.Contracts.Subscriber sub)
        {
            Subscriber dbo = entities.Subscribers.FirstOrDefault(s => s.subscriber == sub.SubscriberId);
            if (dbo == null)
            {
                dbo = entities.Subscribers.CreateObject();
                dbo.subscriber = sub.SubscriberId;
                entities.Subscribers.AddObject(dbo);
            }
            dbo.endpoint = sub.Endpoint;
            dbo.last_seen = sub.LastSeen;
            return dbo;
        }
    }
}
