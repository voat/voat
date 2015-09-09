/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Voat are Copyright (c) 2014 Voat
All Rights Reserved.
*/

// Please feel free to refactor this code, I wrote most of it when I first started playing with JavaScript

$(document).ready(function () {
    // activate bootstrap popovers
    $('[data-toggle="popover"]').popover({ trigger: 'hover', 'placement': 'top' });

    // prepare auth tokens
    securityToken = $('[name=__RequestVerificationToken]').val();
    $('body').bind('ajaxSend', function (elm, xhr, s) {
        if (s.type == 'POST' && typeof securityToken != 'undefined') {
            if (s.data.length > 0) {
                s.data += "&__RequestVerificationToken=" + encodeURIComponent(securityToken);
            } else {
                s.data = "__RequestVerificationToken=" + encodeURIComponent(securityToken);
            }
        }
    });

    $('.whoaSubscriptionMenu > li').bind('mouseover', openSubMenu);
    $('.whoaSubscriptionMenu > li').bind('mouseout', closeSubMenu);
    function openSubMenu() { $(this).find('ul').css('visibility', 'visible'); }
    function closeSubMenu() { $(this).find('ul').css('visibility', 'hidden'); }

    $('#Subverse').autocomplete(
        {
            source: '/ajaxhelpers/autocompletesubversename'
        });

    // drag'n'drop link sharing
    //$(document).on('dragenter', function () {
    //    $('#share-a-link-overlay').show();
    //});

    // prevent spoiler links from opening windows
    $(document).on('click', 'a[href="#s"]', function (e) {
        e.preventDefault();
    });

    $('#share-a-link-overlay').on('dragleave', function (e) {
        if (e.originalEvent.pageX < 10 || e.originalEvent.pageY < 10 || $(window).width() - e.originalEvent.pageX < 10 || $(window).height - e.originalEvent.pageY < 10) {
            $("#share-a-link-overlay").hide();
        }
    });

    $('#share-a-link-overlay').on('dragover', function (e) {
        e.stopPropagation();
        e.preventDefault();
    });

    // tooltipster wireup
    $('.userinfo').tooltipster({
        content: 'Loading user info...',
        contentAsHTML: 'true',

        functionBefore: function (origin, continueTooltip) {

            // make this asynchronous and allow the tooltip to show
            continueTooltip();

            // next, we want to check if our data has already been cached
            if (origin.data('ajax') !== 'cached') {
                $.ajax({
                    type: 'GET',
                    url: '/ajaxhelpers/userinfo/' + origin.attr('data-username'),
                    success: function (data) {
                        // update our tooltip content with our returned data and cache it
                        origin.tooltipster('content', data).data('ajax', 'cached');
                    }
                });
            }
        }
    });

    // SignalR helper methods to start hub connection, update the page and send messages
    $(function () {
        // Reference the auto-generated proxy for the hub.
        if ($.connection != null) {
            var proxy = $.connection.messagingHub;

            if (proxy != null) {

                // Hub accessed function to inform the user about new pending notifications
                proxy.client.setNotificationsPending = function (count) {
                    var originalTitle = $('meta[property="og:title"]').attr('content');
                    if (count > 0) {
                        // set mail icon
                        if ($('#mail').hasClass('nohavemail')) {
                            $('#mail').removeClass('nohavemail').addClass('havemail');
                        };
                        $('#mail').prop('title', 'your have ' + count + ' unread notifications');
                        // show mail counter
                        $('#mailcounter').show();
                        $('#mailcounter').html(count);
                        // set browser title
                        document.title = '(' + count + ') ' + originalTitle;
                    } else {
                        // set no new mail icon
                        if ($('#mail').hasClass('havemail')) {
                            $('#mail').removeClass('havemail').addClass('nohavemail');
                        };
                        $('#mail').prop('title', 'no new messages');
                        // hide mail counter and set count to 0
                        $('#mailcounter').html(0);
                        $('#mailcounter').hide();
                        // set browser title
                        document.title = originalTitle;
                    }
                };

                // Hub accessed function to inform the user about new incoming upvotes
                proxy.client.incomingUpvote = function (type) {
                    var currentValue = 0;
                    if (type == 2) {
                        // this is a comment vote notification
                        // update CCP display
                        currentValue = $('#ccp').html();
                        currentValue++;
                        $('#ccp').html(currentValue);
                    } else {
                        // update SCP display
                        currentValue = $('#scp').html();
                        currentValue++;
                        $('#scp').html(currentValue);
                    }
                };

                // Hub accessed function to inform the user about new incoming downvotes
                proxy.client.incomingDownvote = function (type) {
                    var currentValue = 0;
                    if (type == 2) {
                        // this is a comment vote notification
                        // update CCP display
                        currentValue = $('#ccp').html();
                        currentValue--;
                        $('#ccp').html(currentValue);
                    } else {
                        // update SCP display
                        currentValue = $('#scp').html();
                        currentValue--;
                        $('#scp').html(currentValue);
                    }
                };

                // Hub accessed function to inform the user about new incoming down to upvote
                proxy.client.incomingDownToUpvote = function (type) {
                    var currentValue = 0;
                    if (type == 2) {
                        // this is a comment vote notification
                        // update CCP display
                        currentValue = $('#ccp').html();
                        currentValue = currentValue + 2;
                        $('#ccp').html(currentValue);
                    } else {
                        // update SCP display
                        currentValue = $('#scp').html();
                        currentValue = currentValue + 2;
                        $('#scp').html(currentValue);
                    }
                };

                // Hub accessed function to inform the user about new incoming up to downvote
                proxy.client.incomingUpToDownvote = function (type) {
                    var currentValue = 0;
                    if (type == 2) {
                        // this is a comment vote notification
                        // update CCP display
                        currentValue = $('#ccp').html();
                        currentValue = currentValue - 2;
                        $('#ccp').html(currentValue);
                    } else {
                        // update SCP display
                        currentValue = $('#scp').html();
                        currentValue = currentValue - 2;
                        $('#scp').html(currentValue);
                    }
                };

                // Hub accessed function to append incoming chat message
                proxy.client.appendChatMessage = function (sender, chatMessage) {
                    $("#subverseChatRoom").append('<p><b>' + sender + '</b>: ' + chatMessage + '</p>');
                    scrollChatToBottom();
                };

                // Start the connection.
                $.connection.hub.start().done(function () {
                    //
                });
            }
        }
    });

    // hook scroll event to load more button for endless scrolling
    $(function () {
        var $win = $(window);

        $win.scroll(function () {
            if ($win.height() + $win.scrollTop()
                == $(document).height()) {
                $("#loadmorebutton").trigger("click");
            }
        });
    });

});

