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

using System;
using Voat.Data.Models;

namespace Voat.Models
{
    public class SavedItem
    {
        public DateTime SaveDateTime { get; set; }
        public Submission SavedSubmission { get; set; }
        public Comment SavedComment { get; set; }
    }
}