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

namespace Voat.Domain.Query
{
    //    public static class QueryProcessor
    //    {
    //        public static Action<Data.Models.Submission> AnonSubmission = new Action<Data.Models.Submission>(x => { if (x.IsAnonymized) x.UserName = x.ID.ToString(); });
    //        public static Action<Comment> AnonComment = new Action<Comment>(x => { if (x.IsAnonymized) x.UserName = x.ID.ToString(); });
    //        public static Action<usp_CommentTree_Result> AnonCommentTree = new Action<usp_CommentTree_Result>(x => { if (x.IsAnonymized) x.UserName = x.ID.ToString(); });
    //    }

    //    public static class QuerySelectors
    //    {
    //        public static readonly Func<Subverse, domain.SubverseInformation> ToSubverseInformation =
    //           new Func<Subverse, domain.SubverseInformation>(x =>
    //                new domain.SubverseInformation()
    //                {
    //                    Name = x.Name,
    //                    Title = x.Title,
    //                    Description = x.Description,
    //                    CreationDate = x.CreationDate,
    //                    SubscriberCount = (x.SubscriberCount == null ? 0 : x.SubscriberCount.Value),
    //                    RatedAdult = x.IsAdult,
    //                    Sidebar = x.SideBar,
    //                    Type = x.Type
    //                });
    //    }
}
