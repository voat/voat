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
using Voat.Voting.Models;

namespace Voat.Data
{
    public partial class Repository
    {
        public async Task<CommandResponse<VoteTracker>> RecordUserVote(int voteID, int optionID, bool restrictionsPassed)
        {
            DemandAuthentication();

            //TODO: Fuzzy Trap, ensure this option belongs to the vote

            var response = new CommandResponse<VoteTracker>();
            var userName = User.Identity.Name;
            bool saveChanges = false;

            var voteRecord = await _db.VoteTracker.Where(x => x.VoteID == voteID && x.VoteOptionID == optionID && x.UserName == userName).FirstOrDefaultAsync();

            if (voteRecord == null)
            {
                voteRecord = new VoteTracker() {
                    VoteID = voteID,
                    VoteOptionID = optionID,
                    RestrictionsPassed = restrictionsPassed,
                    CreationDate = CurrentDate,
                    UserName = User.Identity.Name
                };
                _db.VoteTracker.Add(voteRecord);
                saveChanges = true;
            }
            else if (voteRecord.VoteOptionID != optionID)
            {
                voteRecord.VoteOptionID = optionID;
                saveChanges = true;
            }

            if (saveChanges)
            {
                await _db.SaveChangesAsync();
            }

            response.Response = voteRecord;
            response.Status = Status.Success;

            return response;
        }

