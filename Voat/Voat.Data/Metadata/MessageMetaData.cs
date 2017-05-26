using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Data.Models
{
    //CORE_PORT: not supported
    //[MetadataType(typeof(DiscussionMetaData))]
    public partial class Submission
    {

    }

    public class DiscussionMetaData
    {
        //CORE_PORT: not supported
        //[AllowHtml]
        [StringLength(10000, ErrorMessage = "Submission text is limited to 10.000 characters.")]
        public string Content { get; set; }
    }
}
