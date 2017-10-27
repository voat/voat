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
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Voat.Configuration;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Voat.Utilities;

namespace Voat.UI.Utilities
{
    public class ValidateCaptchaAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                if (VoatSettings.Instance.CaptchaEnabled)
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

    public static class ReCaptchaUtility
    {
        public static async Task<bool> Validate(HttpRequest request)
        {
            //if (VoatSettings.Instance.OutgoingTraffic.Enabled)
            //{
                string privateKey = VoatSettings.Instance.RecaptchaPrivateKey;
                string encodedResponse = request.Form["g-Recaptcha-Response"];

                using (var httpResource = new HttpResource(
                    new Uri("https://www.google.com/recaptcha/api/siteverify"), 
                    null))
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("secret", privateKey),
                        new KeyValuePair<string, string>("response", encodedResponse),
                    });

                    await httpResource.GiddyUp(HttpMethod.Post, content);

                    var responseString = await httpResource.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var captchaResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(responseString);

                    return captchaResponse.Success;
                }
            //}
            //else
            //{
            //    return false;
            //}
        }
        private class ReCaptchaResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("error-codes")]
            public List<string> ErrorCodes { get; set; }
        }
    }
}
