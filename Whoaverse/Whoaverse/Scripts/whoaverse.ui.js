//Whoaverse UI JS framework - Version 0.4 - 11/24/2014
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
    debug: false,
    availableWidth: function (container) {
        return $(container).innerWidth();
    },
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
    getDomainName: function(url, includeSub, removeWWW){
        //TODO:
        return "notdone.com";
    },
    currentDomainRoot: function () {
        return location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '');
    },
    resolveUrl: function (relativePath) {
        if (relativePath) {
            if (relativePath.indexOf('http') == 0 || relativePath.indexOf('ftp') == 0) {
                return relativePath;
            } else {
                if (relativePath.indexOf('~/') == 0) {
                    return UI.Common.currentDomainRoot().concat('/', relativePath.replace('~/', ''));
                } else if (relativePath.indexOf('/') == 0) {
                    return UI.Common.currentDomainRoot().concat(relativePath);
                } 

                return UI.Common.currentDomainRoot().concat('/', relativePath);
            }
        }

        return relativePath;
    }
}

UI.LinkHandler = {
    //This will be the object that converts /u/name -> <a href='/user/name'>/u/name</a> and /v/sub -> <a href='/v/sub'>/v/sub</a>
}
UI.ExpandoManager = (function () {

    UI.Notifications.subscribe('DOM', function (context) {
        UI.ExpandoManager.execute(context);
    });

    var expandoDictionary = []; //This is a dictionary i.e. {key:'value', group:[obj Expando, obj Expando, etc.]} 

    return {
        addExpando: function (selector, expandos) {

            var find = expandoDictionary.filter(function (x) {
                if (x.selector == selector) {
                    return x;
                }
            });

            if (find.length > 0) {
                if (expandos instanceof Array) {
                    find[0].group = find[0].group.concat(expandos);
                } else {
                    find[0].group.push(expandos);
                }
            } else {
                if (expandos instanceof Array) {
                    expandoDictionary.push({ 'selector': selector, 'group': expandos });
                } else {
                    expandoDictionary.push({ 'selector': selector, 'group': [expandos] });
                }
            }

        },
        reset: function () {
            expandoDictionary = [];
        },
        execute: function (container) {
            if (expandoDictionary && expandoDictionary.length > 0) {

                expandoDictionary.forEach(function (selectorGroup) {
                    var c = (container == undefined ? $(selectorGroup.selector) : $(selectorGroup.selector, container));
                    if (c.length > 0) {
                        selectorGroup.group.forEach(function (expando) {
                            c.filter(function () {
                                var r = expando.filter.test(this.href);
                                return r;
                            }).each(function (i, x) {
                                expando.process(x);
                            });
                        });
                    }
                });

            }
        }
    }

})();

//base expando class
var LinkExpando = function (hrefFilter) {
    this.filter = hrefFilter;
};
LinkExpando.prototype = {
    process: function (target) {
        //no-op - this is a base class
        if (UI.Common.debug) {
            alert("Class LinkExpando.process(target) method must be overridden in derived class.")
        }
    },
    setDirectLink: function (parentControl, description, url){
        var infoSpan = $('<span/>', { class: 'tagline' }).html(description + ' ').append($('<a/>', { class: 'link-expando-direct', target: '_blank', href: url }).text('Open'));
        parentControl.append(infoSpan);
    },
    getFilter: function(){
        return this.filter;
    }
};

