using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Voat.Caching;

namespace Voat.Configuration
{
    public static class VoatConfiguration
    {
        public static void ConfigureVoat(this IConfigurationRoot config)
        {
            CacheConfigurationSettings.Load(config, "voat:cache");
            RulesEngine.RuleConfigurationSettings.Load(config, "voat:rules");
            Logging.LoggingConfigurationSettings.Load(config, "voat:logging");
            Data.DataConfigurationSettings.Load(config, "voat:data");

            //load web.config.live monitor
            LiveConfigurationManager.Reload(config.GetSection("voat:settings"));
            //CORE_PORT: Live Monitoring not ported
            //LiveConfigurationManager.Start();
        }
    }
}
