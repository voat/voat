#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Voat.Utilities;
using Voat.Configuration;
using Voat.Data;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Voat.UI.Utilities
{
    public class ValidateCaptchaAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                if (!Settings.CaptchaDisabled)
                {
                    var request = filterContext.HttpContext.Request;
                    var captchaValid = ReCaptchaUtility.Validate(request).Result;

                    if (!captchaValid)
                    {
                        // Add a model error if the captcha was not valid
                        filterContext.ModelState.AddModelError(string.Empty, "Incorrect recaptcha answer.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    public class PreventSpamAttribute : ActionFilterAttribute
    {
        private const string CACHEKEY = "PreventSpamHash";

        // This stores the time between Requests (in seconds)
        public int DelayRequest = 10;
        
        // The Error Message that will be displayed in case of excessive Requests
        public string ErrorMessage = "Excessive Request Attempts Detected.";
        
        // This will store the URL to Redirect errors to
        public string RedirectURL;

        private bool trustedUser = false;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //CORE_PORT: Not ported
            throw new NotImplementedException("This methods not ported");
            /*
            var loggedInUser = filterContext.HttpContext.User.Identity.Name;

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
            var request = filterContext.HttpContext.Request;

            // Store our HttpContext.Cache (for easier reference and code brevity)
            var cache = filterContext.HttpContext.Cache;

            // Grab the IP Address from the originating Request (very simple implementation for example purposes)
            var originationInfo = UserHelper.UserIpAddress(request);

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
                cache.Add(hashValue, "", null, Repository.CurrentDate.AddSeconds(DelayRequest), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                request.RequestContext.HttpContext.Items[CACHEKEY] = hashValue;
            }

            base.OnActionExecuting(filterContext);
            */
        }
        public static void Reset()
        {
            //CORE_PORT: Not ported
            throw new NotImplementedException("Core Port: Not ported");
            /*
            var context = System.Web.HttpContext.Current;
            if (context != null)
            {
                if (context.Items.Contains(CACHEKEY))
                {
                    var hashValue = context.Items[CACHEKEY].ToString();
                    if (!String.IsNullOrWhiteSpace(hashValue))
                    {
                        context.Cache.Remove(hashValue);
                    }
                }
            }
            */
        }
    }

    public static class ReCaptchaUtility
    {
        public static async Task<bool> Validate(object request)
        {
            throw new NotImplementedException("Core port not implemented");
        }
        /*
        public static async Task<bool> Validate(HttpRequestMessage request)
        {
            string privateKey = Settings.RecaptchaPrivateKey;
            string encodedResponse = request.Form["g-Recaptcha-Response"];

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", privateKey), 
                new KeyValuePair<string, string>("response", encodedResponse), 
            });
            var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content).ConfigureAwait(false);
            var captchaResponse = await response.Content.ReadAsAsync<ReCaptchaResponse>().ConfigureAwait(false);

            return captchaResponse.Success;
        }
        */
        private class ReCaptchaResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("error-codes")]
            public List<string> ErrorCodes { get; set; }
        }
    }
}