        public async Task<CommandResponse<Domain.Models.Vote>> SaveVote(Domain.Models.Vote vote)
        {

            DemandAuthentication();

            var newDataModel = VoteDomainMaps.Map(vote);
            var domainModel = vote;

            //UPDATE
            if (newDataModel.ID > 0)
            {
                var existingDataModel = await GetVoteDataModel(newDataModel.ID);
                //Check some basics 
                if (existingDataModel == null)
                {
                    return CommandResponse.FromStatus<Domain.Models.Vote>(null, Status.Error, "Vote can not be found");
                }
                if (!existingDataModel.CreatedBy.IsEqual(User.Identity.Name) && !User.IsInAnyRole(new[] { UserRole.GlobalAdmin, UserRole.Admin }))
                {
                    return CommandResponse.FromStatus<Domain.Models.Vote>(null, Status.Error, "Vote can not be edited by current user");
                }
                if (existingDataModel.StartDate <= CurrentDate)
                {
                    return CommandResponse.FromStatus<Domain.Models.Vote>(null, Status.Error, "Vote can not be edited once voting has begun");
                }

                //TODO: Verify incoming model ids all belong to this vote
                var restrictionsAreBelongToUs = newDataModel.VoteRestrictions.Where(x => x.ID > 0).All(x => existingDataModel.VoteRestrictions.Any(e => e.ID == x.ID));
                if (!restrictionsAreBelongToUs)
                {
                    return CommandResponse.FromStatus<Domain.Models.Vote>(null, Status.Error, "Message integrity violated (Restrictions)");
                }
                var optionsAreBelongToUs = newDataModel.VoteOptions.Where(x => x.ID > 0).All(x => existingDataModel.VoteOptions.Any(e => e.ID == x.ID));
                if (!optionsAreBelongToUs)
                {
                    return CommandResponse.FromStatus<Domain.Models.Vote>(null, Status.Error, "Message integrity violated (Options)");
                }
                var outcomesAreBelongToUs = newDataModel.VoteOptions.Where(x => x.ID > 0).SelectMany(x => x.VoteOutcomes.Where(o => o.ID > 0)).All(x => existingDataModel.VoteOptions.SelectMany(y => y.VoteOutcomes).Any(e => e.ID == x.ID));
                if (!outcomesAreBelongToUs)
                {
                    return CommandResponse.FromStatus<Domain.Models.Vote>(null, Status.Error, "Message integrity violated (Outcomes)");
                }



                existingDataModel.LastEditDate = CurrentDate;
                existingDataModel.Title = newDataModel.Title;
                existingDataModel.Content = newDataModel.Content;
                existingDataModel.FormattedContent = newDataModel.FormattedContent;

                newDataModel.VoteOptions.ForEachIndex((option, index) =>
                {

                    //TODO: Ensure ID belongs to proper vote (aka fuzzy will exploit this)
                    if (option.ID > 0)
                    {
                        //Update Existing
                        var existingOption = existingDataModel.VoteOptions.FirstOrDefault(x => x.ID == option.ID);
                        existingOption.Title = option.Title;
                        existingOption.Content = option.Content;
                        existingOption.FormattedContent = option.FormattedContent;
                        existingOption.SortOrder = index;

                        option.VoteOutcomes.ForEachIndex((outcome, oIndex) =>
                        {

                            if (outcome.ID > 0)
                            {
                                var existingOutcome = existingDataModel.VoteOptions[index].VoteOutcomes.FirstOrDefault(x => x.ID == outcome.ID);
                                existingOutcome.Type = outcome.Type;
                                existingOutcome.Data = outcome.Data;
                            }
                            else
                            {
                                var newOutcome = new VoteOutcome();
                                newOutcome.Type = outcome.Type;
                                newOutcome.Data = outcome.Data;
                                existingOption.VoteOutcomes.Add(newOutcome);
                            }
                        });
                        //Remove deleted outcomes
                        var deletedOutcomes = existingOption.VoteOutcomes.Where(c => !option.VoteOutcomes.Any(n => c.ID == n.ID)).ToList();
                        deletedOutcomes.ForEach(x => existingOption.VoteOutcomes.Remove(x));
                    }
                    else
                    {
                        //Add new
                        var newOption = new Data.Models.VoteOption();
                        newOption.Title = option.Title;
                        newOption.Content = option.Content;
                        newOption.FormattedContent = option.FormattedContent;
                        newOption.SortOrder = index;
                        existingDataModel.VoteOptions.Add(newOption);

                        option.VoteOutcomes.ForEachIndex((outcome, oIndex) =>
                        {
                            var newOutcome = new VoteOutcome();
                            newOutcome.Type = outcome.Type;
                            newOutcome.Data = outcome.Data;
                            newOption.VoteOutcomes.Add(newOutcome);
                        });

                    }
                });
                //Remove deleted options
                var deletedOptions = existingDataModel.VoteOptions.Where(c => !newDataModel.VoteOptions.Any(n => c.ID == n.ID)).ToList();
                deletedOptions.ForEach(x => existingDataModel.VoteOptions.Remove(x));

                //handle restrictions 
                newDataModel.VoteRestrictions.ForEachIndex((restriction, index) =>
                {
                    if (restriction.ID > 0)
                    {
                        //Update Existing
                        var existingRestriction = existingDataModel.VoteRestrictions.FirstOrDefault(x => x.ID == restriction.ID);
                        existingRestriction.Type = restriction.Type;
                        existingRestriction.Data = restriction.Data;
                    }
                    else
                    {
                        //Add new
                        var newRestriction = new Data.Models.VoteRestriction();
                        newRestriction.Type = restriction.Type;
                        newRestriction.Data = restriction.Data;
                        existingDataModel.VoteRestrictions.Add(newRestriction);
                    }
                });
                //Remove deleted options
                var deletedRestrictions = existingDataModel.VoteRestrictions.Where(c => !newDataModel.VoteRestrictions.Any(n => c.ID == n.ID)).ToList();
                deletedRestrictions.ForEach(x => existingDataModel.VoteRestrictions.Remove(x));

                await _db.SaveChangesAsync();

                return CommandResponse.FromStatus<Domain.Models.Vote>(existingDataModel.Map(), Status.Success);
            }
            //NEW
            else
            {
                newDataModel.CreatedBy = User.Identity.Name;
                newDataModel.CreationDate = CurrentDate;

                //TODO: Set start end dates according to logic
                newDataModel.StartDate = CurrentDate.AddDays(7);
                newDataModel.EndDate = CurrentDate.AddDays(14);

                _db.Vote.Add(newDataModel);
                await _db.SaveChangesAsync();
                domainModel = VoteDomainMaps.Map(newDataModel);

                return CommandResponse.FromStatus<Domain.Models.Vote>(newDataModel.Map(), Status.Success);
            }


        }
        public async Task<Data.Models.Vote> GetVoteDataModel(int id)
        {
            var dataModel = await _db.Vote.Where(x => x.ID == id)
              .Include(x => x.VoteOptions).ThenInclude(o => o.VoteOutcomes)
              .Include(x => x.VoteRestrictions).FirstOrDefaultAsync();
            return dataModel;
        }
        public async Task<IEnumerable<Data.Models.Vote>> GetVoteDataModel(int[] ids)
        {
            var dataModel = await _db.Vote.Where(x => ids.Contains(x.ID))
              .Include(x => x.VoteOptions).ThenInclude(o => o.VoteOutcomes)
              .Include(x => x.VoteRestrictions).ToListAsync();
            return dataModel;
        }
        public async Task<Domain.Models.Vote> GetVote(int id)
        {

            var dataModel = await GetVoteDataModel(id);

            var domainModel = VoteDomainMaps.Map(dataModel);

            return domainModel;
        }

