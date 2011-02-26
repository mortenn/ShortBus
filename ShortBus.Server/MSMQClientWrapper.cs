using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;
using System.ServiceModel;
using ShortBus.Contracts;

namespace ShortBus
{
    public class MSMQClientWrapper : IServiceBusClient
    {
        public MSMQClientWrapper(string endpoint)
        {
            EndpointAddress endpointAddress = new EndpointAddress(new Uri(endpoint)); 
            NetMsmqBinding clientBinding = new NetMsmqBinding(); 
            clientBinding.Security.Transport.MsmqAuthenticationMode = MsmqAuthenticationMode.None; 
            clientBinding.Security.Transport.MsmqProtectionLevel = System.Net.Security.ProtectionLevel.None;
            clientBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            ChannelFactory<IServiceBusClient> channelFactory = new ChannelFactory<IServiceBusClient>(clientBinding, endpointAddress); 
            channel = channelFactory.CreateChannel();
        }

        public void Consume(ServiceBusEvent data)
        {
            channel.Consume(data);
        }

        public void ConsumeResponse(ServiceBusEventResponse data)
        {
            channel.ConsumeResponse(data);
        }

        public void Heartbeat()
        {
            channel.Heartbeat();
        }

        IServiceBusClient channel;
    }
}
