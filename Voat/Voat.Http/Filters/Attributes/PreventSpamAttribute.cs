using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Voat.Http.Filters
{
    public class PreventSpamAttribute : TypeFilterAttribute
    {
        
        public PreventSpamAttribute(int seconds = 15, string errorMessage = "Excessive Request Attempts Detected") : base(typeof(PreventSpamFilter))
        {
            Arguments = new object[] { new PreventSpamOptions() { Secconds = seconds, ErrorMessage = errorMessage } };
        }
        public class PreventSpamOptions
        {
            public int Secconds { get; set; } = 15;
            public string ErrorMessage { get; set; } = "Excessive Request Attempts Detected";
        }

        public static void Reset(HttpContext context)
        {
            if (context != null)
            {
                var cache = (IMemoryCache)context.RequestServices.GetService(typeof(IMemoryCache));
                if (cache != null)
                {
                    var key = context.Request.Signature();
                    cache.Remove(key);
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

                //// user is submitting a message
                //if (filterContext.ActionParameters.ContainsKey("message"))
                //{
                //    var incomingMessage = (Submission)filterContext.ActionParameters["message"];
                //    var targetSubverse = incomingMessage.Subverse;
                //    // check user LCP for target subverse
                //    if (targetSubverse != null)
                //    {
                //        var LCPForSubverse = Karma.LinkKarmaForSubverse(loggedInUser, targetSubverse);
                //        if (LCPForSubverse >= 40)
                //        {
                //            // lower DelayRequest time
                //            DelayRequest = 10;
                //        }
                //        else if (ModeratorPermission.IsModerator(loggedInUser, targetSubverse))
                //        {
                //            // lower DelayRequest time
                //            DelayRequest = 10;
                //        }
                //    }
                //}
                //// user is submitting a comment
                //else if (filterContext.ActionParameters.ContainsKey("comment"))
                //{
                //    Comment incomingComment = (Comment)filterContext.ActionParameters["comment"];

                //    using (voatEntities db = new VoatUIDataContextAccessor())
                //    {
                //        var relatedMessage = db.Submissions.Find(incomingComment.SubmissionID);
                //        if (relatedMessage != null)
                //        {
                //            var targetSubverseName = relatedMessage.Subverse;

                //            // check user CCP for target subverse
                //            int CCPForSubverse = Karma.CommentKarmaForSubverse(loggedInUser, targetSubverseName);
                //            if (CCPForSubverse >= 40)
                //            {
                //                // lower DelayRequest time
                //                DelayRequest = 10;
                //            }
                //            else if (ModeratorPermission.IsModerator(loggedInUser, targetSubverseName))
                //            {
                //                // lower DelayRequest time
                //                DelayRequest = 10;
                //            }
                //        }
                //    }
                //}

                // Store our HttpContext (for easier reference and code brevity)
                var request = context.HttpContext.Request;
                var key = request.Signature();

                // TODO:
                // Override spam filter if user is authorized poster to target subverse
                // trustedUser = true;

                // Checks if the hashed value is contained in the Cache (indicating a repeat request)
                if (_cache.Get(key) != null && loggedInUser != "system" && trustedUser != true)
                {
                    // Adds the Error Message to the Model and Redirect
                    ((Controller)context.Controller).ViewData.ModelState.AddModelError(string.Empty, _options.ErrorMessage);
                }
                else
                {
                    // Adds an empty object to the cache using the hashValue to a key (This sets the expiration that will determine
                    // if the Request is valid or not
                    _cache.Set(key, "", TimeSpan.FromSeconds(_options.Secconds));
                    //context.HttpContext.Items[CONTEXT_CACHE_KEY] = hashValue;
                }
            }
            

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }

            
        }
    }

}
