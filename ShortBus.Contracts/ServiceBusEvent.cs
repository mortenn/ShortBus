using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ShortBus.Contracts
{
    [Serializable]
    [DataContract(Namespace = "http://shortbus.runsafe.no/schemas/2011/02/")]
    public class ServiceBusEvent
    {
        [DataMember]
        public Guid EventId { get; set; }

        [DataMember]
        public Guid SourceSubscriber { get; set; }

        [DataMember]
        public DateTime MessageSent { get; set; }

        [DataMember]
        public string EventName { get; set; }

        [DataMember]
        public string Payload { get; set; }

        [DataMember]
        public string Sender { get; set; }
    }
}
