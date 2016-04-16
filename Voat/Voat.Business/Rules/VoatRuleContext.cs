using System;
using Voat.Common;
using Voat.Domain;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules
{
    public class VoatRuleContext : RequestContext
    {
        private Guid _id = Guid.NewGuid();
        private Lazy<UserData> _userData;

        public VoatRuleContext()
        {
            PropertyBag.UserName = System.Threading.Thread.CurrentPrincipal.Identity.Name;

            _userData = new Lazy<UserData>(() =>
            {
                var cmd = new QueryUserData(PropertyBag.UserName);
                var result = cmd.Execute().Result;
                return result;
            });

        }

        public UserData UserData
        {
            get
            {
                return _userData.Value;
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
            get { return PropertyBag.UserName; }
            set { PropertyBag.UserName = value; }
        }

        protected override object GetMissingValue(string name)
        {
            switch (name)
            {
                case "Subverse":
                    if (SubmissionID != null)
                    {
                        var cmd = new QuerySubmission(SubmissionID.Value);
                        var submission = cmd.Execute().Result;
                        PropertyBag.Submission = submission;

                        var cmdSubverse = new QuerySubverse(submission.Subverse);
                        var subverse = cmdSubverse.Execute().Result;

                        return subverse;
                    }
                    if (CommentID != null)
                    {
                        var cmdComment = new QueryComment(CommentID.Value);
                        var comment = cmdComment.Execute().Result;
                        PropertyBag.Comment = comment;

                        var cmd = new QuerySubmission(comment.SubmissionID.Value);
                        var submission = cmd.Execute().Result;
                        PropertyBag.Submission = submission;

                        var cmdSubverse = new QuerySubverse(submission.Subverse);
                        var subverse = cmdSubverse.Execute().Result;

                        return subverse;
                    }
                    break;
            }

            return base.GetMissingValue(name);
        }

        #endregion Convience Accessors
    }
}
