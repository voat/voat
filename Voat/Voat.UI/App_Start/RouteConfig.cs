/*
This source file is subject to version 3 of the GPL license,
that is bundled with this package in the file LICENSE, and is
available online at http://www.gnu.org/licenses/gpl.txt;
you may not use this file except in compliance with the License.

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2015 Voat, Inc.
All Rights Reserved.
*/

using System;
using System.Web.Mvc;
using System.Web.Routing;
using Voat.Configuration;
using Voat.Domain.Models;
using Voat.Utilities;

namespace Voat
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.LowercaseUrls = true;

            // /dashboard
            routes.MapRoute(
                name: "dashboard",
                url: "dashboard",
                defaults: new { controller = "Management", action = "Dashboard" }
            );

            // /rss
            routes.MapRoute(
                name: "rss",
                url: "rss/{subverseName}",
                defaults: new { controller = "RSS", action = "RSS", subverseName = UrlParameter.Optional }
            );

            // /advertize
            routes.MapRoute(
                name: "advertize",
                url: "advertize",
                defaults: new { controller = "Home", action = "Advertize" }
            );

            // /random
            routes.MapRoute(
                name: "randomSubverse",
                url: "random/",
                defaults: new { controller = "Subverses", action = "Random" }
            );

            // /randomnsfw
            routes.MapRoute(
                name: "randomNsfwSubverse",
                url: "randomnsfw/",
                defaults: new { controller = "Subverses", action = "RandomNsfw" }
            );

            // /v2, disabled until further notice
            //routes.MapRoute(
            //    name: "v2",
            //    url: "v2",
            //    defaults: new { controller = "Home", action = "IndexV2" }
            //);

            // /set/setId
            routes.MapRoute(
                name: "SingleUserSet",
                url: "set/{setId}",
                defaults: new { controller = "Sets", action = "SingleSet" }
            );

            // /set/setId/edit
            routes.MapRoute(
                name: "EditSet",
                url: "set/{setId}/edit",
                defaults: new { controller = "Sets", action = "EditSet" }
            );

            // /set/setId/page
            routes.MapRoute(
                name: "SingleUserSetPage",
                url: "set/{setId}/{page}",
                defaults: new { controller = "Sets", action = "SingleSetPage" }
            );

            // /subverses/create
            routes.MapRoute(
                name: "CreateSubverse",
                url: "subverses/create",
                defaults: new { controller = "Subverses", action = "CreateSubverse" }
            );

            // /search
            routes.MapRoute(
                name: "Search",
                url: "search",
                defaults: new { controller = "Search", action = "SearchResults" }
            );

            // /search/findsubverse
            routes.MapRoute(
                name: "FindSubverse",
                url: "search/findsubverse",
                defaults: new { controller = "Search", action = "FindSubverse" }
            );

            // /subverses
            routes.MapRoute(
                name: "Subverses",
                url: "subverses/",
                defaults: new { controller = "Subverses", action = "Subverses" }
            );

            // /acceptmodinvitation/{invitationId}
            routes.MapRoute(
                name: "AcceptModInvitation",
                url: "acceptmodinvitation/{invitationId}",
                defaults: new { controller = "Subverses", action = "AcceptModInvitation" }
            );

            // /subverses/search
            routes.MapRoute(
                name: "SearchSubverseForm",
                url: "subverses/search",
                defaults: new { controller = "Subverses", action = "Search" }
            );

            // /subverses/subscribed
            routes.MapRoute(
                name: "SubscribedSubverses",
                url: "subverses/subscribed",
                defaults: new { controller = "Subverses", action = "SubversesSubscribed" }
            );

            // /subverses/active
            routes.MapRoute(
                name: "ActiveSubverses",
                url: "subverses/active",
                defaults: new { controller = "Subverses", action = "ActiveSubverses" }
            );

            // /subverses/adultcontent
            routes.MapRoute(
                name: "AdultContentWarning",
                url: "subverses/adultcontent",
                defaults: new { controller = "Subverses", action = "AdultContentWarning" }
            );

            // /subverses/adultcontentfiltered
            routes.MapRoute(
                name: "AdultContentFiltered",
                url: "subverses/adultcontentfiltered",
                defaults: new { controller = "Subverses", action = "AdultContentFiltered" }
            );

            // /subverses/new
            routes.MapRoute(
                name: "NewestSubverses",
                url: "subverses/{sortingmode}",
                defaults: new { controller = "Subverses", action = "NewestSubverses" }
            );

            // comments/4
            routes.MapRoute(
                name: "comments",
                url: "comments/{submissionID}",
                defaults: new { controller = "Comment", action = "Comments" }
            );

            string commentSortContraint = "(?i)" + String.Join("|", Enum.GetNames(typeof(CommentSortAlgorithm)));

            // /comments/submission/startingpos
            //"/comments/" + submission + "/" + parentId + "/" + command + "/" + startingIndex + "/" + startIndex + "/" + sort + "/",
            routes.MapRoute(
                name: "CommentSegment",
                url: "comments/{submissionID}/{parentID}/{command}/{startingIndex}/{sort}",
                constraints: new
                {
                    sort = commentSortContraint
                },
                defaults: new { controller = "Comment", action = "CommentSegment", sort = "top" }
            );

            routes.MapRoute(
                name: "CommentTree",
                url: "comments/{submissionID}/tree/{sort}",
                constraints: new
                {
                    sort = commentSortContraint
                },
                defaults: new { controller = "Comment", action = "CommentTree", sort = "top" }
            );


            // v/subverse/123456/123456/delete
            routes.MapRoute(
                name: "SubverseStylesheet",
                url: "v/{subverse}/stylesheet",
                constraints: new
                {
                },
                defaults: new
                {
                    controller = "Subverses",
                    action = "Stylesheet",
                }
            );
            // v/subverse/123456/123456/delete
            routes.MapRoute(
                name: "ModeratorDeleteSubmission",
                url: "v/{subverse}/{submissionID}/delete",
                constraints: new
                {
                    submissionID = CONSTANTS.SUBMISSION_ID_REGEX
                },
                defaults: new
                {
                    controller = "Submissions",
                    action = "ModeratorDelete",
                }
            );

            #region Comment Pages

            // v/subversetoshow/123456/123456/delete
            routes.MapRoute(
                name: "ModeratorDeleteComment",
                url: "v/{subverse}/{submissionID}/{commentID}/delete",
                constraints: new
                {
                    submissionID = CONSTANTS.SUBMISSION_ID_REGEX,
                    commentID = CONSTANTS.COMMENT_ID_REGEX
                },
                defaults: new
                {
                    controller = "Comment",
                    action = "ModeratorDelete",
                }
            );

            // v/subverse/comments/123456/new
            routes.MapRoute(
                name: "SubverseCommentsWithSort",
                url: "v/{subverseName}/comments/{submissionID}/{sort}",
                constraints: new
                {
                    sort = commentSortContraint
                },
                defaults: new
                {
                    controller = "Comment",
                    action = "Comments",
                    sort = "top",
                    commentID = UrlParameter.Optional,
                    contextCount = UrlParameter.Optional
                }
            );

            // v/subverse/comments/123456
            routes.MapRoute(
                name: "SubverseComments",
                url: "v/{subverseName}/comments/{submissionID}/{commentID}/{context}",
                defaults: new
                {
                    controller = "Comment",
                    action = "Comments",
                    commentID = UrlParameter.Optional,
                    context = UrlParameter.Optional,
                    sort = "top"
                }
            );

            // v/subverseName/comments/123456/new
            routes.MapRoute(
                name: "SubverseCommentsWithSort_Short",
                url: "v/{subverseName}/{submissionID}/{sort}",
                constraints: new
                {
                    sort = commentSortContraint,
                    submissionID = @"\d+"
                },
                defaults: new
                {
                    controller = "Comment",
                    action = "Comments",
                    sort = "top",
                    commentID = UrlParameter.Optional,
                    contextCount = UrlParameter.Optional
                }
            );

            // v/subversetoshow/comments/123456
            routes.MapRoute(
                name: "SubverseComments_Short",
                url: "v/{subverseName}/{submissionID}/{commentID}/{context}",
                constraints:
                new
                {
                    submissionID = @"\d+",
                    commentID = @"\d+"
                },
                defaults: new
                {
                    controller = "Comment",
                    action = "Comments",
                    commentID = UrlParameter.Optional,
                    context = UrlParameter.Optional,
                    sort = "top"
                }
            );

            // comments/distinguish/412
            routes.MapRoute(
                name: "distinguishcomment",
                url: "comments/distinguish/{commentId}",
                defaults: new { controller = "Comment", action = "DistinguishComment" }
            );
            #endregion Comment Pages



            #region User

            routes.MapRoute(
               name: "UserBlocks",
               url: "{pathPrefix}/blocked/{blockType}",
               defaults: new { controller = "User", action = "Blocked" },
               constraints: new { httpMethod = new HttpMethodConstraint("GET"), pathPrefix = "user|u", blockType = "user|subverse" }
            );
            routes.MapRoute(
              name: "BlockUserPost",
              url: "{pathPrefix}/blocked/{blockType}",
              defaults: new { controller = "User", action = "BlockUser" },
              constraints: new { httpMethod = new HttpMethodConstraint("POST"), pathPrefix = "user|u", blockType = "user|subverse" }
           );

            //routes.MapRoute(
            //   name: "BlockUserPost",
            //   url: "user/blockuser",
            //   defaults: new { controller = "User", action = "BlockUser" },
            //   constraints: new {  }
            //);

         
            routes.MapRoute(
                name: "UserComments",
                url: "{pathPrefix}/{userName}/comments",
                defaults: new { controller = "User", action = "Comments" },
                constraints: new { pathPrefix = "user|u" }
            );

            routes.MapRoute(
               name: "UserSubmissions",
               url: "{pathPrefix}/{userName}/submissions",
               defaults: new { controller = "User", action = "Submissions" },
                constraints: new { pathPrefix = "user|u" }
           );

            routes.MapRoute(
              name: "UserSaved",
              url: "{pathPrefix}/{userName}/saved",
              defaults: new { controller = "User", action = "Saved" },
              constraints: new { pathPrefix = "user|u" }
            );

            routes.MapRoute(
              name: "UserProfile",
              url: "{pathPrefix}/{userName}",
              defaults: new { controller = "User", action = "Overview" },
              constraints: new { pathPrefix = "user|u" }
            );

            routes.MapRoute(
               name: "Block",
               url: "block/{blockType}",
               defaults: new { controller = "User", action = "Block" },
               constraints: new { blockType = "user|subverse" }
           );

            #endregion User

            #region Message

            var messageRoot = "messages";
            var messageController = "Messages";

            routes.MapRoute(
              name: "MessageMark",
              url: messageRoot + "/mark/{type}/{markAction}/{id}",
              defaults: new
              {
                  controller = messageController,
                  action = "Mark",
                  id = UrlParameter.Optional
              },
              constraints: new
              {
                  id = @"^$|\d+",
                  markAction = "read|unread",
                  type = String.Join("|", Enum.GetNames(typeof(MessageTypeFlag))).ToLower()
              }
           );

           routes.MapRoute(
              name: "MessageDelete",
              url: messageRoot + "/delete/{type}/{id}",
              defaults: new
              {
                  controller = messageController,
                  action = "Delete"
                  //,id = UrlParameter.Optional let's make the ID required for now so we don't have a bug that deletes an entire inbox
              },
              constraints: new
              {
                  id = @"\d+",
                  type = String.Join("|", Enum.GetNames(typeof(MessageTypeFlag))).ToLower()
              }
            );


            routes.MapRoute(
                name: "MessageIndex",
                url: messageRoot + "",
                defaults: new {
                    controller = messageController,
                    action = "Index"
                }
            );

            routes.MapRoute(
              name: "MessageInbox",
              url: messageRoot + "/inbox",
              defaults: new
              {
                  controller = messageController,
                  action = "Index"
              }
            );

            routes.MapRoute(
                name: "MessageCompose",
                url: messageRoot + "/compose",
                defaults: new
                {
                    controller = messageController,
                    action = "Compose"
                }
            );
            routes.MapRoute(
              name: "MessageNotifications",
              url: messageRoot + "/notifications",
              defaults: new
              {
                  controller = messageController,
                  action = "Notifications"
              }
          );
            routes.MapRoute(
               name: "MessagesSent",
               url: messageRoot + "/sent",
               defaults: new
               {
                   controller = messageController,
                   action = "Sent"
               }
           );

            routes.MapRoute(
               name: "MessageReply",
               url: messageRoot + "/reply",
               defaults: new
               {
                   controller = messageController,
                   action = "Reply"
               }
            );
            routes.MapRoute(
              name: "MessageReplyForm",
              url: messageRoot + "/reply/{id}",
              defaults: new
              {
                  controller = messageController,
                  action = "ReplyForm"
              },
              constraints: new {
                  id = @"\d+",
                  httpMethod = new HttpMethodConstraint("GET")
              }
            );
            routes.MapRoute(
               name: "MessagesMentions",
               url: messageRoot + "/mentions/{type}",
               defaults: new
               {
                   controller = messageController,
                   action = "Mentions",
                   type = UrlParameter.Optional
               }
            );
            routes.MapRoute(
               name: "MessagesReplies",
               url: messageRoot + "/replies/{type}",
               defaults: new
               {
                   controller = messageController,
                   action = "Replies",
                   type = UrlParameter.Optional
               }
            );
            //public async Task<ActionResult> SubverseIndex(string subverse, MessageTypeFlag type, MessageState? status = null)
            routes.MapRoute(
             name: "SubverseMail",
             url: "v/{subverse}/" + messageRoot + "/{type}/{state}",
             defaults: new
             {
                 controller = messageController,
                 action = "SubverseIndex",
                 state = MessageState.All,
                 type = MessageTypeFlag.Private
             },
             constraints: new
             {
                 state = String.Join("|", Enum.GetNames(typeof(MessageState))).ToLower(),
                 type = "private|sent"
             }
            );

            routes.MapRoute(
            name: "SubverseMailCompose",
            url: "v/{subverse}/" + messageRoot + "/compose",
            defaults: new
            {
                controller = messageController,
                action = "Compose"
            }
           );
            // routes.MapRoute(
            //   name: "ModMessages",
            //   url: "v/{subverse}/" + messageRoot,
            //   defaults: new
            //   {
            //       controller = messageController,
            //       action = "Sent"
            //   }
            //);
            //// inbox/unread
            //routes.MapRoute(
            //    name: "InboxUnread",
            //    url: "messaging/inbox/unread",
            //    defaults: new { controller = "Messaging", action = "InboxPrivateMessagesUnread" }
            //);

            //// compose
            //routes.MapRoute(
            //    name: "Compose",
            //    url: "messaging/compose",
            //    defaults: new { controller = "Messaging", action = "Compose" }
            //);

            //// send private message
            //routes.MapRoute(
            //    name: "SendPrivateMessage",
            //    url: "messaging/sendprivatemessage",
            //    defaults: new { controller = "Messaging", action = "SendPrivateMessage" }
            //);

            //// sent
            //routes.MapRoute(
            //    name: "Sent",
            //    url: "messaging/sent",
            //    defaults: new { controller = "Messaging", action = "Sent" }
            //);

            //// commentreplies
            //routes.MapRoute(
            //    name: "CommentReplies",
            //    url: "messaging/commentreplies",
            //    defaults: new { controller = "Messaging", action = "InboxCommentReplies" }
            //);

            //// postreplies
            //routes.MapRoute(
            //    name: "PostReplies",
            //    url: "messaging/postreplies",
            //    defaults: new { controller = "Messaging", action = "InboxPostReplies" }
            //);

            //// deleteprivatemessage
            //routes.MapRoute(
            //    name: "DeletePrivateMessage",
            //    url: "messaging/delete",
            //    defaults: new { controller = "Messaging", action = "DeletePrivateMessage" }
            //);

            //// deleteprivatemessagefromsent
            //routes.MapRoute(
            //    name: "DeletePrivateMessageFromSent",
            //    url: "messaging/deletesent",
            //    defaults: new { controller = "Messaging", action = "DeletePrivateMessageFromSent" }
            //);

            //// markinboxitemasread
            //routes.MapRoute(
            //    name: "MarkInboxItemAsRead",
            //    url: "messaging/markasread",
            //    defaults: new { controller = "Messaging", action = "MarkAsRead" }
            //);

            #endregion Message

            #region OLD_Messaging

            //// inbox
            //routes.MapRoute(
            //    name: "Inbox",
            //    url: "messaging/inbox",
            //    defaults: new { controller = "Messaging", action = "Inbox" }
            //);

            //// inbox/unread
            //routes.MapRoute(
            //    name: "InboxUnread",
            //    url: "messaging/inbox/unread",
            //    defaults: new { controller = "Messaging", action = "InboxPrivateMessagesUnread" }
            //);

            //// compose
            //routes.MapRoute(
            //    name: "Compose",
            //    url: "messaging/compose",
            //    defaults: new { controller = "Messaging", action = "Compose" }
            //);

            //// send private message
            //routes.MapRoute(
            //    name: "SendPrivateMessage",
            //    url: "messaging/sendprivatemessage",
            //    defaults: new { controller = "Messaging", action = "SendPrivateMessage" }
            //);

            //// sent
            //routes.MapRoute(
            //    name: "Sent",
            //    url: "messaging/sent",
            //    defaults: new { controller = "Messaging", action = "Sent" }
            //);

            //// commentreplies
            //routes.MapRoute(
            //    name: "CommentReplies",
            //    url: "messaging/commentreplies",
            //    defaults: new { controller = "Messaging", action = "InboxCommentReplies" }
            //);

            //// postreplies
            //routes.MapRoute(
            //    name: "PostReplies",
            //    url: "messaging/postreplies",
            //    defaults: new { controller = "Messaging", action = "InboxPostReplies" }
            //);

            //// deleteprivatemessage
            //routes.MapRoute(
            //    name: "DeletePrivateMessage",
            //    url: "messaging/delete",
            //    defaults: new { controller = "Messaging", action = "DeletePrivateMessage" }
            //);

            //// deleteprivatemessagefromsent
            //routes.MapRoute(
            //    name: "DeletePrivateMessageFromSent",
            //    url: "messaging/deletesent",
            //    defaults: new { controller = "Messaging", action = "DeletePrivateMessageFromSent" }
            //);

            //// markinboxitemasread
            //routes.MapRoute(
            //    name: "MarkInboxItemAsRead",
            //    url: "messaging/markasread",
            //    defaults: new { controller = "Messaging", action = "MarkAsRead" }
            //);

            #endregion OLD_Messaging

            // help/pagetoshow
            routes.MapRoute(
                name: "Help",
                url: "help/{*pagetoshow}",
                defaults: new { controller = "Home", action = "Help" }
            );

            // about/pagetoshow
            routes.MapRoute(
                name: "About",
                url: "about/{*pagetoshow}",
                defaults: new { controller = "Home", action = "About" }
            );

            // cla
            routes.MapRoute(
                name: "cla",
                url: "cla/",
                defaults: new { controller = "Home", action = "Cla" }
            );

            // subscribe to subverse
            routes.MapRoute(
                name: "subscribe",
                url: "subscribe/{subverseName}",
                defaults: new { controller = "Subverses", action = "Subscribe" }
            );

            // block a subverse
            routes.MapRoute(
                name: "blocksubverse",
                url: "subverses/block/{subverseName}",
                defaults: new { controller = "Subverses", action = "BlockSubverse" }
            );

            // subscribe to set
            routes.MapRoute(
                name: "subscribetoset",
                url: "subscribetoset/{setId}",
                defaults: new { controller = "Sets", action = "Subscribe" }
            );

            // unsubscribe from subverse
            routes.MapRoute(
                name: "unsubscribe",
                url: "unsubscribe/{subverseName}",
                defaults: new { controller = "Subverses", action = "UnSubscribe" }
            );

            // unsubscribe from set
            routes.MapRoute(
                name: "unsubscribefromset",
                url: "unsubscribefromset/{setId}",
                defaults: new { controller = "Sets", action = "UnSubscribe" }
            );

            // vote
            routes.MapRoute(
                name: "vote",
                url: "vote/{submissionID}/{typeOfVote}",
                defaults: new { controller = "Submissions", action = "Vote" }
            );

            // vote comment
            routes.MapRoute(
                name: "votecomment",
                url: "votecomment/{commentId}/{typeOfVote}",
                defaults: new { controller = "Comment", action = "VoteComment" }
            );

            // save
            routes.MapRoute(
                name: "save",
                url: "save/{messageId}",
                defaults: new { controller = "Submissions", action = "Save" }
            );

            // save comment
            routes.MapRoute(
                name: "savecomment",
                url: "savecomment/{commentId}",
                defaults: new { controller = "Comment", action = "SaveComment" }
            );

            // editcomment
            routes.MapRoute(
                  "editcomment",
                  "editcomment/{id}",
                  new { controller = "Comment", action = "EditComment", id = UrlParameter.Optional }
             );

            // deletecomment
            routes.MapRoute(
                  "deletecomment",
                  "deletecomment/{id}",
                  new { controller = "Comment", action = "DeleteComment", id = UrlParameter.Optional }
             );

            // reportContent
            routes.MapRoute(
                  "reportContent",
                  "report/{type}/{id}",
                  constraints: new
                  {
                      type = "comment|submission",
                      id = @"\d+"
                  },
                  defaults: new
                  {
                      controller = "Report",
                      action = "ReportContent",
                      id = UrlParameter.Optional
                  }
             );

            // editsubmission
            routes.MapRoute(
                  "editsubmission",
                  "editsubmission/{id}",
                  new { controller = "Submissions", action = "EditSubmission", id = UrlParameter.Optional }
             );

            // deletesubmission
            routes.MapRoute(
                  "deletesubmission",
                  "deletesubmission/{id}",
                  new { controller = "Submissions", action = "DeleteSubmission", id = UrlParameter.Optional }
             );

            // submit
            routes.MapRoute(
                name: "submit",
                url: "submit",
                defaults: new { controller = "Home", action = "Submit" }
            );

            // submit link shortcut
            routes.MapRoute(
                name: "submitlinkservice",
                url: "submitlink",
                defaults: new { controller = "Home", action = "SubmitLinkService" }
            );

            // v/selectedsubverse/submit
            routes.MapRoute(
                name: "submitpost",
                url: "v/{selectedsubverse}/submit",
                defaults: new { controller = "Home", action = "Submit" }
            );

            // submitcomment
            routes.MapRoute(
                name: "submitcomment",
                url: "submitcomment",
                defaults: new { controller = "Comment", action = "SubmitComment" }
            );

            //Map Explicit API Keys Controller
            routes.MapRoute(
               name: "apikeys",
               url: "apikeys/{action}/{id}",
               defaults: new
               {
                   controller = "ApiKeys",
                   action = "Index",
                   id = UrlParameter.Optional
               }
            );

            if (!Settings.SignalRDisabled)
            {
                routes.MapRoute(
                    name: "JoinChat",
                    url: "chat/{subverseName}",
                    defaults: new { controller = "Chat", action = "Index", subverseName = UrlParameter.Optional }
                );
            }

            //// /new
            //routes.MapRoute(
            //    name: "FrontNew",
            //    url: "{sort}",
            //    defaults: new { controller = "Subverses", action = "SubverseIndex", subverse = "_front" }
            //);

            // /
            routes.MapRoute(
                name: "FrontIndex",
                url: "{sort}",
                defaults: new {
                    controller = "Subverses",
                    action = "SubverseIndex",
                    subverse = "_front",
                    sort = UrlParameter.Optional
                }
            );

            #region Mod Logs

            routes.MapRoute(
                name: "modLogSubmission",
                url: "v/{subverse}/modlog/submission",
                defaults: new { controller = "ModLog", action = "Submissions" }
            );

            routes.MapRoute(
                name: "modLogComment",
                url: "v/{subverse}/modlog/comment",
                defaults: new { controller = "ModLog", action = "Comments" }
            );

            routes.MapRoute(
                name: "modLogBanned",
                url: "v/{subverse}/modlog/banned",
                defaults: new { controller = "ModLog", action = "Banned" }
            );

            //Compat routes
            //https://voat.co/v/test/modlog/deleted
            routes.MapRoute(
                name: "modLogSubmission_Old",
                url: "v/{subverse}/modlog/deleted",
                defaults: new { controller = "ModLog", action = "Submissions" }
            );
            //https://voat.co/v/test/modlog/deletedcomments
            routes.MapRoute(
                name: "modLogComment_Old",
                url: "v/{subverse}/modlog/deletedcomments",
                defaults: new { controller = "ModLog", action = "Comments" }
            );
            //https://voat.co/v/test/modlog/bannedusers
            routes.MapRoute(
                name: "modLogBanned_Old",
                url: "v/{subverse}/modlog/bannedusers",
                defaults: new { controller = "ModLog", action = "Banned" }
            );

            #endregion Mod Logs

            #region Sub Moderation

            // v/subversetoedit/about/edit
            routes.MapRoute(
                name: "subverseSettings",
                url: "v/{subversetoshow}/about/edit",
                defaults: new { controller = "Subverses", action = "SubverseSettings" }
            );

            // v/subversetoedit/about/edit/stylesheet
            routes.MapRoute(
                name: "subverseStylesheetEditor",
                url: "v/{subversetoshow}/about/edit/stylesheet",
                defaults: new { controller = "Subverses", action = "SubverseStylesheetEditor" }
            );

            // v/subversetoedit/about/flair
            routes.MapRoute(
                name: "subverseFlairSettings",
                url: "v/{subversetoshow}/about/flair",
                defaults: new { controller = "Subverses", action = "SubverseFlairSettings" }
            );

            // v/subversetoedit/about/linkflair/add
            routes.MapRoute(
                name: "addSubverseLinkFlair",
                url: "v/{subversetoshow}/about/linkflair/add",
                defaults: new { controller = "Subverses", action = "AddLinkFlair" }
            );

            // v/subversetoedit/about/linkflair/delete
            routes.MapRoute(
                name: "removeSubverseLinkFlair",
                url: "v/{subversetoshow}/about/flair/delete/{id}",
                defaults: new { controller = "Subverses", action = "RemoveLinkFlair" }
            );

            // v/subversetoedit/about/moderators
            routes.MapRoute(
                name: "subverseModerators",
                url: "v/{subversetoshow}/about/moderators",
                defaults: new { controller = "Subverses", action = "SubverseModerators" }
            );

            // v/subversetoedit/about/bans
            routes.MapRoute(
                name: "subverseBans",
                url: "v/{subversetoshow}/about/bans",
                defaults: new { controller = "Subverses", action = "SubverseBans" }
            );

            // v/subversetoedit/about/moderators/add
            routes.MapRoute(
                name: "addSubverseModerator",
                url: "v/{subversetoshow}/about/moderators/add",
                defaults: new { controller = "Subverses", action = "AddModerator" }
            );

            // v/subversetoedit/about/bans/add
            routes.MapRoute(
                name: "addSubverseBan",
                url: "v/{subversetoshow}/about/bans/add",
                defaults: new { controller = "Subverses", action = "AddBan" }
            );

            // v/subversetoedit/about/moderators/delete
            routes.MapRoute(
                name: "removeSubverseModerator",
                url: "v/{subversetoshow}/about/moderators/delete/{id}",
                defaults: new { controller = "Subverses", action = "RemoveModerator" }
            );

            // v/subversetoedit/about/moderatorinvitations/delete
            routes.MapRoute(
                name: "removeSubverseModeratorInvitation",
                url: "v/{subversetoshow}/about/moderatorinvitations/delete/{invitationId}",
                defaults: new { controller = "Subverses", action = "RecallModeratorInvitation" }
            );

            // v/subversetoedit/about/bans/delete
            routes.MapRoute(
                name: "removeSubverseBan",
                url: "v/{subversetoshow}/about/bans/delete/{id}",
                defaults: new { controller = "Subverses", action = "RemoveBan" }
            );

            // v/subversetoedit/about/moderators/leave
            routes.MapRoute(
                name: "resignAsModerator",
                url: "v/{subversetoresignfrom}/about/moderators/resign/",
                defaults: new { controller = "Subverses", action = "ResignAsModerator" }
            );

            #endregion Sub Moderation

            // v/subversetoshow
            routes.MapRoute(
                name: "SubverseIndex",
                url: "v/{subverse}/{sort}",
                defaults: new {
                    controller = "Subverses",
                    action = "SubverseIndex",
                    sort = UrlParameter.Optional
                }
            );

            //// v/subversetoshow/sortingmode
            //routes.MapRoute(
            //    name: "SortedSubverseFrontpage",
            //    url: "v/{subverse}/{sort}",
            //    defaults: new { controller = "Subverses", action = "SubverseIndex" }
            //);

            // ajaxhelpers/commentreplyform
            routes.MapRoute(
                name: "CommentReplyForm",
                url: "ajaxhelpers/commentreplyform/{parentCommentId}/{messageId}",
                defaults: new { controller = "HtmlElements", action = "CommentReplyForm" }
            );

            #region Domains

            // domains/domainname.com
            routes.MapRoute(
                name: "DomainIndex",
                url: "domains/{domainname}/{sortingmode}",
                defaults: new {
                    controller = "Domains",
                    action = "Index",
                    sortingmode = "hot"
                }
            );
            
            #endregion Domains

            //// ajaxhelpers/singlesubmissioncomment
            //routes.MapRoute(
            //    name: "SingleSubmissionComment",
            //    url: "ajaxhelpers/singlesubmissioncomment/{messageId}/{userName}",
            //    defaults: new { controller = "HtmlElements", action = "SingleMostRecentCommentByUser" }
            //);

            // ajaxhelpers/privatemessagereplyform
            routes.MapRoute(
                name: "PrivateMessageReplyForm",
                url: "ajaxhelpers/privatemessagereplyform/{parentPrivateMessageId}",
                defaults: new { controller = "HtmlElements", action = "PrivateMessageReplyForm" }
            );

            // ajaxhelpers/messagecontent
            routes.MapRoute(
                name: "MessageContent",
                url: "ajaxhelpers/messagecontent/{messageId}",
                defaults: new { controller = "AjaxGateway", action = "MessageContent" }
            );

            // ajaxhelpers/basicuserinfo
            routes.MapRoute(
                name: "BasicUserInfo",
                url: "ajaxhelpers/userinfo/{userName}",
                defaults: new { controller = "AjaxGateway", action = "UserBasicInfo" }
            );

            // ajaxhelpers/videoplayer
            routes.MapRoute(
                name: "VideoPlayer",
                url: "ajaxhelpers/videoplayer/{messageId}",
                defaults: new { controller = "AjaxGateway", action = "VideoPlayer" }
            );

            // ajaxhelpers/rendersubmission
            routes.MapRoute(
                name: "RenderSubmission",
                url: "ajaxhelpers/rendersubmission",
                defaults: new { controller = "AjaxGateway", action = "RenderSubmission" }
            );

            // ajaxhelpers/previewstylesheet
            routes.MapRoute(
                name: "PreviewStylesheet",
                url: "ajaxhelpers/previewstylesheet",
                defaults: new { controller = "AjaxGateway", action = "PreviewStylesheet" }
            );

            // ajaxhelpers/linkflairselectdialog
            routes.MapRoute(
                name: "LinkFlairSelectDialog",
                url: "ajaxhelpers/linkflairselectdialog/{subversetoshow}/{messageId}",
                defaults: new { controller = "AjaxGateway", action = "SubverseLinkFlairs" }
            );

            // ajaxhelpers/titlefromuri
            routes.MapRoute(
                name: "TitleFromUri",
                url: "ajaxhelpers/titlefromuri",
                defaults: new { controller = "AjaxGateway", action = "TitleFromUri" }
            );

            // ajaxhelpers/autocompletesubversename
            routes.MapRoute(
                name: "AutocompleteSubverseName",
                url: "ajaxhelpers/autocompletesubversename",
                defaults: new { controller = "AjaxGateway", action = "AutocompleteSubverseName" }
            );

            // ajaxhelpers/subverseinfo
            routes.MapRoute(
                name: "SetSubverseInfo",
                url: "ajaxhelpers/setsubverseinfo/{setId}/{subverseName}",
                defaults: new { controller = "AjaxGateway", action = "SubverseBasicInfo" }
            );

            // submissions/applylinkflair
            routes.MapRoute(
                name: "ApplyLinkFlair",
                url: "submissions/applylinkflair/{submissionId}/{flairId}",
                defaults: new { controller = "Submissions", action = "ApplyLinkFlair" }
            );

            // submissions/clearlinkflair
            routes.MapRoute(
                name: "ClearLinkFlair",
                url: "submissions/clearlinkflair/{submissionId}",
                defaults: new { controller = "Submissions", action = "ClearLinkFlair" }
            );

            // submissions/togglesticky
            routes.MapRoute(
                name: "ToggleSticky",
                url: "submissions/togglesticky/{submissionId}",
                defaults: new { controller = "Submissions", action = "ToggleSticky" }
            );

            if (!Settings.SetsDisabled)
            {
                // /sets
                routes.MapRoute(
                    name: "Sets",
                    url: "sets/",
                    defaults: new { controller = "Sets", action = "Sets" }
                );

                // /sets/recommended
                routes.MapRoute(
                    name: "RecommendedSets",
                    url: "sets/recommended",
                    defaults: new { controller = "Sets", action = "RecommendedSets" }
                );

                // /sets/create
                routes.MapRoute(
                    name: "CreateSet",
                    url: "sets/create",
                    defaults: new { controller = "Sets", action = "CreateSet" }
                );

                // add subverse to set
                routes.MapRoute(
                    name: "addsubversetoset",
                    url: "sets/addsubverse/{setId}/{subverseName}",
                    defaults: new { controller = "Sets", action = "AddSubverseToSet" }
                );

                // remove subverse from set
                routes.MapRoute(
                    name: "removesubversefromset",
                    url: "sets/removesubverse/{setId}/{subverseName}",
                    defaults: new { controller = "Sets", action = "RemoveSubverseFromSet" }
                );

                // change set name
                routes.MapRoute(
                    name: "changesetinfo",
                    url: "sets/modify/{setId}/{newSetName}",
                    defaults: new { controller = "Sets", action = "ChangeSetInfo" }
                );

                // delete a set
                routes.MapRoute(
                    name: "deleteset",
                    url: "sets/delete/{setId}",
                    defaults: new { controller = "Sets", action = "DeleteSet" }
                );

                // /mysets
                routes.MapRoute(
                    name: "UserSets",
                    url: "mysets",
                    defaults: new { controller = "Sets", action = "UserSets" }
                );

                // /mysets/manage
                routes.MapRoute(
                    name: "UserSetsManage",
                    url: "mysets/manage",
                    defaults: new { controller = "Sets", action = "ManageUserSets" }
                );
            }

            //catch The Others 
            routes.MapRoute(
                name: "Others",
                url: "r/{name}/{*url}",
                defaults: new { controller = "Error", action = "Others" }
            );

            // default route
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new
                {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                }
            );
        }
    }
}
