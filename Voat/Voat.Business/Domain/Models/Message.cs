using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Domain.Models
{
    public class Message
    {

        public static string NewCorrelationID()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }

        public int ID { get; set; }
        public string CorrelationID { get; set; } = NewCorrelationID();
        public Nullable<int> ParentID { get; set; }

        public MessageDirection Direction { get; set; } = MessageDirection.InBound;
        public MessageType Type { get; set; } = MessageType.Private;

        public string Sender { get; set; }
        public MessageIdentityType SenderType { get; set; } = MessageIdentityType.User;
        public string Recipient { get; set; }
        public MessageIdentityType RecipientType { get; set; } = MessageIdentityType.User;

        public string Title { get; set; }
        public string Content { get; set; }
        public string FormattedContent { get; set; }

        public string Subverse { get; set; }
        public Nullable<int> SubmissionID { get; set; }
        public Nullable<int> CommentID { get; set; }


        public bool IsAnonymized { get; set; }
        public Nullable<System.DateTime> ReadDate { get; set; }

        public bool IsRead
        {
            get { return ReadDate != null; }    
        }
        public System.DateTime CreationDate { get; set; }

        public Message Clone()
        {
            return (Message)this.MemberwiseClone();
        }   
    }
}
