using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Voat.Common;
using Voat.Common.Configuration;

namespace Voat.Http.Filters
{

    public interface ISpamTimeAdjustor
    {
        int GetDelay(HttpContext context);
    }

    public class PreventSpamAttribute : TypeFilterAttribute
    {
        public PreventSpamAttribute(
            int seconds = 15,
            string errorMessage = "Sorry, you are doing that too fast. Please try again in {0}.",
            Type adjustor = null
            ) 
            : base(typeof(PreventSpamFilter))
        {
            Arguments = new object[] { new PreventSpamOptions() { Secconds = seconds, ErrorMessage = errorMessage, Adjustor = adjustor } };
        }
        public class PreventSpamOptions
        {
            public Type Adjustor { get; set; } = null;
            public int Secconds { get; set; } = 15;
            public string ErrorMessage { get; set; } = "Excessive Request Attempts Detected";
        }

        public static void Reset(HttpContext context)
        {
            if (context != null)
            {
                var key = context.Items["PreventSpam"];
                if (key != null)
                {
                    var cache = (IMemoryCache)context.RequestServices.GetService(typeof(IMemoryCache));
                    if (cache != null)
                    {
                        Debug.WriteLine($"Prevent Spam Cleared: {key}");
                        cache.Remove(key);
                    }
                }
            }
        }

        private class PreventSpamFilter : IActionFilter
        {
            private IMemoryCache _cache;
            private PreventSpamOptions _options;

            public PreventSpamFilter(IMemoryCache cache, PreventSpamOptions options)
            {
                _cache = cache;
                _options = options;
            }

            public const string CONTEXT_CACHE_KEY = "PreventSpamHash";

            private bool trustedUser = false;
            public void OnActionExecuting(ActionExecutingContext context)
            {
                var loggedInUser = context.HttpContext.User.Identity.Name;

                // Store our HttpContext (for easier reference and code brevity)
                var request = context.HttpContext.Request;
                var key = request.Signature();
                var seconds = _options.Secconds;

                //Adjust if has adjustor 
                if (_options.Adjustor != null)
                {
                    var adjustor = HandlerInfo.Construct<ISpamTimeAdjustor>(_options.Adjustor, null);
                    seconds = adjustor.GetDelay(context.HttpContext);
                }

                // Checks if the hashed value is contained in the Cache (indicating a repeat request)
                if (_cache.Get(key) != null && loggedInUser != "system" && trustedUser != true)
                {
                    // Adds the Error Message to the Model and Redirect
                    ((Controller)context.Controller).ViewData.ModelState.AddModelError(string.Empty, String.Format(_options.ErrorMessage, Age.ToRelative(TimeSpan.FromSeconds(seconds))));
                    Debug.WriteLine($"Prevent Spam Triggered: {key}");
                }
                else
                {
                    // Adds an empty object to the cache using the hashValue to a key (This sets the expiration that will determine
                    // if the Request is valid or not
                    _cache.Set(key, "", TimeSpan.FromSeconds(seconds));
                    context.HttpContext.Items["PreventSpam"] = key;
                    Debug.WriteLine($"Prevent Spam Set: {seconds}s {key}");
                }
            }
            public void OnActionExecuted(ActionExecutedContext context)
            {
            }
        }
    }
}
