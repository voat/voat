using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Voat;
using Voat.Configuration;

[assembly: OwinStartup(typeof(Startup))]
namespace Voat
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            if (!Settings.SignalRDisabled)
            {
                // wire up SignalR
                var config = new HubConfiguration()
                {
                    EnableDetailedErrors = false,
                    EnableJSONP = false,
                    EnableJavaScriptProxies = true
                };
                app.MapSignalR(config);
            }
        }
    }
}
