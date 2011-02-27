using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net.Security;
using System.ServiceModel;
using System.Messaging;

namespace ShortBus.Server
{
    public class Configuration : ConfigurationSection
    {
        private static Configuration config = ConfigurationManager.GetSection("ShortBus.Server.Configuration") as Configuration;

        public static Configuration Config
        {
            get
            {
                return config;
            }
        }

        /// <summary>
        /// The timeout on the heartbeat timer.
        /// </summary>
        [ConfigurationProperty("heartbeatTimeout", DefaultValue = 5000.0, IsRequired = false)]
        public double HeartbeatTimeout { get { return (double)this["heartbeatTimeout"]; } }

        /// <summary>
        /// How long to wait before requesting a heartbeat from each client
        /// </summary>
        [ConfigurationProperty("checkHeartbeatAfter", DefaultValue = "00:10:00", IsRequired = false)]
        public TimeSpan CheckHeartbeatAfter { get { return (TimeSpan)this["checkHeartbeatAfter"]; } }

        /// <summary>
        /// The name of the server queue
        /// </summary>
        [ConfigurationProperty("messageQueueName", DefaultValue = @".\private$\shortbus", IsRequired = false)]
        public string MessageQueueName { get { return (string)this["messageQueueName"]; } }

        /// <summary>
        /// What authentication mode to use for our queue
        /// </summary>
        [ConfigurationProperty("authenticationMode", DefaultValue = MsmqAuthenticationMode.None, IsRequired = false)]
        public MsmqAuthenticationMode AuthenticationMode { get { return (MsmqAuthenticationMode)this["authenticationMode"]; } }

        /// <summary>
        /// What protection level to use for our queue
        /// </summary>
        [ConfigurationProperty("protection", DefaultValue = ProtectionLevel.None, IsRequired = false)]
        public ProtectionLevel Protection { get { return (ProtectionLevel)this["protection"]; } }

        /// <summary>
        /// Whether we require the queue to be encrypted
        /// </summary>
        [ConfigurationProperty("encryption", DefaultValue = EncryptionRequired.Optional, IsRequired = false)]
        public EncryptionRequired Encryption { get { return (EncryptionRequired)this["encryption"]; } }
    }
}
