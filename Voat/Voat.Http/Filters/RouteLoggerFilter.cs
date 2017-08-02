using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voat.Logging;
using Voat.Utilities.Components;

namespace Voat.Http.Filters
{
    public class RouteLoggerFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var logger = EventLogger.Instance;
            var logLevel = LogType.Debug;
            if (logger.IsEnabledFor(logLevel))
            {
                var logInfo = context.HttpContext.GetLogInformation("RouteLoggerFilter", logLevel);
                var data = logInfo.Data;
                logInfo.Data = new {
                    routeValues = context.RouteData.Values,
                    data = data
                };
                logger.Log(logInfo);
            }
        }
    }
}
