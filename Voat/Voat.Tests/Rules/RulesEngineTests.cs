#region LICENSE

/*

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All portions of the code written by Voat, Inc. are Copyright(c) Voat, Inc.

    All Rights Reserved.

*/

#endregion LICENSE

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Voat.Common;
using Voat.RulesEngine;

namespace Voat.Tests.Rules
{
    [TestClass]
    public class RulesEngineTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("RulesEngine")]
        public void TestAddingRules()
        {
            TestRuleEngine t = new TestRuleEngine(/*new ThreadIsolatedContextHandler<RequestContext>()*/);
            t.AddRule(new TestRule("Test1", "1.0", RuleScope.UpVoteComment));
            t.AddRule(new TestRule("Test2", "1.1", RuleScope.DownVoteComment));
            t.AddRule(new TestRule("Test3", "1.2", RuleScope.PostSubmission));
            t.AddRule(new TestRule("Test4", "1.3", RuleScope.PostComment));
            t.AddRule(new TestRule("Test5", "1.4", RuleScope.UpVoteSubmission));
            t.AddRule(new TestRule("Test6", "1.5", RuleScope.DownVoteSubmission));
            t.AddRule(new TestRule("Test7", "1.6", RuleScope.UpVoteComment));
            t.AddRule(new TestRule("Test8", "1.7", RuleScope.UpVoteComment));
            t.AddRule(new TestRule("Test9", "1.8", RuleScope.Global));

            var r = t.GetRules(RuleScope.DownVoteComment);
            Assert.IsTrue(r.Count() == 1);

            //Test if global is included
            r = t.GetRules(RuleScope.DownVoteComment, true);
            Assert.IsTrue(r.Count() == 2);

            r = t.GetRules(RuleScope.UpVoteComment);
            Assert.IsTrue(r.Count() == 3);

            r = t.GetRules(RuleScope.Global);
            Assert.IsTrue(r.Count() == 1);

            r = t.GetRules(RuleScope.UpVoteSubmission);
            Assert.IsTrue(r.Count() == 1);

            r = t.GetRules(RuleScope.PostComment);
            Assert.IsTrue(r.Count() == 1);
        }

        [TestMethod]
        [TestCategory("RulesEngine")]
        public void TestContextUniqueness()
        {
            //var contextHandler = new ThreadIsolatedContextHandler<RequestContext>();

            TestRuleEngine engine = new TestRuleEngine(/*contextHandler*/);

            engine.AddRule(new TestRule("Test1", "1.0", RuleScope.Global));
            engine.AddRule(new TestRule("Test2", "1.1", RuleScope.Global));
            var context = new RequestContext();

            var r = engine.EvaluateRuleSet(context, new RuleScope[] { RuleScope.Global });

            //engine.Context.PropertyBag.Value = 1;

            //Assert.IsTrue(contextHandler.ContextStore.Count == 1);
            //Assert.IsTrue(contextHandler.ContextStore. == 1);
        }

        [TestMethod]
        [TestCategory("RulesEngine")]
        public void TestRuleDescriptionProvider()
        {
            var descriptions = RuleDiscoveryProvider.GetDescriptions(Assembly.Load("Voat.Business"));
            Assert.AreNotEqual(0, descriptions.Count);
        }

        //[TestMethod]
        [TestCategory("RulesEngine")]
        public void TestRulesEngineLoading()
        {
            var x = new UnitTestRulesEngine();

            RulesConfiguration config = new RulesConfiguration();
            config.DiscoverAssemblies = "Voat.Tests;Voat.Data";
            config.DiscoverRules = true;
            config.Enabled = true;

            x.Initialize(config);

            //should find two rules 1 enabled, 1 disabled
            Assert.AreEqual(1, x.Rules.Count());
        }

        [TestMethod]
        [TestCategory("RulesEngine")]
        public void TestRulesSection()
        {
            var config = RuleSection.Instance;
            Assert.IsNotNull(config.Configuration.Rules, "Loaded no rules");
            Assert.AreNotEqual(0, config.Configuration.Rules.Count);
        }
    }

    public class TestRuleEngine : RulesEngine<RequestContext>
    {
        public TestRuleEngine(/*IRequestContextHandler<RequestContext> handler*/) /*: base(handler)*/
        {
            //Handler = handler;
        }

      
        public IEnumerable<Rule> GetRules(RuleScope scope, bool includeGlobals = false)
        {
            return base.GetRulesByScope(scope, includeGlobals, new Func<Rule, RuleScope, bool>((r, s) => r.Scope == s));
        }
    }
}
