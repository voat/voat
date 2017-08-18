using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Voat.Common;
using Voat.Configuration;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.Utils
{
    [TestClass]
    
    public class ActivityContextTests : BaseUnitTest
    {

        [TestMethod]
        [TestCategory("ActivityContext")]
        public void TestCommandActivitySerialization()
        {
            var settings = JsonSettings.DataSerializationSettings;
            var user = TestHelper.SetPrincipal("Frank", "admin", "user");

            var userSubmission = new UserSubmission() { Content = "This is content", Title = "This is title", Url = "http://thisisurl.com", Subverse = "ThisIsSubverse" };
            var command = new CreateSubmissionCommand(userSubmission).SetUserContext(user);
            var activity = (ActivityContext)command.Context;

            var json = JsonConvert.SerializeObject(command, settings);

            var newCommand = JsonConvert.DeserializeObject<CreateSubmissionCommand>(json, settings);
            var newUserSubmission = newCommand.UserSubmission;
            var newActivity = (ActivityContext)newCommand.Context;


            Assert.AreEqual(userSubmission.Content, newUserSubmission.Content);
            Assert.AreEqual(userSubmission.Title, newUserSubmission.Title);
            Assert.AreEqual(userSubmission.Url, newUserSubmission.Url);
            Assert.AreEqual(userSubmission.Subverse, newUserSubmission.Subverse);

            Assert.AreEqual(activity.ActivityID, newActivity.ActivityID);
            Assert.AreEqual(activity.StartDate, newActivity.StartDate);
            Assert.AreEqual(activity.EndDate, newActivity.EndDate);

            Assert.AreEqual(activity.User.Identity.Name, newActivity.User.Identity.Name);
            //We want to ensure deserialization does not retain these VoatSettings.Instance.
            Assert.AreEqual(false, newActivity.User.Identity.IsAuthenticated);
            Assert.AreEqual("", newActivity.User.Identity.AuthenticationType);

        }

        [TestMethod]
        [TestCategory("ActivityContext")]
        public void TestActivitySerialization()
        {
            var settings =  JsonSettings.DataSerializationSettings;
            var user = TestHelper.SetPrincipal("Frank", "admin", "user");

            var originalContext = new ActivityContext(user);
            var json = JsonConvert.SerializeObject(originalContext, settings );

            var newContext = JsonConvert.DeserializeObject<ActivityContext>(json, settings);

            Assert.AreEqual(originalContext.ActivityID, newContext.ActivityID);
            Assert.AreEqual(originalContext.StartDate, newContext.StartDate);
            Assert.AreEqual(originalContext.EndDate, newContext.EndDate);

            Assert.AreEqual(originalContext.User.Identity.Name, newContext.User.Identity.Name);
            
            //We want to ensure deserialization does not retain these VoatSettings.Instance.
            Assert.AreEqual(false, newContext.User.Identity.IsAuthenticated);
            Assert.AreEqual("", newContext.User.Identity.AuthenticationType);

        }
        [TestMethod]
        [TestCategory("ActivityContext")]
        public void TestActivityDispose()
        {
            var settings =  JsonSettings.DataSerializationSettings;
            var user = TestHelper.SetPrincipal("Frank", "admin", "user");
            ActivityContext refContext = null;

            using (var originalContext = new ActivityContext(user))
            {
                refContext = originalContext;
                Assert.AreEqual(null, originalContext.EndDate);
            }
            Assert.AreNotEqual(null, refContext.EndDate);
        }
    }
}
