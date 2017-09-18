using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Voat.Common;
using Voat.Domain.Command;
using Voat.Tests.Infrastructure;
using Voat.Voting;
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
                //GroupName = "Default",
                Type = typeof(ContributionCountRestriction).Name,
                Data = (new ContributionCountRestriction() {
                    ContentType = (ContentTypeRestriction)(Domain.Models.ContentType.Comment | Domain.Models.ContentType.Submission),
                    Duration = TimeSpan.FromDays(180),
                    Subverse = "unit",
                    MinimumCount = 1,
                    EndDate = DateTime.UtcNow
                }).Serialize(),
                VoteID = 1
            };

            var constructed = (IVoteRestriction)VoteItem.Deserialize<VoteItem>(r.Data);
            var user = TestHelper.SetPrincipal(USERNAMES.User500CCP);
            var outcome = constructed.Evaluate(user);

            VoatAssert.IsValid(outcome);


            var restrictionSet = new VoteRestrictionSet();
            restrictionSet.Populate(new Voat.Data.Models.VoteRestriction[] { r });

            var eval = restrictionSet.Evaluate(user);
            Assert.IsTrue(eval.IsValid);

        }
    }
}
