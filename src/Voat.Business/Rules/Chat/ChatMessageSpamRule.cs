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
using System.Threading.Tasks;
using Voat.Common;
using Voat.Configuration;
using Voat.RulesEngine;
using Voat.Utilities;

namespace Voat.Rules.Chat
{

    [RuleDiscovery("Approves action if message isn't spammed", "approved = (IsSpam(message) == false)")]
    public class ChatMessageSpamRule : BaseCCPVote
    {
        //thresholds
        public int _count = 5;
        public TimeSpan _timeSpanWindow = TimeSpan.FromSeconds(25);

        //Adding Min CCP of 100 to send messages
        public ChatMessageSpamRule() : base("Chat Spam", "10.0", VoatSettings.Instance.MinimumCommentPointsForSendingChatMessages, RuleScope.PostChatMessage)
        {
            
        }

        protected override RuleOutcome EvaluateRule(VoatRuleContext context)
        {
            ChatMessage message = context.PropertyBag.ChatMessage;
            var currentDate = Data.Repository.CurrentDate;

            if (message == null)
            {
                return CreateOutcome(RuleResult.Denied, "Rule needs chat message contexarstt");
            }

            if (BanningUtility.ContentContainsBannedDomain(null, message.Message))
            {
                return CreateOutcome(RuleResult.Denied, "Content contains banned domain");
            }

            var history = ChatHistory.History(message.RoomID);
            var historyArray = history.ToArray();

            //Copy Pasta
            //check full history
            var duplicateFound = false;
            //duplicateFound = historyArray.Any(x => x.UserName == message.UserName && x.Message.IsEqual(message.Message.TrimSafe()));

            var lastMessage = historyArray.LastOrDefault(x => x.User.UserName == message.User.UserName);
            if (lastMessage != null)
            {
                duplicateFound = lastMessage.Message.IsEqual(message.Message.TrimSafe());
            }

            if (duplicateFound)
            {
                return CreateOutcome(RuleResult.Denied, "Chat message considered copy/paste spam");
            }

            ////Spammer
            var countInWindow = historyArray.Count(x => x.User.UserName == message.User.UserName && currentDate.Subtract(x.CreationDate) <= _timeSpanWindow);
            if (countInWindow >= _count)
            {
                return CreateOutcome(RuleResult.Denied, "Chat message considered spamming by user");
            }

            return base.EvaluateRule(context);
        }
    }
}
