using Dapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data.Models;
using Voat.Domain;
using Voat.Domain.Command;
using Voat.Domain.Models;

namespace Voat.Data
{
    public partial class Repository
    {
        public async Task<CommandResponse<Domain.Models.Vote>> SaveVote(Domain.Models.Vote vote)
        {

            DemandAuthentication();

            var inputDataModel = VoteDomainMaps.Map(vote);
            var domainModel = vote;

            //UPDATE
            if (inputDataModel.ID > 0)
            {
                throw new NotImplementedException("Editing not yet implemented");
                var currentDataModel = GetVoteDataModel(inputDataModel.ID);
            }
            //NEW
            else
            {
                inputDataModel.CreatedBy = User.Identity.Name;
                inputDataModel.CreationDate = CurrentDate;

                //TODO: Set start end dates according to logic
                inputDataModel.StartDate = CurrentDate.AddDays(7);
                inputDataModel.EndDate = CurrentDate.AddDays(14);

                _db.Vote.Add(inputDataModel);
                await _db.SaveChangesAsync();
                domainModel = VoteDomainMaps.Map(inputDataModel);
            }

            return CommandResponse.FromStatus<Domain.Models.Vote>(domainModel, Status.Success);
        }
        public async Task<Data.Models.Vote> GetVoteDataModel(int id)
        {
            var dataModel = await _db.Vote.Where(x => x.ID == id)
              .Include(x => x.VoteOptions)
              .Include(x => x.VoteRestrictions).FirstOrDefaultAsync();
            return dataModel;

        }
        public async Task<Domain.Models.Vote> GetVote(int id)
        {

            var dataModel = await GetVoteDataModel(id);

            var domainModel = VoteDomainMaps.Map(dataModel);

            return domainModel;
        }
    }
}
