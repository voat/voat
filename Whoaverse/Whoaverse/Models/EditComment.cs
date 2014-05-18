using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whoaverse.Models
{
    [Serializable]
    public class EditComment
    {
        public int CommentId { get; set; }
        public string CommentContent { get; set; }
    }
}