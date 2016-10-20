using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Models;

namespace Voat
{
    public static class FriendlyEnumNames
    {
        public static string ToFriendly(this MessageType value)
        {
            var result = value.ToString();
            switch (value)
            {
                case MessageType.CommentMention:
                    result = "Comment Mention";
                    break;
                case MessageType.CommentReply:
                    result = "Comment Reply";
                    break;
                case MessageType.Private:
                    result = "Inbox";
                    break;
                case MessageType.SubmissionMention:
                    result = "Submission Mention";
                    break;
                case MessageType.SubmissionReply:
                    result = "Submission Reply";
                    break;
            }
            return result;
        }
    }
}
