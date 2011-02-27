using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;
using System.ServiceModel;
using ShortBus.Contracts;

namespace ShortBus
{
    public class MSMQClientFactory : IFactory<IServiceBusClient>
    {
        public override IServiceBusClient Connect(string endpoint)
        {
            EndpointAddress endpointAddress = new EndpointAddress(new Uri(endpoint)); 
            NetMsmqBinding clientBinding = new NetMsmqBinding(); 
            clientBinding.Security.Transport.MsmqAuthenticationMode = MsmqAuthenticationMode.None; 
            clientBinding.Security.Transport.MsmqProtectionLevel = System.Net.Security.ProtectionLevel.None;
            clientBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            ChannelFactory<IServiceBusClient> channelFactory = new ChannelFactory<IServiceBusClient>(clientBinding, endpointAddress);
            return channelFactory.CreateChannel();
        }
    }
}