// a function which handles mouse drop events (sharing links by dragging and dropping)
function dropFunction(event) {
    event.stopPropagation();
    event.preventDefault();

    var droppedData = event.dataTransfer.getData('text/html');

    var url;
    if ($(droppedData).children().length > 0) {
        url = $(droppedData).attr('href');
    } else {
        url = $(droppedData).attr('href');
    }

    // dropped data did not contain a HREF element, try to see if it has a SRC element instead
    if (url != null) {
        window.location.replace("/submit?linkpost=true&url=" + url);
    } else {
        url = $(droppedData).attr('src');
        if (url != null) {
            window.location.replace("/submit?linkpost=true&url=" + url);
        }
    }

    $("#share-a-link-overlay").hide();
}

function click_voting() {
    $(this).toggleClass("arrow upmod login-required");
}

function mustLogin() {
    $('#mustbeloggedinModal').modal();
}

function notEnoughCCP() {
    $('#notenoughccp').modal();
}

function notEnoughCCPUpVote() {
    $('#notenoughccpupvote').modal();
}

function firstTimeVisitorWelcome() {
    $('#firsttimevisitorwelcomemessage').toggle();
}
//locks vote operations
var submissionVoteLock = null;
function voteUpSubmission(submissionid) {

    if (submissionVoteLock == null) {

        submissionVoteLock = new Object();
        //submitUpVote(submissionid);
        $.ajax({
            type: "POST",
            url: "/vote/" + submissionid + "/1",
            complete: function () {
                submissionVoteLock = null;
            },
            success: function () {

                var submission = $(".submission.id-" + submissionid);
                var scoreLikes = +(submission.find('.score.likes').html());
                var scoreDislikes = +(submission.find('.score.dislikes').html());

                //ADD LIKE IF UNVOTED
                if (submission.children(".midcol").is(".unvoted")) {
                    submission.children(".midcol").toggleClass("likes", true); //add class likes
                    submission.children(".midcol").toggleClass("unvoted", false); //remove class unvoted
                    //add upvoted arrow
                    submission.children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true); //set upvote arrow to upvoted
                    submission.children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvote", false); //remove upvote arrow
                    //increment score likes counter        
                    scoreLikes++;
                    submission.find('.score.likes').html(scoreLikes);
                } else if (submission.children(".midcol").is(".likes")) {
                    //REMOVE LIKE IF LIKED
                    submission.children(".midcol").toggleClass("unvoted", true); //add class unvoted
                    submission.children(".midcol").toggleClass("likes", false); //remove class dislikes
                    //remove upvoted arrow
                    submission.children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true); //set arrow to upvote
                    submission.children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false); //remove upvoted arrow
                    //decrement score likes counter
                    scoreLikes--;
                    submission.find('.score.likes').html(scoreLikes);
                    submission.find('.score.unvoted').html(scoreLikes);
                } else if (submission.children(".midcol").is(".dislikes")) {
                    //ADD LIKE IF DISLIKED
                    submission.children(".midcol").toggleClass("dislikes", false); //remove class dislikes
                    submission.children(".midcol").toggleClass("likes", true); //add class likes
                    submission.children(".midcol").toggleClass("unvoted", false); //remove class unvoted        
                    //remove downvoted arrow
                    submission.children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true); //set downvoted arrow to downvote
                    submission.children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false); //remove downvoted arrow
                    submission.children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true); //add upvoted arrow
                    //increment score dislikes counter
                    scoreDislikes--;
                    scoreLikes++;
                    submission.find('.score.dislikes').html(scoreDislikes);
                    submission.find('.score.likes').html(scoreLikes);
                }
            }
        });
    }
}

function voteDownSubmission(submissionid) {

    if (submissionVoteLock == null) {

        submissionVoteLock = new Object();

        //submitUpVote(submissionid);
        $.ajax({
            type: "POST",
            url: "/vote/" + submissionid + "/-1",
            complete: function () {
                submissionVoteLock = null;
            },
            success: function () {

                var submission = $(".submission.id-" + submissionid);
                var scoreDislikes = +(submission.find('.score.dislikes').html());
                var scoreLikes = +(submission.find('.score.likes').html());

                //ADD DISLIKE IF UNVOTED
                if (submission.children(".midcol").is(".unvoted")) {
                    submission.children(".midcol").toggleClass("dislikes", true); //add class dislikes
                    submission.children(".midcol").toggleClass("unvoted", false); //remove class unvoted
                    //add downvoted arrow
                    submission.children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true); //set downvote arrow to downvoted
                    submission.children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvote", false); //remove downvote arrow
                    //increment score dislikes counter
                    scoreDislikes++;
                    submission.find('.score.dislikes').html(scoreDislikes);
                } else if (submission.children(".midcol").is(".dislikes")) {
                    //REMOVE DISLIKE IF DISLIKED
                    submission.children(".midcol").toggleClass("unvoted", true); //add class unvoted
                    submission.children(".midcol").toggleClass("dislikes", false); //remove class dislikes
                    //remove downvoted arrow
                    submission.children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true); //set arrow to downvote
                    submission.children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false); //remove downvoted arrow
                    //decrement score dislikes counter
                    scoreDislikes--;
                    submission.find('.score.dislikes').html(scoreDislikes);
                    submission.find('.score.unvoted').html(scoreLikes);
                } else if (submission.children(".midcol").is(".likes")) {
                    //ADD DISLIKE IF LIKED
                    submission.children(".midcol").toggleClass("likes", false); //remove class likes
                    submission.children(".midcol").toggleClass("dislikes", true); //add class dislikes
                    submission.children(".midcol").toggleClass("unvoted", false); //remove class unvoted
                    //remove upvoted arrow
                    submission.children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true); //set upvoted arrow to upvote
                    submission.children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false); //remove upvoted arrow
                    submission.children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true); //add downvoted arrow
                    //increment score dislikes counter
                    scoreDislikes++;
                    scoreLikes--;
                    submission.find('.score.dislikes').html(scoreDislikes);
                    submission.find('.score.likes').html(scoreLikes);
                }

            }
        });
    }
}

//function submitUpVote(messageid) {
//    $.ajax({
//        type: "POST",
//        url: "/vote/" + messageid + "/1"
//    });
//}

//function submitDownVote(messageid) {
//    $.ajax({
//        type: "POST",
//        url: "/vote/" + messageid + "/-1"
//    });
    //}
//locks vote operations
var commentVoteLock = null;

