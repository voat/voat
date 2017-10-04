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
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Voat.Rules;
using Voat.Common;
using System.Collections.Generic;

namespace Voat.Domain.Command
{
    

    //We are not implementing full Command/Query seperation so this interface will be used
    //to allow commands to execute their own logic and return any necessary data. When full
    //command path pattern is implemented, this interface logic will be moved to handlers.
    public interface IExcutableCommand<R> where R : CommandResponse
    {
        Task<R> Execute();
    }

    /// <summary>
    /// Use this class when the command has all the information necessary to execute UpdateCache method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public abstract class CacheCommand<T> : Command<T> where T : CommandResponse, new()
    {
        protected override async Task<T> ProtectedExecute()
        {
            var result = await CacheExecute();
            if (result.Success)
            {
                UpdateCache(result);
            }

            return result;
        }

        protected abstract Task<T> CacheExecute();

        protected abstract void UpdateCache(T result);
    }

    /// <summary>
    /// Use this class when the command produces input for the UpdateCache method.
    /// </summary>
    /// <typeparam name="T">Type the Command returns</typeparam>
    /// <typeparam name="C">Type used as input for UpdateCache(C c) method</typeparam>
    [Serializable]
    public abstract class CacheCommand<T, C> : Command<T> where T : CommandResponse, new()
    {
        protected override async Task<T> ProtectedExecute()
        {
            var result = await CacheExecute();
            if (result.Item1.Success)
            {
                UpdateCache(result.Item2);
            }

            //Task t = Task.Run(() => UpdateCache(result.Item2)); //don't wait this
            return result.Item1;
        }

        protected abstract Task<Tuple<T, C>> CacheExecute();

        protected abstract void UpdateCache(C result);
    }

    [Serializable]
    public abstract class Command : SecurityContext<IPrincipal>
    {
        //So any command can invoke the rules engine
        //protected RulesEngine.RulesEngine<VoatRuleContext> rulesEngine = VoatRulesEngine.Instance;

        protected CommandResponse Map(RulesEngine.RuleOutcome outcome)
        {
            return CommandResponse.FromStatus(outcome.Message, Status.Denied, outcome.ToString());
        }

        protected CommandResponse<T> Map<T>(RulesEngine.RuleOutcome outcome)
        {
            return CommandResponse.FromStatus(default(T), Status.Denied, outcome.Message);
        }
    }

    [Serializable]
    public abstract class Command<T> : Command, IExcutableCommand<T> where T : CommandResponse, new()
    {
        ////This is for future use
        //private bool _enableQueuing = false;

        protected CommandStage CommandStageMask { get; set; } = CommandStage.All;

        protected abstract Task<T> ProtectedExecute();

        protected virtual Task<T> ExecuteStage(CommandStage stage, T previous)
        {
            return Task.FromResult(previous);

            ////return true
            //var response = new T();
            //response.Status = Status.Success;
            //return Task.FromResult(response);
        }

        private async Task<T> ExecuteStages(IEnumerable<CommandStage> stages, T previous)
        {
            if (previous.Success)
            {
                foreach (var stage in stages)
                {
                    var r = await ExecuteStage(stage, previous);
                    if (!r.Success)
                    {
                        return r;
                    }
                }
            }
            return previous;
        }

        public virtual async Task<T> Execute()
        {
            try
            {
                //Execute prestage
                var response = await ExecuteStages(
                    CommandStageMask.GetEnumFlagsIntersect(
                    CommandStage.OnAuthorization |
                    CommandStage.OnValidation |
                    //CommandStage.OnQueuing |
                    CommandStage.OnExecuting
                ), new T() { Status = Status.Success });

                if (!response.Success)
                {
                    return response;
                }
                //execute
                response = await ProtectedExecute();

                //Execute poststage
                response = await ExecuteStages(CommandStageMask.GetEnumFlagsIntersect(CommandStage.OnExecuted), response);
                return response;
            }
            catch (Exception ex)
            {
                return CommandResponse.Error<T>(ex);
            }
        }
    }
}
