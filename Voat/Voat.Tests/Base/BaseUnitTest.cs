using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Tests
{
    public class BaseUnitTest
    {
        //Different configurations of Test Suites will handle test context 
        //authentication differently [i.e. WindowsIdentity (Authenticated) | GenericIdentity(Not Authenticated)]
        //
        //This method ensures that the user context is cleared before test execution
        [TestInitialize]
        public virtual void Initialize()
        {
            TestHelper.SetPrincipal(null);
        }

    }
}
