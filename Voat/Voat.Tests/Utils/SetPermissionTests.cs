using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voat.Domain.Models;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class SetPermissionTests
    {

        [TestMethod]
        [TestCategory("Set"), TestCategory("Set.Permissions")]
        public void TestSetPermissions()
        {
            Set s = null;

           
            SetPermission perms = null;
            string loggedInUserName = null;


            s = new Set()
            {
                Name = "Front",
                Type = SetType.Front,
                UserName = "Joe",
                IsPublic = true,
            };
            //Unathenticated on Public Set
            TestHelper.SetPrincipal(null);
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            //Owner on Non-Normal Set
            TestHelper.SetPrincipal("Joe");
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(true, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            s = new Set()
            {
                Name = "RandomSet",
                Type = SetType.Normal,
                UserName = "Joe",
                IsPublic = false,
            };
            //Unathenticated on Private Set
            TestHelper.SetPrincipal(null);
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(false, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            //Owner on Normal Private Set
            TestHelper.SetPrincipal("Joe");
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(true, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(true, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(true, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            //Non-owner on Private Set
            TestHelper.SetPrincipal("Eddy");
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(false, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            s = new Set()
            {
                Name = "SystemSet",
                Type = SetType.Normal,
                UserName = null,
                IsPublic = true,
            };
            //Unathenticated on Private Set
            TestHelper.SetPrincipal(null);
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

            //Non-Owner on Normal System Public Set
            TestHelper.SetPrincipal("Joe");
            loggedInUserName = (Thread.CurrentPrincipal.Identity.IsAuthenticated ? Thread.CurrentPrincipal.Identity.Name : "<not authenticated>");
            perms = SetPermission.GetPermissions(s, Thread.CurrentPrincipal.Identity);
            Assert.AreEqual(true, perms.View, $"View permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditList, $"Edit List permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.EditProperties, $"Edit Properties permission mismatch on {s.Name} with account {loggedInUserName}");
            Assert.AreEqual(false, perms.Delete, $"Delete permission mismatch on {s.Name} with account {loggedInUserName}");

           

        }
    }
}
