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
using System.Threading.Tasks;
using Voat.Domain.Models;
using Dapper;


namespace Voat.Data
{
    public partial class Repository 
    {
        #region Search

    

        public async Task<IEnumerable<DomainReferenceDetails>> SearchDomainObjects(DomainType domainType, SearchOptions options)
        {

            var q = BaseDomainObjectSearch(domainType, options);

            var query = q.ToString();

            var results = await _db.Connection.QueryAsync<DomainReferenceDetails>(query, q.Parameters);

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
            var results = await _db.Connection.QueryAsync<DomainReferenceDetails>(query, q.Parameters);

            return results;
        }

        private DapperQuery BaseDomainObjectSearch(DomainType domainType, SearchOptions options)
        {
            var q = new DapperQuery();
            var hasPhrase = !String.IsNullOrEmpty(options.Phrase);

            switch (domainType)
            {
                case DomainType.Subverse:
                    q.SelectColumns = "@DomainType as \"Type\", s.\"Name\", s.\"Title\", s.\"Description\", s.\"CreatedBy\" AS \"OwnerName\", s.\"SubscriberCount\", s.\"CreationDate\"";
                    q.Select = $"DISTINCT {"{0}"} FROM {SqlFormatter.Table("Subverse", "s", null, "NOLOCK")}";
                    if (hasPhrase)
                    {
                        q.Where = "(s.\"Name\" LIKE CONCAT('%', @SearchPhrase, '%') OR s.\"Title\" LIKE CONCAT('%', @SearchPhrase, '%') OR s.\"Description\" LIKE CONCAT('%', @SearchPhrase, '%'))";
                    }
                    q.Append(x => x.Where, $"s.\"IsAdminDisabled\" = {SqlFormatter.BooleanLiteral(false)} AND s.\"IsPrivate\" = {SqlFormatter.BooleanLiteral(false)}");
                    break;
                case DomainType.Set:
                    q.SelectColumns = "@DomainType as \"Type\", s.\"Name\", s.\"Title\", s.\"Description\", s.\"UserName\" AS \"OwnerName\", s.\"SubscriberCount\", s.\"CreationDate\"";
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
