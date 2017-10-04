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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Models;
using Voat.Rules;
using Voat.RulesEngine;
using Voat.Domain.Command;
using Voat.Domain;
using Voat.Caching;
using Dapper;
using Voat.Configuration;
using Microsoft.AspNetCore.Authorization;
using Voat.Utilities;
using Voat.Common;

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

            return await SetSubverseListChange(set, sub, action).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

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

        //    var records = await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

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

            var conn = _db.Connection;
            //var tran = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                var d = new DapperDelete();

                d.Delete = SqlFormatter.DeleteBlock(SqlFormatter.Table("SubverseSetSubscription"));
                d.Where = "\"SubverseSetID\" = @ID";
                await conn.ExecuteAsync(d.ToString(), param);

                d.Delete = SqlFormatter.DeleteBlock(SqlFormatter.Table("SubverseSetList"));
                d.Where = "\"SubverseSetID\" = @ID";
                await conn.ExecuteAsync(d.ToString(), param);

                d.Delete = SqlFormatter.DeleteBlock(SqlFormatter.Table("SubverseSet"));
                d.Where = "\"ID\" = @ID";
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
            using (var db = new VoatDataContext())
            {
                CommandResponse<bool?> response = new CommandResponse<bool?>(true, Status.Success, "");
                var actionTaken = SubscriptionAction.Toggle;

                var setSubverseRecord = db.SubverseSetList.FirstOrDefault(n => n.SubverseSetID == set.ID && n.SubverseID == subverse.ID);

                if (setSubverseRecord == null && ((action == SubscriptionAction.Subscribe) || action == SubscriptionAction.Toggle))
                {
                    db.SubverseSetList.Add(new SubverseSetList { SubverseSetID = set.ID, SubverseID = subverse.ID, CreationDate = CurrentDate });
                    actionTaken = SubscriptionAction.Subscribe;
                }
                else if (setSubverseRecord != null && ((action == SubscriptionAction.Unsubscribe) || action == SubscriptionAction.Toggle))
                {
                    db.SubverseSetList.Remove(setSubverseRecord);
                    actionTaken = SubscriptionAction.Unsubscribe;
                }

                await db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                //If Subscribe is to a front page, update subscriber count
                if (set.Type == (int)SetType.Front && !String.IsNullOrEmpty(set.UserName))
                {
                    await UpdateSubverseSubscriberCount(new DomainReference(DomainType.Subverse, subverse.Name), actionTaken);
                }
                response.Response = actionTaken == SubscriptionAction.Toggle ? (bool?)null : actionTaken == SubscriptionAction.Subscribe;
                return response;
            }

        }

        public async Task<IEnumerable<SubverseSubscriptionDetail>> GetSetListDescription(string name, string userName, int? page = 0)
        {
            var set = GetSet(name, userName);
            if (set != null)
            {
                return await GetSetListDescription(set.ID, page).ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
            }
            return Enumerable.Empty<SubverseSubscriptionDetail>();
        }

        public async Task<IEnumerable<SubverseSubscriptionDetail>> GetSetListDescription(int setID, int? page = 0)
        {
            int count = 50;

            var q = new DapperQuery();
            q.Select = $"s.\"ID\", s.\"Name\", s.\"Title\", s.\"Description\", s.\"CreationDate\", {SqlFormatter.As(SqlFormatter.IsNull("s.\"SubscriberCount\"", "0"), "\"SubscriberCount\"")}, sl.\"CreationDate\" AS \"SubscriptionDate\" FROM {SqlFormatter.Table("SubverseSetList", "sl")} INNER JOIN {SqlFormatter.Table("Subverse", "s")} ON (sl.\"SubverseID\" = s.\"ID\")";
            q.Where = "sl.\"SubverseSetID\" = @ID";
            q.Parameters = new DynamicParameters(new { ID = setID });

            if (page.HasValue)
            {
                q.SkipCount = page.Value * count;
                q.TakeCount = count;
            }

            q.OrderBy = "s.\"Name\" ASC";
            using (var db = new VoatDataContext())
            {
                var result = await db.Connection.QueryAsync<SubverseSubscriptionDetail>(q.ToString(), q.Parameters);
                return result;
            }
        }

        private SubverseSet GetOrCreateSubverseSet(SubverseSet setInfo)
        {
            using (var db = new VoatDataContext())
            {
                var set = db.SubverseSet.FirstOrDefault(x => x.Name.ToLower() == setInfo.Name.ToLower() && x.UserName == setInfo.UserName);
                if (set == null)
                {
                    setInfo.CreationDate = CurrentDate;
                    setInfo.SubscriberCount = 1;
                    db.SubverseSet.Add(setInfo);
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
            var normalization = Normalization.Lower;

            var q = new DapperQuery();
            q.Select = $"SELECT * FROM {SqlFormatter.Table("SubverseSet", "subSet")}";
            q.Where = SqlFormatter.ToNormalized("subSet.\"Name\"", normalization) + " = @Name";

            if (type.HasValue)
            {
                q.Append(x => x.Where, "subSet.\"Type\" = @Type");
            }

            if (!String.IsNullOrEmpty(userName))
            {
                q.Append(x => x.Where, SqlFormatter.ToNormalized("subSet.\"UserName\"", normalization)  + " = @UserName");
            }
            else
            {
                q.Append(x => x.Where, "subSet.\"UserName\" IS NULL");
            }

            q.Parameters = new DynamicParameters(new {
                Name = name.ToNormalized(normalization),
                UserName = userName.ToNormalized(normalization),
                Type = (int?)type
            });

            using (var db = new VoatDataContext())
            {
                var set = db.Connection.QueryFirstOrDefault<SubverseSet>(q.ToString(), q.Parameters);
                return set;
            }
        }

        public async Task<IEnumerable<SubverseSet>> GetUserSets(string userName)
        {
            var q = new DapperQuery();
            q.Select = $"SELECT * FROM {SqlFormatter.Table("SubverseSet", "subSet")}";
            q.Where = "subSet.\"UserName\" = @UserName";
            q.Parameters.Add("UserName", userName);

            if (!userName.IsEqual(UserName))
            {
                q.Append(x => x.Where, $"subSet.\"IsPublic\" = {SqlFormatter.BooleanLiteral(true)}");
            }

            using (var db = new VoatDataContext())
            {
                var set = await db.Connection.QueryAsync<SubverseSet>(q.ToString(), q.Parameters);
                return set;
            }
        }


        public async Task<CommandResponse<Domain.Models.Set>> CreateOrUpdateSet(Set set)
        {
            DemandAuthentication();

            set.Name = set.Name.TrimSafe();
            //Evaulate Rules
            var context = new VoatRuleContext(User);
            context.PropertyBag.Set = set;
            var outcome = VoatRulesEngine.Instance.EvaluateRuleSet(context, RuleScope.CreateSet);
            if (!outcome.IsAllowed)
            {
                return MapRuleOutCome<Set>(outcome, null);
            }

            var existingSet = _db.SubverseSet.FirstOrDefault(x => x.ID == set.ID);
            if (existingSet != null)
            {
                var perms = SetPermission.GetPermissions(existingSet.Map(), User.Identity);
                 
                if (!perms.EditProperties)
                {
                    return CommandResponse.FromStatus<Set>(null, Status.Denied, "User does not have permission to edit this set");
                }

                //HACK: Need to clear this entry out of cache if name changes and check name
                if (!existingSet.Name.IsEqual(set.Name))
                {
                    if (_db.SubverseSet.Any(x => x.Name.ToLower() == set.Name.ToLower() && x.UserName.ToLower() == UserName.ToLower()))
                    {
                        return CommandResponse.FromStatus<Set>(null, Status.Denied, "A set with this name already exists");
                    }
                    CacheHandler.Instance.Remove(CachingKey.Set(existingSet.Name, existingSet.UserName));
                }

                existingSet.Name = set.Name;
                existingSet.Title = set.Title;
                existingSet.Description = set.Description;
                existingSet.IsPublic = set.IsPublic;

                await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                return CommandResponse.FromStatus<Set>(existingSet.Map(), Status.Success);
            }
            else
            {
                //Validation - MOVE TO RULES SYSTEM MAYBE
                if (!VoatSettings.Instance.SetCreationEnabled || VoatSettings.Instance.MaximumOwnedSets <= 0)
                {
                    return CommandResponse.FromStatus<Set>(null, Status.Denied, "Set creation is currently disabled");
                }

                if (VoatSettings.Instance.MaximumOwnedSets > 0)
                {
                    var d = new DapperQuery();
                    d.Select = $"SELECT COUNT(*) FROM {SqlFormatter.Table("SubverseSet", "subSet")}";
                    d.Where = "subSet.\"Type\" = @Type AND subSet.\"UserName\" = @UserName";
                    d.Parameters.Add("Type", (int)SetType.Normal);
                    d.Parameters.Add("UserName", UserName);

                    var setCount = _db.Connection.ExecuteScalar<int>(d.ToString(), d.Parameters);
                    if (setCount >= VoatSettings.Instance.MaximumOwnedSets)
                    {
                        return CommandResponse.FromStatus<Set>(null, Status.Denied, $"Sorry, Users are limited to {VoatSettings.Instance.MaximumOwnedSets} sets and you currently have {setCount}");
                    }
                }


                //Create new set
                try
                {
                    var setCheck = GetSet(set.Name, UserName);
                    if (setCheck != null)
                    {
                        return CommandResponse.FromStatus<Set>(null, Status.Denied, "A set with same name and owner already exists");
                    } 

                    var newSet = new SubverseSet
                    {
                        Name = set.Name,
                        Title = set.Title,
                        Description = set.Description,
                        UserName = UserName,
                        Type = (int)SetType.Normal,
                        IsPublic = set.IsPublic,
                        CreationDate = Repository.CurrentDate,
                        SubscriberCount = 1, //Owner is a subscriber. Reminds me of that hair club commercial: I"m not only the Set Owner, I'm also a subscriber.
                    };

                    _db.SubverseSet.Add(newSet);
                    await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

                    _db.SubverseSetSubscription.Add(new SubverseSetSubscription() { SubverseSetID = newSet.ID, UserName = UserName, CreationDate = CurrentDate });
                    await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);

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
