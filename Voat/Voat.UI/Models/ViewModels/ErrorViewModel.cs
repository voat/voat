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
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voat.Models.ViewModels
{
    public class ErrorViewModel
    {
        public ErrorViewModel() { }

        public ErrorViewModel(string title, string description, string footer)
        {
            this.Title = title;
            this.Description = description;
            this.Footer = footer;
        }

        public bool UseLayout { get; set; } = true;
        public string Title { get; set; } = "Whoops!";
        public string Description { get; set; } = "Well this is embarrassing. Something went wrong and let&#39;s face it, nobody&#39;s happy about it.\nWe&#39;ll dispatch our monstersquad to take a look at it right away!";
        public string Footer { get; set; } = "Thank you for being a chap.";



    }
}
