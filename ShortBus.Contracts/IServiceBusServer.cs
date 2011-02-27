using System;
using System.ServiceModel;

namespace ShortBus.Contracts
{
    [ServiceContract(Namespace = "http://shortbus.runsafe.no/contracts/2011/02/")]
    public interface IServiceBusServer
    {
        /// <summary>
        /// Subscribe a client to this server
        /// </summary>
        /// <param name="clientId">The clients' unique identifier</param>
        /// <param name="endpoint">The endpoint the client expects messages on</param>
        [OperationContract(IsOneWay=true)]
        void Subscribe(Guid clientId, string endpoint);

        /// <summary>
        /// Remove a client subscription from this server
        /// </summary>
        /// <param name="clientId">The clients' unique identifier</param>
        [OperationContract(IsOneWay = true)]
        void Unsubscribe(Guid clientId);

        /// <summary>
        /// Send a message to all clients
        /// </summary>
        /// <param name="data">The message to send</param>
        [OperationContract(IsOneWay = true)]
        void Publish(ServiceBusEvent data);

        /// <summary>
        /// Respond to a heartbeat ping
        /// </summary>
        /// <param name="clientId">The clients' unique identifier</param>
        [OperationContract(IsOneWay = true)]
        void Heartbeat(Guid clientId);
    }
}
