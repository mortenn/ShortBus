using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;
using System.ServiceModel;
using ShortBus.Contracts;

namespace ShortBus
{
    public class MSMQServerWrapper : IServiceBusServer
    {
        /// <summary>
        /// Creates a message queue pushing WCF client
        /// </summary>
        /// <param name="endpoint">net.msmq://server/endpoint</param>
        public MSMQServerWrapper(string endpoint)
        {
            EndpointAddress endpointAddress = new EndpointAddress(new Uri(endpoint)); 
            NetMsmqBinding clientBinding = new NetMsmqBinding(); 
            clientBinding.Security.Transport.MsmqAuthenticationMode = MsmqAuthenticationMode.None; 
            clientBinding.Security.Transport.MsmqProtectionLevel = System.Net.Security.ProtectionLevel.None;
            clientBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            ChannelFactory<IServiceBusServer> channelFactory = new ChannelFactory<IServiceBusServer>(clientBinding, endpointAddress); 
            channel = channelFactory.CreateChannel();
        }

        public void Subscribe(Guid clientId, string endpoint)
        {
            channel.Subscribe(clientId, endpoint);
        }

        public void Unsubscribe(Guid clientId)
        {
            channel.Unsubscribe(clientId);
        }

        public void Publish(ServiceBusEvent data)
        {
            channel.Publish(data);
        }

        public void Heartbeat(Guid clientId)
        {
            channel.Heartbeat(clientId);
        }

        IServiceBusServer channel;
    }
}
