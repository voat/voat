using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Http;
using Voat.Logging;
using Voat.Utilities.Components;

namespace Voat.UI.Runtime
{
    public class RuntimeStateFilter : IAuthorizationFilter
    {

        public RuntimeStateFilter()
        {

        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var redirected = context.HttpContext.HandleRuntimeState();
        }
    }
}
