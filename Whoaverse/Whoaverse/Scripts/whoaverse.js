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
    // activate bootsrap popovers
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
});

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

function voteUpSubmission(submissionid) {
    submitUpVote(submissionid);
    var scoreLikes = +($(".id-" + submissionid).find('.score.likes').html());
    var scoreDislikes = +($(".id-" + submissionid).find('.score.dislikes').html());

    //ADD LIKE IF UNVOTED
    if ($(".id-" + submissionid).children(".midcol").is(".unvoted")) {
        $(".id-" + submissionid).children(".midcol").toggleClass("likes", true); //add class likes
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", false); //remove class unvoted
        //add upvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true); //set upvote arrow to upvoted
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvote", false); //remove upvote arrow
        //increment score likes counter        
        scoreLikes++;
        $(".id-" + submissionid).find('.score.likes').html(scoreLikes);
    } else if ($(".id-" + submissionid).children(".midcol").is(".likes")) {
        //REMOVE LIKE IF LIKED
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", true); //add class unvoted
        $(".id-" + submissionid).children(".midcol").toggleClass("likes", false); //remove class dislikes
        //remove upvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true); //set arrow to upvote
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false); //remove upvoted arrow
        //decrement score likes counter
        scoreLikes--;
        $(".id-" + submissionid).find('.score.likes').html(scoreLikes);
        $(".id-" + submissionid).find('.score.unvoted').html(scoreLikes);
    } else if ($(".id-" + submissionid).children(".midcol").is(".dislikes")) {
        //ADD LIKE IF DISLIKED
        $(".id-" + submissionid).children(".midcol").toggleClass("dislikes", false); //remove class dislikes
        $(".id-" + submissionid).children(".midcol").toggleClass("likes", true); //add class likes
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", false); //remove class unvoted        
        //remove downvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true); //set downvoted arrow to downvote
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false); //remove downvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true); //add upvoted arrow
        //increment score dislikes counter
        scoreDislikes--;
        scoreLikes++;
        $(".id-" + submissionid).find('.score.dislikes').html(scoreDislikes);
        $(".id-" + submissionid).find('.score.likes').html(scoreLikes);
    }

}

function voteDownSubmission(submissionid) {
    submitDownVote(submissionid);

    var scoreDislikes = +($(".id-" + submissionid).find('.score.dislikes').html());
    var scoreLikes = +($(".id-" + submissionid).find('.score.likes').html());

    //ADD DISLIKE IF UNVOTED
    if ($(".id-" + submissionid).children(".midcol").is(".unvoted")) {
        $(".id-" + submissionid).children(".midcol").toggleClass("dislikes", true); //add class dislikes
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", false); //remove class unvoted
        //add downvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true); //set downvote arrow to downvoted
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvote", false); //remove downvote arrow
        //increment score dislikes counter
        scoreDislikes++;
        $(".id-" + submissionid).find('.score.dislikes').html(scoreDislikes);
    } else if ($(".id-" + submissionid).children(".midcol").is(".dislikes")) {
        //REMOVE DISLIKE IF DISLIKED
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", true); //add class unvoted
        $(".id-" + submissionid).children(".midcol").toggleClass("dislikes", false); //remove class dislikes
        //remove downvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true); //set arrow to downvote
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false); //remove downvoted arrow
        //decrement score dislikes counter
        scoreDislikes--;
        $(".id-" + submissionid).find('.score.dislikes').html(scoreDislikes);
        $(".id-" + submissionid).find('.score.unvoted').html(scoreLikes);
    } else if ($(".id-" + submissionid).children(".midcol").is(".likes")) {
        //ADD DISLIKE IF LIKED
        $(".id-" + submissionid).children(".midcol").toggleClass("likes", false); //remove class likes
        $(".id-" + submissionid).children(".midcol").toggleClass("dislikes", true); //add class dislikes
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", false); //remove class unvoted
        //remove upvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true); //set upvoted arrow to upvote
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false); //remove upvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true); //add downvoted arrow
        //increment score dislikes counter
        scoreDislikes++;
        scoreLikes--;
        $(".id-" + submissionid).find('.score.dislikes').html(scoreDislikes);
        $(".id-" + submissionid).find('.score.likes').html(scoreLikes);
    }

}

