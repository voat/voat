using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Voat.Data;
using Voat.Utilities;
using Voat.Common;

namespace Voat.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
               .UseKestrel()
               .UseContentRoot(Directory.GetCurrentDirectory())
               .UseIISIntegration()
               .UseStartup<Startup>()
               //.UseApplicationInsights()
               //.ConfigureLogging((context, logging) => {
               //    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
               //})
               .Build();

            host.Run();
        }
    }
}
