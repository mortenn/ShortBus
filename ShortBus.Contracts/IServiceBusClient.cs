using System.ServiceModel;

namespace ShortBus.Contracts
{
    [ServiceContract(Namespace = "http://shortbus.runsafe.no/contracts/2011/02/")]
    public interface IServiceBusClient
    {
        [OperationContract(IsOneWay = true)]
        void Consume(ServiceBusEvent data);

        [OperationContract(IsOneWay = true)]
        void ConsumeResponse(ServiceBusEventResponse data);

        [OperationContract(IsOneWay = true)]
        void Heartbeat();
    }
}
