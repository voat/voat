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

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Voat.Common;

namespace Voat.Domain.Command
{
    public enum Status
    {
        NotProcessed = 0,
        Success = 1,
        Denied = 2,
        Ignored = 4,
        Invalid = 8,
        Error = 16,
        Queued = 32
    }

    //We are not moving to a full CQRS implementation at this time, so for now some commands need to return
    //data such as identity values and vote counts. It would be inefficient to implement an eventing structure
    //at this time since the data will be present at the time of a command execution.
    public class CommandResponse
    {
        public CommandResponse(Status status, string message)
        {
            this.Status = status;
            this.Message = message;
        }

        public CommandResponse()
        {
        }

        /// <summary>
        /// The friendly description to be used if information is displayed on the UI or to the user.
        /// </summary>
        public virtual string Message { get; set; }

        public IEnumerable<ValidationResult> ValidationErrors { get; set; }

        [JsonIgnore]
        public Exception Exception { get; set; }

        public Status Status { get; set; }

        public bool Success
        {
            get { return this.Status == Status.Success || this.Status == Status.Queued; }
        }

        #region Static Helpers

        private static string DefaultDescription(Status status)
        {
            return $"Operation Result: {status.ToString()}";
        }

        public static CommandResponse FromStatus(Status status, string description)
        {
            return new CommandResponse(status, description);
        }
        public static CommandResponse FromStatus(Status status)
        {
            return new CommandResponse(status, DefaultDescription(status));
        }
        public static CommandResponse<R> FromStatus<R>(R response, Status status, string description)
        {
            return new CommandResponse<R>(response, status, description);
        }
        public static CommandResponse<R> FromStatus<R>(R response, Status status)
        {
            return new CommandResponse<R>(response, status, DefaultDescription(status));
        }
        public static CommandResponse<R> Invalid<R>(IEnumerable<ValidationResult> results)
        {
            
            var message = DefaultDescription(Status.Invalid);
            //This is a hack to return first error message since validation errors being returned from command objects is new
            if (results != null && results.Any())
            {
                message = results.First().ErrorMessage;
            }

            var result = new CommandResponse<R>(default(R), Status.Invalid, message);
            result.ValidationErrors = results;
            return result;
        }
        //public static CommandResponse<R> Denied<R>(R response, string description)
        //{
        //    return new CommandResponse<R>(response, Status.Denied, description);
        //}

        //public static CommandResponse Denied(string description)
        //{
        //    return new CommandResponse(Status.Denied, description);
        //}

        //public static CommandResponse<R> Ignored<R>(R response, string description)
        //{
        //    return new CommandResponse<R>(response, Status.Ignored, description);
        //}

        //public static CommandResponse Ignored(string description)
        //{
        //    return new CommandResponse(Status.Ignored, description);
        //}

        public static CommandResponse<R> Successful<R>(R response)
        {
            return new CommandResponse<R>(response, Status.Success, "");
        }

        public static CommandResponse Successful()
        {
            return new CommandResponse(Status.Success, "");
        }

        public static T Error<T>(Exception ex) where T : CommandResponse, new()
        {
            var r = new T();
            r.Exception = ex;
            if (ex is VoatException)
            {
                r.Status = Status.Invalid;
                r.Message = ex.Message;
            }
            else
            {   //protect any system errors
                r.Status = Status.Error;
                r.Message = "System Error";
            }
            return r;
        }

        public static CommandResponse<M> Map<T, M>(CommandResponse<T> response, M mapped)
        {
            return new CommandResponse<M>(mapped, response.Status, response.Message);
        }

        #endregion Static Helpers
    }

    public class CommandResponse<R> : CommandResponse
    {
        public CommandResponse(R response, Status status, string message) : base(status, message)
        {
            this.Response = response;
        }

        public CommandResponse()
        {
        }

        public R Response { get; set; }
    }
}
