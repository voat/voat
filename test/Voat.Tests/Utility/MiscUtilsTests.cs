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
using System.Threading.Tasks;
using Voat.Business.Utilities;
using Voat.Common;
using Voat.Common.Configuration;
using Voat.Tests.Infrastructure;
using Voat.Utilities;

namespace Voat.Tests.Utils
{
    [TestClass]
    public class MiscUtilsTests : BaseUnitTest
    {
        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Utility.Calculation")]
        public void TestCalcRank()
        {
            double result = Ranking.CalculateNewRank(0.5, 150, 20);
            Assert.AreEqual(0.0012465, result, 0.01, "Rank was not calculated.");
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Formatting")]
        public void TestFormatMarkdown()
        {
            string testString = "**Bold**";

            string result = Formatting.FormatMessage(testString);
            Assert.AreEqual("<p><strong>Bold</strong></p>", result.Trim(), "Markdown formatting failed");
        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Utility.WebRequest")]
        public void TestGetDomainFromUri()
        {
            Uri testUri = new Uri("http://www.youtube.com");

            string result = UrlUtility.GetDomainFromUri(testUri.ToString());
            Assert.AreEqual("youtube.com", result, "Unable to extract domain from given Uri.");
        }

        //[Ignore] //This fails often, ignoring.
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Utility.WebRequest"), TestCategory("ExternalHttp"), TestCategory("HttpResource")]
        public async Task TestGetOpenGraphImageFromUri()
        {

            Uri testUri = new Uri("http://www.bbc.com/news/technology-32194196");
            using (var httpResource = new HttpResource(testUri, new HttpResourceOptions() { Timeout = TimeSpan.FromSeconds(30) }))
            {
                await httpResource.GiddyUp();

                List<string> acceptable = new List<string>() {
                    "://ichef.bbci.co.uk/news/1024/media/images/80755000/jpg/_80755021_163765270.jpg", //'merica test
                    "://ichef-1.bbci.co.uk/news/1024/media/images/80755000/jpg/_80755021_163765270.jpg", //'merica test part 2
                    "://ichef-1.bbci.co.uk/news/1024/media/images/82142000/jpg/_82142761_026611869-1.jpg", //'merica test part 3
                    "://ichef.bbci.co.uk/news/1024/media/images/82142000/jpg/_82142761_026611869-1.jpg", //'merica test part 4
                    "://news.bbcimg.co.uk/media/images/80755000/jpg/_80755021_163765270.jpg" //Yuro test
                };

                Assert.IsNotNull(httpResource.Image, "Expeced a valid Image Uri");
                var expected = httpResource.Image.ToString();

                var passed = acceptable.Any(x => {
                    var result = expected.EndsWith(x, StringComparison.OrdinalIgnoreCase);
                    return result;
                });

                Assert.IsTrue(passed, $"HttpResource was unable to find an acceptable image path. Found: \"{expected}\"");
            }
           

        }

        [TestMethod]
        [TestCategory("Utility")]
        [TestCategory("Utility.Calculation")]
        public void TestScoreBehaviorObject()
        {
            Score vb;
            vb = new Score() { UpCount = 10, DownCount = 10 };
            Assert.IsTrue(vb.Total == 20, "t0.1");
            Assert.IsTrue(vb.UpCount == 10, "t0.2");
            Assert.IsTrue(vb.DownCount == 10, "t0.3");
            Assert.IsTrue(vb.Bias == 1f, "t0.4");
            Assert.IsTrue(vb.UpRatio == 0.5f, "t0.5");
            Assert.IsTrue(vb.DownRatio == 0.5f, "t0.6");
            Assert.IsTrue(vb.UpRatio + vb.DownRatio == 1.0f, "t0.7");

            vb = new Score() { UpCount = 10, DownCount = 5 };
            Assert.IsTrue(vb.Total == 15, "t1.1");
            Assert.IsTrue(vb.UpCount == 10, "t1.2");
            Assert.IsTrue(vb.DownCount == 5, "t1.3");
            Assert.IsTrue(vb.Bias == 2, "t1.4");
            Assert.IsTrue(vb.UpRatio == 0.67, "t1.5");
            Assert.IsTrue(vb.DownRatio == 0.33, "t1.6");

            //ensure negatives aren't stored
            vb = new Score() { UpCount = -10, DownCount = -10 };
            Assert.IsTrue(vb.Total == 0, "t2.1");
            Assert.IsTrue(vb.UpCount == 0, "t2.2");
            Assert.IsTrue(vb.DownCount == 0, "t2.3");
            Assert.IsTrue(vb.Bias == 1, "t2.4");

            vb = (new Score() { UpCount = 10, DownCount = 5 })
                .Combine(new Score() { UpCount = 10, DownCount = 5 })
                .Combine(new Score() { UpCount = 10, DownCount = 5 });
            Assert.IsTrue(vb.Total == 45, "t3.1");
            Assert.IsTrue(vb.UpCount == 30, "t3.2");
            Assert.IsTrue(vb.DownCount == 15, "t3.3");
            Assert.IsTrue(vb.Bias == 2, "t3.4");
        }
        //"\u0000\u0001\u0002\u0003\u0004\u0005\u0006\n\n\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇ"

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestUnicodeDetection1()
        {
            const string testString = "🆆🅰🆂 🅶🅴🆃🆃🅸🅽🅶 🅲🅰🆄🅶🅷🆃 🅿🅰🆁🆃 🅾🅵 🆈🅾🆄🆁 🅿🅻🅰🅽🅴";
            const string testStringWithoutUnicode = "was getting caught part of your plane";

            bool result = testString.ContainsUnicode();
            Assert.IsTrue(result, "Unicode was not detected.");

            bool resultWithoutUnicode = testStringWithoutUnicode.ContainsUnicode();
            Assert.IsFalse(resultWithoutUnicode, "Unicode was not detected.");
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestUnicodeDetection2()
        {
            const string testString = "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\n\n\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇ";

            bool result = testString.ContainsUnicode();
            Assert.IsTrue(result, "Unicode was not detected.");

        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestUnicodeDetection3()
        {
            const string testString = "ÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";
            bool result = testString.ContainsUnicode();
            Assert.IsFalse(result, "Unicode was detected.");
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestTitleStriping1()
        {
            const string testString = "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\n\n\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f";
            var result = testString.StripUnicode();
            Assert.AreEqual("", result);
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestTitleStriping2()
        {
            const string testString = "ÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";
            var result = testString.StripUnicode();
            Assert.AreNotEqual("", result);
        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestUnicodeStripping1()
        {
            const string testString = "NSA holds info over US citizens like loaded gun, but says ‘trust me’ – Snowden";
            const string testStringWithoutUnicode = "NSA holds info over US citizens like loaded gun, but says trust me Snowden";

            string result = testString.StripUnicode();
            Assert.IsTrue(result.Equals(testStringWithoutUnicode));
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestUnicodeStripping2()
        {
            string testString = "|       |";
            string result = testString.StripWhiteSpace();
            Assert.AreEqual("| |", result, "Multiple whitespace strip");

            testString = "| |";
            result = testString.StripWhiteSpace();
            Assert.AreEqual("| |", result, "Single whitespace strip");

            testString = " | |";
            result = testString.StripWhiteSpace();
            Assert.AreEqual("| |", result, "Leading whitespace strip");

            testString = "|     |  ";
            result = testString.StripWhiteSpace();
            Assert.AreEqual("| |", result, "Trailing whitespace strip");

            testString = null;
            result = testString.StripWhiteSpace();
            Assert.IsNull(result, "Null should return null");
        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("Formatting")]
        public void TestZeroPluralizer()
        {

            string result = 0.PluralizeIt("xxx");
            Assert.AreEqual("0 xxxs", result);

            result = 0.0.PluralizeIt("xxx");
            Assert.AreEqual("0 xxxs", result);

            result = 0.PluralizeIt("xxx", "none");
            Assert.AreEqual("none", result);

            result = 0.0.PluralizeIt("xxx", "none");
            Assert.AreEqual("none", result);

        }

        [TestMethod]
        [TestCategory("Utility"), TestCategory("DomainReference")]
        public void DomainReference_Tests()
        {
            var d = Domain.Models.DomainReference.Parse("sub", Domain.Models.DomainType.Subverse);
            Assert.AreEqual("sub", d.Name);

            d = Domain.Models.DomainReference.Parse("&oarstI", Domain.Models.DomainType.Subverse);
            Assert.IsNull(d);

            d = Domain.Models.DomainReference.Parse("UpperDown_", Domain.Models.DomainType.Subverse);
            Assert.IsNull(d);

            //Sets

            d = Domain.Models.DomainReference.Parse("sub", Domain.Models.DomainType.Set);
            Assert.AreEqual("sub", d.Name);
            Assert.AreEqual(null, d.OwnerName);

            d = Domain.Models.DomainReference.Parse("sub" + Utilities.CONSTANTS.SET_SEPERATOR + "owner", Domain.Models.DomainType.Set);
            Assert.AreEqual("sub", d.Name);
            Assert.AreEqual("owner", d.OwnerName);

            d = Domain.Models.DomainReference.Parse("sub" + Utilities.CONSTANTS.SET_SEPERATOR + "owner_-1", Domain.Models.DomainType.Set);
            Assert.AreEqual("sub", d.Name);
            Assert.AreEqual("owner_-1", d.OwnerName);

            d = Domain.Models.DomainReference.Parse("sub/owner_-1", Domain.Models.DomainType.Set);
            Assert.IsNull(d);


        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("ArgumentParser")]
        public void ArgumentParser_Tests()
        {

            var toArgString = new Func<string, Tuple<string, string>[], string>((delim, items) => {
                return items.Aggregate("", (agg, current) => {
                    return agg + (String.IsNullOrEmpty(agg) ? "" : delim) + $"[{current.Item1}]({current.Item2})";
                });
            });

            var arguments = new Tuple<string, string>[] {
                Tuple.Create("System.String", "SomeValue"),
                Tuple.Create("System.Int32", "4")
            };
            var argString = toArgString(";", arguments);
            Assert.AreEqual("[System.String](SomeValue);[System.Int32](4)", argString);
            var parsed = ArgumentParser.Parse(argString);
            Assert.AreEqual("SomeValue", parsed[0]);
            Assert.AreEqual(4, parsed[1]);

            argString = toArgString("", arguments);
            Assert.AreEqual("[System.String](SomeValue)[System.Int32](4)", argString);
            parsed = ArgumentParser.Parse(argString);
            Assert.AreEqual("SomeValue", parsed[0]);
            Assert.AreEqual(4, parsed[1]);


            //inline ;
            arguments = new Tuple<string, string>[] {
                Tuple.Create("System.String", "1:One;2:Two;3:Three;"),
                Tuple.Create("System.Int32", "4")
            };
            argString = toArgString(";", arguments);
            Assert.AreEqual("[System.String](1:One;2:Two;3:Three;);[System.Int32](4)", argString);
            parsed = ArgumentParser.Parse(argString);
            Assert.AreEqual("1:One;2:Two;3:Three;", parsed[0]);
            Assert.AreEqual(4, parsed[1]);

            //inline )
            arguments = new Tuple<string, string>[] {
                Tuple.Create("System.String", "1)One;2)Two;3)Three;"),
                Tuple.Create("System.Int32", "4")
            };
            argString = toArgString(";", arguments);
            Assert.AreEqual("[System.String](1)One;2)Two;3)Three;);[System.Int32](4)", argString);
            parsed = ArgumentParser.Parse(argString);
            Assert.AreEqual("1)One;2)Two;3)Three;", parsed[0]);
            Assert.AreEqual(4, parsed[1]);

        }
        [TestMethod]
        [TestCategory("Utility"), TestCategory("SpooferProofer")]
        public void SpooferProofer_Tests()
        {

            var charSwaps = new Dictionary<string, string>() {
                { "i", "l"},
                { "o", "0"}
            };
            var identifier = "LeeroyJenkins";
            var expectingToFind = "";

            var testSpoofList = new Action<Normalization>(normalization => {

                var spoofList = SpooferProofer.CharacterSwapList(identifier, charSwaps, true, normalization);

                if (normalization == Normalization.None)
                {
                    Assert.AreEqual(4, spoofList.Count());
                }
                else
                {
                    Assert.AreEqual(8, spoofList.Count());
                }

                expectingToFind = identifier.ToNormalized(normalization);
                Assert.AreEqual(expectingToFind, spoofList.FirstOrDefault(x => x == expectingToFind));

                expectingToFind = "Leer0yJenklns".ToNormalized(normalization);
                Assert.AreEqual(expectingToFind, spoofList.FirstOrDefault(x => x == expectingToFind));

                expectingToFind = "LeeroyJenklns".ToNormalized(normalization);
                Assert.AreEqual(expectingToFind, spoofList.FirstOrDefault(x => x == expectingToFind));

                expectingToFind = "Leer0yJenkins".ToNormalized(normalization);
                Assert.AreEqual(expectingToFind, spoofList.FirstOrDefault(x => x == expectingToFind));

            });

            testSpoofList(Normalization.None);
            testSpoofList(Normalization.Lower);
            testSpoofList(Normalization.Upper);

            ////Lowered tests same inputs but all outputs should be lower cased
            //spoofs = SpooferProofer.CharacterSwapList(identifier, charSwaps, true, Normalization.Lower);

            //expectingToFind = identifier.ToLower();
            //Assert.AreEqual(expectingToFind, spoofs.FirstOrDefault(x => x == expectingToFind));

            //expectingToFind = "Leer0yJenklns".ToLower();
            //Assert.AreEqual(expectingToFind, spoofs.FirstOrDefault(x => x == expectingToFind));

            //expectingToFind = "LeeroyJenklns".ToLower();
            //Assert.AreEqual(expectingToFind, spoofs.FirstOrDefault(x => x == expectingToFind));

            //expectingToFind = "Leer0yJenkins".ToLower();
            //Assert.AreEqual(expectingToFind, spoofs.FirstOrDefault(x => x == expectingToFind));

            //Test not reversed swaps 
            identifier = "Leer0yJenklns";
            var spoofs = SpooferProofer.CharacterSwapList(identifier, charSwaps, false);
            Assert.AreEqual(1, spoofs.Count());

            expectingToFind = identifier;
            Assert.AreEqual(expectingToFind, spoofs.FirstOrDefault(x => x == expectingToFind));
        }
        
    }
}
