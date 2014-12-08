//Whoaverse UI JS framework - Version 0.5beta - 12/08/2014
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
    },
    queryString: function (name, url) {

        if (!url || url.length == 0) {
            url = location.href;
        }

        if (url.indexOf('?') > 0) {
            var qs = url.split('?')[1];
            var qspairs = qs.split('&');
            var kvpairs = [];
            //add
            for (var i; i < qspairs.length; i++) {
                var x = kv.split('=');
                var val = x[1];
                kvpairs.push({ 'key': x[0], 'value': unescape(val) });
            }
            //find
            for (var i = 0; i < keypairs.length; i++) {
                var kvpair = kvpairs[i];
                if (kvpair.key == name) {
                    return kvpair.value;
                }
            }
        }

        return null;
    },
    resizeTarget: function (target, sizeUp, parent) {
        try {
            var useCSS = false;
            var w = target.prop('width');
            var h = target.prop('height');
            if (w == 0 || h == 0) {
                w = parseInt(target.css('width'));
                h = parseInt(target.css('height'));
                useCSS = true;
            }
            var ar = w / h;
            var daddy = (typeof parent === 'object' ? parent : target.parent());
            var maxWidth = UI.Common.availableWidth(daddy);
            if (maxWidth < w || (sizeUp && maxWidth > w)) {
                if (useCSS) {
                    target.css('width', maxWidth);
                    target.css('height', (maxWidth / ar));
                } else {
                    target.prop('width', maxWidth);
                    target.prop('height', (maxWidth / ar));
                }
            }
        } catch (ex) {
            if (UI.Common.debug) {
                throw ex;
            }
        }
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
                                return expando.getFilter().test(this.href);
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
    var filter = hrefFilter;
    this.getFilter = function () {
        return filter;
    }
    this.getId = function (path) {
        var id = undefined;
        try {
            id = this.getFilter().exec(path)[1];//returns first matched group
        } catch (ex) {
        }
        return id;
    }
};
LinkExpando.prototype = {
    process: function (target) {
        //no-op - this is a base class
        if (UI.Common.debug) {
            alert("Class LinkExpando.process(target) method must be overridden in derived class.")
        }
    }
};
LinkExpando.setDirectLink = function (parentControl, description, url){
    var infoSpan = $('<span/>', { class: 'tagline' }).html(description + ' ').append($('<a/>', { class: 'link-expando-direct', target: '_blank', href: url }).text('Open'));
    parentControl.append(infoSpan);
}
LinkExpando.setTag = function (target, tagText) {
    if (!target.data('text')) {
        target.data('text', target.text());
    }
    if (tagText) {
        target.html(target.data('text').concat('<span class=\'link-expando-type\'>', tagText, '</span>'))
    } else {
        //revert 
        target.text(target.data('text'));
    }
}
LinkExpando.dataProp = function (target, prop, value) {
    if (value) {
        $(target).data(prop, value);
    }
    return $(target).data(prop)
}
LinkExpando.isLoaded = function (target, value) {
    return LinkExpando.dataProp(target, 'loaded', value);
}
LinkExpando.isVisible = function (target, value) {
    return LinkExpando.dataProp(target, 'visible', value)
}
LinkExpando.isHooked = function (target, value) {
    return LinkExpando.dataProp(target, 'hooked', value);
}
LinkExpando.toggle = function (target, display) {
    target.slideToggle();
}
var ImageLinkExpando = (function () {
    var countAutoLoaded = 0;

    return function () {
        LinkExpando.call(this, /^([^\?]+(\.(jpg|jpeg|gif|giff|png)))$/i);
        this.autoLoadedCount = function () {
            return countAutoLoaded;
        }
        this.process = function (targetObj) {

            var target = $(targetObj);

            //target.data('showing', false);

            if (LinkExpando.isHooked(target)) {
                return;
            }
            
            target.on('click', ImageLinkExpando.onClick);

            LinkExpando.isHooked(target, true);
            LinkExpando.setTag(target, UI.Common.fileExtension(target.prop('href')).toUpperCase());
            LinkExpando.dataProp(target, 'src', target.prop('href'));

            if (UI.ImageExpandoSettings.autoLoad) {
                ImageLinkExpando.loadImage(target, target.prop('href'));
            } else if (UI.ImageExpandoSettings.autoShow) {
                target.click();
            } 
        };

    }
})();
ImageLinkExpando.prototype = new LinkExpando();
ImageLinkExpando.prototype.constructor = ImageLinkExpando;
ImageLinkExpando.onClick = function(event) {
    event.preventDefault();

    var target = $(this);

    if (!LinkExpando.isVisible(target)) {
        //show
        if (LinkExpando.isLoaded(target)) {
            LinkExpando.isVisible(target, true);
            LinkExpando.toggle(target.next(), true);
        } else {
            //load
            ImageLinkExpando.loadImage(target, LinkExpando.dataProp(target, 'src'));
        }
    } else {
        //hide
        LinkExpando.toggle(target.next(), false);
        LinkExpando.isVisible(target, false);
    }
}

