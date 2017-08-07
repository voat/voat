using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Common.Configuration
{
    public abstract class ConfigurationSettings<T> where T : class, new()
    {
        public static T Instance { get; set; }

        public static void Load(IConfigurationRoot config, string section, bool reloading = false)
        {
            T i = default(T);

            var s = config.GetSection(section);
            if (s != null)
            {
                i = s.Get<T>();
            }

            if (!reloading)
            {
                //set the instance
                Instance = i ?? throw new ArgumentException($"Can not load configuration: {section}");
            }
            else
            {
                //let
                if (Instance is IHandlesConfigurationUpdate<T> updateable)
                {
                    updateable.Update(i);
                }
            }

            //register callback if type supports it
            if (i is IHandlesConfigurationUpdate<T>)
            {
                var reloadToken = config.GetReloadToken();
                reloadToken.RegisterChangeCallback(x =>
                {
                    Load((IConfigurationRoot)x, section, true);
                }, config);
            }
        }
        
        public bool Enabled { get; set; } = true;
    }
}
