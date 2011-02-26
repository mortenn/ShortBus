using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;
using ShortBus.Contracts;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Diagnostics;
using System.Timers;

namespace ShortBus
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class ServerPump : IServiceBusServer
    {
        public ServerPump()
        {
            if (!MessageQueue.Exists(".\\private$\\shortbus"))
            {
                MessageQueue queue = MessageQueue.Create(".\\private$\\shortbus", true);
                queue.Authenticate = false;
                queue.EncryptionRequired = EncryptionRequired.Optional;
            }
        }

        public ISubscriberStore Storage { get; set; }

        public void Subscribe(Guid clientId, string endpoint)
        {
            Debug.WriteLine("{0} subscribed at {1}", clientId, endpoint);
            Storage.ClearEndpoint(endpoint);
            if (clientId != Guid.Empty)
                Storage.AddSubscriber(clientId, endpoint);
        }

        public void Unsubscribe(Guid clientId)
        {
            Debug.WriteLine("{0} unsubscribed");
            if (clientId != Guid.Empty)
                Storage.RemoveSubscriber(clientId);
        }

        public void Publish(ServiceBusEvent data)
        {
            Debug.WriteLine("{0} published an event {1} ({2})", data.SourceSubscriber, data.EventName, data.EventId);
            Task.Factory.StartNew(() => Storage.GetSubscribers().ForEach(sub => SendMessage(sub, data)));
            Storage.SubscriberSeen(data.SourceSubscriber);
        }

        public void Heartbeat(Guid clientId)
        {
            if (clientId == Guid.Empty)
                return;

            Debug.WriteLine("Received heartbeat from {0}", clientId);

            if (HeartbeatMonitorList.Contains(clientId))
                HeartbeatMonitorList.Remove(clientId);
            Storage.SubscriberSeen(clientId);
        }

        public void Start()
        {
            if (host == null)
                host = new ServiceHost(this);
            host.Open();
            pulse = new Timer(5000);
            pulse.Elapsed += (s, e) => SendHeartBeat();
            pulse.Start();
        }

        public void Stop()
        {
            host.Close();
            pulse.Dispose();
            pulse = null;
        }

        void SendMessage(Subscriber subscription, ServiceBusEvent data)
        {
            if (!clients.ContainsKey(subscription.SubscriberId))
                clients.Add(subscription.SubscriberId, new MSMQClientWrapper(subscription.Endpoint));
            try
            {
                clients[subscription.SubscriberId].Consume(data);
                Debug.WriteLine("Sent event {0} to client {1}", data.EventName, subscription.SubscriberId);
            }
            catch (EndpointNotFoundException)
            {
                Unsubscribe(subscription.SubscriberId);
            }
        }

        void SendHeartBeat()
        {
            lock (lockHandle)
            {
                foreach (Subscriber sub in Storage.GetUnseenSubscribersSince(DateTime.Now - new TimeSpan(0, 10, 0)))
                {
                    if (HeartbeatMonitorList.Contains(sub.SubscriberId))
                    {
                        Debug.WriteLine("Not feeling a pulse for {0}", sub.SubscriberId);

                        Storage.RemoveSubscriber(sub.SubscriberId);
                        HeartbeatMonitorList.Remove(sub.SubscriberId);
                        continue;
                    }
                    if (!clients.ContainsKey(sub.SubscriberId))
                        clients.Add(sub.SubscriberId, new MSMQClientWrapper(sub.Endpoint));
                    try
                    {
                        Debug.WriteLine("Checking {0} for a pulse", sub.SubscriberId);
                        clients[sub.SubscriberId].Heartbeat();
                        HeartbeatMonitorList.Add(sub.SubscriberId);
                    }
                    catch (EndpointNotFoundException)
                    {
                        Unsubscribe(sub.SubscriberId);
                    }
                }
            }
        }

        Timer pulse;
        Dictionary<Guid, IServiceBusClient> clients = new Dictionary<Guid, IServiceBusClient>();
        List<Guid> HeartbeatMonitorList = new List<Guid>();
        static object lockHandle = new object();
        ServiceHost host;
    }
}
