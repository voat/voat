using System;
using System.Threading.Tasks;
using Voat.Rules;

namespace Voat.Domain.Command
{
    public interface ICommand
    {
        string UserName { get; }
    }

    //We are not implementing full Command/Query seperation so this interface will be used
    //to allow commands to execute their own logic and return any necessary data. When full
    //command path pattern is implemented, this interface logic will be moved to handlers.
    public interface IExcutableCommand<R> where R : CommandResponse
    {
        Task<R> Execute();
    }

    public interface IUpdateCache
    {
        Task Update();
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
    public abstract class Command : ICommand
    {
        //So any command can invoke the rules engine
        protected RulesEngine.RulesEngine<VoatRuleContext> rulesEngine = VoatRulesEngine.Instance;

        private string _userName = null;

        public Command()
        {
            if (System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                _userName = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            }
        }

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

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
        protected abstract Task<T> ProtectedExecute();

        public virtual async Task<T> Execute()
        {
            try
            {
                return await ProtectedExecute();
            }
            catch (Exception ex)
            {
                return CommandResponse.Error<T>(ex);
            }
        }
    }
}
