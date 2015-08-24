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
using System.Linq;
using Voat.Data.Models;

namespace Voat.Models.ViewModels
{
    public class SetFrontpageViewModel
    {
        public bool HasSetSubscriptions { get; set; }

        // list of default sets
        public IQueryable<UserSet> DefaultSets { get; set; }

        // list of user subscribed sets
        public IQueryable<UserSetSubscription> UserSets { get; set; }

        // list of top submissions from all sets
        public List<SetSubmission> SubmissionsList { get; set; }
    }
}