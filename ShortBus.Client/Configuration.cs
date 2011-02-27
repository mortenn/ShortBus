using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.ServiceModel;
using System.Net.Security;
using System.Messaging;

namespace ShortBus.Client
{
    public class Configuration : ConfigurationSection
    {
        private static Configuration config = ConfigurationManager.GetSection("ShortBus.Client.Configuration") as Configuration;

        public static Configuration Config
        {
            get
            {
                return config;
            }
        }

        /// <summary>
        /// The configured server endpoint to subscribe to and send events to
        /// </summary>
        [ConfigurationProperty("serverEndpoint", DefaultValue = null, IsRequired = false)]
        public string ServerEndpoint { get { return (string)this["serverEndpoint"]; } }

        /// <summary>
        /// The format string for our listening interface, used by server to send messages
        /// </summary>
        [ConfigurationProperty("clientEndpointFormat", DefaultValue = "net.msmq://{0}/private/shortbus_{1}", IsRequired = false)]
        public string ClientEndpointFormat { get { return (string)this["clientEndpointFormat"]; } }

        /// <summary>
        /// The format string for our local MSMQ queue
        /// </summary>
        [ConfigurationProperty("clientQueueFormat", DefaultValue = @".\private$\shortbus_{0}", IsRequired = false)]
        public string ClientQueueFormat { get { return (string)this["clientQueueFormat"]; } }

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
