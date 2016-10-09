using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Command
{
    public class SubverseUserActionResponse : UserActionResponse
    {
        /// <summary>
        /// Subverse that the action involved
        /// </summary>
        public string Subverse { get; set; }
    }
}
