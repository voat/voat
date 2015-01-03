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

/*
 * This code is bad. Bad code is bad. Bad code! Okay...
 * I am a beginner when it comes to JavaScript and jQuery, so please, feel free to refactor this as much as you can!
 * - Atko
 */

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
    function openSubMenu() { $(this).find('ul').css('visibility', 'visible'); };
    function closeSubMenu() { $(this).find('ul').css('visibility', 'hidden'); };

    $('#Subverse').autocomplete(
        {
            source: '/ajaxhelpers/autocompletesubversename'
        });
});

function click_voting() {
    $(this).toggleClass("arrow upmod login-required")
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
    $('#firsttimevisitorwelcome').modal();
}

function voteUpSubmission(submissionid) {
    //DEBUG alert('Received model.id in voteUpSubmission: ' + submissionid);

    submitUpVote(submissionid);

    var scoreUnvoted = +($(".id-" + submissionid).find('.score.unvoted').html());
    var scoreLikes = +($(".id-" + submissionid).find('.score.likes').html());
    var scoreDislikes = +($(".id-" + submissionid).find('.score.dislikes').html());

    //ADD LIKE IF UNVOTED
    if ($(".id-" + submissionid).children(".midcol").is(".unvoted")) {
        $(".id-" + submissionid).children(".midcol").toggleClass("likes", true) //add class likes
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", false) //remove class unvoted
        //add upvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true) //set upvote arrow to upvoted
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvote", false) //remove upvote arrow
        //increment score likes counter        
        scoreLikes++;
        $(".id-" + submissionid).find('.score.likes').html(scoreLikes);
    } else if ($(".id-" + submissionid).children(".midcol").is(".likes")) {
        //REMOVE LIKE IF LIKED
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", true) //add class unvoted
        $(".id-" + submissionid).children(".midcol").toggleClass("likes", false) //remove class dislikes
        //remove upvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true) //set arrow to upvote
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false) //remove upvoted arrow
        //decrement score likes counter
        scoreLikes--;
        $(".id-" + submissionid).find('.score.likes').html(scoreLikes);
        $(".id-" + submissionid).find('.score.unvoted').html(scoreLikes);
    } else if ($(".id-" + submissionid).children(".midcol").is(".dislikes")) {
        //ADD LIKE IF DISLIKED
        $(".id-" + submissionid).children(".midcol").toggleClass("dislikes", false) //remove class dislikes
        $(".id-" + submissionid).children(".midcol").toggleClass("likes", true) //add class likes
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", false) //remove class unvoted        
        //remove downvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true) //set downvoted arrow to downvote
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false) //remove downvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true) //add upvoted arrow
        //increment score dislikes counter
        scoreDislikes--;
        scoreLikes++;
        $(".id-" + submissionid).find('.score.dislikes').html(scoreDislikes);
        $(".id-" + submissionid).find('.score.likes').html(scoreLikes);
    }

}

function voteDownSubmission(submissionid) {
    //DEBUG alert('Received model.id in voteDownSubmission: ' + submissionid);
    submitDownVote(submissionid);

    var scoreDislikes = +($(".id-" + submissionid).find('.score.dislikes').html());
    var scoreLikes = +($(".id-" + submissionid).find('.score.likes').html());
    var scoreUnvoted = +($(".id-" + submissionid).find('.score.unvoted').html());

    //ADD DISLIKE IF UNVOTED
    if ($(".id-" + submissionid).children(".midcol").is(".unvoted")) {
        $(".id-" + submissionid).children(".midcol").toggleClass("dislikes", true) //add class dislikes
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", false) //remove class unvoted
        //add downvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true) //set downvote arrow to downvoted
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvote", false) //remove downvote arrow
        //increment score dislikes counter
        scoreDislikes++;
        $(".id-" + submissionid).find('.score.dislikes').html(scoreDislikes);
    } else if ($(".id-" + submissionid).children(".midcol").is(".dislikes")) {
        //REMOVE DISLIKE IF DISLIKED
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", true) //add class unvoted
        $(".id-" + submissionid).children(".midcol").toggleClass("dislikes", false) //remove class dislikes
        //remove downvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true) //set arrow to downvote
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false) //remove downvoted arrow
        //decrement score dislikes counter
        scoreDislikes--;
        $(".id-" + submissionid).find('.score.dislikes').html(scoreDislikes);
        $(".id-" + submissionid).find('.score.unvoted').html(scoreLikes);
    } else if ($(".id-" + submissionid).children(".midcol").is(".likes")) {
        //ADD DISLIKE IF LIKED
        $(".id-" + submissionid).children(".midcol").toggleClass("likes", false) //remove class likes
        $(".id-" + submissionid).children(".midcol").toggleClass("dislikes", true) //add class dislikes
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", false) //remove class unvoted
        //remove upvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true) //set upvoted arrow to upvote
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false) //remove upvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true) //add downvoted arrow
        //increment score dislikes counter
        scoreDislikes++;
        scoreLikes--;
        $(".id-" + submissionid).find('.score.dislikes').html(scoreDislikes);
        $(".id-" + submissionid).find('.score.likes').html(scoreLikes);
    }

}

