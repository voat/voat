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
        public Vote VoteType { get; set; }
    }

    public class StatContentItem : ContentItem
    {
        public int ID { get; set; }

        public Vote VoteType { get; set; }

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
