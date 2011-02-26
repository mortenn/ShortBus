using System;
using System.ServiceModel;

namespace ShortBus.Contracts
{
    [ServiceContract(Namespace = "http://shortbus.runsafe.no/contracts/2011/02/")]
    public interface IServiceBusServer
    {
        [OperationContract(IsOneWay=true)]
        void Subscribe(Guid clientId, string endpoint);

        [OperationContract(IsOneWay = true)]
        void Unsubscribe(Guid clientId);

        [OperationContract(IsOneWay = true)]
        void Publish(ServiceBusEvent data);

        [OperationContract(IsOneWay = true)]
        void Heartbeat(Guid clientId);
    }
}
