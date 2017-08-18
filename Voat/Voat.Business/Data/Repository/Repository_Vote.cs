using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Models;

namespace Voat.Data
{
    public partial class Repository
    {
        public async Task<CommandResponse<Domain.Models.Vote>> SaveVote(Domain.Models.Vote vote)
        {
            return CommandResponse.FromStatus<Domain.Models.Vote>(vote, Status.Success);
        }

    }
}