ImageLinkExpando.loadImage = function (target, href, autoLoading) {

        LinkExpando.setTag(target, "loading");
        LinkExpando.dataProp(target, 'src', href);

        //remove handler while loading
        target.off();
        target.on('click', function (e) { e.preventDefault(); }); //disable the link until loaded, prevent rapid clickers

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

            var desc = UI.Common.fileExtension(href).toUpperCase().concat(' · ', width.toString(), ' x ', height.toString());

            LinkExpando.setDirectLink(displayDiv, desc, href);
            target.prop('title', desc);


            //I HAVE NO IDEA WHY I HAVE TO DO THIS TO REMOVE THE width/height attributes of the image tag itself
            i.css('width', width);
            i.css('height', height);
            this.removeAttribute('width');
            this.removeAttribute('height');

            var startSize = (UI.ImageExpandoSettings.initialSize != 0 ? UI.ImageExpandoSettings.initialSize : UI.Common.availableWidth(target.parent()));

            if (width > startSize) {
                if (width >= height || UI.ImageExpandoSettings.initialSize == 0) {
                    i.css('width', startSize);
                    i.css('height', 'auto');

                    i.data('origWidth', startSize);
                    i.data('origHeight', 'auto');
                } else {
                    i.css('width', 'auto');
                    i.css('height', startSize);

                    i.data('origWidth', 'auto');
                    i.data('origHeight', startSize);
                }
                i.data('inFullMode', false);

                i.data('maxWidth', Math.min(width, UI.Common.availableWidth(target.parent())));

                if (startSize < UI.Common.availableWidth(target.parent())) {
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
            }

            LinkExpando.isLoaded(target, true);

            //reestablish handler
            target.off();
            target.on('click', ImageLinkExpando.onClick);

            LinkExpando.setTag(target, UI.Common.fileExtension(href).toUpperCase());

            if (!autoLoading) {
                LinkExpando.isVisible(target, true);
                LinkExpando.toggle(displayDiv, true);
            }

            if (autoLoading && UI.ImageExpandoSettings.autoShow) {
                target.click();
            }
        };
        img.src = href;
    }


/* HTML5 Video Expando */
var VideoLinkExpando = (function () {

    var vid = document.createElement('video');

    return function (urlRegEx) {
        LinkExpando.call(this, urlRegEx);

        this.isMP4Supported = function () { return vid.canPlayType && vid.canPlayType('video/mp4; codecs="avc1.42E01E,mp4a.40.2"');};
        this.isWEBMSupported = function () { return vid.canPlayType && vid.canPlayType('video/webm; codecs="vp8,vorbis"');};
        this.isVideoSupported = function () { return this.isMP4Supported() || this.isWEBMSupported(); };
        this.embedVideo = function (target, videoProps, sources, description) {

            var item = $('<video/>', videoProps);

            UI.Common.resizeTarget(item, false, target.parent());

            if (sources.length > 0) {
                for (var i = 0; i < sources.length; i++) {
                    item.append($('<source/>', sources[i]));
                }
            } else {
                return;
            }

            var displayDiv = $('<div/>', {
                class: 'link-expando',
                style: 'display:none;'
            }).append(item).insertAfter(target);


            LinkExpando.setTag(target, description);
            LinkExpando.setDirectLink(displayDiv, description, target.prop('href'));

            LinkExpando.isLoaded(target, true);

            return displayDiv;

        }
    }

})();
VideoLinkExpando.prototype = new ImageLinkExpando();
VideoLinkExpando.prototype.constructor = VideoLinkExpando;

