using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Models;

namespace Voat.Data
{
    /// <summary>
    /// Extension methods for mappings
    /// </summary>
    public static class ModelMaps
    {
        public static NestedComment Map(this usp_CommentTree_Result treeComment)
        {
            var nc = new NestedComment();

            nc.ID = treeComment.ID;
            nc.ParentID = treeComment.ParentID;
            nc.ChildCount = treeComment.ChildCount;
            nc.Content = treeComment.Content;
            nc.FormattedContent = treeComment.FormattedContent;
            nc.UserName = treeComment.UserName;
            nc.UpCount = (int)treeComment.UpCount;
            nc.DownCount = (int)treeComment.DownCount;
            nc.CreationDate = treeComment.CreationDate;
            nc.IsAnonymized = treeComment.IsAnonymized;
            nc.IsDeleted = treeComment.IsDeleted;
            nc.IsDistinguished = treeComment.IsDistinguished;
            nc.LastEditDate = treeComment.LastEditDate;
            nc.SubmissionID = treeComment.SubmissionID;
            nc.Subverse = treeComment.Subverse;

            return nc;
        }

    }
}