function submitUpVote(messageid) {
    $.ajax({
        type: "POST",
        url: "/vote/" + messageid + "/1"
    });
}

function voteUpComment(commentid) {
    submitCommentUpVote(commentid);

    // get current score
    var scoreLikes = +($(".id-" + commentid).find('.post_upvotes').filter(":first").html());
    var scoreDislikes = $(".id-" + commentid).find('.post_downvotes').filter(":first").html();

    // ADD LIKE IF UNVOTED
    if ($(".id-" + commentid).children(".midcol").is(".unvoted")) {
        $(".id-" + commentid).children(".midcol").toggleClass("likes", true); //add class likes
        $(".id-" + commentid).children(".midcol").toggleClass("unvoted", false); //remove class unvoted
        // add upvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true); //set upvote arrow to upvoted
        $(".id-" + commentid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvote", false); //remove upvote arrow
        // increment comment points counter and update DOM element
        scoreLikes++;
        $(".id-" + commentid).find('.post_upvotes').filter(":first").html('+' + scoreLikes);
        $(".id-" + commentid).find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
        $(".id-" + commentid).find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
    } else if ($(".id-" + commentid).children(".midcol").is(".likes")) {
        // REMOVE LIKE IF LIKED
        $(".id-" + commentid).children(".midcol").toggleClass("unvoted", true); //add class unvoted
        $(".id-" + commentid).children(".midcol").toggleClass("likes", false); //remove class dislikes
        // remove upvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true); //set arrow to upvote
        $(".id-" + commentid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false); //remove upvoted arrow
        // decrement comment points counter and update DOM element
        scoreLikes--;
        $(".id-" + commentid).find('.post_upvotes').filter(":first").html('+' + scoreLikes);
        $(".id-" + commentid).find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
        $(".id-" + commentid).find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
    } else if ($(".id-" + commentid).children(".midcol").is(".dislikes")) {
        // ADD LIKE IF DISLIKED
        $(".id-" + commentid).children(".midcol").toggleClass("dislikes", false); //remove class dislikes
        $(".id-" + commentid).children(".midcol").toggleClass("likes", true); //add class likes
        $(".id-" + commentid).children(".midcol").toggleClass("unvoted", false); //remove class unvoted
        // remove downvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true); //set downvoted arrow to downvote
        $(".id-" + commentid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false); //remove downvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true); //add upvoted arrow
        // increment/decrement comment points counters and update DOM element
        scoreLikes++;
        scoreDislikes++; // this will always be a negative number        
        $(".id-" + commentid).find('.post_upvotes').filter(":first").html('+' + scoreLikes);
        $(".id-" + commentid).find('.post_downvotes').filter(":first").html(scoreDislikes);
        $(".id-" + commentid).find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
        $(".id-" + commentid).find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
    }
}

function voteDownComment(commentid) {
    submitCommentDownVote(commentid);

    // get current score
    var scoreLikes = +($(".id-" + commentid).find('.post_upvotes').filter(":first").html());
    var scoreDislikes = $(".id-" + commentid).find('.post_downvotes').filter(":first").html();

    // ADD DISLIKE IF UNVOTED
    if ($(".id-" + commentid).children(".midcol").is(".unvoted")) {
        $(".id-" + commentid).children(".midcol").toggleClass("dislikes", true); //add class dislikes
        $(".id-" + commentid).children(".midcol").toggleClass("unvoted", false); //remove class unvoted
        // add downvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true); //set downvote arrow to downvoted
        $(".id-" + commentid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvote", false); //remove downvote arrow
        // increment comment points counter and update DOM element        
        scoreDislikes--;
        $(".id-" + commentid).find('.post_downvotes').filter(":first").html(scoreDislikes);
        $(".id-" + commentid).find('.score.unvoted').filter(":first").html((scoreLikes + scoreDislikes) + " points");
        $(".id-" + commentid).find('.score.onlycollapsed').filter(":first").html((scoreLikes + scoreDislikes) + " points");
    } else if ($(".id-" + commentid).children(".midcol").is(".dislikes")) {
        // REMOVE DISLIKE IF DISLIKED
        $(".id-" + commentid).children(".midcol").toggleClass("unvoted", true); //add class unvoted
        $(".id-" + commentid).children(".midcol").toggleClass("dislikes", false); //remove class dislikes
        // remove downvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true); //set arrow to downvote
        $(".id-" + commentid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false); //remove downvoted arrow
        // decrement comment points counter and update DOM element
        scoreDislikes++;
        $(".id-" + commentid).find('.post_downvotes').filter(":first").html('-' + scoreDislikes);
        $(".id-" + commentid).find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
        $(".id-" + commentid).find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
    } else if ($(".id-" + commentid).children(".midcol").is(".likes")) {
        // ADD DISLIKE IF LIKED
        $(".id-" + commentid).children(".midcol").toggleClass("likes", false); //remove class likes
        $(".id-" + commentid).children(".midcol").toggleClass("dislikes", true); //add class dislikes
        $(".id-" + commentid).children(".midcol").toggleClass("unvoted", false); //remove class unvoted
        // remove upvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true); //set upvoted arrow to upvote
        $(".id-" + commentid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false); //remove upvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true); //add downvoted arrow
        // increment/decrement comment points counters and update DOM element
        scoreLikes--;
        scoreDislikes--; // this will always be a negative number
        $(".id-" + commentid).find('.post_upvotes').filter(":first").html('+' + scoreLikes);
        $(".id-" + commentid).find('.post_downvotes').filter(":first").html(scoreDislikes);
        var score = 0;
        if (scoreDislikes < 0) {
            score = scoreLikes + scoreDislikes;
        } else {
            score = scoreLikes - scoreDislikes;
        }
        $(".id-" + commentid).find('.score.unvoted').filter(":first").html(score + " points");
        $(".id-" + commentid).find('.score.onlycollapsed').filter(":first").html(score + " points");
    }
}

function submitCommentUpVote(commentid) {
    $.ajax({
        type: "POST",
        url: "/votecomment/" + commentid + "/1"
    });
}

function submitDownVote(messageid) {
    $.ajax({
        type: "POST",
        url: "/vote/" + messageid + "/-1"
    });
}

function submitCommentDownVote(commentid) {
    $.ajax({
        type: "POST",
        url: "/votecomment/" + commentid + "/-1"
    });
}

// append a comment reply form to calling area while preventing multiple appends
function reply(parentcommentid, messageid) {
    //exit function if the form is already being shown
    if ($("#commentreplyform-" + parentcommentid).exists()) {
        return;
    }

    var token = $("input[name='__RequestVerificationToken']").val();

    var replyform = $.get(
        "/ajaxhelpers/commentreplyform/" + parentcommentid + "/" + messageid,
        null,
        function (data) {
            $("#" + parentcommentid).append(data);
            //Focus the cursor on the comment reply form textarea, to prevent unnecessary use of the tab key
            $('#commentreplyform-' + parentcommentid).find('#CommentContent').focus();
        }
     );

    var form = $('#commentreplyform-' + parentcommentid)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);
}

// append a private message reply form to calling area
function replyprivatemessage(parentprivatemessageid, recipient, subject) {
    // exit function if the form is already being shown
    if ($("#privatemessagereplyform-" + parentprivatemessageid).exists()) {
        return;
    }

    var token = $("input[name='__RequestVerificationToken']").val();

    var replyform = $.get(
        "/ajaxhelpers/privatemessagereplyform/" + parentprivatemessageid + "?recipient=" + recipient + "&subject=" + subject,
        null,
        function (data) {
            $("#messageContainer-" + parentprivatemessageid).append(data);
            //Focus the cursor on the private message reply form textarea, to prevent unnecessary use of the tab key
            $('#privatemessagereplyform-' + parentprivatemessageid).find('#Body').focus();
        }
     );

    var form = $('#privatemessagereplyform-' + parentprivatemessageid)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);

    // TODO
    // showRecaptcha('recaptchaContainer');
}

