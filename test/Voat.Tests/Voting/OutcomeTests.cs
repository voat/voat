using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Voting.Outcomes;

namespace Voat.Tests.Voting
{
    [TestClass]
    public class OutcomeTests
    {
        [TestMethod]
        [TestCategory("Vote"), TestCategory("Vote.Outcome")]
        public async Task Add_Remove_Moderator_Outcome_Test()
        {
            var addModOutcome = new AddModeratorOutcome();
            addModOutcome.UserName = "UnitTestUser01";
            addModOutcome.Level = Domain.Models.ModeratorLevel.Owner;
            addModOutcome.Subverse = "whatever";

            var response = await addModOutcome.Execute();
            VoatAssert.IsValid(response);

            using (var db = new VoatDataContext())
            {
                var record = db.SubverseModerator.Where(x => x.Subverse == addModOutcome.Subverse && x.UserName == addModOutcome.UserName).FirstOrDefault();
                Assert.IsNotNull(record);
                Assert.AreEqual(addModOutcome.UserName, record.UserName);
                Assert.AreEqual(addModOutcome.Subverse, record.Subverse);
                Assert.AreEqual((int)addModOutcome.Level, record.Power);

            }

            var removeModOutcome = new RemoveModeratorOutcome();
            removeModOutcome.UserName = addModOutcome.UserName;
            removeModOutcome.Subverse = addModOutcome.Subverse;

            response = await removeModOutcome.Execute();
            VoatAssert.IsValid(response);

            using (var db = new VoatDataContext())
            {
                var record = db.SubverseModerator.Where(x => x.Subverse == addModOutcome.Subverse && x.UserName == addModOutcome.UserName).FirstOrDefault();
                Assert.IsNull(record);
            }

        }
    }
}
