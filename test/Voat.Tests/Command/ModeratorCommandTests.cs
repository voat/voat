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
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class ModeratorCommandTests : BaseUnitTest
    {

        public Dictionary<string, SubverseModerator> InitializeSubverseModerators(string subName)
        {
            Dictionary<string, SubverseModerator> _subMods = new Dictionary<string, SubverseModerator>();

            using (var context = new VoatDataContext())
            {

                var sub = context.Subverse.Add(new Subverse()
                {
                    Name = subName,
                    Title = "v/modPerms",
                    Description = "Test Mod Perms",
                    SideBar = "Test Mod Perms",
                    //Type = "link",
                    IsAnonymized = false,
                    CreationDate = DateTime.UtcNow.AddDays(-7),
                });
                context.SaveChanges();

                var modName = "";
                SubverseModerator mod = null;

                modName = "system";
                mod = context.SubverseModerator.Add(new SubverseModerator()
                {
                    Power = 1,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "PuttItOut",
                    CreationDate = DateTime.UtcNow.AddDays(100) //we want to ensure no-one can remove system at L1
                }).Entity;
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "Creator";
                mod = context.SubverseModerator.Add(new SubverseModerator()
                {
                    Power = 1,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = null,
                    CreationDate = null
                }).Entity;
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L1-0";
                mod = context.SubverseModerator.Add(new SubverseModerator()
                {
                    Power = 1,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow.AddDays(-100)
                }).Entity;
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L1-1";
                mod = context.SubverseModerator.Add(new SubverseModerator()
                {
                    Power = 1,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "L1-0",
                    CreationDate = DateTime.UtcNow
                }).Entity;
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L2-0";
                mod = context.SubverseModerator.Add(new SubverseModerator()
                {
                    Power = 2,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow.AddDays(-10)
                }).Entity;
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L2-1";
                mod = context.SubverseModerator.Add(new SubverseModerator()
                {
                    Power = 2,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow
                }).Entity;
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L3-0";
                mod = context.SubverseModerator.Add(new SubverseModerator()
                {
                    Power = 3,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow.AddDays(-10)
                }).Entity;
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L3-1";
                mod = context.SubverseModerator.Add(new SubverseModerator()
                {
                    Power = 3,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow
                }).Entity;
                context.SaveChanges();
                _subMods.Add(modName, mod);


                modName = "L4-0";
                mod = context.SubverseModerator.Add(new SubverseModerator()
                {
                    Power = 4,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow.AddDays(-10)
                }).Entity;
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L4-1";
                mod = context.SubverseModerator.Add(new SubverseModerator()
                {
                    Power = 4,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow
                }).Entity;
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L99-0";
                mod = context.SubverseModerator.Add(new SubverseModerator()
                {
                    Power = 99,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow.AddDays(-10)
                }).Entity;
                context.SaveChanges();
                _subMods.Add(modName, mod);

                modName = "L99-1";
                mod = context.SubverseModerator.Add(new SubverseModerator()
                {
                    Power = 99,
                    UserName = modName,
                    Subverse = subName,
                    CreatedBy = "Creator",
                    CreationDate = DateTime.UtcNow
                }).Entity;
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

            RemoveModeratorByRecordIDCommand cmd = null;
            CommandResponse<RemoveModeratorResponse> response = null;

            string baseUserName = "";
            string originUserName = "";
            string targetUserName = "";

            //Test same level drops
            baseUserName = "L2";
            originUserName = $"{baseUserName}-0";
            targetUserName = $"{baseUserName}-1";
            var user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            baseUserName = "L3";
            originUserName = $"{baseUserName}-0";
            targetUserName = $"{baseUserName}-1";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            baseUserName = "L4";
            originUserName = $"{baseUserName}-0";
            targetUserName = $"{baseUserName}-1";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            baseUserName = "L99";
            originUserName = $"{baseUserName}-0";
            targetUserName = $"{baseUserName}-1";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            //Test L1 denials
            originUserName = "L2-0";
            targetUserName = "L1-0";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L1-1";
            targetUserName = "L1-0";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L1-0";
            targetUserName = "Creator";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");
        }

        [TestMethod]
        public async Task ModeratorRemovals_Allowed_NonOwner()
        {
            string subName = "testAllowed";
            var mods = InitializeSubverseModerators(subName);

            RemoveModeratorByRecordIDCommand cmd = null;
            CommandResponse<RemoveModeratorResponse> response = null;

            string baseUserName = "";
            string originUserName = "";
            string targetUserName = "";

            originUserName = "L2-0";
            targetUserName = "L3-0";
            var user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L1-0";
            targetUserName = "L2-0";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");


        }

        [TestMethod]
        public async Task ModeratorRemovals_Allowed_Owner()
        {
            string subName = "testAllowedOwner";
            var mods = InitializeSubverseModerators(subName);

            RemoveModeratorByRecordIDCommand cmd = null;
            CommandResponse<RemoveModeratorResponse> response = null;

            string baseUserName = "";
            string originUserName = "";
            string targetUserName = "";


            originUserName = "L1-0";
            targetUserName = "L1-1";
            var user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "Creator";
            targetUserName = "L1-0";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Success, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

        }

        [TestMethod]
        public async Task ModeratorRemovals_Denials_Owner()
        {
            string subName = "testDeniedOwner";
            var mods = InitializeSubverseModerators(subName);

            RemoveModeratorByRecordIDCommand cmd = null;
            CommandResponse<RemoveModeratorResponse> response = null;

            string baseUserName = "";
            string originUserName = "";
            string targetUserName = "";

            originUserName = "L1-0";
            targetUserName = "Creator";
            var user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L1-1";
            targetUserName = "Creator";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L1-1";
            targetUserName = "L1-0";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

        }


        [TestMethod]
        public async Task ModeratorRemovals_Denials_System()
        {
            string subName = "testSystemDenials";
            var mods = InitializeSubverseModerators(subName);

            RemoveModeratorByRecordIDCommand cmd = null;
            CommandResponse<RemoveModeratorResponse> response = null;

            string originUserName = "";
            string targetUserName = "";

            originUserName = "Creator";
            targetUserName = "system";
            var user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L1-0";
            targetUserName = "system";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L2-0";
            targetUserName = "system";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L3-0";
            targetUserName = "system";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L4-0";
            targetUserName = "system";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

            originUserName = "L99-0";
            targetUserName = "system";
            user = TestHelper.SetPrincipal(originUserName);
            cmd = new RemoveModeratorByRecordIDCommand(mods[targetUserName].ID, false).SetUserContext(user);
            response = await cmd.Execute();
            Assert.AreEqual(Status.Denied, response.Status, $"Status mismatch on {originUserName} to {targetUserName}");

        }
    }
}
