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

    //public interface IExcutableCommandHandler<C,R> where C: Command where R : CommandResponse
    //{
    //    Task<R> Execute(C command);
    //}

    /// <summary>
    /// Use this class when the command has all the information necessary to execute UpdateCache method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public abstract class CacheCommand<T> : Command<T> where T : CommandResponse
    {
        public override async Task<T> Execute()
        {
            var result = await Task.Run(() => ProtectedExecute());
            Task t = Task.Run(() => UpdateCache());
            return result;
        }

        protected abstract Task<T> ProtectedExecute();

        protected abstract void UpdateCache();
    }

    /// <summary>
    /// Use this class when the command produces input for the UpdateCache method.
    /// </summary>
    /// <typeparam name="T">Type the Command returns</typeparam>
    /// <typeparam name="C">Type used as input for UpdateCache(C c) method</typeparam>
    [Serializable]
    public abstract class CacheCommand<T, C> : Command<T> where T : CommandResponse
    {
        public override async Task<T> Execute()
        {
            var result = await Task.Run(() => ProtectedExecute());
            Task t = Task.Run(() => UpdateCache(result.Item2));
            return result.Item1;
        }

        protected abstract Task<Tuple<T, C>> ProtectedExecute();

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
        }

        //public bool IsAthenticationRequired { get; protected set; }

        protected CommandResponse Map(RulesEngine.RuleOutcome outcome)
        {
            return CommandResponse.Denied(outcome.Message, outcome.ToString());
        }

        protected CommandResponse<T> Map<T>(RulesEngine.RuleOutcome outcome)
        {
            return CommandResponse.Denied<T>(default(T), outcome.Message, outcome.ToString());
        }
    }

    [Serializable]
    public abstract class Command<T> : Command, IExcutableCommand<T> where T : CommandResponse
    {
        public abstract Task<T> Execute();
    }
}
