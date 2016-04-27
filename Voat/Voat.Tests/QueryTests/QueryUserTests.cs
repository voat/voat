using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Query;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class QueryUserTests
    {
        [TestMethod]
        public void UserData_Returns_Null_on_Invalid_User_Name()
        {
            string user = "";
            var q = new QueryUserData(user);
            var userData = q.Execute().Result;
            Assert.IsNull(userData, String.Format("UserData expected to be null for value: '{0}'", user));

            user = null;
            q = new QueryUserData(null);
            userData = q.Execute().Result;
            Assert.IsNull(userData, String.Format("UserData expected to be null for value: '{0}'", "null"));

            user = "____________Doesn't__Exist_________";
            q = new QueryUserData(user);
            userData = q.Execute().Result;
            Assert.IsNull(userData, String.Format("UserData expected to be null for value: '{0}'", user));

        }
    }
}
