using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Common.Configuration
{
    public class ConfigurationSettings<T> where T : class, new()
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

            Instance = i ?? throw new ArgumentException($"Can not load configuration: {section}");
        }

        public bool Enabled { get; set; } = true;
    }
}
