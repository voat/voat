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
using System.Diagnostics;
using Voat.Utilities;
using Voat.Utilities.Components;
using Voat.Tests.Infrastructure;
using Voat.Configuration;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class ContentProcessorTests : BaseUnitTest
    {

      

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void EmptyMarkdownLinkLeftAlone()
        {
            string content = "[](http://somesite.com/someimage.png)";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual(content, processed);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void IgnoreCodeBlockMatchesMentions()
        {
            string content = "@user ~~~ @user ~~~ @user ~~~ @user ~~~";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual($"[@user]({ExpectedProtocol}://voat.co/user/user) ~~~ @user ~~~ [@user]({ExpectedProtocol}://voat.co/user/user) ~~~ @user ~~~", processed);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void IgnoreCodeBlockMatchesMentions2()
        {
            string content = "@user ~~~ @user ~~~ @user ~~~ @user ~~~ @user ~~~";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual($"[@user]({ExpectedProtocol}://voat.co/user/user) ~~~ @user ~~~ [@user]({ExpectedProtocol}://voat.co/user/user) ~~~ @user ~~~ [@user]({ExpectedProtocol}://voat.co/user/user) ~~~", processed);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void IgnoreCodeBlockMatchesMentions3()
        {
            string content = "@user ~~~ @user";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual($"[@user]({SiteRoot}/user/user) ~~~ [@user]({SiteRoot}/user/user)", processed);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void IgnoreCodeBlockMatchesMentions4()
        {
            string content = "~~~ @user ~~~ @user";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual($"~~~ @user ~~~ [@user]({SiteRoot}/user/user)", processed);
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
            Assert.AreEqual(content, processed);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void IgnoreCodeBlockMatchesUrls()
        {
            string content = "http://voat.co ~~~ http://voat.co ~~~";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual("[http://voat.co](http://voat.co) ~~~ http://voat.co ~~~", processed);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SameCaseOutputMention()
        {
            string content = "@userName";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual($"[{content}]({SiteRoot}/user/userName)", processed);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SameCaseOutputSubverse()
        {
            string content = "/v/VoatIs";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual($"[{content}]({SiteRoot}{content})", processed);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SubverseBadLeads()
        {
            string content = "nomatch/v/VoatIs/new";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual(content, processed);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SubverseCommentWithAnchor()
        {
            string content = "/v/somesub/comments/3333#submissionTop";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual($"[{content}]({SiteRoot}{content})", processed);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SubverseWithNew()
        {
            string content = "/v/VoatIs/new";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual($"[{content}]({SiteRoot}{content})", processed);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SubverseWithTop()
        {
            string content = "/v/VoatIs/top";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual($"[{content}]({SiteRoot}{content})", processed);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void SubverseWithTrailingUnknown()
        {
            string content = "/v/VoatIs/NotValid";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual($"[/v/VoatIs]({SiteRoot}/v/VoatIs)/NotValid", processed);
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
            Assert.AreEqual($"I have enabled this: [v/api]({SiteRoot}/v/api) and [r/golf](https://np.reddit.com/r/golf) matching [link](http://voat.co)", processed);
        }

        [TestMethod]
        [TestCategory("Content Processor")]
        [TestCategory("Formatting")]
        public void TestNoNotifyUserMention()
        {
            string content = "-@User";

            string processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual($"[@User]({SiteRoot}/user/User)", processed);

            content = "-/u/User";

            processed = ContentProcessor.Instance.Process(content, ProcessingStage.Outbound, null);
            Assert.AreEqual($"[/u/User]({SiteRoot}/user/User)", processed);
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
            //Assert.IsTrue(content == processed);

            //now we do
            Assert.AreEqual($"[{content}]({ExpectedProtocol}://voat.co/v/VoatIs)", processed);
        }
    }

    [TestClass]
    public class MarkdownFormattingTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void HrefIntactEscaping()
        {
            string input = "[Title Here](http://voat.co)";
            string expected = "<p><a href=\"http://voat.co\">Title Here</a></p>";

            string actual = Formatting.FormatMessage(input);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        [TestCategory("Bug")]
        public void Href_Raw_Bug_Trap_Dash_End()
        {
            string input = "http://www.domain.com/x-";
            string expected = $"<p><a href=\"{input}\">{input}</a></p>";

            string actual = Formatting.FormatMessage(input);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        [TestCategory("Bug")]
        public void Href_Raw_Bug_Trap_Underscore_End()
        {
            string input = "http://www.domain.com/x_";
            string expected = $"<p><a href=\"{input}\">{input}</a></p>";

            string actual = Formatting.FormatMessage(input);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void HrefIntactEscaping2()
        {
            string input = "[Title Here](ftp://voat.co)";
            string expected = "<p><a href=\"ftp://voat.co\">Title Here</a></p>";

            string actual = Formatting.FormatMessage(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Security")]
        [TestCategory("Formatting")]
        public void ScriptEscaping()
        {
            string input = "[Title Here](javascript:alert('test'))";
            string expected = "<p><a href=\"#\" data-ScriptStrip=\"/* script detected: javascript:alert('test') */\">Title Here</a></p>";

            string actual = Formatting.FormatMessage(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Security")]
        [TestCategory("Formatting")]
        public void ScriptEscaping2()
        {
            string input = "[Title Here]( javascript  : alert('test'))";
            string expected = String.Format("<p>{0}</p>", input);

            string actual = Formatting.FormatMessage(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void ClosingTag()
        {
            string input = "Ha! Everything below me will be crossed out<s>";
            string expected = "<p>Ha! Everything below me will be crossed out<s></s></p>";

            string actual = Formatting.FormatMessage(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void BlockClosingTag()
        {
            string input = "><s>";
            string expected = "<blockquote>\n<p><s></s></p>\n</blockquote>";

            string actual = Formatting.FormatMessage(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void EmbeddedClosingTag()
        {
            string input = "<blockquote>\n<p>\nParagraphs</blockquote>";
            //if blockquote is allowed
            //string expected = "<blockquote><p><p>\nParagraphs</p></p>\n</blockquote>";
            //if blockquote is turned off
            string expected = "&lt;blockquote&gt;\n&lt;p&gt;\nParagraphs&lt;/blockquote&gt;";

            string actual = Formatting.FormatMessage(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void EmbeddedCodeTag()
        {
            string input = "~~~\nThis is code:\n\n<s> \n~~~";
            string expected = "<pre><code>This is code:\n\n&lt;s&gt; \n</code></pre>";

            string actual = Formatting.FormatMessage(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void EmbeddedTagSpanClosing()
        {
            string input = "This is great\n\n<s>This is <sup>horrible\n\nEdit: I'm sure now";
            string expected = "<p>This is great</p>\n<p><s>This is <sup>horrible</sup></s></p>\n<p>Edit: I'm sure now</p>";

            string actual = Formatting.FormatMessage(input);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void UserTagsNotMatchingInHtml_IToldDanWeShouldNotAllowHtml_ButNooooooHeDidNotListenToMe()
        {
            //Bug Found Here: https://voat.co/v/AskVoat/1352378/6588193
            //And yes, typo left on purpose because Mick needs spell check for user names.
            string input = "<sub>@Akto, we'll need to have the above poster [hugged] (keep that to yourself, Dear Leader)</sub>";
            string expected = $"<p><sub><a href=\"{SiteRoot}/user/Akto\">@Akto</a>, we'll need to have the above poster [hugged] (keep that to yourself, Dear Leader)</sub></p>";

            string actual = Formatting.FormatMessage(input);

            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void SantizeMaliciousMarkdown()
        {
            string input = ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>";
            string expected = ">";

            string actual = Formatting.SanitizeInput(input);
            Assert.AreEqual(expected, actual);

            input = "> > > > > > > > ";
            actual = Formatting.SanitizeInput(input);
            Assert.AreEqual(expected, actual);

            input = @"Hello 
                    > > > > > > > > Hello";
            expected = @"Hello 
                    >Hello";
            actual = Formatting.SanitizeInput(input);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void TestGeorgesBreak()
        {
            //This should not alter this content because it is inside of a code section. This is going to be fun.
            string input = @"~~~~~

>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

~~~~~";
            string expected = input;

            string actual = Formatting.SanitizeInput(input);
            Assert.AreEqual(expected, actual);

        }
    }
}
