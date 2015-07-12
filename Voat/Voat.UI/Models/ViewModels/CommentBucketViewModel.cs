using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Models.ViewModels
{
    public class CommentBucketViewModel
    {
        public IEnumerable<Comment> FirstComments { get; set; }
        public Message Submission { get; set; }
    }
}
