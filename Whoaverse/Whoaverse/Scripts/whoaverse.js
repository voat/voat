/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
 */

function click_voting() {
    $(this).toggleClass("arrow upmod login-required")
}

function mustLogin() {
    $('#mustbeloggedinModal').modal();        
}

function voteUpSubmission(submissionid) {
    //DEBUG alert('Received model.id in voteUpSubmission: ' + submissionid);

    submitUpVote(submissionid);

    //ADD LIKE IF UNVOTED
    if ($(".id-" + submissionid).children(".midcol").is(".unvoted")) {
        $(".id-" + submissionid).children(".midcol").toggleClass("likes", true) //add class likes
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", false) //remove class unvoted
        //add upvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true) //set upvote arrow to upvoted
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvote", false) //remove upvote arrow
    } else if ($(".id-" + submissionid).children(".midcol").is(".likes")) {
        //REMOVE LIKE IF LIKED
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", true) //add class unvoted
        $(".id-" + submissionid).children(".midcol").toggleClass("likes", false) //remove class dislikes
        //remove upvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true) //set arrow to upvote
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false) //remove upvoted arrow
    } else if ($(".id-" + submissionid).children(".midcol").is(".dislikes")) {
        //ADD LIKE IF DISLIKED
        $(".id-" + submissionid).children(".midcol").toggleClass("dislikes", false) //remove class dislikes
        $(".id-" + submissionid).children(".midcol").toggleClass("likes", true) //add class likes
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", false) //remove class unvoted
        //remove downvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true) //set downvoted arrow to downvote
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false) //remove downvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvote").toggleClass("arrow-upvoted", true) //add upvoted arrow
    }

}

function voteDownSubmission(submissionid) {
    //DEBUG alert('Received model.id in voteDownSubmission: ' + submissionid);
    submitDownVote(submissionid);

    //ADD DISLIKE IF UNVOTED
    if ($(".id-" + submissionid).children(".midcol").is(".unvoted"))
    {
        $(".id-" + submissionid).children(".midcol").toggleClass("dislikes", true) //add class dislikes
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", false) //remove class unvoted
        //add downvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true) //set downvote arrow to downvoted
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvote", false) //remove downvote arrow
    } else if ($(".id-" + submissionid).children(".midcol").is(".dislikes")) {
        //REMOVE DISLIKE IF DISLIKED
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", true) //add class unvoted
        $(".id-" + submissionid).children(".midcol").toggleClass("dislikes", false) //remove class dislikes
        //remove downvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvote", true) //set arrow to downvote
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvoted").toggleClass("arrow-downvoted", false) //remove downvoted arrow
    } else if ($(".id-" + submissionid).children(".midcol").is(".likes")) {
        //ADD DISLIKE IF LIKED
        $(".id-" + submissionid).children(".midcol").toggleClass("likes", false) //remove class likes
        $(".id-" + submissionid).children(".midcol").toggleClass("dislikes", true) //add class dislikes
        $(".id-" + submissionid).children(".midcol").toggleClass("unvoted", false) //remove class unvoted
        //remove upvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvote", true) //set upvoted arrow to upvote
        $(".id-" + submissionid).children(".midcol").children(".arrow-upvoted").toggleClass("arrow-upvoted", false) //remove upvoted arrow
        $(".id-" + submissionid).children(".midcol").children(".arrow-downvote").toggleClass("arrow-downvoted", true) //add downvoted arrow
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

function submitDownVote(messageid) {
    //DEBUG
    //alert('Now entered JS function submitDownVote');

    $.ajax({
        type: "POST",
        url: "/vote/" + messageid + "/-1"
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

//prepare auth tokens
$(document).ready(function () {
    securityToken = $('[name=__RequestVerificationToken]').val();
    $('body').bind('ajaxSend', function (elm, xhr, s) {
        if (s.type == 'POST' && typeof securityToken != 'undefined') {
            if (s.data.length > 0) {
                s.data += "&__RequestVerificationToken=" + encodeURIComponent(securityToken);
            }
            else {
                s.data = "__RequestVerificationToken=" + encodeURIComponent(securityToken);
            }
        }
    });
});

//append a comment reply form to calling area while preventing multiple appends
function reply(parentcommentid, messageid) {
    var token = $("input[name='__RequestVerificationToken']").val();

    var replyform = $("<div id='replyform-"
        + parentcommentid
        + "'>"
        + "<form id='commentreplyform-" + parentcommentid + "' novalidate='novalidate' action='/submitcomment' method='post'>"
        + "<input name='__RequestVerificationToken' value='" + token + "' type='hidden'>"
        + "<input id='ParentId' name='ParentId' value='" + parentcommentid + "' type='hidden'>"
        + "<input id='MessageId' name='MessageId' value='" + messageid + "' type='hidden'>"
        + "<div class='row'>"
        + "<div class='col-md-4'>"
        + "<textarea class='form-control' cols='20' id='CommentContent' name='CommentContent' data-val-required='Comment text is required. Please fill this field.' data-val='true' rows='3'></textarea>"
        + "<span class='field-validation-valid' data-valmsg-for='CommentContent' data-valmsg-replace='true'></span>"
        + "</div></div><br><input value='Submit' class='btn-whoaverse' type='submit'></form>"
        + "<div class='validation-summary-valid' data-valmsg-summary='true'><ul><li style='display:none'></li></ul></div>"
        + "</div>");
    
    $('#replyform-' + parentcommentid).remove();
    $("#" + parentcommentid).append(replyform);

    var form = $('#commentreplyform-' + parentcommentid)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);    
}

//post the comment form
var submitForm = function (parentcommentid, messageid, commentcontent) {
    $.ajax({
        url: '/submitcomment',
        type: 'POST',
        data: {
            MessageId: messageid,
            CommentContent: commentcontent,
            ParentId: parentcommentid
        },
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            alert(data.success);
        },
        error: function () {
            alert("error");
        }
    });
}