function voteUpComment(commentid) {

    if (commentVoteLock == null) {

        commentVoteLock = new Object();

        //submitCommentUpVote(commentid);
        $.ajax({
            type: "POST",
            url: "/votecomment/" + commentid + "/1",
            complete: function () {
                commentVoteLock = null;
            },
            success: function () {
                var comment = $(".comment.id-" + commentid);
                // get current score
                var scoreLikes = +(comment.find('.post_upvotes').filter(":first").html());
                var scoreDislikes = -(comment.find('.post_downvotes').filter(":first").html());

                // ADD LIKE IF UNVOTED
                if (comment.children(".midcol").is(".unvoted")) {
                    comment.children(".midcol").toggleClass("likes", true); //add class likes
                    comment.children(".midcol").toggleClass("unvoted", false); //remove class unvoted
                    // add upvoted arrow
                    comment.children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true); //set upvote arrow to upvoted
                    comment.children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvote", false); //remove upvote arrow
                    // increment comment points counter and update DOM element
                    scoreLikes++;
                    comment.find('.post_upvotes').filter(":first").html('+' + scoreLikes);
                    comment.find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                    comment.find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                } else if (comment.children(".midcol").is(".likes")) {
                    // REMOVE LIKE IF LIKED
                    comment.children(".midcol").toggleClass("unvoted", true); //add class unvoted
                    comment.children(".midcol").toggleClass("likes", false); //remove class dislikes
                    // remove upvoted arrow
                    comment.children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true); //set arrow to upvote
                    comment.children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false); //remove upvoted arrow
                    // decrement comment points counter and update DOM element
                    scoreLikes--;
                    comment.find('.post_upvotes').filter(":first").html('+' + scoreLikes);
                    comment.find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                    comment.find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                } else if (comment.children(".midcol").is(".dislikes")) {
                    // ADD LIKE IF DISLIKED
                    comment.children(".midcol").toggleClass("dislikes", false); //remove class dislikes
                    comment.children(".midcol").toggleClass("likes", true); //add class likes
                    comment.children(".midcol").toggleClass("unvoted", false); //remove class unvoted
                    // remove downvoted arrow
                    comment.children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true); //set downvoted arrow to downvote
                    comment.children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false); //remove downvoted arrow
                    comment.children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true); //add upvoted arrow
                    // increment/decrement comment points counters and update DOM element
                    scoreLikes++;
                    scoreDislikes--;
                    comment.find('.post_upvotes').filter(":first").html('+' + scoreLikes);
                    comment.find('.post_downvotes').filter(":first").html('-' + scoreDislikes);
                    comment.find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                    comment.find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                }
            }
        });
    }
}

 function voteDownComment(commentid) {
     if (commentVoteLock == null) {

         commentVoteLock = new Object();

         $.ajax({
             type: "POST",
             url: "/votecomment/" + commentid + "/-1",
             complete: function () {
                 commentVoteLock = null;
             },
             success: function () {
                 //submitCommentDownVote(commentid);
                 var comment = $(".comment.id-" + commentid);
                 // get current score
                 var scoreLikes = +(comment.find('.post_upvotes').filter(":first").html());
                 var scoreDislikes = -(comment.find('.post_downvotes').filter(":first").html());

                 // ADD DISLIKE IF UNVOTED
                 if (comment.children(".midcol").is(".unvoted")) {
                     comment.children(".midcol").toggleClass("dislikes", true); //add class dislikes
                     comment.children(".midcol").toggleClass("unvoted", false); //remove class unvoted
                     // add downvoted arrow
                     comment.children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true); //set downvote arrow to downvoted
                     comment.children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvote", false); //remove downvote arrow
                     // increment comment points counter and update DOM element        
                     scoreDislikes++;
                     comment.find('.post_downvotes').filter(":first").html('-' + scoreDislikes);
                     comment.find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                     comment.find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                 } else if (comment.children(".midcol").is(".dislikes")) {
                     // REMOVE DISLIKE IF DISLIKED
                     comment.children(".midcol").toggleClass("unvoted", true); //add class unvoted
                     comment.children(".midcol").toggleClass("dislikes", false); //remove class dislikes
                     // remove downvoted arrow
                     comment.children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true); //set arrow to downvote
                     comment.children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false); //remove downvoted arrow
                     // decrement comment points counter and update DOM element
                     scoreDislikes--;
                     comment.find('.post_downvotes').filter(":first").html('-' + scoreDislikes);
                     comment.find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                     comment.find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                 } else if (comment.children(".midcol").is(".likes")) {
                     // ADD DISLIKE IF LIKED
                     comment.children(".midcol").toggleClass("likes", false); //remove class likes
                     comment.children(".midcol").toggleClass("dislikes", true); //add class dislikes
                     comment.children(".midcol").toggleClass("unvoted", false); //remove class unvoted
                     // remove upvoted arrow
                     comment.children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true); //set upvoted arrow to upvote
                     comment.children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false); //remove upvoted arrow
                     comment.children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true); //add downvoted arrow
                     // increment/decrement comment points counters and update DOM element
                     scoreLikes--;
                     scoreDislikes++;
                     comment.find('.post_upvotes').filter(":first").html('+' + scoreLikes);
                     comment.find('.post_downvotes').filter(":first").html('-' + scoreDislikes);
                     comment.find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                     comment.find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                 }
             }
         });
     }
}

//function submitCommentUpVote(commentid) {
//    $.ajax({
//        type: "POST",
//        url: "/votecomment/" + commentid + "/1"
//    });
//}



//function submitCommentDownVote(commentid) {
//    $.ajax({
//        type: "POST",
//        url: "/votecomment/" + commentid + "/-1"
//    });
//}

// append a comment reply form to calling area while preventing multiple appends
var replyCommentFormRequest;
function reply(parentcommentid, messageid) {
    //exit function if the form is already being shown or a request is in progress
    if ($("#commentreplyform-" + parentcommentid).exists() || replyCommentFormRequest) {
        return;
    }

    var token = $("input[name='__RequestVerificationToken']").val();

    replyCommentFormRequest = $.ajax({
        url: "/ajaxhelpers/commentreplyform/" + parentcommentid + "/" + messageid,
        success: function (data) {
            $("#" + parentcommentid).append(data);
            //Focus the cursor on the comment reply form textarea, to prevent unnecessary use of the tab key
            $('#commentreplyform-' + parentcommentid).find('#Content').focus();
        },
        complete: function () {
            replyCommentFormRequest = null;
        }
    });

    var form = $('#commentreplyform-' + parentcommentid)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);
}

