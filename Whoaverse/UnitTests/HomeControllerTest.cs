using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Moq;
using Voat.Controllers;
using Voat.Models;
using Voat.Services.UnitTests;
using Voat.Utils;
using Xunit;

namespace UnitTests
{
    public class SubverseServiceTests
    {
        public static ControllerContext GetControllerContext(bool isLoggedIn)
        {
            var mock = new Mock<ControllerContext>();

            mock.SetupGet(p => p.HttpContext.User.Identity.Name).Returns(isLoggedIn ? "Testing" : string.Empty);
            mock.SetupGet(p => p.HttpContext.User.Identity.IsAuthenticated).Returns(isLoggedIn);
            mock.SetupGet(p => p.HttpContext.Request.IsAuthenticated).Returns(isLoggedIn);
            
            return mock.Object;
        }

        public class IndexMethod
        {
            [Theory, VoatTest]
            public void PullsDefaultFrontpage(whoaverseEntities db, Defaultsubverse defaultsubverse, List<Message> messages)
            {
                // Arrange
                // TODO: See if we can coax Autofixture into handling this for us
                if (defaultsubverse.name.Length > 20)
                    defaultsubverse.name = defaultsubverse.name.Substring(0, 19);
                db.Defaultsubverses.Add(defaultsubverse);
                db.Subverses.Add(new Subverse {name = defaultsubverse.name, title = new Guid().ToString()});
                db.SaveChanges();

                foreach (var message in messages)
                {
                    // TODO: See if we can coax Autofixture into handling this for us
                    if (message.Thumbnail.Length > 40)
                        message.Thumbnail = message.Thumbnail.Substring(0, 39);
                    message.Subverse = defaultsubverse.name;
                }
                db.Messages.AddRange(messages);
                db.SaveChanges();

                var homeController = new HomeController(db);
                homeController.ControllerContext = GetControllerContext(false);

                // Act
                var actionResult = (ViewResult) homeController.Index(null);
                var model = (PaginatedList<Message>) actionResult.Model;

                // Assert
                Assert.True(model.Count == messages.Count);
            }

            [Theory, VoatTest]
            public void PullsDefaultFrontpageWithNoMessagesFromNonDefaults(whoaverseEntities db, Defaultsubverse defaultsubverse, string nonDefaultName, List<Message> messages, List<Message> nonDefaultMessages)
            {
                // Arrange
                // TODO: See if we can coax Autofixture into handling this for us
                if (defaultsubverse.name.Length > 20)
                    defaultsubverse.name = defaultsubverse.name.Substring(0, 19);
                if (nonDefaultName.Length > 20)
                    nonDefaultName = nonDefaultName.Substring(0, 19);

                db.Defaultsubverses.Add(defaultsubverse);
                db.Subverses.Add(new Subverse { name = defaultsubverse.name, title = new Guid().ToString() });
                db.Subverses.Add(new Subverse { name = nonDefaultName, title = new Guid().ToString() });
                db.SaveChanges();

                foreach (var message in messages)
                {
                    // TODO: See if we can coax Autofixture into handling this for us
                    if (message.Thumbnail.Length > 40)
                        message.Thumbnail = message.Thumbnail.Substring(0, 39);
                    message.Subverse = defaultsubverse.name;
                }
                db.Messages.AddRange(messages);

                foreach (var message in nonDefaultMessages)
                {
                    // TODO: See if we can coax Autofixture into handling this for us
                    if (message.Thumbnail.Length > 40)
                        message.Thumbnail = message.Thumbnail.Substring(0, 39);
                    message.Subverse = nonDefaultName;
                }
                db.Messages.AddRange(nonDefaultMessages);
                
                db.SaveChanges();
                
                var homeController = new HomeController(db);
                homeController.ControllerContext = GetControllerContext(false);

                // Act
                var actionResult = (ViewResult)homeController.Index(null);
                var model = (PaginatedList<Message>)actionResult.Model;

                // Assert
                Assert.True(model.Count == messages.Count);
            }
        }
    }
}
