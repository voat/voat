using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Whoaverse.Startup))]
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
