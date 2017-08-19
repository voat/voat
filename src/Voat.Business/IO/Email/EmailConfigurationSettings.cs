using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voat.Common.Configuration;

namespace Voat.IO.Email
{
    public class EmailConfigurationSettings : UpdatableConfigurationSettings<EmailConfigurationSettings>
    {
        public HandlerInfo Handler
        {
            get
            {
                if (Handlers != null)
                {
                    return Handlers.FirstOrDefault(x => x.Enabled);
                }
                return null;
            }
        }

        public HandlerInfo[] Handlers
        {
            get;
            set;
        }
    }
}
