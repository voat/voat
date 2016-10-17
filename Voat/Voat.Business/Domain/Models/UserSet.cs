using System;
using System.Collections.Generic;

namespace Voat.Domain.Models
{
    public class UserSet
    {
        public IList<string> Subverses { get; set; } = new List<String>();
    }
}
