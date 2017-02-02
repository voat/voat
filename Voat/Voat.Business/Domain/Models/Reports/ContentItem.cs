using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Models
{
    public class ContentItem
    {
        public ContentType ContentType { get; set; }
        public Domain.Models.Submission Submission { get; set; }
        public Domain.Models.Comment Comment { get; set; }
    }
}
