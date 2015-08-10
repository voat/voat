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
namespace Voat.Models.ApiModels
{
    public class ApiMessage
    {
        public int CommentCount { get; set; }
        public DateTime Date { get; set; }
        public int Dislikes { get; set; }
        public int Likes { get; set; }
        public int Id { get; set; }
        public Nullable<DateTime> LastEditDate { get; set; }
        public string Linkdescription { get; set; }
        public string MessageContent { get; set; }
        public string Name { get; set; }
        public double Rank { get; set; }
        public string Subverse { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public int Type { get; set; }
    }
}