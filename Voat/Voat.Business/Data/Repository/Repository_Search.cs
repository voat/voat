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
        #region Search

    

        public async Task<IEnumerable<DomainReferenceDetails>> SearchDomainObjects(DomainType domainType, SearchOptions options)
        {

            var q = BaseDomainObjectSearch(domainType, options);

            var query = q.ToString();

            var results = await _db.Database.Connection.QueryAsync<DomainReferenceDetails>(query, q.Parameters);

            return results;

        }
        public async Task<IEnumerable<DomainReferenceDetails>> UserSubscribedSetDetails(string userName, SearchOptions options)
        {

            var q = BaseDomainObjectSearch(DomainType.Set, options);

            q.Select += $" INNER JOIN {SqlFormatter.Table("SubverseSetSubscription", "setSubscription", null, "NOLOCK")} ON setSubscription.\"SubverseSetID\" = s.\"ID\"";
            q.Append(x => x.Where, "setSubscription.\"UserName\" = @UserName");
            q.Parameters.Add("UserName", userName);

            //Reset ispublic on base query
            q.Parameters.Add("IsPublic", (bool?)null);

            var query = q.ToString();
            var results = await _db.Database.Connection.QueryAsync<DomainReferenceDetails>(query, q.Parameters);

            return results;
        }

        private DapperQuery BaseDomainObjectSearch(DomainType domainType, SearchOptions options)
        {
            var q = new DapperQuery();
            var hasPhrase = !String.IsNullOrEmpty(options.Phrase);

            switch (domainType)
            {
                case DomainType.Subverse:
                    q.SelectColumns = "\"Type\" = @DomainType, s.\"Name\", s.\"Title\", s.\"Description\", s.\"CreatedBy\" AS \"OwnerName\", s.\"SubscriberCount\", s.\"CreationDate\"";
                    q.Select = $"DISTINCT {"{0}"} FROM {SqlFormatter.Table("Subverse", "s", null, "NOLOCK")}";
                    if (hasPhrase)
                    {
                        q.Where = "(s.\"Name\" LIKE CONCAT('%', @SearchPhrase, '%') OR s.\"Title\" LIKE CONCAT('%', @SearchPhrase, '%') OR s.\"Description\" LIKE CONCAT('%', @SearchPhrase, '%'))";
                    }
                    q.Append(x => x.Where, $"s.\"IsAdminDisabled\" = {SqlFormatter.BooleanLiteral(false)} AND s.\"IsPrivate\" = {SqlFormatter.BooleanLiteral(false)}");
                    break;
                case DomainType.Set:
                    q.SelectColumns = "\"Type\" = @DomainType, s.\"Name\", s.\"Title\", s.\"Description\", s.\"UserName\" AS \"OwnerName\", s.\"SubscriberCount\", s.\"CreationDate\"";
                    q.Select = $"DISTINCT {"{0}"} FROM {SqlFormatter.Table("SubverseSet", "s", null, "NOLOCK")}";
                    if (hasPhrase)
                    {
                        q.Where = "(s.\"Name\" LIKE CONCAT('%', @SearchPhrase, '%') OR s.\"Title\" LIKE CONCAT('%', @SearchPhrase, '%') OR s.\"Description\" LIKE CONCAT('%', @SearchPhrase, '%'))";
                    }
                    q.Append(x => x.Where, "(s.\"IsPublic\" = @IsPublic OR @IsPublic IS NULL)");
                    break;
            }

            q.Parameters.Add("DomainType", (int)domainType);
            q.Parameters.Add("IsPublic", (bool?)true);

            if (hasPhrase)
            {
                q.Parameters.Add("SearchPhrase", options.Phrase);
            }

            switch (options.Sort)
            {
                case SortAlgorithm.Active:
                    switch (domainType)
                    {
                        case DomainType.Subverse:
                            q.Select += $" INNER JOIN {SqlFormatter.Table("Submission", "sub", null, "NOLOCK")} ON sub.\"Subverse\" = s.\"Name\"";
                            q.SelectColumns += ", MAX(sub.\"CreationDate\") AS \"ThisIsOnlyUsedForSortingByActive\"";
                            q.GroupBy = "s.\"Name\", s.\"Title\", s.\"Description\", s.\"CreatedBy\", s.\"SubscriberCount\", s.\"CreationDate\"";
                            q.OrderBy = "MAX(sub.\"CreationDate\") DESC";
                            break;
                        case DomainType.Set:
                            q.Select += $" INNER JOIN {SqlFormatter.Table("SubverseSetList", "subList", null, "NOLOCK")} ON subList.\"SubverseSetID\" = s.\"ID\"";
                            q.SelectColumns += ", MAX(subList.\"CreationDate\") AS \"ThisIsOnlyUsedForSortingByActive\"";
                            q.GroupBy = "s.\"Name\", s.\"Title\", s.\"Description\", s.\"UserName\", s.\"SubscriberCount\", s.\"CreationDate\"";
                            q.OrderBy = "MAX(subList.\"CreationDate\") DESC";
                            break;
                    }
                    break;
                case SortAlgorithm.New:
                    q.OrderBy = "s.\"CreationDate\" DESC";
                    break;
                case SortAlgorithm.Top:
                default:
                    q.OrderBy = "s.\"SubscriberCount\" DESC";
                    break;
            }

            q.SkipCount = options.Index;
            q.TakeCount = options.Count;

            return q;
        }
        #endregion
    }
}
