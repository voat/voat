using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using Voat.Domain.Query;
using Voat.Common;
using Voat.Data.Models;
using Voat.Models;
using Voat.Domain.Models;
using Voat.Rules;
using Voat.RulesEngine;
using Voat.Utilities;
using Voat.Utilities.Components;
using Voat.Domain.Command;
using System.Text.RegularExpressions;
using Voat.Domain;
using System.Data.Entity;
using Voat.Caching;
using Dapper;
using Voat.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace Voat.Data
{
    public partial class Repository 
    {
        #region Sets
        //TODO: Make subverse an array and process multiple additions
        [Authorize]
        public async Task<CommandResponse<bool?>> SetSubverseListChange(DomainReference setReference, string subverse, SubscriptionAction action)
        {
            DemandAuthentication();
            var set = GetSet(setReference.Name, setReference.OwnerName);

            if (set == null)
            {
                return CommandResponse.FromStatus<bool?>(null, Status.Denied, "Set cannot be found");
            }
            
            //Check Perms
            var perms = SetPermission.GetPermissions(set.Map(), User.Identity);
            if (!perms.EditList)
            {
                return CommandResponse.FromStatus<bool?>(null, Status.Denied, "User not authorized to modify set");
            }

            var sub = GetSubverseInfo(subverse);

            if (sub == null)
            {
                return CommandResponse.FromStatus<bool?>(null, Status.Denied, "Subverse cannot be found");
            }

            return await SetSubverseListChange(set, sub, action).ConfigureAwait(false);

        }
        //[Authorize]
        //public async Task<CommandResponse<bool?>> EditSet(DomainReference setReference, SubverseSet newSetProperties)
        //{
        //    DemandAuthentication();
        //    var set = GetSet(setReference.Name, setReference.OwnerName);

        //    if (set == null)
        //    {
        //        return CommandResponse.FromStatus<bool?>(null, Status.Denied, "Set cannot be found");
        //    }

        //    //Check Perms
        //    var perms = SetPermission.GetPermissions(set.Map(), User.Identity);
        //    if (!perms.EditProperties)
        //    {
        //        return CommandResponse.FromStatus<bool?>(null, Status.Denied, "User not authorized to edit set properties");
        //    }

        //    set.Name = newSetProperties.Name;
        //    set.Description = newSetProperties.Description;
        //    set.IsPublic = newSetProperties.IsPublic;

        //    var records = await _db.SaveChangesAsync().ConfigureAwait(false);

        //    return CommandResponse.FromStatus<bool?>(records == 1, Status.Success);
        //}
        [Authorize]
        public async Task<CommandResponse<bool?>> DeleteSet(DomainReference setReference)
        {
            DemandAuthentication();
            var set = GetSet(setReference.Name, setReference.OwnerName);

            if (set == null)
            {
                return CommandResponse.FromStatus<bool?>(null, Status.Denied, "Set cannot be found");
            }

            //Check Perms
            var perms = SetPermission.GetPermissions(set.Map(), User.Identity);
            if (!perms.Delete)
            {
                return CommandResponse.FromStatus<bool?>(null, Status.Denied, "User not authorized to delete set");
            }

            var param = new DynamicParameters();
            param.Add("ID", set.ID);

            var conn = _db.Database.Connection;
            //var tran = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                var d = new DapperDelete();

                d.Delete = "SubverseSetSubscription";
                d.Where = "SubverseSetID = @ID";
                await conn.ExecuteAsync(d.ToString(), param);

                d.Delete = "SubverseSetList";
                d.Where = "SubverseSetID = @ID";
                await conn.ExecuteAsync(d.ToString(), param);

                d.Delete = "SubverseSet";
                d.Where = "ID = @ID";
                await conn.ExecuteAsync(d.ToString(), param);

                //tran.Commit();

                return CommandResponse.FromStatus<bool?>(true, Status.Success);
            }
            catch (Exception ex)
            {
                //tran.Rollback();
                return CommandResponse.Error<CommandResponse<bool?>>(ex);
            }
        }
        private async Task<CommandResponse<bool?>> SetSubverseListChange(SubverseSet set, Subverse subverse, SubscriptionAction action)
        {
            using (var db = new voatEntities())
            {
                CommandResponse<bool?> response = new CommandResponse<bool?>(true, Status.Success, "");
                var actionTaken = SubscriptionAction.Toggle;

                var setSubverseRecord = db.SubverseSetLists.FirstOrDefault(n => n.SubverseSetID == set.ID && n.SubverseID == subverse.ID);

                if (setSubverseRecord == null && ((action == SubscriptionAction.Subscribe) || action == SubscriptionAction.Toggle))
                {
                    db.SubverseSetLists.Add(new SubverseSetList { SubverseSetID = set.ID, SubverseID = subverse.ID, CreationDate = CurrentDate });
                    actionTaken = SubscriptionAction.Subscribe;
                }
                else if (setSubverseRecord != null && ((action == SubscriptionAction.Unsubscribe) || action == SubscriptionAction.Toggle))
                {
                    db.SubverseSetLists.Remove(setSubverseRecord);
                    actionTaken = SubscriptionAction.Unsubscribe;
                }

                await db.SaveChangesAsync().ConfigureAwait(false);

                //If Subscribe is to a front page, update subscriber count
                if (set.Type == (int)SetType.Front && !String.IsNullOrEmpty(set.UserName))
                {
                    await UpdateSubverseSubscriberCount(new DomainReference(DomainType.Subverse, subverse.Name), actionTaken);
                }
                response.Response = actionTaken == SubscriptionAction.Toggle ? (bool?)null : actionTaken == SubscriptionAction.Subscribe;
                return response;
            }

        }

        public async Task<IEnumerable<SubverseSubscriptionDetail>> GetSetListDescription(string name, string userName, int page = 0)
        {
            var set = GetSet(name, userName);
            if (set != null)
            {
                return await GetSetListDescription(set.ID).ConfigureAwait(false);
            }
            return Enumerable.Empty<SubverseSubscriptionDetail>();
        }

        public async Task<IEnumerable<SubverseSubscriptionDetail>> GetSetListDescription(int setID, int page = 0)
        {
            int count = 50;

            var q = new DapperQuery();
            q.Select =
                @"s.ID, s.Name, s.Title, s.Description, s.CreationDate, s.SubscriberCount, SubscriptionDate = sl.CreationDate
                FROM SubverseSetList sl
                INNER JOIN Subverse s ON (sl.SubverseID = s.ID)
                ";
            q.Where = "sl.SubverseSetID = @ID";
            q.Parameters = new DynamicParameters(new { ID = setID });
            q.SkipCount = page * count;
            q.TakeCount = count;
            q.OrderBy = "s.Name ASC";
            using (var db = new voatEntities())
            {
                var result = await db.Database.Connection.QueryAsync<SubverseSubscriptionDetail>(q.ToString(), q.Parameters);
                return result;
            }
        }

        private SubverseSet GetOrCreateSubverseSet(SubverseSet setInfo)
        {
            using (var db = new voatEntities())
            {
                var set = db.SubverseSets.FirstOrDefault(x => x.Name.Equals(setInfo.Name, StringComparison.OrdinalIgnoreCase) && x.UserName == setInfo.UserName);
                if (set == null)
                {
                    setInfo.CreationDate = CurrentDate;
                    setInfo.SubscriberCount = 1;
                    db.SubverseSets.Add(setInfo);
                    setInfo.SubverseSetSubscriptions.Add(new SubverseSetSubscription() { UserName = setInfo.UserName, CreationDate = CurrentDate });
                    db.SaveChanges();
                    return setInfo;
                }
                else
                {
                    return set;
                }
            }
        }
        public SubverseSet GetSet(string name, string userName, SetType? type = null, bool createIfMissing = false)
        {

            var q = new DapperQuery();
            q.Select = "SELECT * FROM SubverseSet subSet";
            q.Where = "subSet.Name = @Name";

            if (type.HasValue)
            {
                q.Append(x => x.Where, "subSet.Type = @Type");
            }

            if (!String.IsNullOrEmpty(userName))
            {
                q.Append(x => x.Where, "subSet.UserName = @UserName");
            }
            else
            {
                q.Append(x => x.Where, "subSet.UserName IS NULL");
            }

            q.Parameters = new DynamicParameters(new { Name = name, UserName = userName, Type = (int?)type });
            using (var db = new voatEntities())
            {
                var set = db.Database.Connection.QueryFirstOrDefault<SubverseSet>(q.ToString(), q.Parameters);
                return set;
            }
        }

        public async Task<IEnumerable<SubverseSet>> GetUserSets(string userName)
        {
            var q = new DapperQuery();
            q.Select = "SELECT * FROM SubverseSet subSet";
            q.Where = "subSet.UserName = @UserName";
            q.Parameters.Add("UserName", userName);

            if (!userName.IsEqual(User.Identity.Name))
            {
                q.Append(x => x.Where, "subSet.IsPublic = 1");
            }

            using (var db = new voatEntities())
            {
                var set = await db.Database.Connection.QueryAsync<SubverseSet>(q.ToString(), q.Parameters);
                return set;
            }
        }


        public async Task<CommandResponse<Domain.Models.Set>> CreateOrUpdateSet(Set set)
        {
            DemandAuthentication();

            set.Name = set.Name.TrimSafe();
            //Evaulate Rules
            VoatRuleContext context = new VoatRuleContext();
            context.PropertyBag.Set = set;
            var outcome = VoatRulesEngine.Instance.EvaluateRuleSet(context, RuleScope.CreateSet);
            if (!outcome.IsAllowed)
            {
                return MapRuleOutCome<Set>(outcome, null);
            }

            var existingSet = _db.SubverseSets.FirstOrDefault(x => x.ID == set.ID);
            if (existingSet != null)
            {
                var perms = SetPermission.GetPermissions(existingSet.Map(), User.Identity);
                 
                if (!perms.EditProperties)
                {
                    return CommandResponse.FromStatus<Set>(null, Status.Denied, "User does not have permission to edit this set");
                }

                existingSet.Name = set.Name;
                existingSet.Title = set.Title;
                existingSet.Description = set.Description;
                existingSet.IsPublic = set.IsPublic;

                await _db.SaveChangesAsync().ConfigureAwait(false);

                return CommandResponse.FromStatus<Set>(existingSet.Map(), Status.Success);
            }
            else
            {
                //Validation - MOVE TO RULES SYSTEM MAYBE
                if (Settings.SetCreationDisabled || Settings.MaximumOwnedSets <= 0)
                {
                    return CommandResponse.FromStatus<Set>(null, Status.Denied, "Set creation is currently disabled");
                }

                if (Settings.MaximumOwnedSets > 0)
                {
                    var d = new DapperQuery();
                    d.Select = "SELECT COUNT(*) FROM SubverseSet subSet";
                    d.Where = "subSet.Type = @Type AND subSet.UserName = @UserName";
                    d.Parameters.Add("Type", (int)SetType.Normal);
                    d.Parameters.Add("UserName", User.Identity.Name);

                    var setCount = _db.Database.Connection.ExecuteScalar<int>(d.ToString(), d.Parameters);
                    if (setCount >= Settings.MaximumOwnedSets)
                    {
                        return CommandResponse.FromStatus<Set>(null, Status.Denied, $"Sorry, Users are limited to {Settings.MaximumOwnedSets} sets and you currently have {setCount}");
                    }
                }


                //Create new set
                try
                {
                    var setCheck = GetSet(set.Name, User.Identity.Name);
                    if (setCheck != null)
                    {
                        return CommandResponse.FromStatus<Set>(null, Status.Denied, "A set with same name and owner already exists");
                    } 

                    var newSet = new SubverseSet
                    {
                        Name = set.Name,
                        Title = set.Title,
                        Description = set.Description,
                        UserName = User.Identity.Name,
                        Type = (int)SetType.Normal,
                        IsPublic = set.IsPublic,
                        CreationDate = Repository.CurrentDate,
                        SubscriberCount = 1, //Owner is a subscriber. Reminds me of that hair club commercial: I"m not only the Set Owner, I'm also a subscriber.
                    };

                    _db.SubverseSets.Add(newSet);
                    await _db.SaveChangesAsync().ConfigureAwait(false);

                    _db.SubverseSetSubscriptions.Add(new SubverseSetSubscription() { SubverseSetID = newSet.ID, UserName = User.Identity.Name, CreationDate = CurrentDate });
                    await _db.SaveChangesAsync().ConfigureAwait(false);

                    return CommandResponse.Successful(newSet.Map());
                }
                catch (Exception ex)
                {
                    return CommandResponse.Error<CommandResponse<Set>>(ex);
                }
            }
        }
        #endregion
    }
}
