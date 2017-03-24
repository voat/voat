using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Common;

namespace Voat
{
    public class ChatMessage
    {
        public string RoomName { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime CreationDate { get; set; }

        private static Dictionary<string, string> replacements = new Dictionary<string, string>() {
            { @"\n", "" },
            { @"\r", "" },
            { @"^(\#(\s+)?){1,}", "" },
            { @"^(\>(\s+)?){1,}", "" },
            { @"(\-\s+){2,}(\-(\s+)?)", "" }, //three or more - followed by spaces
            { @"(\*\s+){2,}(\*(\s+)?)", "" } //three or more * followed by spaces
        };

        public static string SanitizeInput(string message)
        {
            message = message.TrimSafe();
            if (!String.IsNullOrEmpty(message))
            {
                message = message.SubstringMax(500);
                message = replacements.Aggregate(message, (value, keyPair) => Regex.Replace(value, keyPair.Key, keyPair.Value));
                message = message.TrimSafe();
            }

            return message;
        }

    }

    public static class ChatHistory
    {
        public static Dictionary<string, LimitedQueue<ChatMessage>> _history = new Dictionary<string, LimitedQueue<ChatMessage>>();

        private static LimitedQueue<ChatMessage> GetOrCreateRoomHistory(string name)
        {
            LimitedQueue<ChatMessage> h = null;

            if (!String.IsNullOrEmpty(name))
            {
                name = name.ToLower();


                if (_history.ContainsKey(name))
                {
                    h = _history[name];
                }
                else
                {
                    h = new LimitedQueue<ChatMessage>(50);
                    _history.Add(name, h);
                }
            }

            return h;
        }

        public static LimitedQueue<ChatMessage> History(string name)
        {
            return GetOrCreateRoomHistory(name);
        }

        public static void Add(ChatMessage message)
        {
            LimitedQueue<ChatMessage> h = GetOrCreateRoomHistory(message.RoomName);

            if (h != null)
            {
                message.CreationDate = Data.Repository.CurrentDate;
                h.Add(message);
            }
        }
    }
}
