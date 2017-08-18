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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.IO;
using Voat.Configuration;

namespace Voat.Utils
{
    //These classes for JSON.Net to be used by default instead of the runtime when returning JsonResult
    public class JsonNetResult : JsonResult
    {
        public JsonNetResult() : base(null)
        {
            Settings = JsonSettings.FriendlySerializationSettings;
        }

        public JsonSerializerSettings Settings { get; private set; }
        
        public override void ExecuteResult(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var response = context.HttpContext.Response;
            //CORE_PORT: Content Encoding not found
            throw new NotImplementedException("Content encoding not found");
            //if (this.ContentEncoding != null)
            //{
            //    response.ContentEncoding = this.ContentEncoding;
            //}
            if (base.Value == null)
            {
                return;
            }

            response.ContentType = string.IsNullOrEmpty(this.ContentType) ? "application/json" : this.ContentType;

            var scriptSerializer = JsonSerializer.Create(this.Settings);
            // Serialize the data to the Output stream of the response
            using (var tw = new StreamWriter(response.Body))
            {
                scriptSerializer.Serialize(tw, Value);
            }
        }
    }
    public class JsonNetActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Result.GetType() == typeof(JsonResult))
            {
                // Get the standard result object with unserialized data
                JsonResult result = filterContext.Result as JsonResult;

                // Replace it with our new result object and transfer settings
                filterContext.Result = new JsonNetResult
                {
                    ContentType = result.ContentType,
                    Value = result.Value,
                    //CORE_PORT: Not supported
                    //ContentEncoding = result.ContentEncoding,
                    //JsonRequestBehavior = result.JsonRequestBehavior
                };

                // Later on when ExecuteResult will be called it will be the
                // function in JsonNetResult instead of in JsonResult
            }
            base.OnActionExecuted(filterContext);
        }
    }
}
