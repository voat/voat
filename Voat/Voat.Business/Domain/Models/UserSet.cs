using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Models
{
    public class UserSet 
    {
        public IList<string> Subverses { get; set; } = new List<String>();
    }
}
