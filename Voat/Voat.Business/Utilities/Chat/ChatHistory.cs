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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Caching;
using Voat.Common;

namespace Voat
{
    
    public static class ChatHistory
    {
        public static Dictionary<string, LimitedQueue<ChatMessage>> _history = new Dictionary<string, LimitedQueue<ChatMessage>>();
        //public static Dictionary<string, FlushDetector> _flushers = new Dictionary<string, FlushDetector>();

        private static LimitedQueue<ChatMessage> GetOrCreateRoomHistory(string id)
        {
            LimitedQueue<ChatMessage> h = null;

            if (!String.IsNullOrEmpty(id))
            {
                id = id.ToLower();


                if (_history.ContainsKey(id))
                {
                    h = _history[id];
                }
                else
                {
                    //Try get from cache
                    if (CacheHandler.Instance.Exists(CacheKey(id)))
                    {
                        h = CacheHandler.Instance.Retrieve<LimitedQueue<ChatMessage>>(CacheKey(id));
                    }
                    else
                    {
                        h = new LimitedQueue<ChatMessage>(100);
                    }

                    h.Limit = 100;
                    _history.Add(id, h);
                }
            }

            return h;
        }

        private static string CacheKey(string id)
        {
            return $"ChatHistory:{id}";
        }

        public static LimitedQueue<ChatMessage> History(string id)
        {
            return GetOrCreateRoomHistory(id);
        }

        public static void Add(ChatMessage message)
        {
            LimitedQueue<ChatMessage> h = GetOrCreateRoomHistory(message.RoomID);

            if (h != null)
            {
                message.CreationDate = Data.Repository.CurrentDate;
                h.Add(message);

                Task.Run(() => {
                    CacheHandler.Instance.Replace<LimitedQueue<ChatMessage>>(CacheKey(message.RoomID), h, null);
                });
            }
        }
    }
}