// append a private message reply form to calling area
var replyFormPMRequest;
function replyprivatemessage(parentprivatemessageid, recipient, subject) {
    // exit function if the form is already being shown or a request is in progress
    if ($("#privatemessagereplyform-" + parentprivatemessageid).exists() || replyFormPMRequest) {
        return;
    }

    var token = $("input[name='__RequestVerificationToken']").val();

    replyFormPMRequest = $.ajax({
        url: "/ajaxhelpers/privatemessagereplyform/" + parentprivatemessageid + "?recipient=" + recipient + "&subject=" + subject,
        success: function (data) {
            $("#messageContainer-" + parentprivatemessageid).append(data);
            //Focus the cursor on the private message reply form textarea, to prevent unnecessary use of the tab key
            $('#privatemessagereplyform-' + parentprivatemessageid).find('#Body').focus();
        },
        complete: function () {
            replyFormPMRequest = null;
        }
    });

    var form = $('#privatemessagereplyform-' + parentprivatemessageid)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);

    // TODO
    // showRecaptcha('recaptchaContainer');
}

// append a comment reply form to calling area (used in comment reply notification view)
var replyToCommentFormRequest;
function replyToCommentNotification(commentId, submissionId) {
    // exit function if the form is already being shown or a request is in progress
    if ($("#commentreplyform-" + commentId).exists() || replyToCommentFormRequest) {
        return;
    }

    var token = $("input[name='__RequestVerificationToken']").val();

    replyToCommentFormRequest = $.ajax({
        url: "/ajaxhelpers/commentreplyform/" + commentId + "/" + submissionId,
        success: function (data) {
            $("#commentContainer-" + commentId).append(data);
            //Focus the cursor on the comment reply form textarea, to prevent unnecessary use of the tab key
            $('#commentreplyform-' + commentId).find('#Content').focus();
        },
        complete: function () {
            replyToCommentFormRequest = null;
        }
    });

    var form = $('#commentreplyform-' + commentId)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);
}

// post comment reply form through ajax
function postCommentReplyAjax(senderButton, messageId, userName, parentcommentid) {
    var $form = $(senderButton).parents('form');
    $form.find("#errorMessage").toggle(false);

    if ($form.find("#Content").val().length > 0) {
        $form.find("#submitbutton").val("Please wait...");
        $form.find("#submitbutton").prop('disabled', true);

        $.ajax({
            type: "POST",
            url: $form.attr('action'),
            data: $form.serialize(),
            error: function (xhr, status, error) {
                // submission failed, likely cause: user triggered anti-spam throttle
                $form.find("#submitbutton").val("Submit reply");
                $form.find("#submitbutton").prop('disabled', false);
                $form.find("#errorMessage").html("You are doing that too fast. Please wait 30 seconds before trying again.");
                $form.find("#errorMessage").toggle(true);
            },
            success: function (response) {
             
                removereplyform(parentcommentid);
                $(".id-" + parentcommentid).append(response);

                //notify UI framework of DOM insertion async
                window.setTimeout(function () { UI.Notifications.raise('DOM', $('.id-' + parentcommentid).last('div')); });
            }
        });

        return false;
    } else {
        $form.find("#errorMessage").toggle(true);
    }
}

// post comment reply form through ajax
function postCommentAjax(senderButton, messageId, userName) {
    var $form = $(senderButton).parents('form');
    $form.find("#errorMessage").toggle(false);

    if ($form.find("#Content").val().length > 0) {
        $form.find("#submitbutton").val("Doing the magic...");
        $form.find("#submitbutton").prop('disabled', true);

        $.ajax({
            type: "POST",
            url: $form.attr('action'),
            data: $form.serialize(),
            error: function (xhr, status, error) {
                // submission failed, likely cause: user triggered anti-spam throttle
                $form.find("#submitbutton").val("Submit comment");
                $form.find("#submitbutton").prop('disabled', false);
                $form.find("#errorMessage").html(error.length > 0 && (error != 'Bad Request' && error != 'Internal Server Error') ? error : "You are doing that too fast. Please wait 30 seconds before trying again.");
                $form.find("#errorMessage").toggle(true);
            },
            //response now contains the comment html
            success: function (response) {

                $(".sitetable.nestedlisting").prepend(response);
                // reset submit button
                $form.find("#submitbutton").val("Submit comment");
                $form.find("#submitbutton").prop('disabled', false);
                // reset textbox
                $form.find("#Content").val("");
                //notify UI framework of DOM insertion async
                window.setTimeout(function () { UI.Notifications.raise('DOM', $('.sitetable.nestedlisting').first()); });
            }
        });

        return false;
    } else {
        $form.find("#errorMessage").toggle(true);
    }
}

// post private message reply form through ajax
function postPrivateMessageReplyAjax(senderButton, parentprivatemessageid) {
    var $form = $(senderButton).parents('form');
    $form.find("#errorMessage").toggle(false);

    if ($form.find("#Body").val().length > 0) {
        $form.find("#submitbutton").val("Please wait...");
        $form.find("#submitbutton").prop('disabled', true);

        $.ajax({
            type: "POST",
            url: $form.attr('action'),
            data: $form.serialize(),
            error: function (xhr, status, error) {
                //submission failed, likely cause: user triggered anti-spam throttle
                $form.find("#submitbutton").val("Submit reply");
                $form.find("#submitbutton").prop('disabled', false);
                $form.find("#errorMessage").html("You are doing that too fast. Please wait 30 seconds before trying again.");
                $form.find("#errorMessage").toggle(true);
            },
            success: function (response) {
                // remove reply form 
                removereplyform(parentprivatemessageid);
                // change reply button to "reply sent" and disable it               
                $("#messageContainer-" + parentprivatemessageid).find("#replyPrivateMessage").html("Reply sent.");
                $("#messageContainer-" + parentprivatemessageid).find("#replyPrivateMessage").addClass("disabled");
            }
        });

        return false;
    } else {
        $form.find("#errorMessage").toggle(true);
    }
}

// append a comment edit form to calling area while preventing multiple appends
function edit(parentcommentid, messageid) {

    // hide original text comment
    $("#commentContent-" + parentcommentid).toggle(1);

    // show edit form
    $("#" + parentcommentid).find(".usertext-edit").toggle(1);

    // Focus the cursor on the edit comment form textarea, to prevent unnecessary use of the tab key
    $("#commenteditform-" + parentcommentid).find("#Content").focus();

    var form = $("#commenteditform-" + parentcommentid)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);
}

// append a submission edit form to calling area while preventing multiple appends
function editsubmission(submissionid) {

    //hide original text    
    $("#submissionid-" + submissionid).find('.original').toggle(1);

    //show edit form
    $("#submissionid-" + submissionid).find('.usertext-edit').toggle(1);

    var form = $('#submissioneditform-' + submissionid)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);
}

// remove submission edit form for given submission id and replace it with original content
function removesubmissioneditform(submissionid) {
    //BUG: This code makes previews after a submission edit not display. Low Priority.
    $("#submissionid-" + submissionid).find('.usertext-body').toggle(1);
    $("#submissionid-" + submissionid).find('.usertext-edit').toggle(1);
}

