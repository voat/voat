/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whoaverse.Utils
{
    public static class PromotionUtility
    {
        public static List<string> TwoRandomSubtitles()
        {
            Random rndElement = new Random();
            List<string> tmpList = new List<string>();

            tmpList.Add (SubtitleArchiveOne()[rndElement.Next(SubtitleArchiveOne().Count)]);
            tmpList.Add (SubtitleArchiveTwo()[rndElement.Next(SubtitleArchiveOne().Count)]);

            return tmpList;
        }

        private static List<string> SubtitleArchiveOne()
        {
            List<string> lst = new List<string>(new string[] {			        
                    "...for your favorite game.",
                    "...for your favorite person.",
                    "...for justice?",
                    "...for your movement.",
                    "...for power?",
                    "...for your town.",
                    "...to help people?",
                    "...to save the planet?",
                    "...for your favorite TV show.",
                    "...because why not?",
                    "...for your WoW guild."
			    }
            );

            return lst;
        }

        private static List<string> SubtitleArchiveTwo()
        {
            List<string> lst = new List<string>(new string[] {
			        "...for your classroom.",
                    "...for your favourite tea.",
                    "...for fun?",
                    "...for your community.",
                    "...for mankind?",
                    "...to help animals?",
                    "...for aliens?",
                    "...do it for the children.",
                    "...for fame?",
                    "...just because?",
                    "...for your school.",
                    "...for your favorite hobby."
			    }
            );

            return lst;
        }
    }
    
}