using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Domain.Models;

namespace Voat.Utilities
{
    public class UserEventArgs : EventArgs
    {
        public string UserID { get; set; }

    }
    public class MessageReceivedEventArgs : UserEventArgs
    {
        public MessageType MessageType { get; set; }
        public int ID { get; set; }
    }
    public class VoteReceivedEventArgs : UserEventArgs
    {
        public ContentType VoteType { get; set; }
        public int ID { get; set; }
    }
    public class EventNotification
    {
        
        public event EventHandler<MessageReceivedEventArgs> OnMentionReceived;
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        public event EventHandler<VoteReceivedEventArgs> OnVoatReceived;


        public void SendMentionNotice(string userID, MessageType type, int id)
        {
            if (OnMentionReceived != null)
            {
                OnMentionReceived(this, new MessageReceivedEventArgs() { UserID = userID, MessageType = type, ID = id }); 
            }
            OnMentionReceived?.Invoke(this, new MessageReceivedEventArgs() { UserID = userID, MessageType = type, ID = id });
        }
        public void SendMessageNotice(string userID, MessageType type, int id)
        {
            OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs() { UserID = userID, MessageType = type, ID = id });
        }
        public void SendVoteNotice(string userID, ContentType voteType, int id)
        {
            OnVoatReceived?.Invoke(this, new VoteReceivedEventArgs() { UserID = userID, VoteType = voteType, ID = id });
        }


    }
}