// submit edited submission and replace the old one with formatted response received by server
function editmessagesubmit(submissionid) {
    var submissioncontent = $("#submissionid-" + submissionid).find('.form-control').val();
    var submissionobject = { "SubmissionId": submissionid, "SubmissionContent": submissioncontent };

    $.ajax({
        type: "POST",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(submissionobject),
        url: "/editsubmission",
        datatype: "json",
        success: function (data) {
            $("#submissionid-" + submissionid).find('.md').html(data.response);
            window.setTimeout(function () { UI.Notifications.raise('DOM', $("#submissionid-" + submissionid)); });
        }
    });

    removesubmissioneditform(submissionid);
    return false;
}

// remove comment reply form for given parent id
function removereplyform(parentcommentid) {
    $('#replyform-' + parentcommentid).remove();
}

// remove edit form for given parent id and replace it with original comment
function removeeditform(parentcommentid) {
    $("#" + parentcommentid).find(".usertext-body").show();
    $("#" + parentcommentid).find(".usertext-edit").hide();
}

function showcomment(commentid) {
    //show actual comment
    $("#" + commentid).closest('.noncollapsed').toggle(1);
    //hide show hidden children button
    $("#" + commentid).prev().toggle(1);
    //show voting icons
    $("#" + commentid).parent().parent().find('.midcol').filter(":first").toggle(1);
    //show all children
    $("#" + commentid).parent().parent().find('> .child').toggle(1);

    return (false);
}

function hidecomment(commentid) {
    //hide actual comment
    $("#" + commentid).closest('.noncollapsed').toggle(1);
    //show show hidden children button
    $("#" + commentid).prev().toggle(1);
    //hide voting icons
    $("#" + commentid).parent().parent().find('.midcol').filter(":first").toggle(1);
    //hide all children
    $("#" + commentid).parent().parent().find('> .child').toggle(1);

    return (false);
}

// submit edited comment and replace the old one with formatted response received by server
function editcommentsubmit(commentid) {
    var commentcontent = $("#" + commentid).find('.form-control').val();
    var commentobject = { "ID": commentid, "Content": commentcontent };

    $.ajax({
        type: "POST",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(commentobject),
        url: "/editcomment",
        datatype: "json",
        error: function (xhr, status, error) {
            var msg = error.length > 0 && (error != 'Bad Request' && error != 'Internal Server Error') ? error : "You are doing that too fast. Please wait 30 seconds before trying again.";
            $('#commenteditform-' + commentid + " span.field-validation-error").html(msg);
        },
        success: function (data) {
            $("#" + commentid).find('.md').html(data.response);

            removeeditform(commentid);

            //notify UI framework of DOM insertion async
            window.setTimeout(function () { UI.Notifications.raise('DOM', $('#' + commentid)); });
        }
    });


    return false;
}

// delete comment
function deletecomment(commentid) {
    //hide comment menu buttons
    $("#" + commentid).find('.flat-list').html('');

    //hide original comment text
    $("#" + commentid).find('.md').html("[deleted]");
    $("#" + commentid).find('.md').css("color", "gray");

    //hide comment author
    $("#" + commentid).find('.author').replaceWith(function () {
        return $("<em>" + "[deleted]" + "</em>");
    });

    //hide comment author attributes
    $("#" + commentid).find('.userattrs').html('');

    //hide "are you sure" option
    toggleback(commentid);

    removeeditform(commentid);

    //execute POST call to remove comment from database
    deletecommentsubmit(commentid);
}

// submit comment deletion request
function deletecommentsubmit(commentid) {
    var commentobject = { "commentid": commentid };

    $.ajax({
        type: "POST",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(commentobject),
        url: "/deletecomment",
        datatype: "json"
    });

    removeeditform(commentid);
    return false;
}

// submit submission deletion request
function deletesubmission(senderButton, submissionid) {
    var $form = $(senderButton).parents('form');
    $form.find("#deletestatusmesssage").html("please wait...");

    var submissionobject = { "submissionid": submissionid };

    $.ajax({
        type: "POST",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(submissionobject),
        url: "/deletesubmission",
        datatype: "json"
    });

    // reload body content with background page refresh
    $('body').load($(location).attr('href'));
}

// toggle are you sure question for comment deletion
function toggle(obj, commentid) {
    $(obj).parent().parent().find('.option, .main').toggleClass("active");
    return false;
}

// toggle are you sure question for subverse block action
function toggleblocksubverse(obj) {
    $(obj).parent().parent().find('.option, .error').toggleClass("active");
    return false;
}

// toggle are you sure question for comment report
function togglereport(commentid) {
    $("#" + commentid).find('.report').toggleClass("active");
    return false;
}

// submit report and replace report button with a "thank you" to the user
function reportcomment(obj, commentid) {
    $(obj).parent().parent().find('.togglebutton').attr("onclick", "javascript:void(0)");
    $(obj).parent().parent().find('.option, .main').toggleClass("active");
    $(obj).parent().parent().find('.togglebutton').html("please wait...");

    // submit report
    $.ajax({
        type: "POST",
        url: "/reportcomment/" + commentid,
        success: function () {
            $(obj).parent().parent().find('.togglebutton').html("thank you!");
        },
        error: function () {
            $(obj).parent().parent().find('.togglebutton').html("report failed");
        }
    });

    return false;
}

// togle back are you sure question
function toggleback(obj) {
    $(obj).parent().parent().find('.option, .error').toggleClass("active");
    return false;
}

// toggle are you sure question for submission deletion
function togglesubmission(obj, submissionid) {
    $(obj).parent().parent().find('.option, .main').toggleClass("active");
    return false;
}

// togle back are you sure question for submission deletion
function togglesubmissionback(obj) {
    $(obj).parent().parent().find('.option, .error').toggleClass("active");
    return false;
}

// check if an object exists
$.fn.exists = function () {
    return this.length !== 0;
};

// subscribe to subverse
function subscribe(obj, subverseName) {
    $(obj).attr("onclick", "unsubscribe(this)");
    $(obj).html("unsubscribe");
    $(obj).toggleClass("btn-sub btn-unsub");

    // call the subverse subscribe API
    $.ajax({
        type: "POST",
        url: "/subscribe/" + subverseName,
        success: function () {
            var numberOfSubscribers = +($('#subscriberCount').html());
            numberOfSubscribers++;
            $('#subscriberCount').html(numberOfSubscribers);
        },
        error: function () {
            alert('Something went wrong while sending a subscription request.');
        }
    });
}

