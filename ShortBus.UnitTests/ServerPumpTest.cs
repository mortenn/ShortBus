using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ShortBus.Contracts;
using ShortBus.Server;
using Moq;

namespace ShortBus.UnitTests
{


    /// <summary>
    ///This is a test class for ServerPumpTest and is intended
    ///to contain all ServerPumpTest Unit Tests
    ///</summary>
    [TestClass]
    public class ServerPumpTest
    {
        public ServerPumpTest()
        {
            //clientGuid = Guid.NewGuid();
            //clientEndpoint = ;
        }

        /// <summary>
        ///A test for ServerPump Constructor
        ///</summary>
        [TestMethod]
        public void ServerPumpConstructorTest()
        {
            ServerPump target = new ServerPump();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Heartbeat
        ///</summary>
        [TestMethod]
        public void HeartbeatTest()
        {
            ServerPump target = new ServerPump(); // TODO: Initialize to an appropriate value
            Guid clientId = new Guid(); // TODO: Initialize to an appropriate value
            target.Heartbeat(clientId);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Publish
        ///</summary>
        [TestMethod]
        public void PublishTest()
        {
            ServerPump target = new ServerPump(); // TODO: Initialize to an appropriate value
            ServiceBusEvent data = null; // TODO: Initialize to an appropriate value
            target.Publish(data);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendHeartBeat
        ///</summary>
        [TestMethod]
        [DeploymentItem("ShortBus.Server.dll")]
        public void SendHeartBeatTest()
        {
            ServerPump_Accessor target = new ServerPump_Accessor(); // TODO: Initialize to an appropriate value
            target.SendHeartBeat();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SendMessage
        ///</summary>
        [TestMethod]
        [DeploymentItem("ShortBus.Server.dll")]
        public void SendMessageTest()
        {
            ServerPump_Accessor target = new ServerPump_Accessor(); // TODO: Initialize to an appropriate value
            Subscriber subscription = null; // TODO: Initialize to an appropriate value
            ServiceBusEvent data = null; // TODO: Initialize to an appropriate value
            target.SendMessage(subscription, data);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Subscribe
        ///</summary>
        [TestMethod]
        public void Subscribe_adds_subscription_to_storage()
        {
            Mock<ISubscriberStore> subscribers = new Mock<ISubscriberStore>();
            Guid clientId = Guid.Empty;
            string clientEndpoint = GetFakeClientEndpoint(clientId);
            subscribers.Setup(foo => foo.AddSubscriber(clientId, clientEndpoint))
                .Returns(new Subscriber { LastSeen = DateTime.MinValue, Endpoint = clientEndpoint, SubscriberId = clientId })
                .Verifiable("Server pump did not persist subscriber");

            ServerPump target = new ServerPump();
            target.Storage = subscribers.Object;

            target.Subscribe(clientId, clientEndpoint);
            
            subscribers.Verify();
        }

        /// <summary>
        ///A test for Unsubscribe
        ///</summary>
        [TestMethod]
        public void Unsubscribe_removes_subscription_from_storage()
        {
            Mock<ISubscriberStore> subscribers = new Mock<ISubscriberStore>();
            Guid clientId = Guid.Empty;
            subscribers.Setup(foo => foo.RemoveSubscriber(clientId)).Verifiable();

            ServerPump target = new ServerPump();
            target.Storage = subscribers.Object;

            target.Unsubscribe(clientId);

            subscribers.Verify();
        }

        string GetFakeClientEndpoint(Guid clientId)
        {
            return string.Format("net.msmq://innsmouth/private/shortbus_{0}", clientId);
        }
    }
}
