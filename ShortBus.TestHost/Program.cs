using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using ShortBus;

namespace ShortBus.TestHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerPump pump = new ServerPump();
            pump.Storage = new ShortBus.SQLStore.SubscriberStore();
            
            /*ServiceHost host = new ServiceHost(pump);
            Uri serviceUri = new Uri("net.msmq://localhost/private/shortbus"); 
            NetMsmqBinding serviceBinding = new NetMsmqBinding(); 
            serviceBinding.Security.Transport.MsmqAuthenticationMode = MsmqAuthenticationMode.None; 
            serviceBinding.Security.Transport.MsmqProtectionLevel = System.Net.Security.ProtectionLevel.None; 
            serviceBinding.MaxReceivedMessageSize = 100000; 
            serviceBinding.ReaderQuotas.MaxArrayLength = 500;
            host.AddServiceEndpoint(typeof(ShortBus.Contracts.IServiceBusServer), serviceBinding, serviceUri);
            host.Open();*/
            pump.Start();
            Console.Write("Hit a key to exit..");
            Console.ReadKey();
            pump.Stop();
            //host.Close();
        }
    }
}
