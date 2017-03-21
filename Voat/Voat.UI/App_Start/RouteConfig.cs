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
using System.Diagnostics;
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
            var subverseSortConstraint = @"^$|new|hot|top";
            var setSortContraint = @"^$|new|hot|top|relative";

            // /rss
            routes.MapRoute(
                name: "rss",
                url: "rss/{subverseName}",
                defaults: new { controller = "RSS", action = "RSS", subverseName = UrlParameter.Optional }
            );

            #region Discover 

            routes.MapRoute(
              name: "DiscoverSearch",
              url: "discover/{domainType}/search/{sort}",
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
                url: "discover/{domainType}/{sort}",
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

            // comments/4
            routes.MapRoute(
                name: "comments",
                url: "comments/{submissionID}",
                defaults: new { controller = "Comment", action = "Comments" }
            );

            string commentSortContraint = "(?i)^$|" + String.Join("|", Enum.GetNames(typeof(CommentSortAlgorithm)));

            // /comments/submission/startingpos
            //"/comments/" + submission + "/" + parentId + "/" + command + "/" + startingIndex + "/" + startIndex + "/" + sort + "/",
            routes.MapRoute(
                name: "CommentSegment",
                url: "comments/{submissionID}/{parentID}/{command}/{startingIndex}/{sort}",
                constraints: new
                {
                    sort = commentSortContraint
                },
                defaults: new { controller = "Comment", action = "CommentSegment", sort = (string)null }
            );

            routes.MapRoute(
                name: "CommentTree",
                url: "comments/{submissionID}/tree/{sort}",
                constraints: new
                {
                    sort = commentSortContraint
                },
                defaults: new { controller = "Comment", action = "CommentTree", sort = (string)null }
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
                    sort = (string)null,
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
                    sort = (string)null
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
                    sort = (string)null,
                    commentID = UrlParameter.Optional,
                    contextCount = UrlParameter.Optional
                }
            );

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
                    sort = (string)null
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
               name: "Vote",
               url: "user/vote/{contentType}/{id}/{voteStatus}",
               defaults: new { controller = "User", action = "Vote" }
           );

            routes.MapRoute(
                name: "Save",
                url: "user/save/{contentType}/{id}",
                defaults: new { controller = "User", action = "Save" }
            );

            //user/subscribe/subverse/technology/jason/toggle
            //Domain.Models.DomainType domainType, string name, string ownerName, Domain.Models.SubscriptionAction action
            //user/subscribe/subverse/technology/_/toggle
            routes.MapRoute(
                 name: "UserSubscribeAction",
                 url: "{pathPrefix}/subscribe/{domainType}/{name}/{ownerName}/{subscribeAction}",
                 defaults: new {
                     controller = "User",
                     action = "Subscribe"
                 }
                 , constraints: new
                 {
                     httpMethod = new HttpMethodConstraint("POST"),
                     pathPrefix = "user|u",
                     domainType = "set|subverse",
                     subscribeAction = String.Join("|", Enum.GetNames(typeof(SubscriptionAction))).ToLower()
                 }
            );

            routes.MapRoute(
              name: "UserSubscriptions",
              url: "{pathPrefix}/subscribed/{domainType}",
              defaults: new { controller = "User", action = "Subscribed" },
              constraints: new { httpMethod = new HttpMethodConstraint("GET"), pathPrefix = "user|u", domainType = "set|subverse" }
           );

            // /user/blocked/subverse
            routes.MapRoute(
               name: "UserBlocks",
               url: "{pathPrefix}/blocked/{blockType}",
               defaults: new { controller = "User", action = "Blocked" },
               constraints: new { httpMethod = new HttpMethodConstraint("GET"), pathPrefix = "user|u", blockType = "user|subverse" }
            );

            // /user/blocked/subverse
            routes.MapRoute(
              name: "BlockUserPost",
              url: "{pathPrefix}/blocked/{blockType}",
              defaults: new { controller = "User", action = "BlockUser" },
              constraints: new { httpMethod = new HttpMethodConstraint("POST"), pathPrefix = "user|u", blockType = "user|subverse" }
           );
            
            routes.MapRoute(
                 name: "UserSetCreate",
                 url: "{pathPrefix}/{userName}/sets/create",
                 defaults: new { controller = "Set", action = "Create" },
                 constraints: new { pathPrefix = "user|u" }
           );

            routes.MapRoute(
                  name: "UserSets",
                  url: "{pathPrefix}/{userName}/sets",
                  defaults: new { controller = "User", action = "Sets" },
                  constraints: new { pathPrefix = "user|u" }
            );

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

            #region Smail 
                routes.MapRoute(
                 name: "SubverseMail",
                 url: "v/{subverse}/about/" + messageRoot + "/{type}/{state}",
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
                url: "v/{subverse}/about/" + messageRoot + "/compose",
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
                url: "help/{*pagetoshow}",
                defaults: new { controller = "Home", action = "Help" }
            );

            // about/pagetoshow
            routes.MapRoute(
                name: "About",
                url: "about/{*pagetoshow}",
                defaults: new { controller = "Home", action = "About" }
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

            #region Submissions / Subverse



            routes.MapRoute(
                name: Models.ROUTE_NAMES.FRONT_INDEX,
                url: "{sort}",
                defaults: new {
                    controller = "Subverses",
                    action = "SubverseIndex",
                    //subverse = "_front",
                    sort = UrlParameter.Optional
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

            //https://voat.co/v/test/modlog/bannedusers
            routes.MapRoute(
                name: "modLogBanned_Old",
                url: "v/{subverse}/modlog/bannedusers",
                defaults: new { controller = "ModLog", action = "Banned" }
            );

            #endregion Mod Logs

            #region Reports
            // UserReportDialog(string subverse, ContentType type, int id)
            routes.MapRoute(
                  "subverseReportsDialog",
                  "v/{subverse}/about/reports/{type}/{id}/dialog",
                  constraints: new
                  {
                      type = String.Join("|", Enum.GetNames(typeof(ContentType))).ToLower()
                  },
                  defaults: new
                  {
                      controller = "Report",
                      action = "UserReportDialog",
                      id = UrlParameter.Optional
                  }
             );

            // v/subverseName/about/reports/comment|submission
            routes.MapRoute(
                name: "subverseReportsMark",
                url: "v/{subverse}/about/reports/{type}/{id}/mark",
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
                url: "v/{subverse}/about/reports/{type}",
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
                url: "v/{subverse}/about/reports",
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
                url: "acceptmodinvitation/{invitationId}",
                defaults: new { controller = "SubverseModeration", action = "AcceptModInvitation" }
            );

            routes.MapRoute(
                name: "subverseSettings",
                url: "v/{subverse}/about/edit",
                defaults: new { controller = "SubverseModeration", action = "Update" }
            );

            routes.MapRoute(
                name: "subverseStylesheetEditor",
                url: "v/{subverse}/about/edit/stylesheet",
                defaults: new { controller = "SubverseModeration", action = "SubverseStylesheetEditor" }
            );

            routes.MapRoute(
                name: "subverseFlairSettings",
                url: "v/{subverse}/about/flair",
                defaults: new { controller = "SubverseModeration", action = "SubverseFlairSettings" }
            );

            routes.MapRoute(
                name: "addSubverseLinkFlair",
                url: "v/{subverse}/about/flair/add",
                defaults: new { controller = "SubverseModeration", action = "AddLinkFlair" }
            );

            routes.MapRoute(
                name: "removeSubverseLinkFlair",
                url: "v/{subverse}/about/flair/delete/{id}",
                defaults: new { controller = "SubverseModeration", action = "RemoveLinkFlair" }
            );

            routes.MapRoute(
                name: "subverseModerators",
                url: "v/{subverse}/about/moderators",
                defaults: new { controller = "SubverseModeration", action = "SubverseModerators" }
            );

            routes.MapRoute(
                name: "subverseBans",
                url: "v/{subverse}/about/bans",
                defaults: new { controller = "SubverseModeration", action = "SubverseBans" }
            );

            routes.MapRoute(
                name: "addSubverseModerator",
                url: "v/{subverse}/about/moderators/add",
                defaults: new { controller = "SubverseModeration", action = "AddModerator" }
            );

            routes.MapRoute(
                name: "addSubverseBan",
                url: "v/{subverse}/about/bans/add",
                defaults: new { controller = "SubverseModeration", action = "AddBan" }
            );

            routes.MapRoute(
                name: "removeSubverseModerator",
                url: "v/{subverse}/about/moderators/delete/{id}",
                defaults: new { controller = "SubverseModeration", action = "RemoveModerator" }
            );

            routes.MapRoute(
                name: "removeSubverseModeratorInvitation",
                url: "v/{subverse}/about/moderatorinvitations/delete/{invitationId}",
                defaults: new { controller = "SubverseModeration", action = "RecallModeratorInvitation" }
            );

            routes.MapRoute(
                name: "removeSubverseBan",
                url: "v/{subverse}/about/bans/delete/{id}",
                defaults: new { controller = "SubverseModeration", action = "RemoveBan" }
            );

            routes.MapRoute(
                name: "resignAsModerator",
                url: "v/{subverse}/about/moderators/resign/",
                defaults: new { controller = "SubverseModeration", action = "ResignAsModerator" }
            );

            #endregion Sub Moderation

            routes.MapRoute(
               name: "SubverseIndexNoSort",
               url: "v/{subverse}",
               defaults: new
               {
                   controller = "Subverses",
                   action = "SubverseIndex",
                   sort = UrlParameter.Optional
               }
           );

            routes.MapRoute(
                name: Models.ROUTE_NAMES.SUBVERSE_INDEX,
                url: "v/{subverse}/{sort}",
                defaults: new {
                    controller = "Subverses",
                    action = "SubverseIndex",
                    sort = UrlParameter.Optional
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
                url: "domains/{domainname}/{sortingmode}",
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
                url: "ajaxhelpers/commentreplyform/{parentCommentId}/{messageId}",
                defaults: new { controller = "HtmlElements", action = "CommentReplyForm" }
            );

            // ajaxhelpers/privatemessagereplyform
            routes.MapRoute(
                name: "PrivateMessageReplyForm",
                url: "ajaxhelpers/privatemessagereplyform/{parentPrivateMessageId}",
                defaults: new { controller = "HtmlElements", action = "PrivateMessageReplyForm" }
            );

            //// ajaxhelpers/messagecontent
            //routes.MapRoute(
            //    name: "MessageContent",
            //    url: "ajaxhelpers/messagecontent/{messageId}",
            //    defaults: new { controller = "AjaxGateway", action = "MessageContent" }
            //);

            // ajaxhelpers/basicuserinfo
            routes.MapRoute(
                name: "BasicUserInfo",
                url: "ajaxhelpers/userinfo/{userName}",
                defaults: new { controller = "AjaxGateway", action = "UserBasicInfo" }
            );

            //// ajaxhelpers/videoplayer
            //routes.MapRoute(
            //    name: "VideoPlayer",
            //    url: "ajaxhelpers/videoplayer/{messageId}",
            //    defaults: new { controller = "AjaxGateway", action = "VideoPlayer" }
            //);

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
                url: "ajaxhelpers/linkflairselectdialog/{subverse}/{id}",
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
            #endregion

            #region Sets
            if (!Settings.SetsDisabled)
            {
                var setSuffix = "s";

                //sets
                routes.MapRoute(
                    name: "SetIndex",
                    url: setSuffix + "/{name}/{sort}",
                    defaults: new {
                        controller = "Set",
                        action = "Index",
                        sort = UrlParameter.Optional
                    },
                   constraints: new
                   {
                       sort = setSortContraint
                   }
                );

                //sets
                routes.MapRoute(
                   name: "SetDetailsSystem",
                   url: setSuffix + "/{name}/about/details",
                   defaults: new { controller = "Set", action = "Details", userName = (string)null }
               );
                routes.MapRoute(
                    name: "SetDetails",
                    url: setSuffix + "/{name}/{userName}/about/details",
                    defaults: new { controller = "Set", action = "Details" }
                );
                routes.MapRoute(
                   name: "EditSet",
                   url: setSuffix + "/{name}/{userName}/about/edit",
                   defaults: new { controller = "Set", action = "Edit" }
                );

                routes.MapRoute(
                   name: "DeleteSet",
                   url: setSuffix + "/{name}/{userName}/about/delete",
                   defaults: new { controller = "Set", action = "Delete" }
                );

                //sets
                routes.MapRoute(
                    name: "SetIndexUser",
                    url: setSuffix + "/{name}/{userName}/{sort}",
                    defaults: new {
                        controller = "Set",
                        action = "Index",
                        sort = UrlParameter.Optional
                    },
                    constraints: new
                    {
                        sort = setSortContraint
                    }
                );
                
                // /s/{name}/{userName}/{subverse}?action=Subscribe|Unsubscribe
                routes.MapRoute(
                    name: "SetSubListChange",
                    url: setSuffix + "/{name}/{ownerName}/{subverse}/{subscribeAction}",
                    defaults: new {
                        controller = "Set",
                        action = "ListChange"
                    }
                    , constraints: new
                    {
                        httpMethod = new HttpMethodConstraint("POST"),
                        subscribeAction = String.Join("|", Enum.GetNames(typeof(SubscriptionAction))).ToLower()
                    }
                );

                //// /sets
                //routes.MapRoute(
                //    name: "Sets",
                //    url: "sets/",
                //    defaults: new { controller = "Sets", action = "Sets" }
                //);

                //// /sets/recommended
                //routes.MapRoute(
                //    name: "RecommendedSets",
                //    url: "sets/recommended",
                //    defaults: new { controller = "Sets", action = "RecommendedSets" }
                //);

                //// /sets/create
                //routes.MapRoute(
                //    name: "CreateSet",
                //    url: "sets/create",
                //    defaults: new { controller = "Sets", action = "CreateSet" }
                //);

                //// add subverse to set
                //routes.MapRoute(
                //    name: "addsubversetoset",
                //    url: "sets/addsubverse/{setId}/{subverseName}",
                //    defaults: new { controller = "Sets", action = "AddSubverseToSet" }
                //);

                //// remove subverse from set
                //routes.MapRoute(
                //    name: "removesubversefromset",
                //    url: "sets/removesubverse/{setId}/{subverseName}",
                //    defaults: new { controller = "Sets", action = "RemoveSubverseFromSet" }
                //);

                //// change set name
                //routes.MapRoute(
                //    name: "changesetinfo",
                //    url: "sets/modify/{setId}/{newSetName}",
                //    defaults: new { controller = "Sets", action = "ChangeSetInfo" }
                //);

                //// delete a set
                //routes.MapRoute(
                //    name: "deleteset",
                //    url: "sets/delete/{setId}",
                //    defaults: new { controller = "Sets", action = "DeleteSet" }
                //);

                //// /mysets
                //routes.MapRoute(
                //    name: "UserSets",
                //    url: "mysets",
                //    defaults: new { controller = "Sets", action = "UserSets" }
                //);

                //// /mysets/manage
                //routes.MapRoute(
                //    name: "UserSetsManage",
                //    url: "mysets/manage",
                //    defaults: new { controller = "Sets", action = "ManageUserSets" }
                //);
            }
            #endregion

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

            //catch The Others 
            routes.MapRoute(
                name: "Others",
                url: "r/{name}/{*url}",
                defaults: new { controller = "Error", action = "Others" }
            );

            routes.MapMvcAttributeRoutes();

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