// unsubscribe from subverse
function unsubscribe(obj, subverseName) {
    $(obj).attr("onclick", "subscribe(this)");
    $(obj).html("subscribe");
    $(obj).toggleClass("btn-sub btn-unsub");

    // call the subverse unsubscribe API
    $.ajax({
        type: "POST",
        url: "/unsubscribe/" + subverseName,
        success: function () {
            var numberOfSubscribers = +($('#subscriberCount').html());
            numberOfSubscribers--;
            $('#subscriberCount').html(numberOfSubscribers);
        },
        error: function () {
            alert('Something went wrong while sending unsubscription request.');
        }
    });
}

// subscribe to set
function subscribeToSet(obj, setId) {
    $(obj).attr("onclick", "unsubscribe(this)");
    $(obj).html("unsubscribe");

    // call the set subscribe API
    $.ajax({
        type: "POST",
        url: "/subscribetoset/" + setId,
        success: function () {
            var numberOfSubscribers = +($('#subscribercount').html());
            numberOfSubscribers++;
            $('#subscribercount').html(numberOfSubscribers);
        },
        error: function () {
            alert('Something went wrong while sending a set subscription request.');
        }
    });
}

// unsubscribe from set
function unsubscribeFromSet(obj, setId) {
    $(obj).attr("onclick", "subscribe(this)");
    $(obj).html("subscribe");

    // call the unsubscribe API
    $.ajax({
        type: "POST",
        url: "/unsubscribefromset/" + setId,
        success: function () {
            var numberOfSubscribers = +($('#subscriberCount').html());
            numberOfSubscribers--;
            $('#subscriberCount').html(numberOfSubscribers);
        },
        error: function () {
            alert('Something went wrong while sending unsubscription request.');
        }
    });
}

// remove a subverse from a set
function removeSubFromSet(obj, setId, subverseName) {
    $(obj).html("Hold on...");

    // call remove subverse from set API
    $.ajax({
        type: "POST",
        url: "/sets/removesubverse/" + setId + "/" + subverseName,
        success: function () {
            // remove the remove button along with sub info
            $("#subverse-" + subverseName).remove();
        },
        error: function () {
            $(obj).html("Something went wrong.");
        }
    });
}

// add a subverse to a set
function addSubToSet(obj, setId) {
    $(obj).html("Hold on...");
    var subverseName = $("#Subverse").val();

    if (!subverseName) {
        $(obj).html("Add this subverse to set");
        $("#status").html("please enter a subverse name to add");
        $("#status").show();
        return;
    }

    // call add subverse to set API
    $.ajax({
        type: "POST",
        url: "/sets/addsubverse/" + setId + "/" + subverseName,
        success: function () {
            var subverseInfo = $.get(
                "/ajaxhelpers/setsubverseinfo/" + setId + "/" + subverseName,
                null,
                function (data) {
                    $("#subverselisting").append(data);
                    $("#status").hide();
                    $(obj).html("Add this subverse to set");
                }
             );
        },
        error: function () {
            $("#status").html("Subverse probably does not exist.");
            $("#status").show();
            $(obj).html("Add this subverse to set");
        }
    });
}

// a function to load content of a self post and append it to calling object
function loadSelfText(obj, messageId) {
    // load content only if collapsed, don't cache as author may edit the submission
    var isExpanded = false;
    if ($(obj).hasClass('collapsed')) {
        //fetch message content and append under class md
        var messageContent = $.get(
            "/ajaxhelpers/messagecontent/" + messageId,
            null,
            function (data) {
                $(obj).parent().find(".expando").find(".md").html(data);
                window.setTimeout(function () { UI.Notifications.raise('DOM', $(obj).parent().find(".expando")); });
            }
         );
    }

    $(obj).toggleClass("collapsed");
    $(obj).toggleClass("expanded");

    // toggle message content display
    $(obj).parent().find(".expando").toggle();
}

//// a function to embed a video via expando
//[Obsolete - Remove once UI Expandos are tested]
function loadVideoPlayer(obj, messageId) {

    $(obj).toggleClass("collapsed");
    $(obj).toggleClass("expanded");

    // fetch message content and append under class md
    var messageContent = $.get(
        "/ajaxhelpers/videoplayer/" + messageId,
        null,
        function (data) {
            $(obj).parent().find(".expando").nextAll().find(".videoplayer").html(data);
            window.setTimeout(function () {
                UI.Notifications.raise('iFrameLoaded', $(obj).parent().find(".expando").nextAll().find(".videoplayer"));
            });
        }
     );

    // note: the nextnextnextnext thing is ugly, feel free to write a cleaner solution. Thanks!
    $(obj).parent().find(".expando").next().next().next().toggle();
}

// function to post delete private message request to messaging controller and remove deleted message DOM
function deletePrivateMessage(obj, privateMessageId) {
    var privateMessageObject = { "privateMessageId": privateMessageId };

    $(obj).html("please wait...");

    $.ajax({
        type: "POST",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(privateMessageObject),
        success: function () {
            //remove message DOM
            $("#messageContainer-" + privateMessageId).remove();
        },
        url: "/messaging/delete",
        datatype: "json"
    });

    return false;
}

// function to post delete sent private message request to messaging controller and remove deleted message DOM
function deletePrivateMessageFromSent(obj, privateMessageId) {
    var privateMessageObject = { "privateMessageId": privateMessageId };

    $(obj).html("please wait...");

    $.ajax({
        type: "POST",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(privateMessageObject),
        success: function () {
            //remove message DOM
            $("#messageContainer-" + privateMessageId).remove();
        },
        url: "/messaging/deletesent",
        datatype: "json"
    });

    return false;
}

// function to load select link flair modal dialog for given subverse and given submission
function selectflair(messageId, subverseName) {
    var flairSelectDialog = $.get(
        "/ajaxhelpers/linkflairselectdialog/" + subverseName + "/" + messageId,
        null,
        function (data) {
            $("#linkFlairSelectModal").html(data);
            $('#linkFlairSelectModal').modal();
        }
     );
}

// function to apply flair to a given submission
function applyflair(submissionID, flairID, flairLabel, flairCssClass) {
    $.ajax({
        type: "POST",
        url: "/submissions/applylinkflair/" + submissionID + "/" + flairID,
        success: function () {
            $('#linkFlairSelectModal').modal('hide');

            //set linkflair
            $('#linkflair').attr('class', "flair " + flairCssClass);
            $('#linkflair').attr('title', flairLabel);
            $('#linkflair').html(flairLabel);
        },
        error: function () {
            alert('Unable to apply link flair.');
        }
    });
}

// function to clear flair from a given submission
function clearflair(messageId) {
    $.ajax({
        type: "POST",
        url: "/submissions/clearlinkflair/" + messageId,
        success: function () {
            $('#linkFlairSelectModal').modal('hide');

            //clear linkflair
            $('#linkflair').attr('class', "");
            $('#linkflair').attr('title', "");
            $('#linkflair').html("");
        },
        error: function () {
            alert('Unable to clear link flair.');
        }
    });
}

