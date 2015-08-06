using Voat.RulesEngine;

namespace Voat.Rules
{
    public class VoatRuleContext : RuleContext
    {


        public VoatRuleContext(string userName)
        {
            base.PropertyBag.UserName = userName;
        }

        public VoatRuleContext()
        {
            base.PropertyBag.UserName = System.Threading.Thread.CurrentPrincipal.Identity.Name;
        }

        #region Convience Accessors

        public string UserName
        {
            get { return PropertyBag.UserName; }
            set { PropertyBag.UserName = value; }
        }

        public int? CommentID
        {
            get { return PropertyBag.CommentID; }
            set { PropertyBag.CommentID = value; }
        }
        public int? SubmissionID
        {
            get { return PropertyBag.SubmissionID; }
            set { PropertyBag.SubmissionID = value; }
        }
        public int? VoteValue
        {
            get { return PropertyBag.VoteValue; }
            set { PropertyBag.VoteValue = value; }
        }

        public string SubverseName
        {
            get { return PropertyBag.SubverseName; }
            set { PropertyBag.SubverseName = value; }
        }

        #endregion

    }
}