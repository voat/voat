using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whoaverse.Models
{
    public class CommentViewModel
    {
        public CommentViewModel(){

            this.ChildComments = new HashSet<Comment>();            

        }


        public int Id { get; set; }
        public Nullable<int> Votes { get; set; }
        public string Name { get; set; }
        public System.DateTime Date { get; set; }
        public Nullable<int> MessageId { get; set; }
        public string CommentContent { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public Nullable<int> ParentId { get; set; }

        public virtual ICollection<Comment> ChildComments { get; set; }
        public virtual Message Message { get; set; }
    }

    
}