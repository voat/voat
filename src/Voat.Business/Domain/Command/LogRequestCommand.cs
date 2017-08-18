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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Voat.Common;
using Voat.Domain.Models;
using Voat.Logging;
using Voat.Utilities.Components;

namespace Voat.Domain.Command
{
    [Obsolete("No", true)]
    public class LogRequestCommand : Command<CommandResponse>
    {
        private string _origin;
        private RequestInfo _info;

        public LogRequestCommand(string origin, RequestInfo requestInfo)
        {
            this._origin = origin;
            this._info = requestInfo;
        }

        protected override async Task<CommandResponse> ProtectedExecute()
        {

            var logEntry = new LogInformation
            {
                Origin = _origin,
                Type = LogType.Debug,
                UserName = _info.UserName,
                Message = "Request",
                Category = "Monitor",
                Data = _info
            };

            EventLogger.Instance.Log(logEntry);

            return CommandResponse.Successful();
        }
    }
    public class RequestInfo
    {
        public string Method { get; set; }
        public string UserName { get; set; }
        public string Url { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }

        //CORE_PORT: Incompatible, replacing both methods with one that generates an error
        public static RequestInfo Parse(object o)
        {
            throw new NotImplementedException("Core Port Build");
        }
        /*
        public static RequestInfo Parse(HttpRequestBase request)
        {
            var r = new RequestInfo();
            r.Method = request.HttpMethod;
            r.Url = request.Url.PathAndQuery;
            r.UserName = Thread.CurrentPrincipal.Identity.Name;
            //r.Headers = request.Headers;
            return r;
        }
        public static RequestInfo Parse(HttpRequest request)
        {
            var r = new RequestInfo();
            r.Method = request.HttpMethod;
            r.Url = request.Url.PathAndQuery;
            r.UserName = Thread.CurrentPrincipal.Identity.Name;
            //r.UserName = request.
            //r.Headers = request.Headers;
            return r;
        }
        */
    }
}
