using Microsoft.Extensions.Configuration;
using System;

namespace Voat.Configuration
{
    public static class VoatConfiguration
    {
        public static void ConfigureVoat(this IConfigurationRoot config)
        {
            ConfigureVoat(config, false);
        }
        private static void ConfigureVoat(IConfigurationRoot config, bool reloading)
        {

            Caching.CacheConfigurationSettings.Load(config, "voat:cache", reloading);
            RulesEngine.RuleConfigurationSettings.Load(config, "voat:rules", reloading);
            Logging.LoggingConfigurationSettings.Load(config, "voat:logging", reloading);
            Data.DataConfigurationSettings.Load(config, "voat:data", reloading);
            VoatSettings.Load(config, "voat:settings", reloading);
            IO.FileManagerConfigurationSettings.Load(config, "voat:fileManager", reloading);

            //Register Change Callback - I LOVE .NET CORE BTW
            //Update: This seems to fire twice which is a bit weird, I sure hope future people will figure this out.
            var reloadToken = config.GetReloadToken();
            reloadToken.RegisterChangeCallback(x =>
            {
                ConfigureVoat((IConfigurationRoot)x, true);
            }, config);
        }
    }
}