// function to toggle distinguish flag for a given comment
function distinguish(commentId, obj) {
    $(obj).html("please wait...");

    $.ajax({
        type: "POST",
        url: "/comments/distinguish/" + commentId,
        success: function () {
            $(obj).html("done");

            // TODO: find the comment and add class "moderator" to give better distinguish feedback
        },
        error: function () {
            $(obj).html("unable to comply");
        }
    });
}

// a function to suggest a title for given Uri
function suggestTitle() {
    $("#suggest-title").off('click', suggestTitle);
    $("#suggest-title").text('Please wait...');

    var uri = $("#Content").val();

    // request a url title from title service
    var title = $.get(
        "/ajaxhelpers/titlefromuri?uri=" + uri,
        null,
        function (data) {
            $("#LinkDescription").val(data);
            $("#suggest-title").text("Enter the URL above, then click here to suggest a title");
            $("#suggest-title").on('click', suggestTitle);
        }).fail(function () {
            $("#LinkDescription").val("We were unable to suggest a title.");
            $("#suggest-title").text("Enter the URL above, then click here to suggest a title");
            $("#suggest-title").on('click', suggestTitle);
        });
}

// a function to toggle sticky mode for a submission
function toggleSticky(messageId) {
    $.ajax({
        type: "POST",
        url: "/submissions/togglesticky/" + messageId,
        success: function () {
            $('#togglesticky').html("toggled");
        },
        error: function () {
            alert('Something went wrong while sending a sticky toggle request.');
        }
    });
}

// a function to display a preview of a message without submitting it
function showMessagePreview(senderButton, messageContent, previewArea) {
    var rawSubmissionContent = $(messageContent).val();
    if (!rawSubmissionContent.length > 0) {
        $(previewArea).find("#submission-preview-area-container").html("Please enter some text in order to get a preview.");
        $(previewArea).show();
        return false;
    }

    $(senderButton).val("Please wait");

    // get rendered submission and show it
    var submissionModel = {
        MessageContent: $(messageContent).val()
    };

    $.ajax({
        url: '/ajaxhelpers/rendersubmission/',
        type: 'post',
        dataType: 'html',
        success: function (data) {
            $(previewArea).find("#submission-preview-area-container").html(data);
            UI.ExpandoManager.execute();
        },
        data: submissionModel
    });

    // show the preview area
    $(previewArea).show();
    $(senderButton).val("Preview");
    return false;
}

// a function to fetch 1 page for a set and append to the bottom of the given set
var loadMoreSetRequest;
function loadMoreSetItems(obj, setId) {
    if (loadMoreSetRequest) { return; }
    $(obj).html("Sit tight...");

    // try to see if this request is a subsequent request
    var currentPage = $("#set-" + setId + "-page").html();
    if (currentPage == null) {
        currentPage = 1;
    } else {
        currentPage++;
    }

    loadMoreSetRequest = $.ajax({
        url: "/set/" + setId + "/" + currentPage + "/",
        success: function (data) {
            $("#set-" + setId + "-page").remove();
            $("#set-" + setId + "-container").append(data);
            $(obj).html("load more &#9660;");
        },
        error: function () {
            {
                $(obj).html("That's it. There was nothing else to show.");
            }
        },
        complete: function () {
            loadMoreSetRequest = null;
        }
    });
}

// a function that toggles the visibility of the comment/submission/message source textarea
function toggleSource(senderButton) {
    //toggle textarea visibility
    $(senderButton.parentElement.parentElement.parentElement).find('#sourceDisplay').toggle();
    //change label name according to current state
    if (senderButton.text == "source") {
        senderButton.text = "hide source";
    } else {
        senderButton.text = "source";
    }
}

// a function to change set title name
function changeSetName() {
    $('#setName').removeAttr("onclick");
    $('#setName').hide();

    // show textbox
    $('#newSetName').show();
    $('#newSetNameEditBox').focus();

    $('#newSetNameEditBox').on('keypress', function (e) {
        if (e.keyCode === 13) {
            $('#setName').html($('#newSetNameEditBox').val());

            $('#setName').bind('click', changeSetName);
            $('#newSetName').hide();
            $('#setName').show();
        }
    });
}

function cancelSetTitleChange() {
    $('#setName').bind('click', changeSetName);
    $('#newSetName').hide();
    $('#setName').show();
}

function saveSetTitle(obj, setId) {
    $(obj).html('Please wait...');

    $.ajax({
        type: "POST",
        url: "/sets/modify/" + setId + "/" + $('#newSetNameEditBox').val(),
        success: function () {
            $('#setName').html($('#newSetNameEditBox').val());
            $('#setName').bind('click', changeSetName);
            $('#newSetName').hide();
            $('#setName').show();

            $(obj).html('Save');
        },
        error: function () {
            $(obj).html('Max 20 characters');
        }
    });
}

// a function to ask the user to confirm permanent set deletion request
function deleteSet(obj, setId) {
    $(obj).html("Are you sure?");

    $(obj).bind({
        click: function () {
            deleteSetExecute(obj, setId);
        }
    });

    return false;
}

// a function to permanently delete a given set
function deleteSetExecute(obj, setId) {
    $(obj).html('Please wait...');

    $.ajax({
        type: "POST",
        url: "/sets/delete/" + setId,
        success: function () {
            // remove the set from view
            $("#set-" + setId).remove();
        },
        error: function () {
            $(obj).html('Nope.');
        }
    });
}

// a function to fetch 1 comment bucket for a submission and append to the bottom of the page
var loadCommentsRequest2;
function loadMoreComments2(eventSource, appendTarget, submissionId, parentId, command, startingIndex, sort) {
    if (loadCommentsRequest2) { return; }
    eventSource.html('Sit tight...');

    // try to see if this request is a subsequent request
    var currentPage = $("#comments-" + submissionId + "-page").html();
    if (currentPage == null) {
        currentPage = 1;
    } else {
        currentPage++;
    }
    var bucketUrl =  "/comments/" + submissionId + "/" + (parentId == null ? 'null' : parentId) + "/" + command + "/" + startingIndex + "/" + sort;
    loadCommentsRequest2 = $.ajax({
        url: bucketUrl,
        success: function (data) {
            //$("#comments-" + submissionId + "-page").remove();
            appendTarget.append(data);
            window.setTimeout(function () { UI.Notifications.raise('DOM', appendTarget); });
            eventSource.parent().remove();
        },
        error: function () {
            eventSource.html('A problem happened.');
        },
        complete: function () {
            loadCommentsRequest2 = null;
        }
    });
}

