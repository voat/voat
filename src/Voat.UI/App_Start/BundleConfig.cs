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

using System;

namespace Voat
{
    public class BundleConfig
    {
        //CORE_PORT: Different bundle config
        public static void RegisterBundles(object bundles)
        {
            throw new NotImplementedException("Different bundle config");
        }
        /*
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region script bundles
            bundles.Add(new ScriptBundle("~/bundles/javascript").Include(
                        "~/Scripts/libs/modernizr-*",
                        "~/Scripts/libs/jquery-{version}.js",
                        "~/Scripts/libs/jquery-ui.js",
                        "~/Scripts/libs/jquery.tooltipster.js",
                        "~/Scripts/libs/jquery.unobtrusive*",
                        "~/Scripts/libs/jquery.validate*",
                        "~/Scripts/voat.dashboard.js", 
                        "~/Scripts/voat.ui.js",
                        "~/Scripts/voat.js",
                        "~/Scripts/markdownEditor.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/libs/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/libs/bootstrap.js",
                      "~/Scripts/libs/respond.js"));
            #endregion

            #region style bundles
            bundles.Add(new StyleBundle("~/Content/light").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/tooltipster.css",
                      "~/Content/voat-light.css",
                      "~/Content/markdowneditor.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/dark").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/tooltipster.css",
                      "~/Content/voat-dark.css",
                      "~/Content/markdowneditor.css"
                      ));
            #endregion
        }
        */
    }
}
