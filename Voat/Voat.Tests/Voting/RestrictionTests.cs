using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Voat.Common;
using Voat.Domain.Command;
using Voat.Tests.Infrastructure;
using Voat.Voting;
using Voat.Voting.Options;
using Voat.Voting.Restrictions;

namespace Voat.Tests.Voting
{
    [TestClass]
    public class RestrictionTests
    {
        [TestMethod]
        public void TestRestrictionGrouping()
        {
            var r = new Voat.Data.Models.VoteRestriction()
            {
                ID = 1,
                GroupName = "Default",
                Type = typeof(ContributionCountRestriction).ShortAssemblyQualifiedName(),
                Options = (new ContentOption() {
                    ContentType = Domain.Models.ContentType.Comment,
                    Duration = TimeSpan.FromDays(14),
                    Subverse = "unit",
                    MinimumCount = 1,
                    EndDate = DateTime.UtcNow
                }).Serialize(),
                VoteID = 1
            };

            var constructed = (IVoteRestriction)OptionHandler.Construct(r.Type, r.Options);
            var user = TestHelper.SetPrincipal("User500CCP");
            var outcome = constructed.Evaluate(user);

            Assert.AreEqual(Status.Success, outcome.Status);


            var restrictionSet = new VoteRestrictionSet();
            restrictionSet.Populate(new Voat.Data.Models.VoteRestriction[] { r });

            var eval = restrictionSet.Evaluate(user);
            Assert.IsTrue(eval.IsValid);

        }
    }
}