// a function to fetch 1 comment bucket for a submission and append to the bottom of the page
var loadCommentsRequest;
function loadMoreComments(obj, submissionId) {
    if (loadCommentsRequest) { return; }
    $(obj).html("Sit tight...");

    // try to see if this request is a subsequent request
    var currentPage = $("#comments-" + submissionId + "-page").html();
    if (currentPage == null) {
        currentPage = 1;
    } else {
        currentPage++;
    }
    loadCommentsRequest = $.ajax({
        url: "/comments/" + submissionId + "/" + currentPage + "/",
        success: function (data) {
            $("#comments-" + submissionId + "-page").remove();
            $(obj).before(data);
            window.setTimeout(function () { UI.Notifications.raise('DOM', $(obj).parent()); });
            $(obj).html("load more &#9660;");
        },
        error: function () {
            $(obj).html("That's it. There was nothing else to show. Phew. This was hard.");
        },
        complete: function () {
            loadCommentsRequest = null;
        }
    });
}

// a function to fetch the parent of a comment.
function goToParent(event, parentId) {
    //If the parent is on the page this js should scroll to it.
    //Otherwise, href should request a new page.
    if ($("#" + parentId).exists()) {
        //Stop event and scroll
        event.preventDefault();
        window.location.hash = "#" + parentId;
    }
}

// a function to scroll chat box content up
function scrollChatToBottom() {
    var elem = document.getElementById('subverseChatRoom');
    elem.scrollTop = elem.scrollHeight;
}

// a function to submit chat message to subverse chat room
function sendChatMessage(userName, subverse) {
    if ($.connection != null) {
        var messageToSend = $("#chatInputBox").val();
        var chatProxy = $.connection.messagingHub;
        chatProxy.server.sendChatMessage(userName, messageToSend, subverse);
        scrollChatToBottom();
        // clear input
        $("#chatInputBox").val('');
    }
}

// a function to add a client to a subverse chat room
function joinSubverseChatRoom(subverseName) {
    if ($.connection != null) {
        // Start the connection.
        $.connection.hub.start().done(function () {
            var chatProxy = $.connection.messagingHub;
            chatProxy.server.joinSubverseChatRoom(subverseName);
        });
    }
}

function toggleNightMode() {
    $.ajax({
        type: "POST",
        url: "/account/togglenightmode/",
        complete: function () {
            //Reload Page to get new styles
            window.location.reload();
        }
    });
}

function toggleSaveSubmission(submissionId) {
    var saveLink = $(".submission.id-" + submissionId + " .savelink");
    if (saveLink.exists()) {
        if (saveLink.text() === "save") {
            saveLink.text("unsave");
        } else {
            saveLink.text("save");
        }
        $.ajax({
            type: "POST",
            url: "/save/" + submissionId
        });
    }
}

function toggleSaveComment(commentId) {
    var saveLink = $(".comment.id-" + commentId + " .savelink").first();
    if (saveLink.exists()) {
        if (saveLink.text() === "save") {
            saveLink.text("unsave");
        } else {
            saveLink.text("save");
        }
        $.ajax({
            type: "POST",
            url: "/savecomment/" + commentId
        });
    }
}

// a function to submit subverse block/unblock request
function toggleBlockSubverse(obj, subverseName) {
    $(obj).toggleClass("btn-blocksubverse btn-unblocksubverse");
    var blockButton = $(obj);
    if (blockButton.exists()) {
        if (blockButton.text() === "block") {
            blockButton.text("unblock");
        } else {
            blockButton.text("block");
        }

        // submit block request
        postBlockSubverse(subverseName);
    }
}

// a function to submit subverse block/unblock request via SFLButtonBlockSubverse
function toggleBlockSubverseFLButton(obj, subverseName) {
    var blockButton = $(obj);
    if (blockButton.exists()) {
        if (blockButton.text() === "block subverse") {
            blockButton.text("undo");
        } else {
            blockButton.text("block subverse");
        }

        // submit block request
        postBlockSubverse(subverseName);
    }
}

// a function to post subverse block request
function postBlockSubverse(subverseName) {
    $.ajax({
        type: "POST",
        url: "/subverses/block/" + subverseName
    });
}

// a function to check username availability
function checkUsernameAvailability(obj) {
    if ($(obj).val().length > 1) {
        if (!/\s/g.test($(obj).val())) {
            var checkRequest = $.ajax({
                type: "POST",
                url: "/account/CheckUsernameAvailability",
                data: { userName: $(obj).val() },
                success: function (data) {
                    // analyze response and inform the user
                    if (data.Available) {
                        $('#usernameAvailabilityStatus').hide();
                    } else {
                        $('#usernameAvailabilityStatus').show();
                    }
                }
            });
        }
    }
}

// a function to call mark as read messaging endpoint
function markAsRead(obj, itemType, itemId, markAll) {
    $(obj).attr("onclick", "");
    // mark single item as read
    if (itemId != null && markAll === false) {
        var markAsReadRequest = $.ajax({
            type: "GET",
            url: "/messaging/markasread",
            data: {
                itemType: itemType,
                itemId: itemId,
                markAll: markAll
            },
            success: function (data) {
                // inform the user
                $(obj).text("marked.");
            },
            error: function (data) {
                $(obj).text("something went wrong.");
            }
        });
    } else {
        // mark all items as read
        var markAllAsReadRequest = $.ajax({
            type: "GET",
            url: "/messaging/markasread",
            data: {
                itemType: itemType,
                markAll: markAll
            },
            success: function (data) {
                // inform the user
                $(obj).text("marked.");
            },
            error: function (data) {
                $(obj).text("something went wrong.");
            }
        });
    }
}

// a function to preview stylesheet called from subverse stylesheet editor
function previewStylesheet(obj, subverseName) {
    var sendingButton = $(obj);
    sendingButton.html("Hold on...");
    sendingButton.prop('disabled', true);

    $.ajax({
        type: 'GET',
        url: '/ajaxhelpers/previewstylesheet?subversetoshow=' + subverseName + '&previewMode=true',
        dataType: 'html',
        success: function (data) {
            $("#stylesheetpreviewarea").html(data);
            sendingButton.html("Preview");
            sendingButton.prop('disabled', false);

            // remove the old stylesheet from document
            var sheetToRemove = document.getElementById('custom_css');
            var sheetParent = sheetToRemove.parentNode;
            sheetParent.removeChild(sheetToRemove);

            // inject the new stylesheet
            var sheetToAdd = document.createElement('style');
            sheetToAdd.innerHTML = $("#Stylesheet").val();
            document.body.appendChild(sheetToAdd);
        }
    });
}