﻿using Microsoft.Owin;
using Owin;
using Voat;

[assembly: OwinStartup(typeof(Startup))]
namespace Voat
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            if (!MvcApplication.SignalRDisabled)
            {
                // wire up SignalR
                app.MapSignalR();
            }
        }
    }
}
