using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Voat.Data.Models
{
    //TODO: This is a hack from the UI. We need to get rid of this but as AllowHtml requires a reference to the Mvc libraries and
    //the MetadataType needs to be compiled in the current assembly.
    [MetadataType(typeof(CommentMetaData))]
    public partial class Comment
    {

    }

    public class CommentMetaData
    {
        [Required(ErrorMessage = "Comment text is required. Please fill this field.")]
        [StringLength(10000, ErrorMessage = "Comment text is limited to 10.000 characters.")]
        [AllowHtml]
        public string Content { get; set; }
    }
}
