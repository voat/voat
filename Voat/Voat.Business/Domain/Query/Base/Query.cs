using System.Threading.Tasks;

namespace Voat.Domain.Query
{
    public interface IQuery<T>
    {
        Task<T> ExecuteAsync();
    }

    public abstract class Query<T> : IQuery<T>
    {
        private string _userName;

        public Query()
        {
            if (System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                _userName = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            }
        }

        /// <summary>
        /// Represents the currently authenticated user name or the User who is executing/owns the context
        /// </summary>
        public string UserName
        {
            get
            {
                return _userName;
            }
        }

        //BLOCK: This needs fixed
        public abstract Task<T> ExecuteAsync();

        public virtual T Execute()
        {
            //BLOCK: This needs fixed
            Task<T> t = Task.Run(ExecuteAsync);
            Task.WaitAny(t);
            return t.Result;
        }
    }
}