// append a comment reply form to calling area (used in comment reply notification view)
function replyToCommentNotification(commentId, submissionId) {
    // exit function if the form is already being shown
    if ($("#commentreplyform-" + commentId).exists()) {
        return;
    }

    var token = $("input[name='__RequestVerificationToken']").val();

    var replyform = $.get(
        "/ajaxhelpers/commentreplyform/" + commentId + "/" + submissionId,
        null,
        function (data) {
            $("#commentContainer-" + commentId).append(data);
            //Focus the cursor on the comment reply form textarea, to prevent unnecessary use of the tab key
            $('#commentreplyform-' + commentId).find('#CommentContent').focus();
        }
     );

    var form = $('#commentreplyform-' + commentId)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);
}

// post comment reply form through ajax
function postCommentReplyAjax(senderButton, messageId, userName, parentcommentid) {
    var $form = $(senderButton).parents('form');
    $form.find("#errorMessage").toggle(false);

    if ($form.find("#CommentContent").val().length > 0) {
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
                $form.find("#errorMessage").html("You are doing that too fast. Please wait 2 minutes before trying again.");
                $form.find("#errorMessage").toggle(true);
            },

            success: function (response) {
                // remove reply form
                removereplyform(parentcommentid);

                // load rendered comment that was just posted and append it
                var replyresult = $.get(
                    "/ajaxhelpers/singlesubmissioncomment/" + messageId + "/" + userName,
                    null,
                    function (data) {
                        $(".id-" + parentcommentid).append(data);

                        // TODO: use prepend or append after element entry unvoted
                        // $('#div_' + element_id)[0].scrollIntoView(true);

                        //notify UI framework of DOM insertion async
                        window.setTimeout(function () { UI.Notifications.raise('DOM', $('.id-' + parentcommentid).last('div')); });

                    }
                 );
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

    if ($form.find("#CommentContent").val().length > 0) {
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
                $form.find("#errorMessage").html("You are doing that too fast. Please wait 2 minutes before trying again.");
                $form.find("#errorMessage").toggle(true);
            },

            success: function (response) {
                // load rendered comment that was just posted
                var replyresult = $.get(
                    "/ajaxhelpers/singlesubmissioncomment/" + messageId + "/" + userName,
                    null,
                    function (data) {
                        // append rendered comment
                        $(".sitetable.nestedlisting").prepend(data);

                        // reset submit button
                        $form.find("#submitbutton").val("Submit comment");
                        $form.find("#submitbutton").prop('disabled', false);

                        // reset textbox
                        $form.find("#CommentContent").val("");

                        //notify UI framework of DOM insertion async
                        window.setTimeout(function () { UI.Notifications.raise('DOM', $('.sitetable.nestedlisting').first()); });

                        // TODO: scroll to the newly posted comment
                        // $('#div_' + element_id)[0].scrollIntoView(true);
                    }
                 );
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
                $form.find("#errorMessage").html("You are doing that too fast. Please wait 2 minutes before trying again.");
                $form.find("#errorMessage").toggle(true);
            },

            success: function (response) {
                // remove reply form 
                removereplyform(parentprivatemessageid);
                // change reply button to "reply sent"                
                $("#messageContainer-" + parentprivatemessageid).find("#replyPrivateMessage").html("Reply sent.");
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
    $("#commenteditform-" + parentcommentid).find("#CommentContent").focus();

    var form = $("#commenteditform-" + parentcommentid)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);
}

// append a submission edit form to calling area while preventing multiple appends
function editsubmission(submissionid) {

    //hide original text    
    $("#submissionid-" + submissionid).find('.usertext-body').toggle(1);

    //show edit form
    $("#submissionid-" + submissionid).find('.usertext-edit').toggle(1);

    var form = $('#submissioneditform-' + submissionid)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);
}

