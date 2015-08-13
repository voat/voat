/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System.Web.Optimization;

namespace Voat
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region script bundles
            bundles.Add(new ScriptBundle("~/bundles/javascript").Include(
                        "~/Scripts/modernizr-*",
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui.js",
                        "~/Scripts/jquery.tooltipster.js",
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/voat.ui.js",
                        "~/Scripts/voat.js",
                        "~/Scripts/markdownEditor.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));
            #endregion

            #region style bundles
            bundles.Add(new StyleBundle("~/Content/light").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/Site.css",
                      "~/Content/tooltipster.css",
                      "~/Content/Voat.css",
                      "~/Content/PagedList.css",
                      "~/Content/markdownEditor.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/dark").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/Site.css",
                      "~/Content/tooltipster.css",
                      "~/Content/Voat-Dark.css",
                      "~/Content/PagedList.css",
                      "~/Content/markdownEditor.css"
                      ));
            #endregion
        }
    }
}
