using System.Runtime.Serialization;
using System;

namespace ShortBus.Contracts
{
    [DataContract(Namespace = "http://shortbus.runsafe.no/schemas/2011/02/")]
    public class ServiceBusEventResponse
    {
        [DataMember]
        public Guid InResponseTo { get; set; }

        [DataMember]
        public Guid SourceSubscriber { get; set; }

        [DataMember]
        public string PayLoad { get; set; }
    }
}
