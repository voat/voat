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
using System.Security.Principal;
using Voat.Domain;
using Voat.Domain.Query;
using Voat.RulesEngine;

namespace Voat.Rules
{
    public class VoatRuleContext : RequestContext
    {
        private Guid _id = Guid.NewGuid();
        private UserData _userData;
        private IPrincipal _principal;

        public VoatRuleContext(IPrincipal principal)
        {
            _principal = principal;
        }

        public IPrincipal User
        {
            get
            {
                return _principal;
            }
        }
        public UserData UserData
        {
            get
            {
                if (_userData == null)
                {
                    //var cmd = new QueryUserData(PropertyBag.UserName);
                    _userData = new UserData(User);
                }
                return _userData;
            }
        }

        #region Convience Accessors

        public int? CommentID
        {
            get
            {
                return PropertyBag.CommentID;
            }

            set
            {
                PropertyBag.CommentID = value;
            }
        }

        public int? SubmissionID
        {
            get
            {
                return PropertyBag.SubmissionID;
            }

            set
            {
                PropertyBag.SubmissionID = value;
            }
        }

        public Data.Models.Subverse Subverse
        {
            get
            {
                return PropertyBag.Subverse;
            }

            set
            {
                PropertyBag.Subverse = value;
            }
        }

        public string UserName
        {
            get { return User.Identity.Name; }
        }

        protected override object GetMissingValue(string name)
        {
            //TODO: This lazy loading needs to be optimized and rewritten. I. Don't. Like. This.
            switch (name)
            {
                case "Comment":
                    if (CommentID != null)
                    {
                        var cmdComment = new QueryComment(CommentID.Value);
                        var comment = cmdComment.Execute();
                        PropertyBag.Comment = comment;

                        return comment;
                    }
                    break;
                case "Submission":
                    if (SubmissionID != null)
                    {
                        var cmd = new QuerySubmission(SubmissionID.Value);
                        var submission = cmd.Execute();
                        return submission;
                    }
                    if (CommentID != null)
                    {
                        var cmdComment = new QueryComment(CommentID.Value);
                        var comment = cmdComment.Execute();
                        PropertyBag.Comment = comment;

                        var cmd = new QuerySubmission(comment.SubmissionID.Value);
                        var submission = cmd.Execute();
                        return submission;
                    }
                    break;
                case "Subverse":
                    if (SubmissionID != null)
                    {
                        var cmd = new QuerySubmission(SubmissionID.Value);
                        var submission = cmd.Execute();
                        PropertyBag.Submission = submission;

                        var cmdSubverse = new QuerySubverse(submission.Subverse);
                        var subverse = cmdSubverse.Execute();

                        return subverse;
                    }
                    if (CommentID != null)
                    {
                        var cmdComment = new QueryComment(CommentID.Value);
                        var comment = cmdComment.Execute();
                        PropertyBag.Comment = comment;

                        var cmd = new QuerySubmission(comment.SubmissionID.Value);
                        var submission = cmd.Execute();
                        PropertyBag.Submission = submission;

                        var cmdSubverse = new QuerySubverse(submission.Subverse);
                        var subverse = cmdSubverse.Execute();

                        return subverse;
                    }
                    break;
            }

            return base.GetMissingValue(name);
        }

        #endregion Convience Accessors
    }
}
