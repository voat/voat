﻿#region LICENSE

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
using System.Diagnostics;
using Voat.Utilities;

using Voat.Utilities.Components;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class ContentProcessorTests
    {
        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void EmptyMarkdownLinkLeftAlone()
        {
            string content = "[](http://somesite.com/someimage.png)";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == content);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void IgnoreCodeBlockMatchesMentions()
        {
            string content = "@user ~~~ @user ~~~ @user ~~~ @user ~~~";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == "[@user](https://voat.co/u/user) ~~~ @user ~~~ [@user](https://voat.co/u/user) ~~~ @user ~~~");
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void IgnoreCodeBlockMatchesMentions2()
        {
            string content = "@user ~~~ @user ~~~ @user ~~~ @user ~~~ @user ~~~";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == "[@user](https://voat.co/u/user) ~~~ @user ~~~ [@user](https://voat.co/u/user) ~~~ @user ~~~ [@user](https://voat.co/u/user) ~~~");
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void IgnoreCodeBlockMatchesMentions3()
        {
            string content = "@user ~~~ @user";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == "[@user](https://voat.co/u/user) ~~~ [@user](https://voat.co/u/user)");
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void IgnoreCodeBlockMatchesMentions4()
        {
            string content = "~~~ @user ~~~ @user";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == "~~~ @user ~~~ [@user](https://voat.co/u/user)");
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void IgnoreCodeBlockMatchesMentions5()
        {
            string content =
                            @"~~~
                            @NO-Match
                            ~~~";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == content);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void IgnoreCodeBlockMatchesUrls()
        {
            string content = "http://voat.co ~~~ http://voat.co ~~~";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == "[http://voat.co](http://voat.co) ~~~ http://voat.co ~~~");
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SameCaseOutputMention()
        {
            string content = "@userName";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == String.Format("[{0}]({1})", content, "https://voat.co/u/userName"));
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SameCaseOutputSubverse()
        {
            string content = "/v/VoatIs";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == String.Format("[{0}]({1})", content, "https://voat.co" + content));
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SubverseBadLeads()
        {
            string content = "nomatch/v/VoatIs/new";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == content);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SubverseCommentWithAnchor()
        {
            string content = "/v/somesub/comments/3333#submissionTop";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == String.Format("[{0}](https://voat.co{1})", content, content));
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SubverseWithNew()
        {
            string content = "/v/VoatIs/new";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == String.Format("[{0}]({1})", content, "https://voat.co" + content));
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SubverseWithTop()
        {
            string content = "/v/VoatIs/top";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == String.Format("[{0}]({1})", content, "https://voat.co" + content));
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SubverseWithTrailingUnknown()
        {
            string content = "/v/VoatIs/NotValid";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == String.Format("[{0}]({1})/NotValid", "/v/VoatIs", "https://voat.co/v/VoatIs"));
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]

        // Found at FakeVout during testing
        public void TestFakeVoutRedditLinkBug()
        {
            // r/golf wasn't matching because of a greedy markdown reqex when determining
            //if match was in the bounds of a markdown link leaving this test in to verify bug
            //never gets reintroduced
            string content = "I have enabled this: v/api and r/golf matching [link](http://voat.co)";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == "I have enabled this: [v/api](https://voat.co/v/api) and [r/golf](https://np.reddit.com/r/golf) matching [link](http://voat.co)");
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void TestNoNotifyUserMention()
        {
            string content = "-@User";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == "[@User](https://voat.co/u/User)");

            content = "-/u/User";

            processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.IsTrue(processed == "[/u/User](https://voat.co/u/User)");
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void TestNoNotifyUserMentionIncoming()
        {
            string content = "-@user";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.InboundPostSave, null);
            //Assert.IsTrue(processed == "[@user](https://voat.co/u/user)");

            content = "@user";
            processed = ContentProcessor.Instance.Process(content, ProcessingStage.InboundPostSave, null);

            content = "-/u/user";
            processed = ContentProcessor.Instance.Process(content, ProcessingStage.InboundPostSave, null);

            content = "/u/user";
            processed = ContentProcessor.Instance.Process(content, ProcessingStage.InboundPostSave, null);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void TestParsingTimes()
        {
            //HAVE TO USE NON-HTTPCONTEXT DEPENDENT MATCHES FOR TESTS
            // The unit test runs without an HTTP context, the username and subverse matches error out when trying to resolve the processing
            // because there is no HTTP context. This drastically increases the time of the unit test and isn't accurate. Don't believe me
            // try this and compare against the raw http links or reddit matches.
            //string content = "@user @user @user";

            //string content = "~~~ /r/aww ~~~ /r/aww `/r/aww`";
            //string content = "/r/aww /r/aww /r/aww";

            string content = "~~~ http://voat.co ~~~ http://voat.co `http://voat.co`";
            //string content = "http://voat.co http://voat.co http://voat.co";

            string result = "";

            int iterations = 100000;

            Stopwatch s = new Stopwatch();
            s.Start();

            for (int i = 0; i < iterations; i++)
            {
                result = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            }

            s.Stop();

            Debug.WriteLine("Milliseconds: {0}", s.ElapsedMilliseconds.ToString());
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void UpperVSubverse()
        {
            string content = "/V/VoatIs";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);

            //right now we don't process upper case V's
            Assert.IsTrue(content == processed);
            //Assert.IsTrue(processed == String.Format("[{0}]({1})", content, "https://voat.co" + content));
        }
    }

    [TestClass]
    public class MardownFormattingTests
    {
        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void HrefIntactExcaping()
        {
            string content = "[Title Here](http://voat.co)";

            string processed = Formatting.FormatMessage(content);
            Assert.IsTrue(processed == "<p><a href=\"http://voat.co\">Title Here</a></p>");
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void HrefIntactExcaping2()
        {
            string content = "[Title Here](ftp://voat.co)";

            string processed = Formatting.FormatMessage(content);
            Assert.IsTrue(processed == "<p><a href=\"ftp://voat.co\">Title Here</a></p>");
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Security")]
        [TestCategory("Formatting")]
        public void ScriptExcaping()
        {
            string content = "[Title Here](javascript:alert('test'))";

            string processed = Formatting.FormatMessage(content);
            Assert.IsTrue(processed == "<p><a href=\"#\" data-ScriptStrip=\"/* script detected: javascript:alert('test') */\">Title Here</a></p>");
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Security")]
        [TestCategory("Formatting")]
        public void ScriptExcaping2()
        {
            string content = "[Title Here]( javascript  : alert('test'))";
            string processed = Formatting.FormatMessage(content);
            Assert.IsTrue(processed == String.Format("<p>{0}</p>", content));
        }
    }
}
