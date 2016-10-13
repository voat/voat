using System.Threading.Tasks;

namespace Voat.Domain.Query
{
    public interface IQuery<T>
    {
        Task<T> ExecuteAsync();
    }

    public abstract class Query<T> : IQuery<T>
    {
        //if we wish to pass in a context
        //private voatEntities _dataContext;
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

        public virtual async Task<T> ExecuteAsync()
        {
            return await Task.Run(() => Execute());
        }
        public abstract T Execute();
    }
}
