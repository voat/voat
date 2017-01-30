using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Models;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class RuleReportTests : BaseUnitTest
    {

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            //create basic rules
            using (var db = new voatEntities())
            {
                db.RuleSets.Add(new RuleSet() { IsActive = true, ContentType = null, SortOrder = -100, Name = "Spam", Description = "Spam", CreatedBy = "Voat", CreationDate = Voat.Data.Repository.CurrentDate });
                db.RuleSets.Add(new RuleSet() { IsActive = true, ContentType = null, SortOrder = -90, Name = "No Dox", Description = "No Dox Description", CreatedBy = "Voat", CreationDate = Voat.Data.Repository.CurrentDate });
                db.RuleSets.Add(new RuleSet() { IsActive = true, ContentType = null, SortOrder = -80, Name = "No Illegal", Description = "No Illegal Description", CreatedBy = "Voat", CreationDate = Voat.Data.Repository.CurrentDate });

                //add rules per sub
                db.RuleSets.Add(new RuleSet() { IsActive = true, ContentType = null, Subverse = "unit", SortOrder = 1, Name = "Rule #1", Description = "Rule #1", CreatedBy = "unit", CreationDate = Voat.Data.Repository.CurrentDate });
                db.RuleSets.Add(new RuleSet() { IsActive = true, ContentType = null, Subverse = "unit", SortOrder = 2, Name = "Rule #2", Description = "Rule #2", CreatedBy = "unit", CreationDate = Voat.Data.Repository.CurrentDate });

                db.SaveChanges();

            }
        }


        [TestMethod]
        [TestCategory("Reports")]
        public async Task SubmitSubmissionReport_Basic()
        {
            TestHelper.SetPrincipal("unit");

            //first report
            await SubmitAndVerify("unit", ContentType.Submission, 1, 1);
            //duplicate report
            await SubmitAndVerify("unit", ContentType.Submission, 1, 1);
            //alt report, ignore
            await SubmitAndVerify("unit", ContentType.Submission, 1, 2, 0);
        }

        [TestMethod]
        [TestCategory("Reports")]
        public async Task SubmitCommentReport_Basic()
        {
            TestHelper.SetPrincipal("unit");

            //first report
            await SubmitAndVerify("unit", ContentType.Comment, 1, 1);
            //duplicate report
            await SubmitAndVerify("unit", ContentType.Comment, 1, 1);
            //alt report
            await SubmitAndVerify("unit", ContentType.Comment, 1, 2, 0);
        }

        [TestMethod]
        [TestCategory("Reports")]
        public async Task GroupedSubmissionReports()
        {
            TestHelper.SetPrincipal("unit");
            var cmd = new CreateSubmissionCommand(new UserSubmission() { Subverse = "unit", Title = "This is spammy spam", Content = "http://somespamsite.com" });
            var response = await cmd.Execute();
            Assert.IsTrue(response.Success, response.Message);
            var submission = response.Response;

            string userName = "unit";
            TestHelper.SetPrincipal(userName);
            await SubmitAndVerify(userName, ContentType.Submission, submission.ID, 1);
            int ruleid = 1;

            for (int i = 1; i < 20; i++)
            {
                userName = $"TestUser{i.ToString().PadLeft(2, '0')}";
                TestHelper.SetPrincipal(userName);
                await SubmitAndVerify(userName, ContentType.Submission, submission.ID, ruleid);
                ruleid++;
                if (ruleid > 5)
                    ruleid = 1;
            }

        }


        private async Task SubmitAndVerify(string userName, ContentType contentType, int id, int ruleID, int expectedCount = 1)
        {
            var cmd = new ReportContentCommand(contentType, id, ruleID);
            var r = await cmd.Execute();
            Assert.IsTrue(r.Success, r.Message);
            int count = 0;

            using (var db = new voatEntities())
            {
                if (contentType == ContentType.Submission)
                {
                    count = db.RuleReports.Where(x => x.CreatedBy == userName && x.RuleSetID == ruleID && x.SubmissionID == id && x.CommentID == null).Count();
                }
                else
                {
                    count = db.RuleReports.Where(x => x.CreatedBy == userName && x.RuleSetID == ruleID && x.CommentID == id).Count();
                }
            }
            Assert.AreEqual(expectedCount, count);
        }
    }
}
