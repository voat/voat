#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Voat.Domain.Models;

namespace Voat.Models.ViewModels
{
    public class CreateSubmissionViewModel
    {
        [Required(ErrorMessage = "Post title is required. Please provide this field")]
        [StringLength(200, ErrorMessage = "The title must be at least 10 and no more than 200 characters long", MinimumLength = 10)]
        public string Title { get; set; }

        [MaxLength(10000, ErrorMessage = "Content is limited to 10,000 characters")]
        public string Content { get; set; }

        [Required(ErrorMessage = "You must enter a subverse to send the post to. Examples: programming, videos, pics, funny.")]
        [MaxLength(50, ErrorMessage = "Subverse is limited to {1} characters")]
        public string Subverse { get; set; }

        [Required(ErrorMessage = "URL is required. Please provide this field")]
        [Url(ErrorMessage = "Please enter a valid http or https link")]
        [MaxLength(3000, ErrorMessage = "URL is limited to {1} characters")]
        public string Url { get; set; }

        public SubmissionType Type { get; set; }

        public bool RequireCaptcha { get; set; }

        [DisplayName("Is Anonymized?")]
        [Description("Check this box if this post should hide your user name")]
        public bool IsAnonymized { get; set; }

        public bool AllowAnonymized { get; set; }

        [DisplayName("Is Adult (NSFW)?")]
        [Description("Check this box if this post is not safe for work (NSFW)")]
        public bool IsAdult { get; set; }

    }
}
