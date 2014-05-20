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

using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whoaverse.Utils
{
    
    public static class WhoaversePagerOptions
    {

        private static PagedListRenderOptions plro = new PagedListRenderOptions();

        // setup custom paged list render options and allow reuse throughout views which depend on paged list, avoiding code repeat
        public static PagedListRenderOptions PagedListRenderOptions()
        {
            plro.Display = PagedListDisplayMode.IfNeeded;
            plro.DisplayLinkToLastPage = PagedListDisplayMode.Never;
            plro.DisplayLinkToFirstPage = PagedListDisplayMode.Never;
            plro.DisplayPageCountAndCurrentLocation = false;
            plro.DisplayLinkToIndividualPages = false;
            plro.DisplayLinkToNextPage = PagedListDisplayMode.IfNeeded;
            plro.DisplayLinkToPreviousPage = PagedListDisplayMode.IfNeeded;
            plro.LinkToNextPageFormat = "next ›";
            plro.LinkToPreviousPageFormat = "‹ prev";
            plro.ContainerDivClasses = new[] { "pagination-container" };
            plro.LiElementClasses = new[] { "btn-whoaverse-paging" };
            plro.UlElementClasses = null;
            plro.ClassToApplyToFirstListItemInPager = null;
            plro.ClassToApplyToLastListItemInPager = null;

            return plro;
        }

    }
}