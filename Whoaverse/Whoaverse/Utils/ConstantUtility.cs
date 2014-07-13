using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whoaverse.Utils
{
    // Temporary fix
    public static class ConstantUtility
    {
        /// <summary>
        /// Simple email validation
        /// </summary>
        public const string EmailRegex = "^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\\.[a-zA-Z0-9-.]+$";

        /// <summary>
        /// Server Email Address to use when sending an email to a recipient.
        /// </summary>
        public const string ServerEmailAddress = "";

        /// <summary>
        /// Password for Email Credential
        /// </summary>
        public const string EmailPassword = "";

        /// <summary>
        /// Gateway for forwarding email to recipient.
        /// </summary>
        public const string SMTPGateWay = "smtp.gmail.com";

        /// <summary>
        /// Server's current DNS Address
        /// </summary>
        public const string HostName = "http://localhost";
    }
}