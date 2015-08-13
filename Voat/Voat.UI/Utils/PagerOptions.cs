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

using PagedList.Mvc;

namespace Voat.UI.Utilities
{
    
    public static class VoatPagerOptions
    {

        private static readonly PagedListRenderOptions Plro = new PagedListRenderOptions();

        // custom paged list render options without pagination
        public static PagedListRenderOptions PagedListRenderOptions()
        {
            Plro.Display = PagedListDisplayMode.IfNeeded;
            Plro.DisplayLinkToLastPage = PagedListDisplayMode.Never;
            Plro.DisplayLinkToFirstPage = PagedListDisplayMode.Never;
            Plro.DisplayPageCountAndCurrentLocation = false;
            Plro.DisplayLinkToIndividualPages = false;
            Plro.DisplayLinkToNextPage = PagedListDisplayMode.IfNeeded;
            Plro.DisplayLinkToPreviousPage = PagedListDisplayMode.IfNeeded;
            Plro.LinkToNextPageFormat = "next ›";
            Plro.LinkToPreviousPageFormat = "‹ prev";
            Plro.ContainerDivClasses = new[] { "pagination-container" };
            Plro.LiElementClasses = new[] { "btn-whoaverse-paging" };
            Plro.UlElementClasses = null;
            Plro.ClassToApplyToFirstListItemInPager = null;
            Plro.ClassToApplyToLastListItemInPager = null;

            return Plro;
        }

    }
}