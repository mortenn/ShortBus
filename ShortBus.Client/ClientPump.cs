using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShortBus.Contracts;
using System.Messaging;
using System.ServiceModel;
using System.Threading;

namespace ShortBus.Client
{
    /// <summary>
    /// ShortBus client service.
    /// Sets up a local MSMQ queue and subscribes to events from the server.
    /// To use this, supply a serverEndpoint, set the Consumer property and then call Start().
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class ClientPump : IServiceBusClient
    {
        /// <summary>
        /// Connect to the server specified app.config using a new client ID
        /// </summary>
        public ClientPump()
            : this(Configuration.Config.ServerEndpoint, Guid.NewGuid())
        {
        }

        /// <summary>
        /// Connect to the given server endpoint and subscribe to events using a new client ID
        /// </summary>
        /// <param name="serverEndpoint"></param>
        public ClientPump(string serverEndpoint)
            : this(serverEndpoint, Guid.NewGuid())
        {
        }

        /// <summary>
        /// Connect to the given server endpoint and subscribe to events using the given client ID
        /// </summary>
        /// <param name="serverEndpoint">net.msmq://servername/private/shortbus</param>
        /// <param name="clientId">The Guid to use for this client</param>
        public ClientPump(string serverEndpoint, Guid clientId)
        {
            myId = clientId;
            Endpoint = new Uri(string.Format(Configuration.Config.ClientEndpointFormat, System.Net.Dns.GetHostName(), myId));
            QueueName = string.Format(Configuration.Config.ClientQueueFormat, myId);

            server = new MSMQServerFactory().Connect(serverEndpoint);
            if (!MessageQueue.Exists(QueueName))
            {
                MessageQueue queue = MessageQueue.Create(QueueName, true);
                queue.Authenticate = Configuration.Config.AuthenticationMode != MsmqAuthenticationMode.None;
                queue.EncryptionRequired = Configuration.Config.Encryption;
            }
        }

        ~ClientPump()
        {
            if (!string.IsNullOrEmpty(QueueName) && MessageQueue.Exists(QueueName))
                MessageQueue.Delete(QueueName);
        }

        /// <summary>
        /// The name of the local MSMQ queue
        /// </summary>
        public string QueueName
        {
            get;
            set;
        }

        /// <summary>
        /// The WCF endpoint address for the local MSMQ queue
        /// </summary>
        public Uri Endpoint
        {
            get;
            set;
        }

        /// <summary>
        /// Set this to a callback that will handle incoming events
        /// </summary>
        public Action<ServiceBusEvent> Consumer
        {
            get;
            set;
        }

        /// <summary>
        /// Entry point for subscription events from the server
        /// </summary>
        /// <param name="data">Event data</param>
        public void Consume(ServiceBusEvent data)
        {
            if (Consumer != null)
                Consumer(data);
        }

        /// <summary>
        /// Set this to a callback that will handle incoming responses to messages you sent using this client pump
        /// </summary>
        public Action<ServiceBusEventResponse> ResponseConsumer
        {
            get;
            set;
        }

        /// <summary>
        /// Entry point for response events from other clients
        /// </summary>
        /// <param name="data">Event response data</param>
        public void ConsumeResponse(ServiceBusEventResponse data)
        {
            if (ResponseConsumer != null)
                ResponseConsumer(data);
        }

        /// <summary>
        /// Method for the server to ping this client
        /// </summary>
        public void Heartbeat()
        {
            server.Heartbeat(myId);
        }

        /// <summary>
        /// Send a Test event with the given payload
        /// </summary>
        /// <param name="payload">Payload for the test event</param>
        public void Broadcast(string payload)
        {
            server.Publish(new ServiceBusEvent
            {
                EventId = Guid.NewGuid(),
                EventName = "Test",
                MessageSent = DateTime.Now,
                Payload = payload,
                Sender = Environment.UserName,
                SourceSubscriber = myId
            });
        }

        /// <summary>
        /// Broadcast an event
        /// </summary>
        /// <param name="data">Event data. SourceSubscriber and MessageSent will be set automatically.</param>
        public void Broadcast(ServiceBusEvent data)
        {
            data.SourceSubscriber = myId;
            data.MessageSent = DateTime.Now;
            server.Publish(data);
        }

        /// <summary>
        /// Subscribe to events from the server and open the local queue.
        /// </summary>
        public void Start()
        {
            if (host == null)
            {
                host = new ServiceHost(this);
                NetMsmqBinding serviceBinding = new NetMsmqBinding();
                serviceBinding.Security.Transport.MsmqAuthenticationMode = Configuration.Config.AuthenticationMode;
                serviceBinding.Security.Transport.MsmqProtectionLevel = Configuration.Config.Protection;
                serviceBinding.MaxReceivedMessageSize = 100000;
                serviceBinding.ReaderQuotas.MaxArrayLength = 500;
                host.AddServiceEndpoint(typeof(IServiceBusClient), serviceBinding, Endpoint);
            }
            host.Open();
            server.Subscribe(myId, Endpoint.AbsoluteUri);
        }

        /// <summary>
        /// Close the local queue and unsubscribe from events on the server.
        /// </summary>
        public void Stop()
        {
            server.Unsubscribe(myId);
            host.Close();
        }

        ServiceHost host;
        Guid myId;
        IServiceBusServer server;
    }
}