var ImageLinkExpando = function () {
    LinkExpando.call(this, /^([^\?]+(\.(jpg|jpeg|gif|giff|png)))$/i);
    this.load = function(target, autoLoading) {

        var anchorText = target.text();

        if (UI.ImageExpandoSettings.onLoading) {
            UI.ImageExpandoSettings.onLoading(target, target.text());
        }

        //remove handler while loading
        target.off();
        target.on('click', function (e) { e.preventDefault(); }); //disable the link until loaded, prevent rapid clickers
        var me = this;

        var img = new Image();
        img.onerror = function () {
            //can't determine what kind of error... could be 404, could be a working non-image source, etc.
            img.src = UI.Common.resolveUrl(UI.ImageExpandoSettings.errorImageUrl);
        }
        img.onload = function () {

            if (!this.complete) {
                return;
            }

            var parent = target.parent();

            var displayDiv = $('<div/>', {
                class: 'link-expando',
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

            var desc = UI.Common.fileExtension(target.prop('href')).toUpperCase().concat(' · ', width.toString(), ' x ', height.toString());

            me.setDirectLink(displayDiv, desc, target.prop('href'));
            target.prop('title', desc);


            //I HAVE NO IDEA WHY I HAVE TO DO THIS TO REMOVE THE width/height attributes of the image tag itself
            i.css('width', width);
            i.css('height', height);
            this.removeAttribute('width');
            this.removeAttribute('height');

            if (width > UI.ImageExpandoSettings.maxSize || height > UI.ImageExpandoSettings.maxSize) {
                if (width >= height) {
                    i.css('width', UI.ImageExpandoSettings.maxSize);
                    i.css('height', 'auto');

                    i.data('origWidth', UI.ImageExpandoSettings.maxSize);
                    i.data('origHeight', 'auto');

                } else {
                    i.css('width', 'auto');
                    i.css('height', UI.ImageExpandoSettings.maxSize);

                    i.data('origWidth', 'auto');
                    i.data('origHeight', UI.ImageExpandoSettings.maxSize);
                }
                i.data('inFullMode', false);

                i.data('maxWidth', Math.min(width, UI.Common.availableWidth(target.parent())));

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

            //reestablish handler
            target.off();
            target.on('click', me.clickRoutine);

            if (UI.ImageExpandoSettings.onLoaded) {
                UI.ImageExpandoSettings.onLoaded(target, anchorText);
            }

            if (!autoLoading) {
                target.data('showing', true);
                UI.ImageExpandoSettings.toggleFunction(displayDiv, true);
            }

            if (autoLoading && UI.ImageExpandoSettings.autoShow) {
                target.click();
            }
        };
        img.src = target.prop('href');
    }
    this.clickRoutine = function(event) {
        event.preventDefault();

        var target = $(this);

        if (!target.data('showing')) {
            //show
            if (target.data('loaded')) {
                target.data('showing', true);
                UI.ImageExpandoSettings.toggleFunction(target.next(), true);
            } else {
                //load
                load(target, false);
            }
        } else {
            //hide
            UI.ImageExpandoSettings.toggleFunction(target.next(), false);
            target.data('showing', false);
        }
    }

}
ImageLinkExpando.prototype = new LinkExpando();
ImageLinkExpando.prototype.constructor = LinkExpando;
ImageLinkExpando.prototype.process = function (target) {
    $(target).data('showing', false);

    //TODO: Fix this hacktastic hack
    if ($(target).data('hooked') == true) {
        return;
    }

    $(target).on('click', this.clickRoutine);

    $(target).data('hooked', true);

    if (UI.ImageExpandoSettings.autoLoad) {
        this.load($(target), true);
    } else if (UI.ImageExpandoSettings.autoShow) {
        $(target).click();
    }
};

/* IFrameEmbedder */
var IFrameEmbedderExpando = function (urlRegEx) {
    LinkExpando.call(this, urlRegEx);
    this.defaultRatio = 0.5625;
    this.hook = function (target, description, iFrameSettings) {

        if ($(target).data('hooked') == true) {
            return;
        }
        $(target).data('hooked', true);

        var id = undefined;
        try {
            id = this.filter.exec(target.prop('href'))[1];
            if (!id || id == undefined) {
                return; //bail, we have a problem
            }
        } catch (ex) {
            return; //bail, we have a problem
        }

        var displayDiv = $('<div/>', {
            class: 'link-expando',
            style: 'display:none;'
        }).insertAfter(target);

        //<iframe width="560" height="315" src="//www.youtube.com/embed/JUDSeb2zHQ0" frameborder="0" allowfullscreen></iframe>
        iFrameSettings.src = this.getSrcUrl(id);
        var iFrame = $('<iframe/>', iFrameSettings);

        displayDiv.html(iFrame);

        this.setDirectLink(displayDiv, description, target.prop('href'));
        target.prop('title', description);

        target.on('click', function (e) { e.preventDefault(); target.next().slideToggle(); });
        target.html(target.text().concat('<span class=\'link-expando-type\'>', description, '</span>'));

    }
}
IFrameEmbedderExpando.prototype = new LinkExpando();
IFrameEmbedderExpando.prototype.constructor = LinkExpando;

/* YouTube */
var YouTubeExpando = function(){
    IFrameEmbedderExpando.call(this, /(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^"&?\/ ]{11})/i);
    this.getSrcUrl = function (id) { return '//www.youtube.com/embed/' + id; };
};
YouTubeExpando.prototype = new IFrameEmbedderExpando();
YouTubeExpando.prototype.constructor = IFrameEmbedderExpando;
YouTubeExpando.prototype.process = function (target) {
    var width = Math.min(560, UI.Common.availableWidth($(target).parent()));

    this.hook($(target), 'YouTube', {
        width: width.toString(),
        height: (width * this.defaultRatio).toString(),
        frameborder: '0',
        allowfullscreen: true
    });
};

/* Imgur Album */
var ImgurAlbumExpando = function () {
    IFrameEmbedderExpando.call(this, /imgur\.com\/a\/(\w+)\/?/i);
    this.getSrcUrl = function (id) { return '//imgur.com/a/' + id + '/embed'; };
};
ImgurAlbumExpando.prototype = new IFrameEmbedderExpando();
ImgurAlbumExpando.prototype.constructor = IFrameEmbedderExpando;
ImgurAlbumExpando.prototype.process = function (target) {
    var width = Math.min(560, UI.Common.availableWidth($(target).parent()));

    //<iframe class="imgur-album" width="100%" height="550" frameborder="0" src="//imgur.com/a/aEBi9/embed"></iframe>
    this.hook($(target), "Imgur Album", {
        width: width.toString(),
        height: (width * .8).toString(),
        frameborder: '0'
    });
};
/* VIMEO */
var VimeoExpando = function () {
    IFrameEmbedderExpando.call(this, /vimeo\.com\/[\w\/]*\/(\d{8,})\/?/i);
    this.getSrcUrl = function (id) { return '//player.vimeo.com/video/' + id; };
};
VimeoExpando.prototype = new IFrameEmbedderExpando();
VimeoExpando.prototype.constructor = IFrameEmbedderExpando;
VimeoExpando.prototype.process = function (target) {

    var width = Math.min(560, UI.Common.availableWidth($(target).parent()));

    //<iframe src="//player.vimeo.com/video/111431415" width="500" height="281" frameborder="0" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>
    this.hook($(target), 'Vimeo', {
        width: width.toString(),
        height: (width * this.defaultRatio),
        frameborder: '0',
        webkitallowfullscreen: 1,
        mozallowfullscreen: 1,
        allowfullscreen: 1
    });
};


UI.ImageExpandoSettings = (function () {
    return {
        autoLoad: true, //this setting will preload all image links
        autoShow: false, //if true then the click routine is run during event hookup
        maxSize: 250,
        toggleFunction: function (element, display) { //element (obj) to show/hide, display (bool) show/hide
            element.slideToggle();
        },
        onLoading: function (element, rawText) {
            element.html(rawText.concat('<span class=\'link-expando-type\'>loading</span>'));
        },
        onLoaded: function (element, rawText) {
            element.html(rawText.concat('<span class=\'link-expando-type\'>', UI.Common.fileExtension(element.prop('href')).toUpperCase(), '</span>'));
        },
        errorImageUrl: '~/Graphics/missing_image.png' //only relative path is supported right now.
    }
})();


$(document).ready(function () {

    UI.Common.debug = false;

    UI.ExpandoManager.addExpando('.usertext-body > .md a:not(.link-expando-direct)',
        [
            new ImageLinkExpando(),
            new YouTubeExpando(),
            new ImgurAlbumExpando(),
            new VimeoExpando()
        ]);

    if (UI.Common.isCommentPage()) {
        UI.ExpandoManager.execute();
    }

});







