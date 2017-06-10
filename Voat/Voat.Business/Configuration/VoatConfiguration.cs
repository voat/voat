using Microsoft.Extensions.Configuration;
using System;

namespace Voat.Configuration
{
    public static class VoatConfiguration
    {
        public static void ConfigureVoat(this IConfigurationRoot config)
        {
            Caching.CacheConfigurationSettings.Load(config, "voat:cache");
            RulesEngine.RuleConfigurationSettings.Load(config, "voat:rules");
            Logging.LoggingConfigurationSettings.Load(config, "voat:logging");
            Data.DataConfigurationSettings.Load(config, "voat:data");
            VoatSettings.Load(config, "voat:settings");

            //load web.config.live monitor
            //LiveConfigurationManager.Reload(config.GetSection("voat:settings"));
            //CORE_PORT: Live Monitoring not ported
            //LiveConfigurationManager.Start();
        }
    }
}
