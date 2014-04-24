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
    $(".id-" + submissionid).fadeIn(100).fadeOut(500).fadeIn(500);

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
    $(".id-" + submissionid).fadeIn(100).fadeOut(500).fadeIn(500);

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

//TODO
function submitVote() {
    // get the form data
    // using jQuery (class, id etc)
    var formData = {
        'name': $('input[name=name]').val(),
        'email': $('input[name=email]').val(),
        'superheroAlias': $('input[name=superheroAlias]').val()
    };

    // process the form
    $.ajax({
        type: 'POST', // define the type of HTTP verb we want to use (POST for our form)
        url: 'process.php', // the url where we want to POST
        data: formData, // our data object
        dataType: 'json' // what type of data do we expect back from the server
    })

}
