using System;
using Voat.Domain.Models;

namespace Voat.Utilities
{
    /// <summary>
    /// This class is an eventing hub used to simply raise events.
    /// </summary>
    public class EventNotification
    {
        private static EventNotification _instance;

        public event EventHandler<MessageReceivedEventArgs> OnMentionReceived;

        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        public event EventHandler<MessageReceivedEventArgs> OnCommentReplyReceived;

        public event EventHandler<VoteReceivedEventArgs> OnVoteReceived;

        public event EventHandler<BasicMessageEventArgs> OnHeadButtReceived;

        public event EventHandler<ChatMessageEventArgs> OnChatMessageReceived;

        public static EventNotification Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(EventNotification))
                    {
                        if (_instance == null)
                        {
                            _instance = new EventNotification();
                        }
                    }
                }
                return _instance;
            }

            set
            {
                _instance = value;
            }
        }

        public void SendMentionNotice(string userName, string sendingUserName, ContentType type, int referenceID, string message)
        {
            OnMentionReceived?.Invoke(this, new MessageReceivedEventArgs() { TargetUserName = userName, MessageType = MessageTypeFlag.CommentMention, ReferenceType = type, ReferenceID = referenceID, Message = message });
        }

        //public void SendCommentReplyNotice(string userName, MessageType type, int id)
        //{
        //    OnCommentReplyReceived?.Invoke(this, new MessageReceivedEventArgs() { UserName = userName, MessageType = type, ReferenceID = id });
        //}
        public void SendMessageNotice(string userName, string sendingUserName, MessageTypeFlag type, ContentType? referenceType, int? referenceID, string message = null)
        {
            OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs() { TargetUserName = userName, SendingUserName = sendingUserName, MessageType = type, ReferenceType = referenceType, ReferenceID = referenceID, Message = message });
        }

        public void SendVoteNotice(string userName, string sendingUserName, ContentType referenceType, int referenceID, int voteValue)
        {
            OnVoteReceived?.Invoke(this, new VoteReceivedEventArgs() { TargetUserName = userName, SendingUserName = sendingUserName, ReferenceType = referenceType, ReferenceID = referenceID, ChangeValue = voteValue });
        }

        public void SendHeadButtNotice(string userName, string sendingUserName, string message)
        {
            OnHeadButtReceived?.Invoke(this, new BasicMessageEventArgs() { TargetUserName = userName, SendingUserName = sendingUserName, Message = message });
        }

        public void SendChatMessageNotice(string userName, string sendingUserName, string chatRoom, string message)
        {
            OnChatMessageReceived?.Invoke(this, new ChatMessageEventArgs() { TargetUserName = userName, SendingUserName = sendingUserName, Message = message, Chatroom = chatRoom });
        }
    }

    #region Event Args Classes

    public class UserEventArgs : EventArgs
    {
        public string TargetUserName { get; set; }

        public string SendingUserName { get; set; }
    }

    public class BasicMessageEventArgs : UserEventArgs
    {
        public ContentType? ReferenceType { get; set; }

        public int? ReferenceID { get; set; }

        public string Message { get; set; }
    }

    public class ChatMessageEventArgs : BasicMessageEventArgs
    {
        public string Chatroom { get; set; }
    }

    public class MessageReceivedEventArgs : BasicMessageEventArgs
    {
        public MessageTypeFlag MessageType { get; set; }
    }

    public class VoteReceivedEventArgs : BasicMessageEventArgs
    {
        public int ChangeValue { get; set; }
    }

    #endregion Event Args Classes
}
