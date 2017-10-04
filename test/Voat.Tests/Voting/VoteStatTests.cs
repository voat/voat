using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Tests.Infrastructure;
using Voat.Voting.Models;

namespace Voat.Tests.Voting
{
    [TestClass]
    [TestCategory("Vote")]
    public class VoteStatTests
    {
        [TestMethod]
        public void EnsureVoteStatsCalculate_Empty()
        {
            var stats = new VoteStatistics();
            var allstats = stats.All;
            var friendly = stats.Friendly;

            Assert.IsNotNull(friendly);
            Assert.AreEqual(0, friendly.Keys.Count);

        }

        [TestMethod]
        public async Task Verify_Database_Stats()
        {
            var user = TestHelper.SetPrincipal("unit");

            var createVote = new PersistVoteCommand(new Domain.Models.Vote()
            {
                Title = "Unit Test Vote For Vote Stats",
                Content = "We often wonder if you are willing to do it, so are you?",
                Subverse = "unit",
                Options = new List<Domain.Models.VoteOption>() {
                    new Domain.Models.VoteOption(){ Title = "Yes", Content = "This is most likely a yes" },
                    new Domain.Models.VoteOption(){ Title = "No", Content = "This is most likely a no" },
                    new Domain.Models.VoteOption(){ Title = "Maybe", Content = "This is most likely a maybe" }
                },
                DisplayStatistics = true
            }).SetUserContext(user);

            var response = await createVote.Execute();
            VoatAssert.IsValid(response);
            var vote = response.Response;

            var seed = 5;
            var multiple = 0;

            using (var db = new VoatDataContext())
            {
                foreach (var option in vote.Options)
                {
                    for (int i = 0; i < seed; i++)
                    {
                        db.VoteTracker.Add(new VoteTracker() { CreationDate = DateTime.UtcNow, VoteID = vote.ID, VoteOptionID = option.ID, RestrictionsPassed = true, UserName = $"UnitTestUser{(multiple * (i + 1)).ToString().PadLeft(2,'0')}" });
                        db.VoteTracker.Add(new VoteTracker() { CreationDate = DateTime.UtcNow, VoteID = vote.ID, VoteOptionID = option.ID, RestrictionsPassed = false, UserName = $"UnitTestUser{(multiple * (i + 1)).ToString().PadLeft(2, '0')}" });
                    }
                    multiple += 1;
                }
                db.SaveChanges();
            }



            using (var repo = new Voat.Data.Repository())
            {
                var data = await repo.GetVoteStatistics(vote.ID);

                Assert.IsNotNull(data);
                var friendly = data.Friendly;
                Assert.AreEqual(3, friendly.Count);

                foreach (var x in friendly)
                {
                    foreach (var voteOption in x.Value)
                    {
                        Assert.AreEqual(x.Key == VoteRestrictionStatus.All ? seed * 2 : seed, voteOption.Value.Count);
                        //Assert.AreEqual(5, voteOption.Value.Count);
                    }
                }
            }
        }

        [TestMethod]
        public void EnsureVoteStatsCalculate_Certified_Uncertified()
        {

            var stats = new VoteStatistics();

            var passedDictionary = new Dictionary<int, int>() {
                    { 1, 5 },
                    { 2, 5 },
                    { 3, 5 },
                };

            var failedDictionary = new Dictionary<int, int>() {
                    { 2, 2 },
                    { 3, 3 },
                    { 4, 4 }
               };

            stats.Raw.Add(VoteRestrictionStatus.Certified,
                passedDictionary
            );

            stats.Raw.Add(VoteRestrictionStatus.Uncertified,
                failedDictionary
            );

            var allstats = stats.All;

            var friendly = stats.Friendly;
            Assert.IsNotNull(friendly);
            Assert.AreEqual(3, friendly.Keys.Count);

            foreach (var key in friendly.Keys)
            {
                var friendlyDictionary = friendly[key];
                var sum = friendlyDictionary.Values.Sum(x => x.Percentage);
                sum = Math.Round(sum);
                Assert.AreEqual(1, sum);

                Dictionary<int, int> dict = stats.All[key];

                if (key == VoteRestrictionStatus.All)
                {
                    var keys = passedDictionary.Keys.Union(failedDictionary.Keys);
                    var summedold = keys.ToDictionary(k => k, k => (passedDictionary.Keys.Contains(k) ? passedDictionary[k] : 0) + (failedDictionary.Keys.Contains(k) ? failedDictionary[k] : 0));

                    dict = summedold;
                }
                else if (key == VoteRestrictionStatus.Certified)
                {
                    dict = passedDictionary;
                }
                else if (key == VoteRestrictionStatus.Uncertified)
                {
                    dict = failedDictionary;
                }

                Assert.AreEqual(friendlyDictionary.Count, dict.Count);

                var dictCount = dict.Values.Sum();
                var friendCount = friendlyDictionary.TotalCount;
                Assert.AreEqual(dictCount, friendCount);

                foreach (var keyPair in friendlyDictionary)
                {
                    Assert.AreEqual(keyPair.Value.Count, dict[keyPair.Key]);
                }
            }
        }
        [TestMethod]
        public void EnsureVoteStatsCalculate_Certified()
        {

            var stats = new VoteStatistics();

            var passedDictionary = new Dictionary<int, int>() {
                    { 1, 5 },
                    { 2, 5 },
                    { 3, 5 },
                };

            
            stats.Raw.Add(VoteRestrictionStatus.Certified,
                passedDictionary
            );

            var allstats = stats.All;

            var friendly = stats.Friendly;
            Assert.IsNotNull(friendly);
            Assert.AreEqual(1, friendly.Keys.Count);

            //foreach (var key in friendly.Keys)
            //{
            //    var friendlyDictionary = friendly[key];
            //    var sum = friendlyDictionary.Values.Sum(x => x.Percentage);
            //    sum = Math.Round(sum);
            //    Assert.AreEqual(1, sum);

            //    Dictionary<int, int> dict = stats.All[key];

            //    if (key == VoteRestrictionStatus.All)
            //    {
            //        var keys = passedDictionary.Keys.Union(failedDictionary.Keys);
            //        var summedold = keys.ToDictionary(k => k, k => (passedDictionary.Keys.Contains(k) ? passedDictionary[k] : 0) + (failedDictionary.Keys.Contains(k) ? failedDictionary[k] : 0));

            //        dict = summedold;
            //    }
            //    else if (key == VoteRestrictionStatus.Certified)
            //    {
            //        dict = passedDictionary;
            //    }
            //    else if (key == VoteRestrictionStatus.Uncertified)
            //    {
            //        dict = failedDictionary;
            //    }

            //    Assert.AreEqual(friendlyDictionary.Count, dict.Count);

            //    var dictCount = dict.Values.Sum();
            //    var friendCount = friendlyDictionary.TotalCount;
            //    Assert.AreEqual(dictCount, friendCount);

            //    foreach (var keyPair in friendlyDictionary)
            //    {
            //        Assert.AreEqual(keyPair.Value.Count, dict[keyPair.Key]);
            //    }
            //}
        }
    }
}
