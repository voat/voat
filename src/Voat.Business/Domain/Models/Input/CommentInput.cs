using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Voat.Domain.Models.Input
{
   
    public class CommentInput
    {
        [Required(ErrorMessage = "Comment text is required. Please fill this field.")]
        [StringLength(10000, ErrorMessage = "Comment text is limited to 10,000 characters.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "A submission ID is required")]
        public int? SubmissionID { get; set; }

        public int? ParentID { get; set; }

    }
    public class CommentEditInput
    {
        [Required(ErrorMessage = "Comment text is required. Please fill this field.")]
        [StringLength(10000, ErrorMessage = "Comment text is limited to 10,000 characters.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "A comment ID is required")]
        public int ID { get; set; }

    }
}
