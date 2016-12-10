using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Voat.Domain.Models;

namespace Voat.Models.ViewModels
{
    public class CreateSubmissionViewModel
    {
        [Required(ErrorMessage = "Post title is required. Please fill this field.")]
        [StringLength(200, ErrorMessage = "The title must be at least 10 and no more than 200 characters long.", MinimumLength = 10)]
        public string Title { get; set; }

        [Required(ErrorMessage = "You must enter a subverse to send the post to. Examples: programming, videos, pics, funny...")]
        public string Subverse { get; set; }

        [MaxLength(10000, ErrorMessage = "Content is limited to 10000 characters")]
        [AllowHtml]
        public string Content { get; set; }

        [Required(ErrorMessage = "URL is required. Please provide this field.")]
        [Url(ErrorMessage = "Please enter a valid http, https, or ftp URL.")]
        public string Url { get; set; }

        public SubmissionType Type { get; set; }

        public bool RequireCaptcha { get; set; }
    }
}
