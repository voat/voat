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

using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using System;
using Voat.Configuration;
using Voat.Domain.Models;
using Voat.Utilities;
using Microsoft.AspNetCore.Routing.Constraints;

namespace Voat
{
    public class RouteConfig
    {
        public static void RegisterRoutes(IRouteBuilder routes)
        {
            //routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            //routes.LowercaseUrls = true;

            var subverseSortConstraint = @"^$|new|hot|top";
            var setSortContraint = @"^$|new|hot|top|relative";

            var subversePathSuffix = "{subversePathSuffix}";
            var subversePathSuffixConstraint = "v";


            //CORE_PORT: No rss!
            //routes.MapRoute(
            //    name: "rss",
            //    template: "rss/{subverseName?}",
            //    defaults: new { controller = "RSS", action = "RSS", subverseName = UrlParameter.Optional }
            //);

            #region Discover 

           

            routes.MapRoute(
              name: "DiscoverSearch",
              template: "discover/{domainType}/search/{sort}",
              defaults: new
              {
                  controller = "Discover",
                  action = "Search",
                  domainType = (DomainType?)null,
                  sort = (SortAlgorithm?)null
              }
            );

            routes.MapRoute(
                name: "Discover",
                template: "discover/{domainType}/{sort}",
                defaults: new {
                    controller = "Discover",
                    action = "Index",
                    domainType = (DomainType?)null,
                    sort = (SortAlgorithm?)null
                }
            );

           

            #endregion

            // /advertize
            routes.MapRoute(
                name: "advertize",
                template: "advertize",
                defaults: new { controller = "Home", action = "Advertize" }
            );

            // /random
            routes.MapRoute(
                name: "randomSubverse",
                template: "random/",
                defaults: new { controller = "Subverses", action = "Random" }
            );

            // /randomnsfw
            routes.MapRoute(
                name: "randomNsfwSubverse",
                template: "randomnsfw/",
                defaults: new { controller = "Subverses", action = "RandomNsfw" }
            );

            // /subverses/create
            routes.MapRoute(
                name: "CreateSubverse",
                template: "subverses/create",
                defaults: new { controller = "Subverses", action = "CreateSubverse" }
            );

            // /search
            routes.MapRoute(
                name: "Search",
                template: "search",
                defaults: new { controller = "Search", action = "SearchResults" }
            );

            // /subverses/adultcontent
            routes.MapRoute(
                name: "AdultContentWarning",
                template: "subverses/adultcontent",
                defaults: new { controller = "Subverses", action = "AdultContentWarning" }
            );

            // /subverses/adultcontentfiltered
            routes.MapRoute(
                name: "AdultContentFiltered",
                template: "subverses/adultcontentfiltered",
                defaults: new { controller = "Subverses", action = "AdultContentFiltered" }
            );

            // comments/4
            routes.MapRoute(
                name: "comments",
                template: "comments/{submissionID}",
                defaults: new { controller = "Comment", action = "Comments" }
            );

            string commentSortContraint = "(?i)^$|" + String.Join("|", Enum.GetNames(typeof(CommentSortAlgorithm)));

            // /comments/submission/startingpos
            //"/comments/" + submission + "/" + parentId + "/" + command + "/" + startingIndex + "/" + startIndex + "/" + sort + "/",
            routes.MapRoute(
                name: "CommentSegment",
                template: "comments/{submissionID}/{parentID}/{command}/{startingIndex}/{sort}",
                constraints: new
                {
                    sort = commentSortContraint
                },
                defaults: new { controller = "Comment", action = "CommentSegment", sort = (string)null }
            );

            routes.MapRoute(
                name: "CommentTree",
                template: "comments/{submissionID}/tree/{sort}",
                constraints: new
                {
                    sort = commentSortContraint
                },
                defaults: new { controller = "Comment", action = "CommentTree", sort = (string)null }
            );


            // v/subverse/123456/123456/delete
            routes.MapRoute(
                name: "SubverseStylesheet",
                template: "v/{subverse}/stylesheet",
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
                template: "v/{subverse}/{submissionID}/delete",
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

            routes.MapRoute(
                name: "ModeratorDeleteComment",
                template: "v/{subverse}/{submissionID}/{commentID}/delete",
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
                template: "v/{subverseName}/comments/{submissionID}/{sort}",
                constraints: new
                {
                    sort = commentSortContraint
                },
                defaults: new
                {
                    controller = "Comment",
                    action = "Comments",
                    sort = (string)null,
                }
            );

            // v/subverse/comments/123456
            routes.MapRoute(
                name: "SubverseComments",
                template: "v/{subverseName}/comments/{submissionID}/{commentID?}/{context?}",
                defaults: new
                {
                    controller = "Comment",
                    action = "Comments",
                    sort = (string)null
                }
            );

            // v/subverseName/comments/123456/new
            routes.MapRoute(
                name: "SubverseCommentsWithSort_Short",
                template: "v/{subverseName}/{submissionID}/{sort?}",
                constraints: new
                {
                    sort = commentSortContraint,
                    submissionID = @"\d+"
                },
                defaults: new
                {
                    controller = "Comment",
                    action = "Comments",
                    sort = (string)null,
                }
            );

            routes.MapRoute(
                name: "SubverseComments_Short",
                template: "v/{subverseName}/{submissionID}/{commentID?}/{context?}",
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
                    sort = (string)null
                }
            );

            // comments/distinguish/412
            routes.MapRoute(
                name: "distinguishcomment",
                template: "comments/distinguish/{commentId}",
                defaults: new { controller = "Comment", action = "DistinguishComment" }
            );
            #endregion Comment Pages

            #region User

            routes.MapRoute(
               name: "Vote",
               template: "user/vote/{contentType}/{id}/{voteStatus}",
               defaults: new { controller = "User", action = "Vote" }
           );

            routes.MapRoute(
                name: "Save",
                template: "user/save/{contentType}/{id}",
                defaults: new { controller = "User", action = "Save" }
            );

            //user/subscribe/subverse/technology/jason/toggle
            //Domain.Models.DomainType domainType, string name, string ownerName, Domain.Models.SubscriptionAction action
            //user/subscribe/subverse/technology/_/toggle
            routes.MapRoute(
                 name: "UserSubscribeAction",
                 template: "{pathPrefix}/subscribe/{domainType}/{name}/{subscribeAction}",
                 defaults: new {
                     controller = "User",
                     action = "Subscribe"
                 }
                 , constraints: new
                 {
                     httpMethod = new HttpMethodRouteConstraint("POST"),
                     pathPrefix = "user|u",
                     domainType = "set|subverse",
                     subscribeAction = String.Join("|", Enum.GetNames(typeof(SubscriptionAction))).ToLower()
                 }
            );

            routes.MapRoute(
              name: "UserSubscriptions",
              template: "{pathPrefix}/subscribed/{domainType}",
              defaults: new { controller = "User", action = "Subscribed" },
              constraints: new { httpMethod = new HttpMethodRouteConstraint("GET"), pathPrefix = "user|u", domainType = "set|subverse" }
           );

            // /user/blocked/subverse
            routes.MapRoute(
               name: "UserBlocks",
               template: "{pathPrefix}/blocked/{blockType}",
               defaults: new { controller = "User", action = "Blocked" },
               constraints: new { httpMethod = new HttpMethodRouteConstraint("GET"), pathPrefix = "user|u", blockType = "user|subverse" }
            );

            // /user/blocked/subverse
            routes.MapRoute(
              name: "BlockUserPost",
              template: "{pathPrefix}/blocked/{blockType}",
              defaults: new { controller = "User", action = "BlockUser" },
              constraints: new { httpMethod = new HttpMethodRouteConstraint("POST"), pathPrefix = "user|u", blockType = "user|subverse" }
           );
            
            routes.MapRoute(
                 name: "UserSetCreate",
                 template: "{pathPrefix}/{userName}/sets/create",
                 defaults: new { controller = "Set", action = "Create" },
                 constraints: new { pathPrefix = "user|u" }
           );

            routes.MapRoute(
                  name: "UserSets",
                  template: "{pathPrefix}/{userName}/sets",
                  defaults: new { controller = "User", action = "Sets" },
                  constraints: new { pathPrefix = "user|u" }
            );

            routes.MapRoute(
                name: "UserComments",
                template: "{pathPrefix}/{userName}/comments",
                defaults: new { controller = "User", action = "Comments" },
                constraints: new { pathPrefix = "user|u" }
            );

            routes.MapRoute(
               name: "UserSubmissions",
               template: "{pathPrefix}/{userName}/submissions",
               defaults: new { controller = "User", action = "Submissions" },
                constraints: new { pathPrefix = "user|u" }
           );

            routes.MapRoute(
              name: "UserSaved",
              template: "{pathPrefix}/{userName}/saved",
              defaults: new { controller = "User", action = "Saved" },
              constraints: new { pathPrefix = "user|u" }
            );

            routes.MapRoute(
              name: "UserProfile",
              template: "{pathPrefix}/{userName}",
              defaults: new { controller = "User", action = "Overview" },
              constraints: new { pathPrefix = "user|u" }
            );

            routes.MapRoute(
               name: "Block",
               template: "block/{blockType}",
               defaults: new { controller = "User", action = "Block" },
               constraints: new { blockType = "user|subverse" }
           );

            #endregion User

            #region Message

            var messageRoot = "messages";
            var messageController = "Messages";

            routes.MapRoute(
              name: "MessageMark",
              template: messageRoot + "/mark/{type}/{markAction}/{id?}",
              defaults: new
              {
                  controller = messageController,
                  action = "Mark"
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
              template: messageRoot + "/delete/{type}/{id}",
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
                template: messageRoot + "",
                defaults: new {
                    controller = messageController,
                    action = "Index"
                }
            );

            routes.MapRoute(
              name: "MessageInbox",
              template: messageRoot + "/inbox",
              defaults: new
              {
                  controller = messageController,
                  action = "Index"
              }
            );

            routes.MapRoute(
                name: "MessageCompose",
                template: messageRoot + "/compose",
                defaults: new
                {
                    controller = messageController,
                    action = "Compose"
                }
            );
            routes.MapRoute(
              name: "MessageNotifications",
              template: messageRoot + "/notifications",
              defaults: new
              {
                  controller = messageController,
                  action = "Notifications"
              }
          );
            routes.MapRoute(
               name: "MessagesSent",
               template: messageRoot + "/sent",
               defaults: new
               {
                   controller = messageController,
                   action = "Sent"
               }
           );

            routes.MapRoute(
               name: "MessageReply",
               template: messageRoot + "/reply",
               defaults: new
               {
                   controller = messageController,
                   action = "Reply"
               }
            );
            routes.MapRoute(
              name: "MessageReplyForm",
              template: messageRoot + "/reply/{id}",
              defaults: new
              {
                  controller = messageController,
                  action = "ReplyForm"
              },
              constraints: new {
                  id = @"\d+",
                  httpMethod = new HttpMethodRouteConstraint("GET")
              }
            );
            routes.MapRoute(
               name: "MessagesMentions",
               template: messageRoot + "/mentions/{type?}",
               defaults: new
               {
                   controller = messageController,
                   action = "Mentions"
               }
            );
            routes.MapRoute(
               name: "MessagesReplies",
               template: messageRoot + "/replies/{type?}",
               defaults: new
               {
                   controller = messageController,
                   action = "Replies"
               }
            );

            #region Smail 
                routes.MapRoute(
                 name: "SubverseMail",
                 template: "v/{subverse}/about/" + messageRoot + "/{type}/{state}",
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
                template: "v/{subverse}/about/" + messageRoot + "/compose",
                defaults: new
                {
                    controller = messageController,
                    action = "Compose"
                }
               );
            #endregion
           
            #endregion Message

            // help/pagetoshow
            routes.MapRoute(
                name: "Help",
                template: "help/{*pagetoshow}",
                defaults: new { controller = "Home", action = "Help" }
            );

            // about/pagetoshow
            routes.MapRoute(
                name: "About",
                template: "about/{*pagetoshow}",
                defaults: new { controller = "Home", action = "About" }
            );
            

           
            // editcomment
            routes.MapRoute(
                  "editcomment",
                  "editcomment/{id?}",
                  new { controller = "Comment", action = "EditComment"}
             );

            // deletecomment
            routes.MapRoute(
                  "deletecomment",
                  "deletecomment/{id?}",
                  new { controller = "Comment", action = "DeleteComment" }
             );
            
            // editsubmission
            routes.MapRoute(
                  "editsubmission",
                  "editsubmission",
                  new { controller = "Submissions", action = "EditSubmission" }
             );

            // deletesubmission
            routes.MapRoute(
                  "deletesubmission",
                  "deletesubmission/{id?}",
                  new { controller = "Submissions", action = "DeleteSubmission" }
             );

            // submit
            routes.MapRoute(
                name: "submit",
                template: "submit",
                defaults: new { controller = "Home", action = "Submit" }
            );

            // submit link shortcut
            routes.MapRoute(
                name: "submitlinkservice",
                template: "submitlink",
                defaults: new { controller = "Home", action = "SubmitLinkService" }
            );

            // v/selectedsubverse/submit
            routes.MapRoute(
                name: "submitpost",
                template: "v/{selectedsubverse}/submit",
                defaults: new { controller = "Home", action = "Submit" }
            );

            // submitcomment
            routes.MapRoute(
                name: "submitcomment",
                template: "submitcomment",
                defaults: new { controller = "Comment", action = "SubmitComment" }
            );

            //Map Explicit API Keys Controller
            routes.MapRoute(
               name: "apikeys",
               template: "apikeys/{action}/{id?}",
               defaults: new
               {
                   controller = "ApiKeys",
                   action = "Index"
               }
            );

            if (VoatSettings.Instance.SignalrEnabled && VoatSettings.Instance.ChatEnabled)
            {
                routes.MapRoute(
                    name: "ChatPassword",
                    template: "chat/action/password",
                    defaults: new { controller = "Chat", action = "Password" }
                );

                routes.MapRoute(
                    name: "CreateChat",
                    template: "chat/action/create",
                    defaults: new { controller = "Chat", action = "Create" }
                );

                routes.MapRoute(
                    name: "JoinChat",
                    template: "chat/{id}",
                    defaults: new { controller = "Chat", action = "Index" }
                );
            }

            #region Submissions / Subverse



            routes.MapRoute(
                name: Models.ROUTE_NAMES.FRONT_INDEX,
                template: "{sort?}",
                defaults: new {
                    controller = "Subverses",
                    action = "SubverseIndex"
                },
               constraints: new
               {
                   sort = subverseSortConstraint
               }
            );

            #endregion
            #region Mod Logs

            routes.MapRoute(
                name: "modLogSubmission",
                template: "v/{subverse}/modlog/submission",
                defaults: new { controller = "ModLog", action = "Submissions" }
            );

            routes.MapRoute(
                name: "modLogComment",
                template: "v/{subverse}/modlog/comment",
                defaults: new { controller = "ModLog", action = "Comments" }
            );

            routes.MapRoute(
                name: "modLogBanned",
                template: "v/{subverse}/modlog/banned",
                defaults: new { controller = "ModLog", action = "Banned" }
            );

            //https://voat.co/v/test/modlog/bannedusers
            routes.MapRoute(
                name: "modLogBanned_Old",
                template: "v/{subverse}/modlog/bannedusers",
                defaults: new { controller = "ModLog", action = "Banned" }
            );

            #endregion Mod Logs

            #region Reports
            // UserReportDialog(string subverse, ContentType type, int id)
            routes.MapRoute(
                  "subverseReportsDialog",
                  "v/{subverse}/about/reports/{type}/{id?}/dialog",
                  constraints: new
                  {
                      type = String.Join("|", Enum.GetNames(typeof(ContentType))).ToLower()
                  },
                  defaults: new
                  {
                      controller = "Report",
                      action = "UserReportDialog"
                  }
             );

            // v/subverseName/about/reports/comment|submission
            routes.MapRoute(
                name: "subverseReportsMark",
                template: "v/{subverse}/about/reports/{type}/{id}/mark",
                constraints: new {
                    type = String.Join("|", Enum.GetNames(typeof(ContentType))).ToLower()
                },
                defaults: new
                {
                    controller = "Report",
                    action = "Mark"
                }
            );
            // v/subverseName/about/reports/comment|submission
            routes.MapRoute(
                name: "subverseReports",
                template: "v/{subverse}/about/reports/{type}",
                constraints: new {
                    type = String.Join("|", Enum.GetNames(typeof(ContentType))).ToLower()
                },
                defaults: new
                {
                    controller = "Report",
                    action = "Reports"
                }
            );
            // v/subverseName/about/reports
            routes.MapRoute(
                name: "subverseReports2",
                template: "v/{subverse}/about/reports",
                defaults: new
                {
                    type = ContentType.Submission,
                    controller = "Report",
                    action = "Reports"
                }
            );
            #endregion

            #region Sub Moderation

            routes.MapRoute(
                name: "AcceptModInvitation",
                template: "acceptmodinvitation/{invitationId}",
                defaults: new { controller = "SubverseModeration", action = "AcceptModInvitation" }
            );

            routes.MapRoute(
                name: "subverseSettings",
                template: "v/{subverse}/about/edit",
                defaults: new { controller = "SubverseModeration", action = "Update" }
            );

            routes.MapRoute(
                name: "subverseStylesheetEditor",
                template: "v/{subverse}/about/edit/stylesheet",
                defaults: new { controller = "SubverseModeration", action = "SubverseStylesheetEditor" }
            );

            routes.MapRoute(
                name: "subverseFlairSettings",
                template: "v/{subverse}/about/flair",
                defaults: new { controller = "SubverseModeration", action = "SubverseFlairSettings" }
            );

            routes.MapRoute(
                name: "addSubverseLinkFlair",
                template: "v/{subverse}/about/flair/add",
                defaults: new { controller = "SubverseModeration", action = "AddLinkFlair" }
            );

            routes.MapRoute(
                name: "removeSubverseLinkFlair",
                template: "v/{subverse}/about/flair/delete/{id}",
                defaults: new { controller = "SubverseModeration", action = "RemoveLinkFlair" }
            );

            routes.MapRoute(
                name: "subverseModerators",
                template: "v/{subverse}/about/moderators",
                defaults: new { controller = "SubverseModeration", action = "SubverseModerators" }
            );

            routes.MapRoute(
                name: "subverseBans",
                template: "v/{subverse}/about/bans",
                defaults: new { controller = "SubverseModeration", action = "SubverseBans" }
            );

            routes.MapRoute(
                name: "addSubverseModerator",
                template: "v/{subverse}/about/moderators/add",
                defaults: new { controller = "SubverseModeration", action = "AddModerator" }
            );

            routes.MapRoute(
                name: "addSubverseBan",
                template: "v/{subverse}/about/bans/add",
                defaults: new { controller = "SubverseModeration", action = "AddBan" }
            );

            routes.MapRoute(
                name: "removeSubverseModerator",
                template: "v/{subverse}/about/moderators/delete/{id}",
                defaults: new { controller = "SubverseModeration", action = "RemoveModerator" }
            );

            routes.MapRoute(
               name: "acceptSubverseModeratorInvitation",
               template: "v/{subverse}/about/moderatorinvitations/accept/{invitationId}",
               defaults: new { controller = "SubverseModeration", action = "AcceptModeratorInvitation" }
           );
            routes.MapRoute(
                name: "removeSubverseModeratorInvitation",
                template: "v/{subverse}/about/moderatorinvitations/delete/{invitationId}",
                defaults: new { controller = "SubverseModeration", action = "RecallModeratorInvitation" }
            );

            routes.MapRoute(
                name: "removeSubverseBan",
                template: "v/{subverse}/about/bans/delete/{id}",
                defaults: new { controller = "SubverseModeration", action = "RemoveBan" }
            );

            routes.MapRoute(
                name: "resignAsModerator",
                template: "v/{subverse}/about/moderators/resign/",
                defaults: new { controller = "SubverseModeration", action = "ResignAsModerator" }
            );

            #endregion Sub Moderation

            routes.MapRoute(
               name: "SubverseIndexNoSort",
               template: "v/{subverse}",
               defaults: new
               {
                   controller = "Subverses",
                   action = "SubverseIndex"
               }
           );

            routes.MapRoute(
                name: Models.ROUTE_NAMES.SUBVERSE_INDEX,
                template: "v/{subverse}/{sort?}",
                defaults: new {
                    controller = "Subverses",
                    action = "SubverseIndex"
                },
               constraints: new
               {
                   sort = subverseSortConstraint
               }
            );

            #region Domains

            // domains/domainname.com
            routes.MapRoute(
                name: "DomainIndex",
                template: "domains/{domainname}/{sortingmode}",
                defaults: new {
                    controller = "Domains",
                    action = "Index",
                    sortingmode = "hot"
                }
            );

            #endregion Domains

            #region Ajax
            // ajaxhelpers/commentreplyform
            routes.MapRoute(
                name: "CommentReplyForm",
                template: "ajaxhelpers/commentreplyform/{parentCommentId}/{messageId}",
                defaults: new { controller = "HtmlElements", action = "CommentReplyForm" }
            );

            // ajaxhelpers/privatemessagereplyform
            routes.MapRoute(
                name: "PrivateMessageReplyForm",
                template: "ajaxhelpers/privatemessagereplyform/{parentPrivateMessageId}",
                defaults: new { controller = "HtmlElements", action = "PrivateMessageReplyForm" }
            );

            //// ajaxhelpers/messagecontent
            //routes.MapRoute(
            //    name: "MessageContent",
            //    template: "ajaxhelpers/messagecontent/{messageId}",
            //    defaults: new { controller = "AjaxGateway", action = "MessageContent" }
            //);

            // ajaxhelpers/basicuserinfo
            routes.MapRoute(
                name: "BasicUserInfo",
                template: "ajaxhelpers/userinfo/{userName}",
                defaults: new { controller = "AjaxGateway", action = "UserBasicInfo" }
            );

            //// ajaxhelpers/videoplayer
            //routes.MapRoute(
            //    name: "VideoPlayer",
            //    template: "ajaxhelpers/videoplayer/{messageId}",
            //    defaults: new { controller = "AjaxGateway", action = "VideoPlayer" }
            //);

            // ajaxhelpers/rendersubmission
            routes.MapRoute(
                name: "RenderSubmission",
                template: "ajaxhelpers/rendersubmission",
                defaults: new { controller = "AjaxGateway", action = "RenderSubmission" }
            );

            // ajaxhelpers/previewstylesheet
            routes.MapRoute(
                name: "PreviewStylesheet",
                template: "ajaxhelpers/previewstylesheet",
                defaults: new { controller = "AjaxGateway", action = "PreviewStylesheet" }
            );

            // ajaxhelpers/linkflairselectdialog
            routes.MapRoute(
                name: "LinkFlairSelectDialog",
                template: "ajaxhelpers/linkflairselectdialog/{subverse}/{id}",
                defaults: new { controller = "AjaxGateway", action = "SubverseLinkFlairs" }
            );

            // ajaxhelpers/titlefromuri
            routes.MapRoute(
                name: "TitleFromUri",
                template: "ajaxhelpers/titlefromuri",
                defaults: new { controller = "AjaxGateway", action = "TitleFromUri" }
            );

            // ajaxhelpers/autocompletesubversename
            routes.MapRoute(
                name: "AutocompleteSubverseName",
                template: "ajaxhelpers/autocompletesubversename",
                defaults: new { controller = "AjaxGateway", action = "AutocompleteSubverseName" }
            );

            // ajaxhelpers/subverseinfo
            routes.MapRoute(
                name: "SetSubverseInfo",
                template: "ajaxhelpers/setsubverseinfo/{setId}/{subverseName}",
                defaults: new { controller = "AjaxGateway", action = "SubverseBasicInfo" }
            );
            #endregion

            #region Sets
            if (VoatSettings.Instance.SetsEnabled)
            {
                var setSuffix = "s";

                //sets
                routes.MapRoute(
                    name: "SetIndex",
                    template: setSuffix + "/{name}/{sort?}",
                    defaults: new {
                        controller = "Set",
                        action = "Index"
                    },
                   constraints: new
                   {
                       sort = setSortContraint
                   }
                );

                //sets
                routes.MapRoute(
                   name: "SetDetails",
                   template: setSuffix + "/{name}/about/details",
                   defaults: new { controller = "Set", action = "Details", userName = (string)null }
               );
                //routes.MapRoute(
                //    name: "SetDetails",
                //    template: setSuffix + "/{name}/{userName}/about/details",
                //    defaults: new { controller = "Set", action = "Details" }
                //);
                routes.MapRoute(
                   name: "EditSet",
                   template: setSuffix + "/{name}/about/edit",
                   defaults: new { controller = "Set", action = "Edit" }
                );

                routes.MapRoute(
                   name: "DeleteSet",
                   template: setSuffix + "/{name}/about/delete",
                   defaults: new { controller = "Set", action = "Delete" }
                );

                //sets
                //routes.MapRoute(
                //    name: "SetIndexUser",
                //    template: setSuffix + "/{name}/{userName}/{sort}",
                //    defaults: new {
                //        controller = "Set",
                //        action = "Index",
                //        sort = UrlParameter.Optional
                //    },
                //    constraints: new
                //    {
                //        sort = setSortContraint
                //    }
                //);

                // /s/{name}/{userName}/{subverse}?action=Subscribe|Unsubscribe
                routes.MapRoute(
                    name: "SetSubListChange",
                    template: setSuffix + "/{name}/{subverse}/{subscribeAction}",
                    defaults: new {
                        controller = "Set",
                        action = "ListChange"
                    }
                    , constraints: new
                    {
                        httpMethod = new HttpMethodRouteConstraint("POST"),
                        subscribeAction = String.Join("|", Enum.GetNames(typeof(SubscriptionAction))).ToLower()
                    }
                );

                //// /sets
                //routes.MapRoute(
                //    name: "Sets",
                //    template: "sets/",
                //    defaults: new { controller = "Sets", action = "Sets" }
                //);

                //// /sets/recommended
                //routes.MapRoute(
                //    name: "RecommendedSets",
                //    template: "sets/recommended",
                //    defaults: new { controller = "Sets", action = "RecommendedSets" }
                //);

                //// /sets/create
                //routes.MapRoute(
                //    name: "CreateSet",
                //    template: "sets/create",
                //    defaults: new { controller = "Sets", action = "CreateSet" }
                //);

                //// add subverse to set
                //routes.MapRoute(
                //    name: "addsubversetoset",
                //    template: "sets/addsubverse/{setId}/{subverseName}",
                //    defaults: new { controller = "Sets", action = "AddSubverseToSet" }
                //);

                //// remove subverse from set
                //routes.MapRoute(
                //    name: "removesubversefromset",
                //    template: "sets/removesubverse/{setId}/{subverseName}",
                //    defaults: new { controller = "Sets", action = "RemoveSubverseFromSet" }
                //);

                //// change set name
                //routes.MapRoute(
                //    name: "changesetinfo",
                //    template: "sets/modify/{setId}/{newSetName}",
                //    defaults: new { controller = "Sets", action = "ChangeSetInfo" }
                //);

                //// delete a set
                //routes.MapRoute(
                //    name: "deleteset",
                //    template: "sets/delete/{setId}",
                //    defaults: new { controller = "Sets", action = "DeleteSet" }
                //);

                //// /mysets
                //routes.MapRoute(
                //    name: "UserSets",
                //    template: "mysets",
                //    defaults: new { controller = "Sets", action = "UserSets" }
                //);

                //// /mysets/manage
                //routes.MapRoute(
                //    name: "UserSetsManage",
                //    template: "mysets/manage",
                //    defaults: new { controller = "Sets", action = "ManageUserSets" }
                //);
            }
            #endregion

            #region Votes

            routes.MapRoute(
                name: "ViewVote",
                template: "v/{subverse}/vote/{id:int}",
                defaults: new { controller = "Vote", action = "View" }
            );
            routes.MapRoute(
                name: "CreateVote",
                template: "v/{subverse}/vote/create",
                defaults: new { controller = "Vote", action = "Create" }
            );
            routes.MapRoute(
                name: "SaveVote",
                template: "vote/save",
                defaults: new { controller = "Vote", action = "Save" }
            );
            routes.MapRoute(
               name: "EditVote",
               template: "v/{subverse}/vote/edit/{id:int}",
               defaults: new { controller = "Vote", action = "Edit" }
           );
            routes.MapRoute(
                name: "DeleteVote",
                template: "v/{subverse}/vote/delete/{id:int}",
                defaults: new { controller = "Vote", action = "Delete" }
            );
            routes.MapRoute(
               name: "VoteElement",
               template: "v/{subverse}/vote/element",
               defaults: new { controller = "Vote", action = "Element" }
           );
            routes.MapRoute(
               name: "ListVotes",
               template: "v/{subverse}/votes",
               defaults: new { controller = "Vote", action = "List" }
           );
            #endregion

            // submissions/applylinkflair
            routes.MapRoute(
                name: "ApplyLinkFlair",
                template: "submissions/applylinkflair/{submissionId}/{flairId}",
                defaults: new { controller = "Submissions", action = "ApplyLinkFlair" }
            );

            // submissions/clearlinkflair
            routes.MapRoute(
                name: "ClearLinkFlair",
                template: "submissions/clearlinkflair/{submissionId}",
                defaults: new { controller = "Submissions", action = "ClearLinkFlair" }
            );

            // submissions/togglesticky
            routes.MapRoute(
                name: "ToggleSticky",
                template: "submissions/togglesticky/{submissionId}",
                defaults: new { controller = "Submissions", action = "ToggleSticky" }
            );
            routes.MapRoute(
                name: "ToggleNSFW",
                template: "submissions/togglensfw/{submissionId}",
                defaults: new { controller = "Submissions", action = "ToggleNSFW" }
            );

            #region Log
            routes.MapRoute(
               name: "GloballyBannedUserLog",
               template: "log/banned/user",
               defaults: new { controller = "Log", action = "BannedUsers" }
           );
            routes.MapRoute(
               name: "GloballyBannedDomainLog",
               template: "log/banned/domain",
               defaults: new { controller = "Log", action = "BannedDomains" }
           );

            #endregion

            //catch Errors
            routes.MapRoute(
                name: "ErrorStatus",
                template: "error/status/{statusCode:int}",
                defaults: new { controller = "Error", action = "Status" }
            );

            //catch Errors
            routes.MapRoute(
                name: "Error",
                template: "error/{*type}",
                defaults: new { controller = "Error", action = "Type" }
            );

            //catch The Others 
            routes.MapRoute(
                name: "Others",
                template: "r/{name}/{*url}",
                defaults: new { controller = "Error", action = "Type", type = "TheOthers" }
            );

            //Map Admin Area
            Voat.UI.Areas.Admin.RouteConfig.RegisterRoutes(routes);

            // default route
            routes.MapRoute(
                name: "Default",
                template: "{controller}/{action}/{id?}",
                defaults: new
                {
                    controller = "Home",
                    action = "Index"
                }
            );

#if DEBUG
            //var sb = new System.Text.StringBuilder();
            //foreach (Route route in routes)
            //{
            //    sb.AppendLine(route.Url);
            //}
            //Debug.WriteLine(sb.ToString());
#endif


        }
    }
}
