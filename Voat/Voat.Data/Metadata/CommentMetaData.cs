using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Data.Models
{
    //TODO: This is a hack from the UI. We need to get rid of this but as AllowHtml requires a reference to the Mvc libraries and
    //the MetadataType needs to be compiled in the current assembly.
    //CORE_PORT: not supported
    //[MetadataType(typeof(CommentMetaData))]
    public partial class Comment
    {

    }

    public class CommentMetaData
    {
        [Required(ErrorMessage = "Comment text is required. Please fill this field.")]
        [StringLength(10000, ErrorMessage = "Comment text is limited to 10000 characters.")]
        //CORE_PORT: not supported
        //[AllowHtml]
        public string Content { get; set; }
    }
}
