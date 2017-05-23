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
        
        // Currently not exposing this through the API but UI needs it and it is mapped from the Query
        [JsonIgnore]
        public Domain.Models.Submission Submission {get; set; }

        // Currently not exposing this through the API but UI needs it and it is mapped from the Query
        [JsonIgnore]
        public Domain.Models.Comment Comment { get; set; }

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
