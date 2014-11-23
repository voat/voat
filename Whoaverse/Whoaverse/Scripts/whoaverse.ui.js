//Whoaverse UI JS framework - Version 0.3 - 11/21/2014
//Tested only with the latest version of IE, FF, & Chrome

var UI = window.UI || {};

//Generic handler for User defined event notifications
UI.Notifications = (function () {
    //private
    var _subscribers = [];

    function notify(event, context) {
        _subscribers.forEach(function (x) {
            if (x.event == event) {
                x.callback(context);
            }
        });
    }

    return {
        //public
        subscribe: function (event, callback) {
            _subscribers.push({ 'event': event, 'callback': callback });
        },
        unSubscribe: function (event, callback) {
            _subscribers = _subscribers.filter(function (x) {
                if (!(x.event == event && x.callback == callback)) {
                    return x;
                }
            });
        },
        clear: function (event) {
            _subscribers = _subscribers.filter(function (x) {
                if (x.event !== event) {
                    return x;
                }
            });
        },
        raise: function (event, context) {
            notify(event, context);
        }
    }
})();

UI.Common = {
    isMobile: function () {
        return false; //TODO: determine what conditions qualify for a 'mobile' view
    },
    isCommentPage: function () {
        return /\/comments\//i.test(window.location.href);
    },
    fileExtension: function (path, includeDot) {
        if (path) {
            try {
                var ext = /\.+\w+$/i.exec(path);
                if (ext && ext.length > 0) {
                    return (includeDot) ? ext[0] : ext[0].replace('.', '');

                }
            } catch (ex) {
                return '';
            }
        }
        return '';
    },
    domainRoot: function () {
        return location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '');
    },
    resolveUrl: function (relativePath) {

        if (relativePath) {
            if (relativePath.indexOf('http') == 0 || relativePath.indexOf('ftp') == 0) {
                return relativePath;
            } else {
                if (relativePath.indexOf('~/') == 0) {
                    return UI.Common.domainRoot().concat('/', relativePath.replace('~/', ''));
                } else if (relativePath.indexOf('/') == 0) {
                    return UI.Common.domainRoot().concat(relativePath);
                } 

                return UI.Common.domainRoot().concat('/', relativePath);
            }
        }

        return relativePath;
    }
}

UI.LinkHandler = {
    //This will be the object that converts /u/name -> <a href='/user/name'>/u/name</a> and /v/sub -> <a href='/v/sub'>/v/sub</a>
}

