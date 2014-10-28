using Microsoft.Owin;
using Owin;
using Whoaverse;

[assembly: OwinStartup(typeof(Startup))]
namespace Whoaverse
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