var GfyCatLinkExpando = function () {
    LinkExpando.call(this, /gfycat\.com\/([^"&?\/\.]*)/i);
    this.hook = function (target) {

        if (LinkExpando.isHooked(target)) {
            return;
        } else {
            LinkExpando.isHooked(target, true);
        }

        LinkExpando.dataProp(target, 'id', this.getId(target.prop('href')));

        var me = this;
        target.on('click', function (e) {

            e.preventDefault();
            var target = $(this);


            if (!LinkExpando.isLoaded(target)) {
                LinkExpando.setTag(target, "loading");
                me.getSourceInfo(LinkExpando.dataProp(target, 'id'), 
                    function (result) {

                        if (me.isVideoSupported()) {
                            //vid
                            var div = me.embedVideo(target,
                                {
                                    'width': result.gfyItem.width,
                                    'height': result.gfyItem.height,
                                    'autoplay': 1,
                                    'loop': 1
                                },
                                [{
                                    'id': 'mp4source',
                                    'src': result.gfyItem.mp4Url,
                                    'type': 'video/mp4'
                                },
                                {
                                    'id': 'webmsource',
                                    'src': result.gfyItem.webm,
                                    'type': 'video/webm'
                                }], 'GfyCat Video'
                            );

                            LinkExpando.isLoaded(target, true);
                            LinkExpando.toggle(div, true);

                        } else {
                            //gif - load using default ImageLinkExpando logic
                            ImageLinkExpando.loadImage(target, result.gfyItem.gifUrl);
                        }

                    },
                    function (result) {
                        //bail
                        LinkExpando.setTag(target, 'Error');
                        target.off('click');
                    }
                );
            }
            target.next().slideToggle();

        });
        LinkExpando.setTag($(target), "GfyCat");

    }
    this.getSourceInfo = function (id, fnCallback, fnErrorHandler) {

        $.ajax({
            url:'http://gfycat.com/cajax/get/' + id, 
            type: 'GET'
        }).done(fnCallback).fail(fnErrorHandler);

    }

}
GfyCatLinkExpando.prototype = new VideoLinkExpando();
GfyCatLinkExpando.prototype.constructor = GfyCatLinkExpando;
GfyCatLinkExpando.prototype.process = function (target) {
    this.hook($(target));
}

var ImgurGifvLinkExpando = function () {

    LinkExpando.call(this, /i\.imgur\.com\/([^"&?\/\.]*)\.gifv/i);
    this.getSrcUrl = function(id, extension) {
        return 'http://i.imgur.com/'.concat(id, extension);
    }
    this.hook = function (target) {

        if (LinkExpando.isHooked(target)) {
            return;
        } else {
            LinkExpando.isHooked(target, true);
        }

        LinkExpando.dataProp(target, 'id', this.getId(target.prop('href')));

        var me = this;
        target.on('click', function (e) {

            e.preventDefault();
            var target = $(this);

            var id = me.getId(target.prop('href'));

            if (!LinkExpando.isLoaded(target)) {
                LinkExpando.setTag(target, "loading");
               
                if (me.isVideoSupported()) {
                    //vid
                    var div = me.embedVideo(target,
                        {
                            'width': '100%',
                            'height': 'auto',
                            'autoplay': 1,
                            'loop': 1
                        },
                        [{
                            'id': 'mp4source',
                            'src': me.getSrcUrl(id, '.mp4'),
                            'type': 'video/mp4'
                        },
                        {
                            'id': 'webmsource',
                            'src': me.getSrcUrl(id, '.webm'),
                            'type': 'video/webm'
                        }], 'Imgur Gifv Video'
                    );

                    LinkExpando.isLoaded(target, true);
                } else {
                    //kill it, it looks like imgur removes .gif files
                    target.off('click');
                    LinkExpando.setTag(target);
                    //ImageLinkExpando.loadImage(target, me.getSrcUrl(id, '.gif'));
                }

            }
            target.next().slideToggle();

        });
        LinkExpando.setTag($(target), "Imgur Gifv");

    }

}
ImgurGifvLinkExpando.prototype = new VideoLinkExpando();
ImgurGifvLinkExpando.prototype.constructor = ImgurGifvLinkExpando;
ImgurGifvLinkExpando.prototype.process = function (target) {
    this.hook($(target));
}



/* IFrameEmbedder */
var IFrameEmbedderExpando = function (urlRegEx) {
    LinkExpando.call(this, urlRegEx);
    this.defaultRatio = 0.5625;
    this.hook = function (target, description, iFrameSettings) {

        if (LinkExpando.isHooked(target)) {
            return;
        } else {
            LinkExpando.isHooked(target, true);
        }
        
        var id = this.getId(target.prop('href'));
        if (!id) {
            return;
        }

        LinkExpando.dataProp($(target), 'source', this.getSrcUrl(id));
        target.prop('title', description);
        
        target.on('click',
            function (e) {
                e.preventDefault();

                var target = $(this);
                if (!LinkExpando.isLoaded(target)) {
                    var displayDiv = $('<div/>', {
                        class: 'link-expando',
                        style: 'display:none;'
                    });
                    //<iframe width="560" height="315" src="//www.youtube.com/embed/JUDSeb2zHQ0" frameborder="0" allowfullscreen></iframe>
                    iFrameSettings.src = LinkExpando.dataProp(target, 'source');
                    var iFrame = $('<iframe/>', iFrameSettings);
                    displayDiv.html(iFrame);
                    LinkExpando.setDirectLink(displayDiv, description, target.prop('href'));
                    LinkExpando.isLoaded(target, true);

                    displayDiv.insertAfter(target);
                    UI.Common.resizeTarget($('iframe', displayDiv), false, target.parent());
                }
                target.next().slideToggle();
            });

        LinkExpando.setTag(target, description);

    }
}
IFrameEmbedderExpando.prototype = new LinkExpando();
IFrameEmbedderExpando.prototype.constructor = IFrameEmbedderExpando;

/* YouTube */
var YouTubeExpando = function(){
    IFrameEmbedderExpando.call(this, /(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^"&?\/ ]{11})/i);
    this.getSrcUrl = function (id) { return '//www.youtube.com/embed/' + id; };
};
YouTubeExpando.prototype = new IFrameEmbedderExpando();
YouTubeExpando.prototype.constructor = YouTubeExpando;
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
ImgurAlbumExpando.prototype.constructor = ImgurAlbumExpando;
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
    IFrameEmbedderExpando.call(this, /vimeo\.com\/(?:.\*|.*\/)?([\d]+)\/?/i);
    this.getSrcUrl = function (id) { return '//player.vimeo.com/video/' + id; };
};
VimeoExpando.prototype = new IFrameEmbedderExpando();
VimeoExpando.prototype.constructor = VimeoExpando;
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

/* SoundCloud - UNDONE */
var SoundCloudExpando = function () {
    IFrameEmbedderExpando.call(this, /xxx/i);
};
SoundCloudExpando.prototype = new IFrameEmbedderExpando();
SoundCloudExpando.prototype.constructor = SoundCloudExpando;
SoundCloudExpando.prototype.process = function (target) {
    //TODO
    var width = Math.min(560, UI.Common.availableWidth($(target).parent()));
    //<iframe width="100%" height="450" scrolling="no" frameborder="no" src="https://w.soundcloud.com/player/?url=https%3A//api.soundcloud.com/tracks/179814178&amp;auto_play=false&amp;hide_related=false&amp;show_comments=true&amp;show_user=true&amp;show_reposts=false&amp;visual=true"></iframe>
  
};


UI.ImageExpandoSettings = (function () {
    return {
        autoLoad: false, //this setting will preload all image links
        autoShow: false, //if true then the click routine is run during event hookup
        initialSize: 0, //max size for initial display, if image exceeds this a click toggle is enabled. A value of 0 == max container width.
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
            new VimeoExpando(),
            new ImgurGifvLinkExpando(),
            new GfyCatLinkExpando()
        ]);

    if (UI.Common.isCommentPage()) {
        UI.ExpandoManager.execute();
    }

    UI.Notifications.subscribe('iFrameLoaded', function (context) {
        var iframe = $('iframe', context);
        if (iframe) {
            UI.Common.resizeTarget(iframe, false, iframe.parent());
        }
    });


});