function submitUpVote(messageid) {
    //DEBUG
    //alert('Now entered JS function submitUpvote');

    $.ajax({
        type: "POST",
        url: "/vote/" + messageid + "/1"
        //success: function () {
        //    alert('Voting was sucessful!');
        //},
        //error: function () {
        //    alert('Something went wrong.');
        //},
        //complete: function () {
        //    alert('Ajax call completed.');
        //}
    });
}

function voteUpComment(commentid) {
    submitCommentUpVote(commentid);

    // get current score
    var scoreLikes = +($(".id-" + commentid).find('.post_upvotes').filter(":first").html());
    var scoreDislikes = $(".id-" + commentid).find('.post_downvotes').filter(":first").html();
    var scoreUnvoted = $(".id-" + commentid).find('.score.unvoted').filter(":first").html();

    // ADD LIKE IF UNVOTED
    if ($(".id-" + commentid).children(".midcol").is(".unvoted")) {
        $(".id-" + commentid).children(".midcol").toggleClass("likes", true) //add class likes
        $(".id-" + commentid).children(".midcol").toggleClass("unvoted", false) //remove class unvoted
        // add upvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true) //set upvote arrow to upvoted
        $(".id-" + commentid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvote", false) //remove upvote arrow
        // increment comment points counter and update DOM element
        scoreLikes++;
        $(".id-" + commentid).find('.post_upvotes').filter(":first").html('+' + scoreLikes);
        $(".id-" + commentid).find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
        $(".id-" + commentid).find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
    } else if ($(".id-" + commentid).children(".midcol").is(".likes")) {
        // REMOVE LIKE IF LIKED
        $(".id-" + commentid).children(".midcol").toggleClass("unvoted", true) //add class unvoted
        $(".id-" + commentid).children(".midcol").toggleClass("likes", false) //remove class dislikes
        // remove upvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true) //set arrow to upvote
        $(".id-" + commentid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false) //remove upvoted arrow
        // decrement comment points counter and update DOM element
        scoreLikes--;
        $(".id-" + commentid).find('.post_upvotes').filter(":first").html('+' + scoreLikes);
        $(".id-" + commentid).find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
        $(".id-" + commentid).find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
    } else if ($(".id-" + commentid).children(".midcol").is(".dislikes")) {
        // ADD LIKE IF DISLIKED
        $(".id-" + commentid).children(".midcol").toggleClass("dislikes", false) //remove class dislikes
        $(".id-" + commentid).children(".midcol").toggleClass("likes", true) //add class likes
        $(".id-" + commentid).children(".midcol").toggleClass("unvoted", false) //remove class unvoted
        // remove downvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true) //set downvoted arrow to downvote
        $(".id-" + commentid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false) //remove downvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true) //add upvoted arrow
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
    var scoreUnvoted = $(".id-" + commentid).find('.score.unvoted').filter(":first").html();

    // ADD DISLIKE IF UNVOTED
    if ($(".id-" + commentid).children(".midcol").is(".unvoted")) {
        $(".id-" + commentid).children(".midcol").toggleClass("dislikes", true) //add class dislikes
        $(".id-" + commentid).children(".midcol").toggleClass("unvoted", false) //remove class unvoted
        // add downvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true) //set downvote arrow to downvoted
        $(".id-" + commentid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvote", false) //remove downvote arrow
        // increment comment points counter and update DOM element        
        scoreDislikes--;
        $(".id-" + commentid).find('.post_downvotes').filter(":first").html(scoreDislikes);
        $(".id-" + commentid).find('.score.unvoted').filter(":first").html((scoreLikes + scoreDislikes) + " points");
        $(".id-" + commentid).find('.score.onlycollapsed').filter(":first").html((scoreLikes + scoreDislikes) + " points");
    } else if ($(".id-" + commentid).children(".midcol").is(".dislikes")) {
        // REMOVE DISLIKE IF DISLIKED
        $(".id-" + commentid).children(".midcol").toggleClass("unvoted", true) //add class unvoted
        $(".id-" + commentid).children(".midcol").toggleClass("dislikes", false) //remove class dislikes
        // remove downvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true) //set arrow to downvote
        $(".id-" + commentid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false) //remove downvoted arrow
        // decrement comment points counter and update DOM element
        scoreDislikes++;
        $(".id-" + commentid).find('.post_downvotes').filter(":first").html('-' + scoreDislikes);
        $(".id-" + commentid).find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
        $(".id-" + commentid).find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
    } else if ($(".id-" + commentid).children(".midcol").is(".likes")) {
        // ADD DISLIKE IF LIKED
        $(".id-" + commentid).children(".midcol").toggleClass("likes", false) //remove class likes
        $(".id-" + commentid).children(".midcol").toggleClass("dislikes", true) //add class dislikes
        $(".id-" + commentid).children(".midcol").toggleClass("unvoted", false) //remove class unvoted
        // remove upvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true) //set upvoted arrow to upvote
        $(".id-" + commentid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false) //remove upvoted arrow
        $(".id-" + commentid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true) //add downvoted arrow
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
            $("#" + parentcommentid).append(data)
        }
     );

    var form = $('#commentreplyform-' + parentcommentid)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);
}

// append a private message reply form to calling area
function replyprivatemessage(parentprivatemessageid, recipient, subject) {
    //exit function if the form is already being shown
    if ($("#privatemessagereplyform-" + parentprivatemessageid).exists()) {
        return;
    }

    var token = $("input[name='__RequestVerificationToken']").val();

    var replyform = $.get(
        "/ajaxhelpers/privatemessagereplyform/" + parentprivatemessageid + "?recipient=" + recipient + "&subject=" + subject,
        null,
        function (data) {
            $("#messageContainer-" + parentprivatemessageid).append(data)
        }
     );

    var form = $('#privatemessagereplyform-' + parentprivatemessageid)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);

    // TODO
    // showRecaptcha('recaptchaContainer');
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
                    "/ajaxhelpers/singlesubmissioncomment/"+messageId+"/"+userName,
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

    //hide original text comment
    $("#" + parentcommentid).find('.usertext-body').toggle(1);

    //show edit form
    $("#" + parentcommentid).find('.usertext-edit').toggle(1);

    var form = $('#commenteditform-' + parentcommentid)
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
            window.setTimeout(function () { UI.Notifications.raise('DOM', $("#submissionid-" + submissionid)) });
        }
    });

    removesubmissioneditform(submissionid);
    return false;
}