        public async Task<IEnumerable<Domain.Models.Vote>> GetVotes(string subverse, SearchOptions searchOptions)
        {

            IEnumerable<Data.Models.Vote> votes = null;
            //IEnumerable<Data.Models.VoteOption> options = null;
            //IEnumerable<Data.Models.VoteOutcome> outcomes = null;
            //IEnumerable<Data.Models.VoteRestriction> restrictions = null;

            var q = new DapperQuery();
            q.Select = $"SELECT {SqlFormatter.QuoteIndentifier("ID")} FROM {SqlFormatter.Table("Vote")}";
            if (!String.IsNullOrEmpty(subverse))
            {
                q.Where = "\"Subverse\" = @Subverse";
                q.Parameters.Add("Subverse", subverse);
            }
            q.OrderBy = SqlFormatter.OrderBy("CreationDate", true);
            q.TakeCount = searchOptions.Count;
            q.SkipCount = searchOptions.Index;

            var voteIDs = await _db.Connection.QueryAsync<int>(q.ToString(), q.Parameters);

            if (voteIDs != null && voteIDs.Any())
            {
                //var voteIDs = votes.Select(x => x.ID).ToList();

                var data = await GetVoteDataModel(voteIDs.ToArray());
                votes = data;

                //q = new DapperQuery();
                //q.Select = $"SELECT * FROM {SqlFormatter.Table("VoteRestriction")} WHERE \"VoteID\" {SqlFormatter.In("@VoteID")} ";
                //q.Select += $"SELECT * FROM {SqlFormatter.Table("VoteOption")} WHERE \"VoteID\" {SqlFormatter.In("@VoteID")} ";
                //q.Parameters.Add("VoteID", voteIDs);

                //using (var multi = await _db.Connection.QueryMultipleAsync(q.ToString(), q.Parameters))
                //{
                //    restrictions = multi.Read<Data.Models.VoteRestriction>().ToList();
                //    options = multi.Read<Data.Models.VoteOption>().ToList();
                //}

                //var voteOptionIDs = options.Select(x => x.ID).ToList();
                //q = new DapperQuery();
                //q.Select = $"SELECT * FROM {SqlFormatter.Table("VoteOutcome")} WHERE \"VoteOptionID\" {SqlFormatter.In("@VoteOptionID")} ";
                //q.Parameters.Add("VoteOptionID", voteIDs);

                //outcomes = await _db.Connection.QueryAsync<Data.Models.VoteOutcome>(q.ToString(), q.Parameters);
            }

            ////CONSTRUCT 
            //votes.ForEach(v => {
            //    v.VoteRestrictions = new List<VoteRestriction>();
            //    v.VoteRestrictions.AddRange(restrictions.Where(r => r.VoteID == v.ID).ToList());

            //    v.VoteOptions = new List<Models.VoteOption>();
            //    v.VoteOptions.AddRange(options.Where(o => o.VoteID == v.ID).ToList());

            //    v.VoteOptions.ForEach(option => {
            //        option.VoteOutcomes = new List<VoteOutcome>();
            //        var outcomeList = outcomes.Where(o => option.ID == o.VoteOptionID).ToList();
            //        option.VoteOutcomes.AddRange(outcomeList);
            //    });
            //});

            var domainVotes = votes.IsNullOrEmpty() ?
                Enumerable.Empty<Domain.Models.Vote>() :
                votes.Select(x => VoteDomainMaps.Map(x)).ToList();

            return domainVotes;
        }
        //GetVotes
        public async Task<VoteStatistics> GetVoteStatistics(int id)
        {
            var result = new VoteStatistics();

            var data = (from v in _db.VoteTracker
                        where v.VoteID == id
                        group v by new { v.RestrictionsPassed, v.VoteOptionID } into g
                        select new {
                                g.Key.RestrictionsPassed,
                                g.Key.VoteOptionID,
                                Count = g.Count()
                        }).ToList();

            var passed = data.Where(x => x.RestrictionsPassed).ToDictionary(x => x.VoteOptionID, y => y.Count);
            var failed = data.Where(x => !x.RestrictionsPassed).ToDictionary(x => x.VoteOptionID, y => y.Count);

            result.VoteID = id;
            if (passed != null && passed.Count > 0)
            {
                result.Raw.Add(VoteRestrictionStatus.Certified, passed);
            }
            if (failed != null && failed.Count > 0)
            {
                result.Raw.Add(VoteRestrictionStatus.Uncertified, failed);
            }

            return result;
        }
    }
}
