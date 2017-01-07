using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Models
{
    public class SubverseSubmissionSetting
    {
        public string Name { get; set; }
        public bool IsAdult { get; set; }
        public bool? IsAnonymized { get; set; }

    }
}
