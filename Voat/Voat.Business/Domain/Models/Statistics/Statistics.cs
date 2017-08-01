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
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Models
{
    public abstract class Statistics
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    }

    public class Statistics<T> : Statistics
    {
        public T Data { get; set; }
    }

    public class StatBase
    {
        public ContentType ContentType { get; set; }
        public VoteValue VoteType { get; set; }
    }

    public class StatContentItem : ContentItem
    {
        public int ID { get; set; }

        public VoteValue VoteType { get; set; }

        public object Data { get; set; }

    }

    public class UserVoteStats : StatBase
    {
        public string UserName { get; set; }
        public int TotalCount { get; set; }
    }

    public class UserVoteReceivedStats : UserVoteStats
    {
        public int TotalVotes { get; set; }
        public decimal AvgVotes { get; set; }
    }
}
