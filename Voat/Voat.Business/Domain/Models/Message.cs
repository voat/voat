using Newtonsoft.Json;
using System;

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

        public MessageType Type { get; set; } = MessageType.Private;

        public string Sender { get; set; }

        public IdentityType SenderType { get; set; } = IdentityType.User;

        [JsonIgnore]
        public UserDefinition SenderDefinition
        {
            get
            {
                return new UserDefinition() { Name = Sender, Type = SenderType };
            }

            set
            {
                this.Sender = value.Name;
                this.SenderType = value.Type;
            }
        }

        public string Recipient { get; set; }

        public IdentityType RecipientType { get; set; } = IdentityType.User;

        [JsonIgnore]
        public UserDefinition RecipientDefinition
        {
            get
            {
                return new UserDefinition() { Name = Recipient, Type = RecipientType };
            }

            set
            {
                this.Recipient = value.Name;
                this.RecipientType = value.Type;
            }
        }

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

        public string CreatedBy { get; set; }

        public System.DateTime CreationDate { get; set; }

        public Message Clone()
        {
            return (Message)this.MemberwiseClone();
        }

        public bool IsSubverseMail
        {
            get
            {
                return
                    (Type == MessageType.Private && RecipientType == IdentityType.Subverse) ||
                    (Type == MessageType.Sent && SenderType == IdentityType.Subverse);
            }
        }
    }
}
