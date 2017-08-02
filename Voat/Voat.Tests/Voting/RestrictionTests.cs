using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
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
                Type = typeof(ContributionCountRestriction).FullName,
                Options = (new ContentOption() {
                    ContentType = Domain.Models.ContentType.Comment,
                    Duration = TimeSpan.FromDays(14),
                    Subverse = "AskVoat",
                    MinimumCount = 100,
                    CutOffDate = DateTime.Now
                }).ToString(),
                VoteID = 1
            };

            var constructed = VoteOptionItem.Construct(r.Type, r.Options);
            var description = constructed.ToString();
            
        }
    }
}
