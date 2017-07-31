using System;
using System.Collections.Generic;
using System.Text;
using Voat.Domain.Models;

namespace Voat.Voting.Options
{
    public class ContentOption : SubverseOption
    {
        public ContentType ContentType { get; set; }
        public int MinimumCount { get; set; }
    }
}
