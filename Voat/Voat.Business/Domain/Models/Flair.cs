using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Models
{
    public enum AttributeType
    {
        Flair = 0,
        Data
    }
    public class ContentAttribute
    {
        public int ID { get; set; }
        public AttributeType Type { get; set; }
        public string Name { get; set; }
        public string CssClass { get; set; }
    }
}
