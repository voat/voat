using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;
using Voat.Domain.Command;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class ModeratorCommandTests : BaseUnitTest
    {

        public Dictionary<string, SubverseModerator> InitializeSubverseModerators(string subName)
        {
            Dictionary<string, SubverseModerator> _subMods = new Dictionary<string, SubverseModerator>();

            using (var context = new voatEntities())
            {

                var sub = context.Subverses.Add(new Subverse()
                {
                    Name = subName,
                    Title = "v/modPerms",
                    Description = "Test Mod Perms",
                    SideBar = "Test Mod Perms",
                    Type = "link",
                    IsAnonymized = false,
                    CreationDate = DateTime.UtcNow.AddDays(-7),
                });
                context.SaveChanges();

                var modName = "";
                SubverseModerator mod = null;

                modName = "system";
                mod = context.SubverseModerators.Add(new SubverseModerator()
                {
                    Power = 1,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "PuttItOut",
                    CreationDate = DateTime.Now.AddDays(100) //we want to ensure no-one can remove system at L1
                });
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "Creator";
                mod = context.SubverseModerators.Add(new SubverseModerator()
                {
                    Power = 1,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = null,
                    CreationDate = null
                });
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L1.0";
                mod = context.SubverseModerators.Add(new SubverseModerator()
                {
                    Power = 1,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow.AddDays(-100)
                });
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L1.1";
                mod = context.SubverseModerators.Add(new SubverseModerator()
                {
                    Power = 1,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "L1.0",
                    CreationDate = DateTime.UtcNow
                });
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L2.0";
                mod = context.SubverseModerators.Add(new SubverseModerator()
                {
                    Power = 2,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow.AddDays(-10)
                });
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L2.1";
                mod = context.SubverseModerators.Add(new SubverseModerator()
                {
                    Power = 2,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow
                });
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L3.0";
                mod = context.SubverseModerators.Add(new SubverseModerator()
                {
                    Power = 3,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow.AddDays(-10)
                });
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L3.1";
                mod = context.SubverseModerators.Add(new SubverseModerator()
                {
                    Power = 3,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow
                });
                context.SaveChanges();
                _subMods.Add(modName, mod);


                modName = "L4.0";
                mod = context.SubverseModerators.Add(new SubverseModerator()
                {
                    Power = 4,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow.AddDays(-10)
                });
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L4.1";
                mod = context.SubverseModerators.Add(new SubverseModerator()
                {
                    Power = 4,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow
                });
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L99.0";
                mod = context.SubverseModerators.Add(new SubverseModerator()
                {
                    Power = 99,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow.AddDays(-10)
                });
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L99.1";
                mod = context.SubverseModerators.Add(new SubverseModerator()
                {
                    Power = 99,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow
                });
                context.SaveChanges();
                _subMods.Add(modName, mod);

            }
            return _subMods;
        }

        [TestMethod]
        public async Task ModeratorRemovals_Denials_NonOwner()
        {
            string subName = "testDenials";
            var mods = InitializeSubverseModerators(subName);

            RemoveModeratorCommand cmd = null;
            CommandResponse<RemoveModeratorResponse> response = null;

            string baseUserName = "";
            string originUserName = "";
            string targetUserName = "";

            //Test same level drops
            baseUserName = "L2";
            originUserName = $"{baseUserName}.0";
            targetUserName = $"{baseUserName}.1";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            baseUserName = "L3";
            originUserName = $"{baseUserName}.0";
            targetUserName = $"{baseUserName}.1";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            baseUserName = "L4";
            originUserName = $"{baseUserName}.0";
            targetUserName = $"{baseUserName}.1";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            baseUserName = "L99";
            originUserName = $"{baseUserName}.0";
            targetUserName = $"{baseUserName}.1";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            //Test L1 denials
            originUserName = "L2.0";
            targetUserName = "L1.0";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L1.1";
            targetUserName = "L1.0";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L1.0";
            targetUserName = "Creator";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");
        }

        [TestMethod]
        public async Task ModeratorRemovals_Allowed_NonOwner()
        {
            string subName = "testAllowed";
            var mods = InitializeSubverseModerators(subName);

            RemoveModeratorCommand cmd = null;
            CommandResponse<RemoveModeratorResponse> response = null;

            string baseUserName = "";
            string originUserName = "";
            string targetUserName = "";

            originUserName = "L2.0";
            targetUserName = "L3.0";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L1.0";
            targetUserName = "L2.0";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");


        }

        [TestMethod]
        public async Task ModeratorRemovals_Allowed_Owner()
        {
            string subName = "testAllowedOwner";
            var mods = InitializeSubverseModerators(subName);

            RemoveModeratorCommand cmd = null;
            CommandResponse<RemoveModeratorResponse> response = null;

            string baseUserName = "";
            string originUserName = "";
            string targetUserName = "";


            originUserName = "L1.0";
            targetUserName = "L1.1";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "Creator";
            targetUserName = "L1.0";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

        }

        [TestMethod]
        public async Task ModeratorRemovals_Denials_Owner()
        {
            string subName = "testDeniedOwner";
            var mods = InitializeSubverseModerators(subName);

            RemoveModeratorCommand cmd = null;
            CommandResponse<RemoveModeratorResponse> response = null;

            string baseUserName = "";
            string originUserName = "";
            string targetUserName = "";

            originUserName = "L1.0";
            targetUserName = "Creator";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L1.1";
            targetUserName = "Creator";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L1.1";
            targetUserName = "L1.0";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

        }


        [TestMethod]
        public async Task ModeratorRemovals_Denials_System()
        {
            string subName = "testSystemDenials";
            var mods = InitializeSubverseModerators(subName);

            RemoveModeratorCommand cmd = null;
            CommandResponse<RemoveModeratorResponse> response = null;

            string originUserName = "";
            string targetUserName = "";

            originUserName = "Creator";
            targetUserName = "system";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L1.0";
            targetUserName = "system";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L2.0";
            targetUserName = "system";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L3.0";
            targetUserName = "system";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L4.0";
            targetUserName = "system";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L99.0";
            targetUserName = "system";
            TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorCommand(mods[targetUserName].ID, false);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

        }
    }
}
