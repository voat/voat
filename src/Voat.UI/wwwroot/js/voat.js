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

$(document).ready(function () {
    //THIS VALUE MUST MATCH CONSTANTS.REQUEST_VERIFICATION_HEADER_NAME
    var requestVerificationTokenName = "VoatRequestVerificationToken";

    // activate bootstrap popovers
    $('[data-toggle="popover"]').popover({ trigger: 'hover', 'placement': 'top' });

    // prepare auth tokens
    securityToken = $('[name=' + requestVerificationTokenName+']').val();
    $(document).ajaxSend(function (elm, xhr, s) {
        if (s.type == 'POST' && typeof securityToken != 'undefined') {
            if (s.contentType.toLowerCase().lastIndexOf('application/json', 0) === 0) {
                //json request
                xhr.setRequestHeader(requestVerificationTokenName, securityToken);
            } else {
                //form request
                if (!s.data || s.data.indexOf(requestVerificationTokenName) == -1) {
                    s.data = (s.data && s.data.length > 0 ? s.data + '&' : '') + requestVerificationTokenName + '=' + encodeURIComponent(securityToken);
                    //this will force the data to be re-evaled if none is provided on initiation call
                    xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
                }
            }
        }
    });

    //$('.whoaSubscriptionMenu > li').bind('mouseover', openSubMenu);
    //$('.whoaSubscriptionMenu > li').bind('mouseout', closeSubMenu);
    function openSubMenu() {
        if ($(this).find("ul").css("display") != "block") {
            $(this).find("ul").css("display", "block");
        }
        //$(this).find('ul').css('display', 'block');
    }
    function closeSubMenu() {
        if ($(this).find("ul").css("display") != "none") {
            $(this).find("ul").css("display", "none");
        }
        //$(this).find('ul').css('display', 'none');
    }
    var postingSubverse = '';
    var subverseAutoCompleteSelector = 'input[data-autocomplete-subverse=1]'
    $(subverseAutoCompleteSelector).blur(function () {
        var sub = $(subverseAutoCompleteSelector).val();
        if (sub.length > 0 && postingSubverse != sub) {
            postingSubverse = sub;
            var url = '/ajaxhelpers/autocompletesubversename?exact=true&term=' + sub;
            //set up UI
            $.ajax({
                type: "GET",
                url: url,
                //data: $form.serialize(),
                error: function (xhr, status, error) {
                    var msg = getErrorMessage(error);
                    //TODO: Why is this here? I know I did this but why? WHY!? Please find my why.
                    //alert(msg);
                },
                success: function (response) {
                    if (response && response.length > 0) {
                        var item = response[0];

                        var adultCheckBox = $('#IsAdult');
                        var anonCheckBox = $('#IsAnonymized');
                        var anonHiddenField = $('#AllowAnonymized');
                        var anonDiv = $('#AnonDiv')

                        //anonDiv.hide();
                        anonCheckBox.prop("disabled", true);
                        anonCheckBox.prop("readonly", true);

                        if (item.allowAnonymized) {
                            anonHiddenField.val(item.allowAnonymized);
                        }

                        if (item.isAdult) {
                            if (!adultCheckBox.prop('checked')) {
                                adultCheckBox.prop("checked", true);
                            }
                        }

                        //allows anon content, leave alone
                        if (item.isAnonymized == null) {
                            anonCheckBox.prop("disabled", false);
                            anonCheckBox.prop("readonly", false);
                            anonDiv.show();
                        } else if (item.isAnonymized == false) {
                            anonCheckBox.prop("checked", false);

                        } else if (item.isAnonymized == true) {
                            anonCheckBox.prop("checked", true);
                        }
                    }
                }
            });
        }

    });
    $(subverseAutoCompleteSelector).autocomplete({
        source: '/ajaxhelpers/autocompletesubversename',
        minLength: 2,
        select: function (event, ui) {
            $(subverseAutoCompleteSelector).val(ui.item.name);
            return false;
        },
        focus: function () {
            // prevent value inserted on focus
            return false;
        }
    }).autocomplete("instance")._renderItem = function (ul, item) {
        return $("<li>")
        .append("<div>" + item.name + "</div>")
        .appendTo(ul);
    };

    // drag'n'drop link sharing
    //$(document).on('dragenter', function () {
    //    $('#share-a-link-overlay').show();
    //});

    // prevent spoiler links from opening windows
    $(document).on('click', 'a[href="#s"]', function (e) {
        e.preventDefault();
    });

    //$('#share-a-link-overlay').on('dragleave', function (e) {
    //    if (e.originalEvent.pageX < 10 || e.originalEvent.pageY < 10 || $(window).width() - e.originalEvent.pageX < 10 || $(window).height - e.originalEvent.pageY < 10) {
    //        $("#share-a-link-overlay").hide();
    //    }
    //});

    //$('#share-a-link-overlay').on('dragover', function (e) {
    //    e.stopPropagation();
    //    e.preventDefault();
    //});

    // tooltipster wireup
    wireTooltips();

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
    //register signalr callbacks
    $(function () {
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

                proxy.client.voteChange = function (type, value) {
                    var currentValue = 0;
                    if (type == 2) {
                        // this is a comment vote notification
                        // update CCP display
                        currentValue = $('#ccp').html();
                        currentValue = parseInt(currentValue) + parseInt(value); //Fix concat issue
                        $('#ccp').html(currentValue);
                    } else {
                        // update SCP display
                        currentValue = $('#scp').html();
                        currentValue = parseInt(currentValue) + parseInt(value); //Fix concat issue
                        $('#scp').html(currentValue);
                    }
                };

                // Hub accessed function to append incoming chat message
                proxy.client.appendChatMessage = function (sender, chatMessage, time) {
                    //$("#subverseChatRoom").append('<p><b><a href="/user/' + sender + '">' + sender + '</a></b> (' + time + ' UTC):</p>' + chatMessage);
                    $("#subverseChatRoom").append('<div class="chat-message"><div class="chat-message-head"><p><b><a href="/user/' + sender + '">' + sender + '</a></b> <span class="chat-message-timestamp">(' + time + ')</span>:</p></div><div class="chat-message-body">' + chatMessage + '</div></div>');
                    scrollChatToBottom();
                };
            }
        }
    });
});

//SignalR helper methods to start hub connection, update the page and send messages
function initiateWSConnection() {
    if ($.connection != null) {
        // Start the connection.
        $.connection.hub.start({ transport: 'webSockets' })
            .done(function () {
                //what shall we do? Read a book.
            });
    }
}

