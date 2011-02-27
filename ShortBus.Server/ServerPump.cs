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

namespace ShortBus.Server
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class ServerPump : IServiceBusServer
    {
        /// <summary>
        /// Sets up the message pump with heartbeat after 10 minutes with a 5 second timeout, on the private queue shortbus
        /// If you change the queue naming in app.config, you must provide a new value to MessageQueueName
        /// </summary>
        public ServerPump()
        {
            ClientFactory = new MSMQClientFactory();
            HeartbeatTimeout = Configuration.Config.HeartbeatTimeout;
            CheckHeartbeatAfter = Configuration.Config.CheckHeartbeatAfter;
            MessageQueueName = Configuration.Config.MessageQueueName;
        }

        /// <summary>
        /// A storage object that holds subscriptions. No default value
        /// </summary>
        public ISubscriberStore Storage { get; set; }

        /// <summary>
        /// A factory object that creates proxies for communicating with the clients. Defaults to a MSMQClientFactory
        /// </summary>
        public IFactory<IServiceBusClient> ClientFactory { get; set; }

        /// <summary>
        /// Wait this many milliseconds before removing a non-responsive subscriber. Defaults to 5 seconds.
        /// </summary>
        public double HeartbeatTimeout { get; set; }

        /// <summary>
        /// Send heartbeat requests to client after they have gone unseen for this amount of time. Defaults to 10 minutes.
        /// </summary>
        public TimeSpan CheckHeartbeatAfter { get; set; }

        /// <summary>
        /// The local message queue to get messages from. Defaults to ".\private$\shortbus".
        /// </summary>
        public string MessageQueueName { get; set; }

        /// <summary>
        /// Interface method used by clients
        /// </summary>
        /// <param name="clientId">Calling clients' ID</param>
        /// <param name="endpoint">Calling clients' listening endpoint</param>
        public void Subscribe(Guid clientId, string endpoint)
        {
            Debug.WriteLine("{0} subscribed at {1}", clientId, endpoint);
            if (clientId != Guid.Empty)
                Storage.AddSubscriber(clientId, endpoint);
        }

        /// <summary>
        /// Interface method used by clients
        /// </summary>
        /// <param name="clientId">Calling clients' ID</param>
        public void Unsubscribe(Guid clientId)
        {
            Debug.WriteLine("{0} unsubscribed");
            if (clientId != Guid.Empty)
                Storage.RemoveSubscriber(clientId);
        }

        /// <summary>
        /// Interface method used by clients
        /// </summary>
        /// <param name="data">A message to broadcast</param>
        public void Publish(ServiceBusEvent data)
        {
            Debug.WriteLine("{0} published an event {1} ({2})", data.SourceSubscriber, data.EventName, data.EventId);
            Task.Factory.StartNew(() => Storage.GetSubscribers().ForEach(sub => SendMessage(sub, data)));
            Storage.SubscriberSeen(data.SourceSubscriber);
        }

        /// <summary>
        /// Interface method used by clients
        /// </summary>
        /// <param name="clientId">Calling clients' ID</param>
        public void Heartbeat(Guid clientId)
        {
            if (clientId == Guid.Empty)
                return;

            Debug.WriteLine("Received heartbeat from {0}", clientId);

            if (HeartbeatMonitorList.Contains(clientId))
                HeartbeatMonitorList.Remove(clientId);
            Storage.SubscriberSeen(clientId);
        }

        /// <summary>
        /// Start the message pump at the endpoint configured in app.config
        /// </summary>
        public void Start()
        {
            if (!MessageQueue.Exists(MessageQueueName))
            {
                MessageQueue queue = MessageQueue.Create(MessageQueueName, true);
                queue.Authenticate = false;
                queue.EncryptionRequired = EncryptionRequired.Optional;
            }
            if (host == null)
                host = new ServiceHost(this);
            host.Open();

            pulse = new Timer(HeartbeatTimeout);
            pulse.Elapsed += (s, e) => SendHeartBeat();
            pulse.Start();
        }

        /// <summary>
        /// Stop the message pump
        /// </summary>
        public void Stop()
        {
            host.Close();
            pulse.Dispose();
            pulse = null;
        }

        /// <summary>
        /// Send a message to a subscription
        /// </summary>
        /// <param name="subscription">The subscription the message is destined for</param>
        /// <param name="data">The message to send to the subscriber</param>
        void SendMessage(Subscriber subscription, ServiceBusEvent data)
        {
            if (!clients.ContainsKey(subscription.SubscriberId))
                clients.Add(subscription.SubscriberId, ClientFactory.Connect(subscription.Endpoint));
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

        /// <summary>
        /// Send heartbeat messages to clients we haven't seen in a while
        /// </summary>
        void SendHeartBeat()
        {
            lock (lockHandle)
            {
                foreach (Subscriber sub in Storage.GetUnseenSubscribersSince(DateTime.Now - CheckHeartbeatAfter))
                {
                    if (HeartbeatMonitorList.Contains(sub.SubscriberId))
                        SubscriberTimedOut(sub);

                    else
                        SendHeartBeat(sub);
                }
            }
        }

        /// <summary>
        /// Sends a heartbeat message to a given subscription
        /// </summary>
        /// <param name="sub">The subscription to ping</param>
        void SendHeartBeat(Subscriber sub)
        {
            if (!clients.ContainsKey(sub.SubscriberId))
                clients.Add(sub.SubscriberId, ClientFactory.Connect(sub.Endpoint));
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

        /// <summary>
        /// Removes a subscriber that didn't respond to a heartbeat request
        /// </summary>
        /// <param name="sub"></param>
        void SubscriberTimedOut(Subscriber sub)
        {
            Debug.WriteLine("Not feeling a pulse for {0}", sub.SubscriberId);

            Storage.RemoveSubscriber(sub.SubscriberId);
            HeartbeatMonitorList.Remove(sub.SubscriberId);
        }

        ServiceHost host;
        Timer pulse;
        Dictionary<Guid, IServiceBusClient> clients = new Dictionary<Guid, IServiceBusClient>();
        List<Guid> HeartbeatMonitorList = new List<Guid>();
        static object lockHandle = new object();
    }
}
