using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;
using System.ServiceModel;
using ShortBus.Contracts;

namespace ShortBus
{
    /// <summary>
    /// A client should only ever have one of these
    /// </summary>
    public class MSMQServerFactory : IFactory<IServiceBusServer>
    {
        /// <summary>
        /// Creates a message queue pushing WCF client
        /// </summary>
        /// <param name="endpoint">net.msmq://server/endpoint</param>
        public override IServiceBusServer Connect(string endpoint)
        {
            EndpointAddress endpointAddress = new EndpointAddress(new Uri(endpoint));
            NetMsmqBinding clientBinding = new NetMsmqBinding();
            clientBinding.Security.Transport.MsmqAuthenticationMode = MsmqAuthenticationMode.None;
            clientBinding.Security.Transport.MsmqProtectionLevel = System.Net.Security.ProtectionLevel.None;
            clientBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            ChannelFactory<IServiceBusServer> channelFactory = new ChannelFactory<IServiceBusServer>(clientBinding, endpointAddress);
            return channelFactory.CreateChannel();
        }
    }
}
