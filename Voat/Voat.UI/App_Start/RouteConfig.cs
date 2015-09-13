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

using System.Web.Mvc;
using System.Web.Routing;
using Voat.Configuration;

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
                url: "comments/{id}",
                defaults: new { controller = "Comment", action = "Comments" }
            );
            
            // /comments/submission/startingpos
            //"/comments/" + submission + "/" + parentId + "/" + command + "/" + startingIndex + "/" + startIndex + "/" + sort + "/",
            routes.MapRoute(
                name: "BucketOfComments",
                url: "comments/{submissionId}/{parentId}/{command}/{startingIndex}/{sort}",
                defaults: new { controller = "Comment", action = "BucketOfComments" }
            );
            // v/subversetoshow/comments/123456/new
            routes.MapRoute(
                name: "SubverseCommentsWithSort",
                url: "v/{subversetoshow}/comments/{id}/{sort}",
                constraints: new
                {
                    sort = "new|top"
                },
                defaults: new
                {
                    controller = "Comment",
                    action = "Comments",
                    startingcommentid = UrlParameter.Optional,
                    commentToHighLight = UrlParameter.Optional
                }
            );        
            // v/subversetoshow/comments/123456
            routes.MapRoute(
                name: "SubverseComments",
                url: "v/{subversetoshow}/comments/{id}/{startingcommentid}/{commentToHighLight}",
                defaults: new
                {
                    controller = "Comment",
                    action = "Comments",
                    startingcommentid = UrlParameter.Optional,
                    commentToHighLight = UrlParameter.Optional,
                    sort = "top"
                }
            );
           

           


            // comments/distinguish/412
            routes.MapRoute(
                name: "distinguishcomment",
                url: "comments/distinguish/{commentId}",
                defaults: new { controller = "Comment", action = "DistinguishComment" }
            );
            // user/someuserhere/thingtoshow
            routes.MapRoute(
                name: "UserProfile",
                url: "user/{id}/{whattodisplay}",
                defaults: new { controller = "Home", action = "UserProfile", whattodisplay = UrlParameter.Optional }
            );

            // user/someuserhere
            routes.MapRoute(
                name: "user",
                url: "user/{id}",
                defaults: new { controller = "Home", action = "UserProfile" }
            );

            // u/someuserhere
            routes.MapRoute(
                name: "usershortroute",
                url: "u/{id}",
                defaults: new { controller = "Home", action = "UserProfile" }
            );

            // inbox
            routes.MapRoute(
                name: "Inbox",
                url: "messaging/inbox",
                defaults: new { controller = "Messaging", action = "Inbox" }
            );

            // inbox/unread
            routes.MapRoute(
                name: "InboxUnread",
                url: "messaging/inbox/unread",
                defaults: new { controller = "Messaging", action = "InboxPrivateMessagesUnread" }
            );

            // compose
            routes.MapRoute(
                name: "Compose",
                url: "messaging/compose",
                defaults: new { controller = "Messaging", action = "Compose" }
            );

            // send private message
            routes.MapRoute(
                name: "SendPrivateMessage",
                url: "messaging/sendprivatemessage",
                defaults: new { controller = "Messaging", action = "SendPrivateMessage" }
            );

            // sent
            routes.MapRoute(
                name: "Sent",
                url: "messaging/sent",
                defaults: new { controller = "Messaging", action = "Sent" }
            );

            // commentreplies
            routes.MapRoute(
                name: "CommentReplies",
                url: "messaging/commentreplies",
                defaults: new { controller = "Messaging", action = "InboxCommentReplies" }
            );

            // postreplies
            routes.MapRoute(
                name: "PostReplies",
                url: "messaging/postreplies",
                defaults: new { controller = "Messaging", action = "InboxPostReplies" }
            );

            // deleteprivatemessage
            routes.MapRoute(
                name: "DeletePrivateMessage",
                url: "messaging/delete",
                defaults: new { controller = "Messaging", action = "DeletePrivateMessage" }
            );

            // deleteprivatemessagefromsent
            routes.MapRoute(
                name: "DeletePrivateMessageFromSent",
                url: "messaging/deletesent",
                defaults: new { controller = "Messaging", action = "DeletePrivateMessageFromSent" }
            );

            // markinboxitemasread
            routes.MapRoute(
                name: "MarkInboxItemAsRead",
                url: "messaging/markasread",
                defaults: new { controller = "Messaging", action = "MarkAsRead" }
            ); 

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
                url: "vote/{messageId}/{typeOfVote}",
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

            // reportcomment
            routes.MapRoute(
                  "reportcomment",
                  "reportcomment/{id}",
                  new { controller = "Report", action = "ReportComment", id = UrlParameter.Optional }
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

           

            // /new
            routes.MapRoute(
                name: "New",
                url: "{sortingmode}",
                defaults: new { controller = "Home", action = "New" }
            );

            // /
            routes.MapRoute(
                name: "Index",
                url: "",
                defaults: new { controller = "Home", action = "Index" }
            );

            // v/subverse/modlog/deleted
            routes.MapRoute(
                name: "subverseSubmissionRemovalLog",
                url: "v/{subversetoshow}/modlog/deleted",
                defaults: new { controller = "Subverses", action = "SubmissionRemovalLog" }
            );

            // v/subverse/modlog/deletedcomments
            routes.MapRoute(
                name: "subverseCommentRemovalLog",
                url: "v/{subversetoshow}/modlog/deletedcomments",
                defaults: new { controller = "Subverses", action = "CommentRemovalLog" }
            );

            // v/subverse/modlog/bannedusers
            routes.MapRoute(
                name: "subverseBannedUsersLog",
                url: "v/{subversetoshow}/modlog/bannedusers",
                defaults: new { controller = "Subverses", action = "BannedUsersLog" }
            );

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

            // v/subversetoshow
            routes.MapRoute(
                name: "SubverseIndex",
                url: "v/{subversetoshow}",
                defaults: new { controller = "Subverses", action = "SubverseIndex" }
            );

            // domains/domainname.com
            routes.MapRoute(
                name: "DomainIndex",
                url: "domains/{domainname}.{ext}",
                defaults: new { controller = "Domains", action = "Index" }
            );

            // domains/domainname.com/new
            routes.MapRoute(
                name: "DomainIndexSorted",
                url: "domains/{domainname}.{ext}/{sortingmode}",
                defaults: new { controller = "Domains", action = "New", sortingmode = UrlParameter.Optional }
            );
    

            // v/subversetoshow/sortingmode
            routes.MapRoute(
                name: "SortedSubverseFrontpage",
                url: "v/{subversetoshow}/{sortingmode}",
                defaults: new { controller = "Subverses", action = "SortedSubverseFrontpage" }
            );

            // ajaxhelpers/commentreplyform
            routes.MapRoute(
                name: "CommentReplyForm",
                url: "ajaxhelpers/commentreplyform/{parentCommentId}/{messageId}",
                defaults: new { controller = "HtmlElements", action = "CommentReplyForm" }
            );

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

            // p/partnerprogram
            routes.MapRoute(
                name: "PartnerProgramInformation",
                url: "p/partnerprogram",
                defaults: new { controller = "Partner", action = "PartnerProgramInformation" }
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

            // p/partnerintent
            //routes.MapRoute(
            //    name: "PartnerProgramIntent",
            //    url: "p/apply",
            //    defaults: new { controller = "Partner", action = "PartnerIntentRegistration" }
            //);
            
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
