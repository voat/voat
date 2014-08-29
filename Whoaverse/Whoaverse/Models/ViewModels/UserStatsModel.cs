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

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Whoaverse.Models.ViewModels
{
    public class UserStatsModel
    {
        public IEnumerable<SubverseStats> TopSubversesUserContributedTo { get; set; }        
        public int LinkSubmissionsSubmitted { get; set; }
        public int MessageSubmissionsSubmitted { get; set; }
        public int TotalCommentsSubmitted { get; set; }
        public IEnumerable<Message> HighestRatedSubmissions { get; set; }
        public IEnumerable<Message> LowestRatedSubmissions { get; set; }
        public IEnumerable<Comment> HighestRatedComments { get; set; }
        public IEnumerable<Comment> LowestRatedComments { get; set; }
    }

    public class SubverseStats
    {
        public string SubverseName { get; set; }
        public int Count { get; set; }
    }
}