// remove reply form for given parent id
function removereplyform(parentcommentid) {
    $('#replyform-' + parentcommentid).remove();
}

// remove edit form for given parent id and replace it with original comment
function removeeditform(parentcommentid) {
    $("#" + parentcommentid).find('.usertext-body').toggle(1);
    $("#" + parentcommentid).find('.usertext-edit').toggle(1);
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
    var commentobject = { "CommentId": commentid, "CommentContent": commentcontent };

    $.ajax({
        type: "POST",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(commentobject),
        url: "/editcomment",
        datatype: "json",
        success: function (data) {
            $("#" + commentid).find('.md').html(data.response);
            //notify UI framework of DOM insertion async
            window.setTimeout(function () { UI.Notifications.raise('DOM', $('#' + commentid)) });

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
}

// subscribe button
function subscribe(obj, subverseName) {
    $(obj).attr("onclick", "unsubscribe(this)");
    $(obj).html("unsubscribe");

    // call the actual subscribe API
    submitSubscribeRequest(subverseName);
}

// unsubscribe button
function unsubscribe(obj, subverseName) {
    $(obj).attr("onclick", "subscribe(this)");
    $(obj).html("subscribe");

    // call the actual unsubscribe API
    submitUnSubscribeRequest(subverseName);
}

function submitSubscribeRequest(subverseName) {
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

function submitUnSubscribeRequest(subverseName) {
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

// a function to load content of a self post and append it to calling object
function loadSelfText(obj, messageId) {

    $(obj).toggleClass("collapsed");
    $(obj).toggleClass("expanded");

    //fetch message content and append under class md
    var messageContent = $.get(
        "/ajaxhelpers/messagecontent/" + messageId,
        null,
        function (data) {
            $(obj).parent().find(".expando").nextAll().find(".md").html(data);
            window.setTimeout(function () { UI.Notifications.raise('DOM', $(obj).parent().find(".expando").nextAll()); });
        }
     );

    //toggle message content display
    //note: the nextnextnextnext thing is ugly, feel free to write a cleaner solution. Thanks!
    $(obj).parent().find(".expando").next().next().next().toggle();
}

// a function to embed a video via expando
function loadVideoPlayer(obj, messageId) {

    $(obj).toggleClass("collapsed");
    $(obj).toggleClass("expanded");

    // fetch message content and append under class md
    var messageContent = $.get(
        "/ajaxhelpers/videoplayer/" + messageId,
        null,
        function (data) {
            $(obj).parent().find(".expando").nextAll().find(".videoplayer").html(data);
            window.setTimeout(function() {
                UI.Notifications.raise('iFrameLoaded', $(obj).parent().find(".expando").nextAll().find(".videoplayer"));
            });
        }
     );

    //note: the nextnextnextnext thing is ugly, feel free to write a cleaner solution. Thanks!
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

// a function to suggest a title for given Uri
function suggestTitle() {
    var uri = $("#MessageContent").val();

    // request a title from uri to title service
    var title = $.get(
        "/ajaxhelpers/titlefromuri?uri=" + uri,
        null,
        function (data) {
            $("#Linkdescription").val(data)
        }).fail(function () {
            $("#Linkdescription").val("We were unable to suggest a title.")
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

// a function to display a preview of submission without submitting it
// I am pretty surprised at how my javascript functions have evolved over time, this one may actually be quite alright :)
function showSubmissionPreview(senderButton) {
    var rawSubmissionContent = $("#MessageContent").val();
    if (!rawSubmissionContent.length>0) {
        $("#submission-preview-area-container").html("Please enter a message in order to get a preview.");
        $("#submission-preview-area").show();
        return false;
    }

    $(senderButton).val("Please wait");

    // get rendered submission and show it
    var submissionModel = {
        MessageContent: $("#MessageContent").val()
    }

    $.ajax({
        url: '/ajaxhelpers/rendersubmission/',
        type: 'post',
        dataType: 'html',
        success: function (data) {
            $("#submission-preview-area-container").html(data);
        },
        data: submissionModel
    });

    // show the preview area
    $("#submission-preview-area").show();
    $(senderButton).val("Preview");
    return false;
}

/* Adds left and right markdown tags to each line of the selected text
 * textComponent:		the textarea
 * leftTag:				text to add at the left of each line of the selected text
 * rightTag:			text to add at the right of each line of the selected text
 */
function addTagsToEachSelectedTextLine(textComponent, leftTag, rightTag){
	var selectedText = getSelectionArray(textComponent);
	var selectedTextLines = selectedText[1].split("\n");
	//Add text located before the selected text (which should not have been modified)
	textComponent.value = selectedText[0];
	//Add markdown to each line of the selected text
	for (i = 0; i < selectedTextLines.length; i++){
		textComponent.value += addTagsToText(selectedTextLines[i], leftTag, rightTag) + "\n";
	}
	//Add text located after the selected text (which should not have been modified)
	textComponent.value += selectedText[2];
	//Temporary variable so that I can put the cursor at intended position
	var cursorPos = textComponent.value.length - rightTag.length - selectedText[2].length;
	//Set the focus back on the text component at the intended position
	setSelectionRange(textComponent, cursorPos, cursorPos);
}

/* Adds left and right markdown tags to the selected text
 * textComponent:		the textarea
 * leftTag:				text to add at the left of the selected text
 * rightTag:			text to add at the right of the selected text
 */
function addTagsToSelectedText(textComponent, leftTag, rightTag){
	var selectedText = getSelectionArray(textComponent);
	//Add text located before the selected text (which should not have been modified)
	textComponent.value = selectedText[0];
	//Add markdown to each line of the selected text
	textComponent.value += addTagsToText(selectedText[1], leftTag, rightTag);
	//Add text located after the selected text (which should not have been modified)
	textComponent.value += selectedText[2];
	//Temporary variable so that I can put the cursor at intended position
	var cursorPos = textComponent.value.length - rightTag.length - selectedText[2].length;
	//Set the focus back on the text component at the intended position
	setSelectionRange(textComponent, cursorPos, cursorPos);
}

/* Adds left and right tags to the given text
 * text:				the text that will be manipulated
 * leftTag:				the tag that will be added at the left side of the text
 * rightTag:			the tag that will be added at the right side of the text
 */
function addTagsToText(text, leftTag, rightTag){
	return leftTag + text + rightTag;
}

/* Gets selection array
 * textComponent:		the textarea
 * returns				an array with the following format: [text before selection, selected text, text after selection] if there is any selected text, [entire textarea text,'',''] if there isn't any selected text
 */
function getSelectionArray(textComponent){
	var selectedText = [textComponent.value,'',''];
	//Check if there is any selected text
	if (textComponent.selectionStart != undefined)
	{
		var selectedStartPos = textComponent.selectionStart;
		var selectedEndPos = textComponent.selectionEnd;
		//Get text on the left of the selected text
		selectedText[0] = textComponent.value.substring(0, selectedStartPos)
		//Get selected text
		selectedText[1] = textComponent.value.substring(selectedStartPos, selectedEndPos)
		//Get text on the right of the selected text
		selectedText[2] = textComponent.value.substring(selectedEndPos, textComponent.value.length)
	}
	return selectedText;
}

/* Focus on the text component and select the text between selectionStart and selectionEnd
 * textComponent:		the textarea
 * selectionStart:		start position of the selection
 * selectionEnd:		end position of the selection
 */
function setSelectionRange(textComponent, selectionStart, selectionEnd) {
	if (textComponent.setSelectionRange) {
		textComponent.focus();
		textComponent.setSelectionRange(selectionStart, selectionEnd);
	}
	else if (textComponent.createTextRange) {
		var range = textComponent.createTextRange();
		range.collapse(true);
		range.moveEnd('character', selectionEnd);
		range.moveStart('character', selectionStart);
		range.select();
	}
}

/*
 * Increments the number of columns to create in the table
 * columns:				the element that contains the number of columns that the table will have
 * numColumnsDisplay:	the element that displays the number of columns that the table will have
 */
function incColumns(columns, numColumnsDisplay){
	columns.value++;
	updateNumColumnsDisplay(columns, numColumnsDisplay);
}

/*
 * Decrements the number of columns to create in the table
 * columns:				the element that contains the number of columns that the table will have
 * numColumnsDisplay:	the element that displays the number of columns that the table will have
 */
function decColumns(columns, numColumnsDisplay){
	if (columns.value > 1){
		columns.value--;
	}
	updateNumColumnsDisplay(columns, numColumnsDisplay);
}

/*
 * Updates the number of columns display
 * columns:				the element that contains the number of columns that the table will have
 * numColumnsDisplay:	the element that displays the number of columns that the table will have
 */
function updateNumColumnsDisplay(columns, numColumnsDisplay){
	numColumnsDisplay.innerHTML = columns.value;
}

/*
 * Increments the number of rows to create in the table
 * rows:				the element that contains the number of rows that the table will have
 * numRowsDisplay:		the element that displays the number of rows that the table will have
 */
function incRows(rows, numRowsDisplay){
	rows.value++;
	updateNumRowsDisplay(rows, numRowsDisplay);
}

/*
 * Decrements the number of rows to create in the table
 * rows:				the element that contains the number of rows that the table will have
 * numRowsDisplay:		the element that displays the number of rows that the table will have
 */
function decRows(rows, numRowsDisplay){
	if (rows.value > 1){
		rows.value--;
	}
	updateNumRowsDisplay(rows, numRowsDisplay);
}

/*
 * Updates the number of rows display
 * rows:				the element that contains the number of rows that the table will have
 * numRowsDisplay:		the element that displays the number of rows that the table will have
 */
function updateNumRowsDisplay(rows, numRowsDisplay){
	numRowsDisplay.innerHTML = rows.value;
}

/*
 * Creates the markdown for the table
 * textComponent:		the textarea
 * numColumns:			the number of columns that the table will have
 * numRows:				the number of rows that the table will have
 */
function createTable(textComponent, numColumns, numRows){
	var selectedText = getSelectionArray(textComponent);
	if (numColumns >= 1 && numRows >= 1){
		//Add text located before the selected text (which should not have been modified)
		textComponent.value = selectedText[0];
		//Add table markdown
		//Create first row (columns title)
		for (c = 0; c < numColumns; c++){
			textComponent.value += "Title |"; 
		}
		textComponent.value += "\n";
		for (c = 0; c < numColumns; c++){
			textComponent.value += "--- |"; 
		}
		textComponent.value += "\n";
		//Create next rows (columns content)
		for (r = 0; r < numRows; r++){
			for (c = 0; c < numColumns; c++){
				textComponent.value += "Text |";
			}
			textComponent.value += "\n";
		}
		//Add text located after the selected text (which should not have been modified)
		textComponent.value += selectedText[2];
		//Temporary variable so that I can put the cursor at intended position
		var cursorPos = textComponent.value.length - selectedText[2].length;
		//Set the focus back on the text component at the intended position
		setSelectionRange(textComponent, cursorPos, cursorPos);
	} else {
		alert('Error: Unable to create table! Invalid number of columns/rows.');
	}
}