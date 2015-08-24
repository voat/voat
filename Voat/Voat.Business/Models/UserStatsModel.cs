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

using System.Collections.Generic;
using Voat.Data.Models;

namespace Voat.Models
{
    public class UserStatsModel
    {
        public IEnumerable<SubverseStats> TopSubversesUserContributedTo { get; set; }        
        public int LinkSubmissionsSubmitted { get; set; }
        public int MessageSubmissionsSubmitted { get; set; }
        public int TotalCommentsSubmitted { get; set; }
        public IEnumerable<Submission> HighestRatedSubmissions { get; set; }
        public IEnumerable<Submission> LowestRatedSubmissions { get; set; }
        public IEnumerable<Comment> HighestRatedComments { get; set; }
        public IEnumerable<Comment> LowestRatedComments { get; set; }

        public int TotalCommentsUpvoted { get; set; }
        public int TotalCommentsDownvoted { get; set; }
        public int TotalSubmissionsUpvoted { get; set; }
        public int TotalSubmissionsDownvoted { get; set; }
    }

    public class SubverseStats
    {
        public string SubverseName { get; set; }
        public int Count { get; set; }
    }
}