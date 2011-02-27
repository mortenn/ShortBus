using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using ShortBus.Server;

namespace ShortBus.TestHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerPump pump = new ServerPump
            {
                Storage = new ShortBus.SQLStore.SubscriberStore()
            };
            pump.Start();
            Console.Write("Hit a key to exit..");
            Console.ReadKey();
            pump.Stop();
        }
    }
}
