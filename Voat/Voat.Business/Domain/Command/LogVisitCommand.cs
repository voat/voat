using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;

namespace Voat.Domain.Command
{
    public class LogVisitCommand : CacheCommand<CommandResponse>, IExcutableCommand<CommandResponse>
    {
        protected int _submissionID;
        protected string _clientIPAddress;

        public LogVisitCommand(int submissionID, string clientIPAddress)
        {
            _submissionID = submissionID;
            _clientIPAddress = clientIPAddress;
        }

        protected override async Task<CommandResponse> CacheExecute()
        {
            using (var repo = new Repository())
            {
                //TODO: Convert to async repo method
                var response = await repo.LogVisit(_submissionID, _clientIPAddress);
                return response;
            }
        }

        protected override void UpdateCache(CommandResponse result)
        {
            if (result.Success)
            {
               
            }
        }
    }
}