// remove submission edit form for given submission id and replace it with original content
function removesubmissioneditform(submissionid) {
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
    $("#" + commentid).parent().parent().find('.child').toggle(1);

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
    $("#" + commentid).parent().parent().find('.child').toggle(1);

    return (false);
}

// submit edited comment and replace the old one with formatted response received by server
function editcommentsubmit(commentid) {
    var commentcontent = $("#" + commentid).find('.form-control').val();
    var commentobject = { "Id": commentid, "CommentContent": commentcontent };

    $.ajax({
        type: "POST",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(commentobject),
        url: "/editcomment",
        datatype: "json",
        success: function (data) {
            $("#" + commentid).find('.md').html(data.response);
            //notify UI framework of DOM insertion async
            window.setTimeout(function () { UI.Notifications.raise('DOM', $('#' + commentid)); });

        }
    });

    removeeditform(commentid);
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

    $(obj).toggleClass("collapsed");
    $(obj).toggleClass("expanded");

    //fetch message content and append under class md
    var messageContent = $.get(
        "/ajaxhelpers/messagecontent/" + messageId,
        null,
        function (data) {
            $(obj).parent().find(".expando").find(".md").html(data);
            window.setTimeout(function () { UI.Notifications.raise('DOM', $(obj).parent().find(".expando")); });
        }
     );
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
function applyflair(messageId, flairId, flairLabel, flairCssClass) {
    $.ajax({
        type: "POST",
        url: "/submissions/applylinkflair/" + messageId + "/" + flairId,
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

    var uri = $("#MessageContent").val();

    // request a url title from title service
    var title = $.get(
        "/ajaxhelpers/titlefromuri?uri=" + uri,
        null,
        function (data) {
            $("#Linkdescription").val(data);
            $("#suggest-title").text("Enter the URL above, then click here to suggest a title");
            $("#suggest-title").on('click', suggestTitle);
        }).fail(function () {
            $("#Linkdescription").val("We were unable to suggest a title.");
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
        $(previewArea).find("#submission-preview-area-container").html("Please enter a message in order to get a preview.");
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
function loadMoreSetItems(obj, setId) {
    $(obj).html("Sit tight...");

    // try to see if this request is a subsequent request
    var currentPage = $("#set-" + setId + "-page").html();
    if (currentPage == null) {
        currentPage = 1;
    } else {
        currentPage++;
    }

    $.ajax({
        url: "/set/" + setId + "/" + currentPage + "/",
        success: function (data) {
            $("#set-" + setId + "-page").remove();
            $(obj).before(data);
            $(obj).html("load more &#9660;");
        },
        error: function () {
            {
                $(obj).html("That's it. There was nothing else to show.");
            }
        }
    });
}

// a function that toggles the visibility of the comment/submission source textarea
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
function loadMoreComments(obj, submissionId) {
    $(obj).html("Sit tight...");

    // try to see if this request is a subsequent request
    var currentPage = $("#comments-" + submissionId + "-page").html();
    if (currentPage == null) {
        currentPage = 1;
    } else {
        currentPage++;
    }

    $.ajax({
        url: "/comments/" + submissionId + "/" + currentPage + "/",
        success: function (data) {
            $("#comments-" + submissionId + "-page").remove();
            $(obj).before(data);
            $(obj).html("load more &#9660;");
        },
        error: function () {
            $(obj).html("That's it. There was nothing else to show. Phew. This was hard.");
        }
    });
}