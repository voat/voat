$('.swipebox').swipebox({
    useCSS: true, // false will force the use of jQuery for animations
    useSVG: true, // false to force the use of png for buttons
    initialIndexOnArray: 0, // which image index to init when a array is passed
    hideCloseButtonOnMobile: false, // true will hide the close button on mobile devices
    hideBarsDelay: 0, // delay before hiding bars on desktop
    videoMaxWidth: 1140, // videos max width
    beforeOpen: function () { }, // called before opening
    afterOpen: null, // called after opening
    afterClose: function () { }, // called after closing
    loopAtEnd: false // true will return to the first image after the last image is reached
});

$(document).ready(function () {
    var imageArray = [];

    $.getJSON("/api/Top100ImagesByDate", function (data) {

        console.log("json loaded, now parsing...");

        for (var i in data["Items"]) {
            var imageObj = {
                submissionid: data["Items"][i].SubmissionId,
                href: data["Items"][i].Img,
                title: data["Items"][i].Alt,
                subverse: data["Items"][i].Subverse,
                submittedby: data["Items"][i].SubmittedBy,
                submittedon: data["Items"][i].SubmittedOn,
                upvotes: data["Items"][i].UpVotes,
                downvotes: data["Items"][i].DownVotes
        };
            
            imageArray.push(imageObj);
        };

        // launch the gallery
        $.swipebox(imageArray);
    });
});
