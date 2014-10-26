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

using OpenGraph_Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Whoaverse.Utils
{
    public static class UrlUtility
    {

        // return domain from URI
        public static string GetDomainFromUri(string completeUri)
        {
            try
            {
                Uri tmpUri = new Uri(completeUri);
                return tmpUri.GetLeftPart(UriPartial.Authority).Replace("/www.", "/").Replace("http://", "").Replace("https://", "");      
            }
            catch (Exception)
            {
                return "http://whoaverse.com";
            }                  
        }

        // return remote page title from URI
        public static string GetTitleFromUri(string @remoteUri)
        {
            try
            {
                OpenGraph graph = OpenGraph.ParseUrl(@remoteUri);
                if (graph.Title != null && graph.Title.Length > 0)
                {
                    return graph.Title;
                }
                else
                {
                    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(@remoteUri);
                    req.Timeout = 3000;
                    StreamReader SR = new StreamReader(req.GetResponse().GetResponseStream());

                    Char[] buffer = new Char[256];
                    int counter = SR.Read(buffer, 0, 256);
                    while (counter > 0)
                    {
                        String outputData = new String(buffer, 0, counter);
                        Match match = Regex.Match(outputData, @"<title>([^<]+)", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            return match.Groups[1].Value;
                        }
                        counter = SR.Read(buffer, 0, 256);
                    }
                }
                
                return "We were unable to suggest a title.";

            }
            catch (Exception)
            {
                return "We were unable to suggest a title.";
            }            
        }

    }

    
}