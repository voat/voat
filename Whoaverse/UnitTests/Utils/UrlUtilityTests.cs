﻿﻿/*
This source file is subject to version 3 of the GPL license,
that is bundled with this package in the file LICENSE, and is
available online at http://www.gnu.org/licenses/gpl.txt;
you may not use this file except in compliance with the License.

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Voat.Utils;

namespace UnitTests.Utils
{
    [TestClass]
    public class UrlUtilityTests
    {
        [TestMethod]
        public void GetYoutubeIdFromUrl()
        {
            var expected = "Gjk5udJY_gM";

            var youtubeUrl = "https://www.youtube.com/watch?v=Gjk5udJY_gM";
            var output = UrlUtility.GetVideoIdFromUrl(youtubeUrl);
            Assert.AreEqual(expected, output);

            var miniUrl = "https://youtu.be/Gjk5udJY_gM?t=11s";
            output = UrlUtility.GetVideoIdFromUrl(miniUrl);
            Assert.AreEqual(expected, output);
        }
    }
}