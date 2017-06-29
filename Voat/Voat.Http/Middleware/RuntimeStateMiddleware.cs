using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Voat.Http.Middleware
{
    public class RuntimeStateMiddleware : BaseMiddleware
    {
        public RuntimeStateMiddleware(RequestDelegate next) : base(next)
        {
        }

        public override async Task Invoke(HttpContext context)
        {
            var redirected = context.HandleRuntimeState();
            if (!redirected)
            {
                await base.Invoke(context);
            }
        }
    }
}
