/*
 * This source file is subject to The Code Project Open License (CPOL) 1.02
 * Original code can be found at: http://rionscode.wordpress.com/2013/02/24/prevent-repeated-requests-using-actionfilters-in-asp-net-mvc/
 * Copyright Rion Williams 2013
 */

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Caching;
using System.Web.Mvc;
using Whoaverse.Models;

namespace Whoaverse.Utils
{
    public class PreventSpamAttribute : ActionFilterAttribute
    {
        // This stores the time between Requests (in seconds)
        public int DelayRequest = 10;
        // The Error Message that will be displayed in case of excessive Requests
        public string ErrorMessage = "Excessive Request Attempts Detected.";
        // This will store the URL to Redirect errors to
        public string RedirectURL;

        private bool trustedUser = false;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var loggedInUser = filterContext.HttpContext.User.Identity.Name;
            string targetSubverseName = null;

            if (filterContext.ActionParameters.ContainsKey("message"))
            {
                // user is submitting a message
                Whoaverse.Models.Message incomingMessage = (Whoaverse.Models.Message)filterContext.ActionParameters["message"];
                var targetSubverse = incomingMessage.Subverse;

                // check user LCP for target subverse
                if (targetSubverse != null)
                {
                    var LCPForSubverse = Whoaverse.Utils.Karma.LinkKarmaForSubverse(loggedInUser, targetSubverse);
                    if (LCPForSubverse >= 40)
                    {
                        // lower DelayRequest time
                        this.DelayRequest = 10;
                    } 
                }                 

                // trigger trustedUser or lower DelayRequest time
                this.DelayRequest = 10;
            }
            else if (filterContext.ActionParameters.ContainsKey("comment"))
            {
                // user is submitting a comment
                Whoaverse.Models.Comment incomingComment = (Whoaverse.Models.Comment)filterContext.ActionParameters["comment"];
                
                using (whoaverseEntities db = new whoaverseEntities())
                {
                    var relatedMessage = db.Messages.Find(incomingComment.MessageId);
                    if (relatedMessage != null)
                    {
                        targetSubverseName = relatedMessage.Subverse;

                        // check user CCP for target subverse
                        int CCPForSubverse = Whoaverse.Utils.Karma.CommentKarmaForSubverse(loggedInUser, targetSubverseName);
                        if (CCPForSubverse >= 40)
                        {
                            // lower DelayRequest time
                            this.DelayRequest = 10;
                        }     
                    }
                }                
            }

            // Store our HttpContext (for easier reference and code brevity)
            var request = filterContext.HttpContext.Request;

            // Store our HttpContext.Cache (for easier reference and code brevity)
            var cache = filterContext.HttpContext.Cache;

            // Grab the IP Address from the originating Request (very simple implementation for example purposes)
            var originationInfo = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress;

            // Append the User Agent
            originationInfo += request.UserAgent;

            // Now we just need the target URL Information
            var targetInfo = request.RawUrl + request.QueryString;

            // Generate a hash for your strings (this appends each of the bytes of the value into a single hashed string
            var hashValue = string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(originationInfo + targetInfo)).Select(s => s.ToString("x2")));

            // TODO:
            // Override spam filter if user is authorized poster to target subverse
            // trustedUser = true;
            
            // Checks if the hashed value is contained in the Cache (indicating a repeat request)
            if (cache[hashValue] != null && loggedInUser != "system" && trustedUser != true)
            {
                // Adds the Error Message to the Model and Redirect
                filterContext.Controller.ViewData.ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            else
            {
                // Adds an empty object to the cache using the hashValue to a key (This sets the expiration that will determine
                // if the Request is valid or not
                cache.Add(hashValue, "", null, DateTime.Now.AddSeconds(DelayRequest), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}