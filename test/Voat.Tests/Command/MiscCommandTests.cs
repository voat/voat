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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Configuration;
using Voat.Data;
using Voat.Data.Models;
using Voat.Domain.Command;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Rules;
using Voat.Tests.Repository;
using Voat.Utilities;
using Voat.Tests.Infrastructure;

namespace Voat.Tests.CommandTests
{
    [TestClass]
    public class MiscCommandTests : BaseCommandTest
    {

        [TestMethod]
        public async Task TestActiveSessionCount()
        {
            var d = new DomainReference(DomainType.Subverse, "whatever");
            var qa = new QueryActiveSessionCount(d);
            var result = await qa.ExecuteAsync();
            Assert.IsTrue(result >= 0, "Expecting a positive number");
        }
        [TestMethod]
        public async Task Test_LogActivityCommand_Serialization()
        {
            var user = TestHelper.SetPrincipal(USERNAMES.Unit);
            var cmd = new LogVisitCommand("_all", 799, "111.111.111.111").SetUserContext(user);
            var deserialized = EnsureCommandIsSerialziable(cmd);

            //Won't pass
            Assert.AreEqual(cmd.Subverse, deserialized.Subverse);
            Assert.AreEqual(cmd.SubmissionID, deserialized.SubmissionID);
            Assert.AreEqual(cmd.ClientIPAddress, deserialized.ClientIPAddress);
        }

        [TestMethod]
        public async Task Test_CreateApiKeyCommand_Serialization()
        {
            var user = TestHelper.SetPrincipal(USERNAMES.Unit);
            var cmd = new CreateApiKeyCommand("name", "description", "http://somedomain.com", "http://somedomain.com/app").SetUserContext(user);
            EnsureCommandIsSerialziable(cmd);

        }

        [TestMethod]
        public async Task Test_AllCommands_Serialization()
        {

            var user = TestHelper.SetPrincipal("TestUser01");
            //all
            var commandRegexPattern = "[A-Za-z0-9]Command";
            //commandRegexPattern = "MarkMessagesCommand";

            List<string> errors = new List<string>();
            List<string> passed = new List<string>();
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(VoatRuleContext));
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (Regex.IsMatch(type.Name, commandRegexPattern))
                {
                    if (!type.IsAbstract)
                    {
                        Command instance = null;
                        try
                        {
                            instance = (Command)Construct(type);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"FAIL: {type.Name} could not be constructed ({ex.Message})");
                        }

                        if (instance != null)
                        {
                            try
                            {
                                instance.SetUserContext(user);
                                EnsureCommandIsSerialziable((Command)instance);
                                passed.Add($"PASS: {type.Name} passed serialization");
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"FAIL: {type.Name} was not serialized correctly ({ex.Message})");
                            }
                        }
                    }
                }
            }

            if (errors.Count > 0)
            {
                Assert.Fail(String.Join(System.Environment.NewLine, errors) + System.Environment.NewLine + String.Join(System.Environment.NewLine, passed));
            }

        }

        private object Construct(Type type)
        {
            var constructors = type.GetConstructors();
            var defaultConstructor = constructors.FirstOrDefault(x => x.GetParameters().Length == 0);
            object instance = null;
            if (defaultConstructor != null)
            {
                instance = Activator.CreateInstance(type);
            }
            else
            {
                var constructor = constructors.OrderBy(x => x.GetParameters().Length).FirstOrDefault();
                var parameters = constructor.GetParameters();
                object[] arguments = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    var pValue = GetDefault(parameter.ParameterType);
                    arguments[i] = pValue;
                }
                instance = Activator.CreateInstance(type, arguments);
            }
            return instance;
        }

        public object GetDefault(Type type)
        {
            if (type == typeof(String))
            {
                return Guid.NewGuid().ToString().ToUpper().Replace("-", "");
            }
            if (type == typeof(Int32))
            {
                return (new Random()).Next(1, 2);
            }

            return GetType().GetMethod("GetDefaultGeneric").MakeGenericMethod(type).Invoke(this, null);
        }
        public T GetDefaultGeneric<T>()
        {
            return default(T);
        }

    }
}
