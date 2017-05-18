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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Tests
{
    [TestClass]
    public class BaseUnitTest
    {
        public BaseUnitTest()
        {
        }

        //Different configurations of Test Suites will handle test context 
        //authentication differently [i.e. WindowsIdentity (Authenticated) | GenericIdentity(Not Authenticated)]
        //
        //This method ensures that the user context is cleared before test execution
        [TestInitialize]
        public virtual void TestInitialize()
        {
            TestHelper.SetPrincipal(null);
        }
        
        [TestClassInitialize]
        public virtual void ClassInitialize()
        {

        }

        public static string GetMethodName(bool fullyQualified = false, string additionalData = null)
        {
            string callerName = $"NoIdea-{Guid.NewGuid().ToString()}";
            StackTrace trace = new StackTrace();
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var currentMethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            for (int i = 1; i < trace.FrameCount; i++)
            {
                StackFrame frame = trace.GetFrame(i);
                var method = frame.GetMethod();
                var type = method.DeclaringType;
                if (type.Assembly == assembly && method.Name != currentMethodName && !type.CustomAttributes.Any(x => x.AttributeType == typeof(CompilerGeneratedAttribute)))
                {
                    callerName = (fullyQualified ? $"{type.FullName}.{method.Name}" : method.Name);
                    break;
                }
            }

            //int caller = 1;
            //StackFrame frame = trace.GetFrame(caller);
            //string callerName = frame.GetMethod().Name;

            return String.Format("{0}{1}",
                callerName,
                String.IsNullOrEmpty(additionalData) ? "" : String.Format("-{0}", additionalData));

        }
    }
}