UI.CommentImageHandler = (function () {

    UI.Notifications.subscribe('DOM', function (context) {
        UI.CommentImageHandler.execute(context);
    });
    function clickRoutine(event) {
        event.preventDefault();

        var target = $(this);

        if (!target.data('showing')) {
            //show
            if (target.data('loaded')) {
                target.data('showing', true);
                UI.CommentImageHandlerSettings.toggleFunction(target.next(), true);
            } else {
                //load
                load(target, false);
            }
        } else {
            //hide
            UI.CommentImageHandlerSettings.toggleFunction(target.next(), false);
            target.data('showing', false);
        }
    }
    function load(target, autoLoading) {

        var anchorText = target.text();
        
        if (UI.CommentImageHandlerSettings.onLoading) {
            UI.CommentImageHandlerSettings.onLoading(target, target.text());
        }

        //remove handler while loading
        target.off();
        target.on('click', function (e) { e.preventDefault(); }); //disable the link until loaded, prevent rapid clickers

        var img = new Image();
        img.onerror = function () {
            //can't determine what kind of error... could be 404, could be a working non-image source, etc.
            img.src = UI.Common.resolveUrl(UI.CommentImageHandlerSettings.errorImageUrl);
        }
        img.onload = function () {

            if (!this.complete) {
                return;
            }
            
            var parent = target.parent();

            var displayDiv = $('<div/>', {
                class: 'async-img',
                style: 'display:none;'
            }).insertAfter(target);

            var i = $(this);

            displayDiv.html(i);

            //BEGIN: Evil sizing code because IE is *special*
            var width, height;
            if (this.naturalWidth) {
                width = this.naturalWidth;
            } else {
                width = this.width;
            }
            if (this.naturalHeight) {
                height = this.naturalHeight;
            } else {
                height = this.height;
            }

            //append info attributes
            var desc = UI.Common.fileExtension(target.prop('href')).toUpperCase().concat(' · ', width.toString(), ' x ', height.toString());
            var infoSpan = $('<span/>', { class: 'tagline' }).text(desc + ' ');
            var link = $('<a/>', { class: 'async-img-direct', target: '_blank', href: target.prop('href') }).text('Open');
            target.prop('title', desc);
            displayDiv.append(infoSpan);
            infoSpan.append(link);
            
            //I HAVE NO IDEA WHY I HAVE TO DO THIS TO REMOVE THE width/height attributes of the image tag itself
            i.css('width', width);
            i.css('height', height);
            this.removeAttribute('width');
            this.removeAttribute('height');

            if (width > UI.CommentImageHandlerSettings.maxSize || height > UI.CommentImageHandlerSettings.maxSize) {
                if (width >= height) {
                    i.css('width', UI.CommentImageHandlerSettings.maxSize);
                    i.css('height', 'auto');

                    i.data('origWidth', UI.CommentImageHandlerSettings.maxSize);
                    i.data('origHeight', 'auto');

                } else {
                    i.css('width', 'auto');
                    i.css('height', UI.CommentImageHandlerSettings.maxSize);

                    i.data('origWidth', 'auto');
                    i.data('origHeight', UI.CommentImageHandlerSettings.maxSize);
                }
                i.data('inFullMode', false);

                i.data('maxWidth', Math.min(width, UI.CommentImageHandlerSettings.maxFullSizeWidth)); //trying to fix the extra large image thing

                displayDiv.click(function () {
                    var childImg = $(this).children('img');
                    if (childImg.data('inFullMode')) {
                        childImg.css('width', childImg.data('origWidth'));
                        childImg.css('height', childImg.data('origHeight'));
                        childImg.data('inFullMode', false);
                    } else {
                        childImg.css('width', childImg.data('maxWidth'));
                        childImg.css('height', 'auto');
                        childImg.data('inFullMode', true);
                    }
                });
                displayDiv.css('cursor', 'pointer');
            }

            target.data('loaded', true);

            //remove handler while loading
            target.off();
            target.on('click', clickRoutine);

            if (UI.CommentImageHandlerSettings.onLoaded) {
                UI.CommentImageHandlerSettings.onLoaded(target, anchorText);
            }

            if (!autoLoading) {
                target.data('showing', true);
                UI.CommentImageHandlerSettings.toggleFunction(displayDiv, true);
            }

            if (autoLoading && UI.CommentImageHandlerSettings.autoShow) {
               target.click();
            }
        };
        img.src = target.prop('href');
    }

    return {

        bind: function (element) {

            $(element).data('showing', false);

            //this will be fixed later - the ajax nodes were getting hooked multiple times and I just haven't isolated this yet so lets just set a flag for if this element has been hooked before.
            if ($(element).data('hooked') == true) {
                return;
            }

            $(element).on('click', clickRoutine);
            
            $(element).data('hooked', true);
            
            if (UI.CommentImageHandlerSettings.autoLoad) {
                load($(element), true);
            } else if (UI.CommentImageHandlerSettings.autoShow) {
                $(element).click();
            }

        },

        execute: function (container) {
            //no need to process if not on comments page
            if (!UI.Common.isCommentPage()) {
                return;
            }

            var settings = UI.CommentImageHandlerSettings;
            var c = (container == undefined ? $(settings.selector) : $(settings.selector, container));

            c.filter(function () { return settings.filter.test(this.href) }).each(function (i, x) {
                UI.CommentImageHandler.bind(x); 
            });
        }
    }
})();

UI.CommentImageHandlerSettings = (function () {
    return {
        autoLoad: true, //this setting will preload all image links
        autoShow: false, //if true then the click routine is run during event hookup
        selector: '.usertext-body > .md a:not(.async-img-direct)', //elements to find image anchor tags
        filter: /^([^\?]+(\.(jpg|jpeg|gif|giff|png)))$/i, //regex for href links to load
        maxSize: 250,
        toggleFunction: function (element, display) { //element (obj) to show/hide, display (bool) show/hide
            element.slideToggle();
        },
        onLoading: function (element, rawText) {
            element.text(rawText + ' (loading)');
        },
        onLoaded: function (element, rawText) {
            element.html(rawText.concat('<span class=\'async-img-type\'>', UI.Common.fileExtension(element.prop('href')).toUpperCase(), '</span>'));
        },
        errorImageUrl: '~/Graphics/missing_image.png', //only relative path is supported right now.
        maxFullSizeWidth: 600, //in pixels, needs to be numeric
        //TODO: Settings that need implemented
        maxFileSizeInKB: 2048
    }
})();


$(document).ready(function () {

    UI.CommentImageHandler.execute();
});







