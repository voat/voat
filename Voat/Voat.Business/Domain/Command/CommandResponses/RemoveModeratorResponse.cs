using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data.Models;

namespace Voat.Domain.Command
{
    public class RemoveModeratorResponse : SubverseUserActionResponse
    {
        public SubverseModerator SubverseModerator { get; set; }
    }
}
