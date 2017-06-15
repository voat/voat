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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class RuleReportTests : BaseUnitTest
    {

        public override void ClassInitialize()
        {
            //create basic rules
            using (var db = new VoatDataContext())
            {
                db.RuleSet.Add(new RuleSet() { IsActive = true, ContentType = null, SortOrder = -100, Name = "Spam", Description = "Spam", CreatedBy = "Voat", CreationDate = Voat.Data.Repository.CurrentDate });
                db.RuleSet.Add(new RuleSet() { IsActive = true, ContentType = null, SortOrder = -90, Name = "No Dox", Description = "No Dox Description", CreatedBy = "Voat", CreationDate = Voat.Data.Repository.CurrentDate });
                db.RuleSet.Add(new RuleSet() { IsActive = true, ContentType = null, SortOrder = -80, Name = "No Illegal", Description = "No Illegal Description", CreatedBy = "Voat", CreationDate = Voat.Data.Repository.CurrentDate });

                //add rules per sub
                db.RuleSet.Add(new RuleSet() { IsActive = true, ContentType = null, Subverse = SUBVERSES.Unit, SortOrder = 1, Name = "Rule #1", Description = "Rule #1", CreatedBy = USERNAMES.Unit, CreationDate = Voat.Data.Repository.CurrentDate });
                db.RuleSet.Add(new RuleSet() { IsActive = true, ContentType = null, Subverse = SUBVERSES.Unit, SortOrder = 2, Name = "Rule #2", Description = "Rule #2", CreatedBy = USERNAMES.Unit, CreationDate = Voat.Data.Repository.CurrentDate });

                db.SaveChanges();

            }
        }


        [TestMethod]
        [TestCategory("Reports")]
        public async Task SubmitSubmissionReport_Basic()
        {
            var userName = USERNAMES.Unit;
            var user = TestHelper.SetPrincipal(userName);

            //first report
            await SubmitAndVerify(user, ContentType.Submission, 1, 1);
            //duplicate report
            await SubmitAndVerify(user, ContentType.Submission, 1, 1);
            //alt report, ignore
            await SubmitAndVerify(user, ContentType.Submission, 1, 2, 0);
        }

        [TestMethod]
        [TestCategory("Reports")]
        public async Task SubmitCommentReport_Basic()
        {
            var userName = USERNAMES.Unit;
            var user = TestHelper.SetPrincipal(userName);

            //first report
            await SubmitAndVerify(user, ContentType.Comment, 1, 1);
            //duplicate report
            await SubmitAndVerify(user, ContentType.Comment, 1, 1);
            //alt report
            await SubmitAndVerify(user, ContentType.Comment, 1, 2, 0);
        }

        [TestMethod]
        [TestCategory("Reports")]
        public async Task GroupedSubmissionReports()
        {
            var userName = USERNAMES.Unit;
            var user = TestHelper.SetPrincipal(userName);
            var cmd = new CreateSubmissionCommand(new UserSubmission() { Subverse = SUBVERSES.Unit, Title = "This is spammy spam", Content = "http://somespamsite.com" }).SetUserContext(user);
            var response = await cmd.Execute();
            Assert.IsTrue(response.Success, response.Message);
            var submission = response.Response;

            userName = USERNAMES.Unit;
            user = TestHelper.SetPrincipal(userName);
            await SubmitAndVerify(user, ContentType.Submission, submission.ID, 1);
            int ruleid = 1;

            for (int i = 1; i < 20; i++)
            {
                userName = $"TestUser{i.ToString().PadLeft(2, '0')}";
                user = TestHelper.SetPrincipal(userName);
                await SubmitAndVerify(user, ContentType.Submission, submission.ID, ruleid);
                ruleid++;
                if (ruleid > 5)
                    ruleid = 1;
            }

        }


        private async Task SubmitAndVerify(IPrincipal user, ContentType contentType, int id, int ruleID, int expectedCount = 1)
        {
            var cmd = new ReportContentCommand(contentType, id, ruleID).SetUserContext(user);
            var r = await cmd.Execute();
            Assert.IsTrue(r.Success, r.Message);
            int count = 0;

            using (var db = new VoatDataContext())
            {
                if (contentType == ContentType.Submission)
                {
                    count = db.RuleReport.Where(x => x.CreatedBy == user.Identity.Name && x.RuleSetID == ruleID && x.SubmissionID == id && x.CommentID == null).Count();
                }
                else
                {
                    count = db.RuleReport.Where(x => x.CreatedBy == user.Identity.Name && x.RuleSetID == ruleID && x.CommentID == id).Count();
                }
            }
            Assert.AreEqual(expectedCount, count);
        }
    }
}