function wireTooltips() {
    $('.userinfo:not(.tooltipstered)').tooltipster({
        content: 'Loading user info...',
        contentAsHTML: 'true',
        animation: 'grow',

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
}

//// a function which handles mouse drop events (sharing links by dragging and dropping)
//function dropFunction(event) {
//    event.stopPropagation();
//    event.preventDefault();

//    var droppedData = event.dataTransfer.getData('text/html');

//    var url;
//    if ($(droppedData).children().length > 0) {
//        url = $(droppedData).attr('href');
//    } else {
//        url = $(droppedData).attr('href');
//    }

//    // dropped data did not contain a HREF element, try to see if it has a SRC element instead
//    if (url != null) {
//        window.location.replace("/submit?linkpost=true&url=" + url);
//    } else {
//        url = $(droppedData).attr('src');
//        if (url != null) {
//            window.location.replace("/submit?linkpost=true&url=" + url);
//        }
//    }

//    $("#share-a-link-overlay").hide();
//}

//function click_voting() {
//    $(this).toggleClass("arrow upmod login-required");
//}

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
function voteSubmission(id, voteValue, errorSpan) {

    if (submissionVoteLock == null) {
        submissionVoteLock = new Object();
        voteValue = (voteValue == -1 ? -1 : 1);//standardize bad input

        if (errorSpan) {
            errorSpan.html('');
        }

        //submitUpVote(submissionid);
        $.ajax({
            type: "POST",
            url: "/user/vote/submission/" + id + "/" + voteValue.toString(),
            complete: function () {
                submissionVoteLock = null;
            },
            error: function (data) {
                if (errorSpan) {
                    errorSpan.html("An Error Occurred :(");
                } else {
                    //voting was not registered - show error
                    var submission = $(".submission.id-" + id);
                    var div = submission.children(".entry");
                    div.children('span').remove();
                    div.prepend('<span class="vote-error">An Error Occurred :(</span>');
                }
            },
            success: function (data) {
                var submission = $(".submission.id-" + id);
                //remove error span if present
                submission.children(".entry").children('span').remove();

                if (!data.success) {
                    if (data.message.indexOf('2.2', 0) > 0) {
                        notEnoughCCP();
                    } else if (data.message.indexOf('4.0', 0) > 0 || data.message.indexOf('2.1', 0) > 0) {
                        notEnoughCCPUpVote();
                    }
                    if (errorSpan) {
                        errorSpan.html(data.message);
                        return;
                    } else {
                        var err = submission.children(".entry");
                        err.children('span').remove();
                        err.prepend('<span class="vote-error">' + data.message + '</span>');
                        return;
                    }
                }
                var div = submission.find(".voting-icons");


                var scoreLikes = +(submission.find('.score.likes').html());
                var scoreDislikes = +(submission.find('.score.dislikes').html());
                if (voteValue == 1) {
                    //ADD LIKE IF UNVOTED
                    if (div.is(".unvoted")) {
                        div.toggleClass("likes", true); //add class likes
                        div.toggleClass("unvoted", false); //remove class unvoted
                        //add upvoted arrow
                        div.children(".arrow-upvote").toggleClass("arrow-upvoted", true); //set upvote arrow to upvoted
                        div.children(".arrow-upvote").toggleClass("arrow-upvote", false); //remove upvote arrow
                        //increment score likes counter        
                        scoreLikes++;
                        submission.find('.score.likes').html(scoreLikes);
                    } else if (div.is(".likes")) {
                        //REMOVE LIKE IF LIKED
                        div.toggleClass("unvoted", true); //add class unvoted
                        div.toggleClass("likes", false); //remove class dislikes
                        //remove upvoted arrow
                        div.children(".arrow-upvoted").toggleClass("arrow-upvote", true); //set arrow to upvote
                        div.children(".arrow-upvoted").toggleClass("arrow-upvoted", false); //remove upvoted arrow
                        //decrement score likes counter
                        scoreLikes--;
                        submission.find('.score.likes').html(scoreLikes);
                        submission.find('.score.unvoted').html(scoreLikes);
                    } else if (div.is(".dislikes")) {
                        //ADD LIKE IF DISLIKED
                        div.toggleClass("dislikes", false); //remove class dislikes
                        div.toggleClass("likes", true); //add class likes
                        div.toggleClass("unvoted", false); //remove class unvoted        
                        //remove downvoted arrow
                        div.children(".arrow-downvoted").toggleClass("arrow-downvote", true); //set downvoted arrow to downvote
                        div.children(".arrow-downvoted").toggleClass("arrow-downvoted", false); //remove downvoted arrow
                        div.children(".arrow-upvote").toggleClass("arrow-upvoted", true); //add upvoted arrow
                        //increment score dislikes counter
                        scoreDislikes--;
                        scoreLikes++;
                        submission.find('.score.dislikes').html(scoreDislikes);
                        submission.find('.score.likes').html(scoreLikes);
                    }
                } else {
                    //ADD DISLIKE IF UNVOTED
                    if (div.is(".unvoted")) {
                        div.toggleClass("dislikes", true); //add class dislikes
                        div.toggleClass("unvoted", false); //remove class unvoted
                        //add downvoted arrow
                        div.children(".arrow-downvote").toggleClass("arrow-downvoted", true); //set downvote arrow to downvoted
                        div.children(".arrow-downvote").toggleClass("arrow-downvote", false); //remove downvote arrow
                        //increment score dislikes counter
                        scoreDislikes++;
                        submission.find('.score.dislikes').html(scoreDislikes);
                    } else if (div.is(".dislikes")) {
                        //REMOVE DISLIKE IF DISLIKED
                        div.toggleClass("unvoted", true); //add class unvoted
                        div.toggleClass("dislikes", false); //remove class dislikes
                        //remove downvoted arrow
                        div.children(".arrow-downvoted").toggleClass("arrow-downvote", true); //set arrow to downvote
                        div.children(".arrow-downvoted").toggleClass("arrow-downvoted", false); //remove downvoted arrow
                        //decrement score dislikes counter
                        scoreDislikes--;
                        submission.find('.score.dislikes').html(scoreDislikes);
                        submission.find('.score.unvoted').html(scoreLikes);
                    } else if (div.is(".likes")) {
                        //ADD DISLIKE IF LIKED
                        div.toggleClass("likes", false); //remove class likes
                        div.toggleClass("dislikes", true); //add class dislikes
                        div.toggleClass("unvoted", false); //remove class unvoted
                        //remove upvoted arrow
                        div.children(".arrow-upvoted").toggleClass("arrow-upvote", true); //set upvoted arrow to upvote
                        div.children(".arrow-upvoted").toggleClass("arrow-upvoted", false); //remove upvoted arrow
                        div.children(".arrow-downvote").toggleClass("arrow-downvoted", true); //add downvoted arrow
                        //increment score dislikes counter
                        scoreDislikes++;
                        scoreLikes--;
                        submission.find('.score.dislikes').html(scoreDislikes);
                        submission.find('.score.likes').html(scoreLikes);
                    }
                }

            }
        });
    }


}

var commentVoteLock = null;
function voteComment(id, voteValue) {

    if (commentVoteLock == null) {

        commentVoteLock = new Object();
        voteValue = (voteValue == -1 ? -1 : 1);//standardize bad input

        //submitCommentUpVote(commentid);
        $.ajax({
            type: "POST",
            url: "/user/vote/comment/" + id + "/" + voteValue.toString(),
            complete: function () {
                commentVoteLock = null;
            },
            error: function (data) {
                //voting was not registered - show error
                var comment = $(".comment.id-" + id);
                var div = comment.children(".entry");
                div.children('span').remove();
                div.prepend('<span class="vote-error">An Error Occurred :(</span>');
            },
            success: function (data) {
                //TODO: data object includes vote related json, the below code can use the values this object contains, but not changing right now.
                //alert(data.message);
                var comment = $(".comment.id-" + id);
                //remove error span if present
                comment.children(".entry").children('span').remove();

                if (!data.success) {
                    if (data.message.indexOf('2.2', 0) > 0) {
                        notEnoughCCP();
                    } else if (data.message.indexOf('4.0', 0) > 0 || data.message.indexOf('2.1', 0) > 0) {
                        notEnoughCCPUpVote();
                    }

                    var err = comment.children(".entry");
                    err.children('span').remove();
                    err.prepend('<span class="vote-error">' + data.message + '</span>');
                    return;
                }
                var div = comment.children(".midcol");

                // get current score
                var scoreLikes = +(comment.find('.post_upvotes').filter(":first").html());
                var scoreDislikes = -(comment.find('.post_downvotes').filter(":first").html());

                if (voteValue == 1) {
                    // ADD LIKE IF UNVOTED
                    if (div.is(".unvoted")) {
                        div.toggleClass("likes", true); //add class likes
                        div.toggleClass("unvoted", false); //remove class unvoted
                        // add upvoted arrow
                        div.children(".arrow-upvote").toggleClass("arrow-upvoted", true); //set upvote arrow to upvoted
                        div.children(".arrow-upvote").toggleClass("arrow-upvote", false); //remove upvote arrow
                        // increment comment points counter and update DOM element
                        scoreLikes++;
                    } else if (div.is(".likes")) {
                        // REMOVE LIKE IF LIKED
                        div.toggleClass("likes", false); //remove class dislikes
                        div.toggleClass("unvoted", true); //add class unvoted
                        // remove upvoted arrow
                        div.children(".arrow-upvoted").toggleClass("arrow-upvote", true); //set arrow to upvote
                        div.children(".arrow-upvoted").toggleClass("arrow-upvoted", false); //remove upvoted arrow
                        // decrement comment points counter and update DOM element
                        scoreLikes--;
                    } else if (div.is(".dislikes")) {
                        // ADD LIKE IF DISLIKED
                        div.toggleClass("dislikes", false); //remove class dislikes
                        div.toggleClass("likes", true); //add class likes
                        div.toggleClass("unvoted", false); //remove class unvoted
                        // remove downvoted arrow
                        div.children(".arrow-downvoted").toggleClass("arrow-downvote", true); //set downvoted arrow to downvote
                        div.children(".arrow-downvoted").toggleClass("arrow-downvoted", false); //remove downvoted arrow
                        div.children(".arrow-upvote").toggleClass("arrow-upvoted", true); //add upvoted arrow
                        // increment/decrement comment points counters and update DOM element
                        scoreLikes++;
                        scoreDislikes--;
                        comment.find('.post_downvotes').filter(":first").html('-' + scoreDislikes);
                    }
                    comment.find('.post_upvotes').filter(":first").html('+' + scoreLikes);
                    comment.find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                    comment.find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");

                } else {
                    // ADD DISLIKE IF UNVOTED
                    if (div.is(".unvoted")) {
                        div.toggleClass("dislikes", true); //add class dislikes
                        div.toggleClass("unvoted", false); //remove class unvoted
                        // add downvoted arrow
                        div.children(".arrow-downvote").toggleClass("arrow-downvoted", true); //set downvote arrow to downvoted
                        div.children(".arrow-downvote").toggleClass("arrow-downvote", false); //remove downvote arrow
                        // increment comment points counter and update DOM element        
                        scoreDislikes++;
                    } else if (div.is(".dislikes")) {
                        // REMOVE DISLIKE IF DISLIKED
                        div.toggleClass("unvoted", true); //add class unvoted
                        div.toggleClass("dislikes", false); //remove class dislikes
                        // remove downvoted arrow
                        div.children(".arrow-downvoted").toggleClass("arrow-downvote", true); //set arrow to downvote
                        div.children(".arrow-downvoted").toggleClass("arrow-downvoted", false); //remove downvoted arrow
                        // decrement comment points counter and update DOM element
                        scoreDislikes--;
                    } else if (div.is(".likes")) {
                        // ADD DISLIKE IF LIKED
                        div.toggleClass("likes", false); //remove class likes
                        div.toggleClass("dislikes", true); //add class dislikes
                        div.toggleClass("unvoted", false); //remove class unvoted
                        // remove upvoted arrow
                        div.children(".arrow-upvoted").toggleClass("arrow-upvote", true); //set upvoted arrow to upvote
                        div.children(".arrow-upvoted").toggleClass("arrow-upvoted", false); //remove upvoted arrow
                        div.children(".arrow-downvote").toggleClass("arrow-downvoted", true); //add downvoted arrow
                        // increment/decrement comment points counters and update DOM element
                        scoreLikes--;
                        scoreDislikes++;
                        comment.find('.post_upvotes').filter(":first").html('+' + scoreLikes);
                    }
                    comment.find('.post_downvotes').filter(":first").html('-' + scoreDislikes);
                    comment.find('.score.unvoted').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                    comment.find('.score.onlycollapsed').filter(":first").html((scoreLikes - scoreDislikes) + " points");
                }
            }
        });
    }
}

// append a comment reply form to calling area while preventing multiple appends
var replyCommentFormRequest;
function reply(parentcommentid, messageid) {
    //exit function if the form is already being shown or a request is in progress
    if ($("#commentreplyform-" + parentcommentid).exists() || replyCommentFormRequest) {
        return;
    }

    //var token = $("input[name='__RequestVerificationToken']").val();

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
function messageReplyForm(id) {
    // exit function if the form is already being shown or a request is in progress
    if ($("#privatemessagereplyform-" + id).exists() || replyFormPMRequest) {
        return;
    }

    //var token = $("input[name='__RequestVerificationToken']").val();

    replyFormPMRequest = $.ajax({
        url: "/messages/reply/" + id + "?nocache=" + cachePrevention(),
        success: function (data) {
            $("#messageContainer-" + id).append(data);
            //Focus the cursor on the private message reply form textarea, to prevent unnecessary use of the tab key
            $('#privatemessagereplyform-' + id).find('#Body').focus();
        },
        complete: function () {
            replyFormPMRequest = null;
        }
    });

    var form = $('#privatemessagereplyform-' + id)
            .removeData("validator") /* added by the raw jquery.validate plugin */
            .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

    $.validator.unobtrusive.parse(form);

}

// append a comment reply form to calling area (used in comment reply notification view)
var replyToCommentFormRequest;
function replyToCommentNotification(commentId, submissionId) {
    // exit function if the form is already being shown or a request is in progress
    if ($("#commentreplyform-" + commentId).exists() || replyToCommentFormRequest) {
        return;
    }

    //var token = $("input[name='__RequestVerificationToken']").val();

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

function getErrorObject(arguments) {
    var request = arguments[2];
    if (request.responseJSON) {
        if (request.responseJSON.success === false) {
            return request.responseJSON;
        }
    }
    return null;
}
function getJsonResponse(arguments) {

    //complete
    var request = arguments[0];
    if (request !== undefined && request.responseJSON !== undefined) {
        return request.responseJSON;
    }
    //success
    request = arguments[2];
    if (request !== undefined && request.responseJSON !== undefined) {
        return request.responseJSON;
    }

    if (arguments[0].status != 200) {
        return {
            success: false,
            error: { type: arguments[0].status, message: arguments[0].statusText }
        };
    }
    //return details i suppose...
    return { success: false, data: arguments };
}

//attempting to clean up client side error handling
function getErrorMessage(error, defaultMessage)
{
    //default message
    var msg = defaultMessage ? defaultMessage : "You are doing that too fast. Please wait 30 seconds before trying again.";

    if (error.length > 0 && (
        error != 'Bad Request' 
        &&
        error != 'Internal Server Error'
    ))
    {
        msg = error;
    }
    return msg;
}
function onSaveSet(sender, arguments)
{
    var response = getJsonResponse(arguments);
    var messagePlaceHolder = sender.parents("form").find(".updateResult");
    if (response.success) {
        messagePlaceHolder.html("Set updated");
    } else {
        messagePlaceHolder.html(response.error.message);
    }
}
function submitSetUpdateForm(sender, setName) {

    var callBack = function (sender, arguments) {

        var response = getJsonResponse(arguments);
        var messagePlaceHolder = sender.parents("form").find(".updateResult");
        if (response.success) {
            if (response.data.fullName != setName) {
                location.href = "/s/" + response.data.fullName + "/about/details?message=Set%20Updated";
            }
            else {
                messagePlaceHolder.html("Set Updated");
            }
        } else {
            messagePlaceHolder.html(response.error.message);
        }
    }

    submitForm(sender, callBack);

}
function submitForm(sender, callBack)
{
    var $form = $(sender).parents('form');
    $.ajax({
        type: "POST",
        url: $form.attr('action'),
        data: $form.serialize(),
        error: function (xhr, status, error) {
            callBack(sender, arguments);
        },
        success: function (response) {
            callBack(sender, arguments);
        }
    });
}
// post comment reply form through ajax
function submitComment(senderButton, parentCommentID) {
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
                // comment failed, likely cause: user triggered anti-spam throttle
                $form.find("#submitbutton").val("Submit comment");
                $form.find("#submitbutton").prop('disabled', false);
                $form.find("#errorMessage").html("An unexpected error happened");
                $form.find("#errorMessage").toggle(true);
            },
            success: function (response) {

                var errorObj = getErrorObject(arguments);

                if (errorObj) {
                    $form.find("#submitbutton").val("Submit comment");
                    $form.find("#submitbutton").prop('disabled', false);
                    $form.find("#errorMessage").html(errorObj.error.message);
                    $form.find("#errorMessage").toggle(true);
                } else {
                    if (parentCommentID) {
                        removereplyform(parentCommentID);
                        $(".id-" + parentCommentID).append(response);
                        //notify UI framework of DOM insertion async
                        window.setTimeout(function () { UI.Notifications.raise('DOM', $('.id-' + parentCommentID).last('div')); });
                    } else {
                        $(".sitetable.nestedlisting > #no-comments").remove();
                        $(".sitetable.nestedlisting").prepend(response);
                        // reset submit button
                        $form.find("#submitbutton").val("Submit comment");
                        $form.find("#submitbutton").prop('disabled', false);
                        // reset textbox
                        $form.find("#Content").val("");
                        //notify UI framework of DOM insertion async
                        window.setTimeout(function () { UI.Notifications.raise('DOM', $('.sitetable.nestedlisting').first()); });
                    }
                }
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
                var msg = getErrorMessage(error);

                //submission failed, likely cause: user triggered anti-spam throttle
                $form.find("#submitbutton").val("Submit reply");
                $form.find("#submitbutton").prop('disabled', false);
                $form.find("#errorMessage").html(msg);
                $form.find("#errorMessage").toggle(true);
            },
            success: function (response) {

                if (response.success) {
                    // remove reply form 
                    removereplyform(parentprivatemessageid);
                    // change reply button to "reply sent" and disable it               
                    $("#messageContainer-" + parentprivatemessageid).find("#replyPrivateMessage").html("Reply sent.");
                    $("#messageContainer-" + parentprivatemessageid).find("#replyPrivateMessage").addClass("disabled");

                } else {
                    //submission failed, likely cause: user triggered anti-spam throttle
                    $form.find("#submitbutton").val("Submit reply");
                    $form.find("#submitbutton").prop('disabled', false);
                    $form.find("#errorMessage").html(response.message);
                    $form.find("#errorMessage").toggle(true);
                }
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
function editSubmissionPrepareForm(submissionid) {

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
function removeSubmissionEditForm(submissionid) {
    //BUG: This code makes previews after a submission edit not display. Low Priority.
    $("#submissionid-" + submissionid).find('.usertext-body').toggle(1);
    $("#submissionid-" + submissionid).find('.usertext-edit').toggle(1);
}

// submit edited submission and replace the old one with formatted response received by server
function editSubmission(submissionid) {
    var submissioncontent = $("#submissionid-" + submissionid).find('.form-control').val();
    var submissionobject = { "SubmissionId": submissionid, "SubmissionContent": submissioncontent };

    $.ajax({
        type: "POST",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(submissionobject),
        url: "/editsubmission",
        datatype: "json",
        error: function (xhr, status, error) {
            var msg = getErrorMessage(error, "Oops, a problem happened");
            $("#submissionid-" + submissionid + " span.field-validation-error").html(msg);

        },
        success: function (response) {

            if (response.success) {
                //this has to be called beforehand - to busy to fix it correctly right now
                removeSubmissionEditForm(submissionid);

                var textElement = $("#submissionid-" + submissionid + " .usertext-body");
                textElement.children('div').first().html(response.data.formattedContent); //set new content
                textElement.show();
                window.setTimeout(function () { UI.Notifications.raise('DOM', $("#submissionid-" + submissionid)); });
                //remove edit form

                //clear any error msgs
                $("#submissionid-" + submissionid + " span.field-validation-error").html('');
            } else {
                //var msg = getErrorMessage(response.error., "Oops, a problem happened");
                var msg = response.error.message;
                $("#submissionid-" + submissionid + " span.field-validation-error").html(msg);
            }
        }
    });

    
    return false;
}

// remove comment reply form for given parent id
function removereplyform(parentcommentid) {
    $('#replyform-' + parentcommentid).remove();
}

// remove edit form for given parent id and replace it with original comment
function removeEditForm(parentcommentid) {
    $("#" + parentcommentid).find(".usertext-body").show();
    $("#" + parentcommentid).find(".usertext-edit").hide();
}

function toggleComment(commentID) {

    var element = $("#" + commentID);
    //var t = element.closest('.noncollapsed').css('display');
    var display = element.closest('.noncollapsed').css('display') != 'none';

    //show actual comment
    element.closest('.noncollapsed').toggle(!display);
    //hide show hidden children button
    element.prev().toggle(display);
    //show voting icons
    element.parent().parent().find('.midcol').filter(":first").toggle(!display);
    //show all children
    element.parent().parent().find('> .child').toggle(!display);

    //show all inline loading divs
    element.parent().parent().find('.loadMoreComments').toggle(!display);


    return (false);
}
//obsolete, will be removed soon. use toggleComment instead
function showComment(commentid) {
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
//obsolete, will be removed soon. use toggleComment instead
function hideComment(commentid) {
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
function editCommentSubmit(commentid) {
    var commentcontent = $("#" + commentid).find('.form-control').val();
    var commentobject = { "ID": commentid, "Content": commentcontent };

    $.ajax({
        type: "POST",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(commentobject),
        datatype: "json",
        url: "/editcomment",
        error: function (xhr, status, error) {
            var msg = getErrorMessage(error, "Oops, a problem happened");
            $('#commenteditform-' + commentid + " span.field-validation-error").html(msg);
        },
        success: function (response) {
            if (response.success) {
                $("#" + commentid).find('.md').html(response.data.formattedContent);
                removeEditForm(commentid);
                //notify UI framework of DOM insertion async
                window.setTimeout(function () { UI.Notifications.raise('DOM', $('#' + commentid)); });
            } else {
                $('#commenteditform-' + commentid + " span.field-validation-error").html(response.error.message);
            }
        }
    });


    return false;
}

// delete comment
function deleteComment(commentid) {
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

    removeEditForm(commentid);

    //execute POST call to remove comment from database
    deleteCommentSubmit(commentid);
}

// submit comment deletion request
function deleteCommentSubmit(commentId) {
    var commentobject = { "id": commentId };

    $.ajax({
        type: "POST",
        url: "/deletecomment/" + commentId.toString()
    });

    removeEditForm(commentid);
    return false;
}

// submit submission deletion request
function deleteSubmission(senderButton, submissionid) {

    var $form = $(senderButton).parents('form');
    $form.find("#deletestatusmesssage").html("please wait...");

    $.ajax({
        type: "POST",
        url: "/deletesubmission/" + submissionid.toString(),
        success: function () {
            // reload body content with background page refresh
            window.location = $(location).attr('href');
        },
        error: function () {
            window.location = $(location).attr('href');
        }
    });
}
var reportDialogLock;
function getReportDialog(sender, subverse, type, id) {
    //"v/{subverse}/about/reports/{type}/{id}/dialog"

    if (reportDialogLock) {
        return;
    }

    var urlComplete = "/v/" + subverse + "/about/reports/" + type + "/" + id + "/dialog?nocache=" + cachePrevention();

    reportDialogLock = $.ajax({
        type: "GET",
        url: urlComplete,
        success: function (arg1, request, value) {
            $(sender).hide();
            $(sender).parents('div').first().append(arg1);
        },
        error: function (error) {
            //Something bad happened
            $(sender).text("Oops... a problem");
        },
        complete: function () {
            reportDialogLock = null;
        }
    });
    return false;
}
function cancelReportDialog(sender) {

    //remove report form
    var reportForm = $(sender).parents(".reportDialog").first()
    var toggleButton = reportForm.parent().find('.togglebutton');
    toggleButton.show();
    reportForm.remove();

    return false;
}
function sendReport(sender) {

    var $form = $(sender).parents('form');

    $.ajax({
        type: "POST",
        url: $form.attr('action'),
        data: $form.serialize(),
        error: function (xhr, status, error) {
    
        },
        success: function (response) {

            var errorObj = getErrorObject(arguments);
            var reportForm = $(sender).parents(".reportDialog").first()
            if (errorObj) {
                var errorControl = reportForm.find(".error");

                errorControl.text(errorObj.error.message);
                errorControl.show();
                //toggleButton.text("report failed");
                
            } else {
                var toggleButton = reportForm.parent().find('.report-button');
                toggleButton.show();
                reportForm.remove();
                toggleButton.text("thank you!");
            }
        }
    });
    return false;
}
function markReportAsReviewed(sender, subverse, type, id) {
    //var $form = $(sender).parents('form');
    //"v/{subverse}/about/reports/{type}/{id}/mark"
    var urlComplete = "/v/" + subverse + "/about/reports/" + type + "/" + id + "/mark";

    $.ajax({
        type: "POST",
        url: urlComplete,
        error: function (xhr, status, error) {
            $(sender).parents(".contentReport").remove();
        },
        success: function (response) {

            var errorObj = getErrorObject(arguments);

            if (errorObj) {
                $(sender).val(errorObj.error.message);
            } else {
                $(sender).parents(".contentReport").remove();
            }
        }
    });
    return false;
}
// toggle are you sure question for comment deletion
function toggle(obj, commentid) {
    $(obj).parent().parent().find('.option, .main').toggleClass("active");
    return false;
}

//// toggle are you sure question for subverse block action
//function blockSubverseToggle(obj) {
//    $(obj).parent().parent().find('.option, .error').toggleClass("active");
//    return false;
//}

// toggle are you sure question for comment report
function togglereport(commentid) {
    $("#" + commentid).find('.report').toggleClass("active");
    return false;
}

// submit report and replace report button with a "thank you" to the user
function reportContent(obj, type, id) {
    $(obj).parent().parent().find('.togglebutton').attr("onclick", "javascript:void(0)");
    $(obj).parent().parent().find('.option, .main').toggleClass("active");
    $(obj).parent().parent().find('.togglebutton').html("please wait...");

    // submit report
    $.ajax({
        type: "POST",
        url: "/report/" + type + "/" + id,
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
function toggleSubmissionBack(obj) {
    $(obj).parent().parent().find('.option, .error').toggleClass("active");
    return false;
}

// check if an object exists
$.fn.exists = function () {
    return this.length !== 0;
};
function setSubverseAddCallBack(sender, arguments) {
    var response = new getJsonResponse(arguments);
    var message = "";
    if (response.success) {
        if (response.data == true) {
            message = "Subverse added to set";
            //message = "Subverse " + subverseName + " was added to set " + setName;
        }
        else if (response.data == false) {
            message = "Subverse removed from set";
            //message = "Subverse " + subverseName + " was added to set " + setName;
        }
        else {
            message = "No action was taken";
            //message = "Subverse " + subverseName + " was removed from set " + setName;
        }
        $('input[data-autocomplete-subverse=1]').val('');
    } else {
        message = "Oops: " + response.error.message;
    }
    $(sender).parents(".updateSection").find(".updateResult").html(message);
}
function setSubverseListToggleCallBack(s, arguments) {

    var response = new getJsonResponse(arguments);
    if (response.success) {

        toggleButtonVisualState(s, response.data, "remove", "add");

        //if (response.data) {
        //    s.text("remove");
        //    s.addClass("btn-voat-off");
        //    //backwards compat
        //    s.addClass("btn-unsub");
        //    s.removeClass("btn-sub");

        //} else {
        //    s.text("add");
        //    s.removeClass("btn-voat-off");
        //    //backwards compat
        //    s.addClass("btn-sub");
        //    s.removeClass("btn-unsub");
        //}
    } else {
        s.text(response.error.message);
    }
}
function setSubverseListToggle(sender, setName, subverseName, action, callBack) {
    if (subverseName != "") {
        //$(obj).html("unsubscribe");
        //$(obj).toggleClass("btn-sub btn-unsub");
        var actionType = "toggle";
        if (action !== undefined && action != null) {
            actionType = action;
        }
        var url = "/s/" + setName + '/' + subverseName + '/' + actionType;
        // call the set subverse list API
        $.ajax({
            type: "POST",
            url: url,
            complete: function () {
                callBack(sender, arguments);
            }
        });
    }
}

function subscribe(sender, type, name, callBack)
{
    //{pathPrefix}/subscribe/{domainType}/{name}/{ownerName}/{action}
    var url = "/user/subscribe/" + type + "/" + name + "/toggle" ;
    $.ajax({
        type: "POST",
        url: url,
        complete: function () {
            callBack(sender, arguments);
        }
    });
}

function subscribeToSet(sender, name)
{
    subscribe(sender, "set", name,
        function (s, args) {
            var response = getJsonResponse(args);
            if (response.success) {

                toggleButtonVisualState(s, response.data, "unsubscribe", "subscribe");

                //if (response.data) {
                //    s.text("unsubscribe");
                //    s.addClass("btn-voat-off");
                //    //backwards compat
                //    s.addClass("btn-unsub");
                //    s.removeClass("btn-sub");

                //} else {
                //    s.text("subscribe");
                //    s.removeClass("btn-voat-off");
                //    //backwards compat
                //    s.addClass("btn-sub");
                //    s.removeClass("btn-unsub");
                //}
               
            } else {
                s.text(response.error.message);
            }
        }
    );
}
function subscribeToSubverse(sender, name) {
    subscribe(sender, "subverse", name,
       function (s, args) {
           var response = getJsonResponse(args);
           if (response.success) {

               toggleButtonVisualState(s, response.data, "unsubscribe", "subscribe");

               //if (response.data) {
               //    s.text("unsubscribe");
               //    s.addClass("btn-voat-off");
               //    //backwards compat
               //    s.addClass("btn-unsub");
               //    s.removeClass("btn-sub");

               //} else {
               //    s.text("subscribe");
               //    s.removeClass("btn-voat-off");
               //    //backwards compat
               //    s.addClass("btn-sub");
               //    s.removeClass("btn-unsub");
               //}
           } else {
               s.text(response.error.message);
           }
       }
   );
}
function toggleButtonVisualState(target, enabled, trueText, falseText) {

    //So the enabled setting is the value of whether the item is subscribed or not
    //true = subscribed (thus we show a non-highlighted button)
    //false = we highlight it
    if (enabled) {
        target.text(trueText);
        target.addClass("btn-voat-off");
        //backwards compat
        target.addClass("btn-unsub");
        target.removeClass("btn-sub");

    } else {
        target.text(falseText);
        target.removeClass("btn-voat-off");
        //backwards compat
        target.addClass("btn-sub");
        target.removeClass("btn-unsub");
    }
}

// a function to load content of a self post and append it to calling object
function loadSelfText(obj, messageId) {
    
    $(obj).toggleClass("collapsed");
    $(obj).toggleClass("expanded");

    // toggle message content display
    $(obj).parent().find(".expando").toggle();
}

// function to post delete private message request to messaging controller and remove deleted message DOM
function deleteMessage(obj, type, id, context) {
    $(obj).html("please wait...");
    var endpoint = "/messages/delete/" + type + "/" + ($.isNumeric(id) ? id : '') + (context ? "?subverse=" + context : "");
    $.ajax({
        type: "POST",
        contentType: 'application/json; charset=utf-8',
        url: endpoint,
        success: function () {
            //remove message DOM
            $("#messageContainer-" + id).remove();
        },
        error: function (xhr, status, error) {
            var msg = getErrorMessage(error, "Oops, a problem happened");
            $(obj).html(msg);
        },
        datatype: "json"
    });
    return false;
}

// a function to call mark as read messaging endpoint
function markAsRead(obj, type, action, id, context) {
    $(obj).attr("onclick", "");
    // mark single item as read
    var endpoint = "/messages/mark/" + type + "/" + action + "/" + ($.isNumeric(id) ? id : '') + (context ? "?subverse=" + context : "");
    var markAsReadRequest = $.ajax({
        type: "POST",
        url: endpoint,
        success: function (data) {
            // inform the user
            $(obj).text("marked");
            if (id) {
                $("#messageContainer-" + id + " > .panel").toggleClass("unread");
            } else {
                $(".markAsReadLink").remove();
                $(".unread").removeClass("unread");
            }
        },
        error: function (data) {
            $(obj).text("something went wrong");
        }
    });
}
// function to load select link flair modal dialog for given subverse and given submission
function selectflair(messageId, subverseName) {

    var url = "/ajaxhelpers/linkflairselectdialog/" + subverseName + "/" + messageId + "?nocache=" + cachePrevention();

    var flairSelectDialog = $.get(
        url,
        null,
        function (data) {
            $("#linkFlairSelectModal").html(data);
            $('#linkFlairSelectModal').modal();
        }
     );
}

// function to apply flair to a given submission
function applyflair(sender, submissionID, flairID, flairLabel, flairCssClass) {
    $.ajax({
        type: "POST",
        url: "/submissions/applylinkflair/" + submissionID + "/" + flairID,
        success: function (response) {

            $('#linkFlairSelectModal').modal('hide');

            if (response.success) {
                $('#linkFlairSelectModal').modal('hide');

                //set linkflair
                $('#linkflair').attr('class', "flair " + flairCssClass);
                $('#linkflair').attr('title', flairLabel);
                $('#linkflair').text(flairLabel);
            }
            else
            {
                //sender is a <button> and this code doesn't work'
                //$(sender).html(response.error.message);
                //$(sender).prop('value', response.error.message); 
            }
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
            var tagline = $(obj).parent().parent().parent().find(">.tagline");
            tagline.find(">.author").toggleClass("moderator");
            var userAttrs = tagline.find(">.userattrs");
            submitterAttr = userAttrs.find(">.submitter"),
            moderatorAttr = userAttrs.find(">.moderator");
            userAttrs.html("");

            if (moderatorAttr.length > 0) {
                $(obj).html("distinguish");
                if (submitterAttr.length > 0) {
                    userAttrs.append(["[", submitterAttr, "]"]);
                }
            } else {
                $(obj).html("undistinguish");
                var subAndId = $(obj).parent().parent().find(".bylink").attr("href").match(/^\/v\/([a-zA-Z0-9]+)\/(?:comments\/)?(\d+)/i);
                moderatorAttr = $('<a>M</a>');
                moderatorAttr.attr("href", "/v/" + subAndId[1] + "/" + subAndId[2]);
                moderatorAttr.attr("class", "moderator");
                moderatorAttr.attr("title", "moderator");

                if (submitterAttr.length > 0) {
                    userAttrs.append(["[", submitterAttr, ", "]);
                } else {
                    userAttrs.append("[");
                }

                userAttrs.append([moderatorAttr, "]"]);
            }
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

    var uri = $("#Url").val();

    // request a url title from title service
    var title = $.get(
        "/ajaxhelpers/titlefromuri?uri=" + uri,
        null,
        function (data) {
            $("#Title").val(data);
            $("#suggest-title").text("Enter the URL above, then click here to suggest a title");
            $("#suggest-title").on('click', suggestTitle);
        }).fail(function () {
            $("#Title").val("We were unable to suggest a title.");
            $("#suggest-title").text("Enter the URL above, then click here to suggest a title");
            $("#suggest-title").on('click', suggestTitle);
        });
}

// a function to toggle sticky mode for a submission
function toggleSticky(submissionID) {
    $.ajax({
        type: "POST",
        url: "/submissions/togglesticky/" + submissionID,
        success: function (data) {
            if (data.success) {
                $('#togglesticky').html("toggled");
            } else {
                $('#togglesticky').html(data.error.message);
            }
        },
        error: function () {
            alert('Something went wrong while sending a sticky toggle request.');
        }
    });
}
function toggleNSFW(submissionID) {
    $.ajax({
        type: "POST",
        url: "/submissions/togglensfw/" + submissionID,
        success: function (response) {
            if (response.success) {
                
                if (response.data) {
                    //has nsfw flair add it
                    $('#submissionid-' + submissionID + " p.title").prepend('<span title="Not Safe For Work" class="flair linkflairlabel" id="nsfwflair">NSFW</span>');
                } else {
                    //has not nsfw flair
                    $('#nsfwflair').remove();
                }
                $('#togglensfw').html("toggled");
            } else {
                $('#togglensfw').html(response.error.message);
            }
        },
        error: function () {
            alert('Something went wrong while sending a nsfw toggle request.');
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

    
    var bucketUrl = "/comments/" + submissionId + "/" + (parentId == null ? 'null' : parentId) + "/" + command + "/" + startingIndex + "/" + sort + "?nocache=" + cachePrevention();
    loadCommentsRequest2 = $.ajax({
        url: bucketUrl,
        success: function (data) {
            //$("#comments-" + submissionId + "-page").remove();
            appendTarget.append(data);
            window.setTimeout(function () { UI.Notifications.raise('DOM', appendTarget); });

            wireTooltips();

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
function cachePrevention() {
    var v = 'xxxx'.replace(/[xy]/g, function (c) {
        var rand = Math.random() * 16 | 0
        return rand.toString(16);
    });
    return v;
}
//// a function to fetch 1 comment bucket for a submission and append to the bottom of the page
//var loadCommentsRequest;
//function loadMoreComments(obj, submissionId) {
//    if (loadCommentsRequest) { return; }
//    $(obj).html("Sit tight...");

//    // try to see if this request is a subsequent request
//    var currentPage = $("#comments-" + submissionId + "-page").html();
//    if (currentPage == null) {
//        currentPage = 1;
//    } else {
//        currentPage++;
//    }
//    loadCommentsRequest = $.ajax({
//        url: "/comments/" + submissionId + "/" + currentPage + "/",
//        success: function (data) {
//            $("#comments-" + submissionId + "-page").remove();
//            $(obj).before(data);
//            window.setTimeout(function () { UI.Notifications.raise('DOM', $(obj).parent()); });
//            $(obj).html("load more &#9660;");
//        },
//        error: function () {
//            $(obj).html("That's it. There was nothing else to show. Phew. This was hard.");
//        },
//        complete: function () {
//            loadCommentsRequest = null;
//        }
//    });
//}

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
// ********************************** CHAT ****************************************

// a function to scroll chat box content up
function scrollChatToBottom(force) {
    
    var chatWindow = document.getElementById('subverseChatRoom');
    var margin = 100;
    var difference = (chatWindow.scrollHeight - chatWindow.offsetHeight) - chatWindow.scrollTop;

    var scroll = difference < margin;

    if (force === true || scroll === true)
    {
        chatWindow.scrollTop = chatWindow.scrollHeight;
    }
}

// a function to submit chat message to subverse chat room
function sendChatMessage(id, access) {
    if ($.connection != null) {
        var messageToSend = $("#chatInputBox").val();
        var chatProxy = $.connection.messagingHub;
        chatProxy.server.sendChatMessage(id, messageToSend, access);
        scrollChatToBottom(true);
        // clear input
        $("#chatInputBox").val('');
    }
}

// a function to add a client to a subverse chat room
function joinChat(id, access) {
    if ($.connection != null) {
        // Start the connection.
        $.connection.hub.start().done(function () {
            var chatProxy = $.connection.messagingHub;
            chatProxy.server.joinChat(id, access);
        });
    }
}

function leaveChat(id) {
    if ($.connection != null) {
        var chatProxy = $.connection.messagingHub;
        chatProxy.server.leaveChat(id);
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

function toggleSaveSubmission(id) {
    var saveLink = $(".submission.id-" + id + " .savelink");
    if (saveLink.exists()) {
        if (saveLink.text() === "save") {
            saveLink.text("unsave");
        } else {
            saveLink.text("save");
        }
        var url = "/user/save/submission/" + id.toString()
        $.ajax({
            type: "POST",
            url: url
        });
    }
}

function toggleSaveComment(id) {
    var saveLink = $(".comment.id-" + id + " .savelink").first();
    if (saveLink.exists()) {
        if (saveLink.text() === "save") {
            saveLink.text("unsave");
        } else {
            saveLink.text("save");
        }
        var url = "/user/save/comment/" + id.toString()
        $.ajax({
            type: "POST",
            url: url
        });
    }
}

// a function to submit subverse block/unblock request
function blockSubverseToggle(obj, subverseName) {
    $(obj).toggleClass("btn-blocksubverse btn-unblocksubverse");
    var blockButton = $(obj);
    if (blockButton.exists()) {

        toggleButtonVisualState(blockButton, blockButton.text() === "block", "unblock", "block");

        //if (blockButton.text() === "block") {
        //    blockButton.text("unblock");
        //} else {
        //    blockButton.text("block");
        //}

        // submit block request
        postBlock('subverse', subverseName);
    }
}
function blockUserToggle(obj, name) {
    $(obj).toggleClass("btn-blocksubverse btn-unblocksubverse");
    var blockButton = $(obj);
    if (blockButton.exists()) {

        toggleButtonVisualState(blockButton, blockButton.text() === "block", "unblock", "block");

        //if (blockButton.text() === "block") {
        //    blockButton.text("unblock");
        //} else {
        //    blockButton.text("block");
        //}

        // submit block request
        postBlock('user', name);
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
        postBlock('subverse', subverseName);
    }
}

// a function to post subverse block request
function postBlock(type, name) {
    $.ajax({
        type: "POST",
        url: "/block/" + type,
        data: {name: name}
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
                    if (data.available) {
                        $('#usernameAvailabilityStatus').hide();
                    } else {
                        $('#usernameAvailabilityStatus').show();
                    }
                }
            });
        }
    }
}

// a function to preview stylesheet called from subverse stylesheet editor
function previewStylesheet(obj, subverseName) {
   

    function replaceStyle() {
        $('[id=custom_css]').remove();
        // inject the new stylesheet
        var sheetToAdd = document.createElement('style');
        sheetToAdd.setAttribute('id', 'custom_css');
        sheetToAdd.innerHTML = $('#Stylesheet').val();
        document.body.appendChild(sheetToAdd);
    }

    //Only reload if not loaded
    if ($.trim($("#stylesheetpreviewarea").html()) == '') {
        var sendingButton = $(obj);
        sendingButton.html('Hold on...');
        sendingButton.prop('disabled', true);
        var url = '/ajaxhelpers/previewstylesheet?subverse=' + subverseName + '&previewMode=true' + '&nocache=' + cachePrevention();

        $.ajax({
            type: 'GET',
            url: url,
            dataType: 'html',
            success: function (data) {
                $("#stylesheetpreviewarea").html(data);
                sendingButton.html("Preview");
                sendingButton.prop('disabled', false);
                registerDashboardHandler(); //hooks menu to newly loaded html
                replaceStyle();
            }
        });
    } else {
        replaceStyle();
    }

}
// a function to preview stylesheet called from subverse stylesheet editor
function getCommentTree(submissionID, sort) {

    $("#comment-sort-label").text("Loading...");

    $.ajax({
        type: 'GET',
        url: '/comments/' + submissionID + '/tree/' + sort + "?nocache=" + cachePrevention(),
        dataType: 'html',
        error: function () {

        },
        success: function (data) {
            $(".commentarea").html(data);
            window.setTimeout(function () {
                UI.Notifications.raise('DOM', $(".commentarea"));
                wireTooltips();
            });
        }
    });
}