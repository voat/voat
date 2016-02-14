//Voat UI JS framework
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
    htmlEscape: function(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    },
    isMobile: function () {
        return false; //TODO: determine what conditions qualify for a 'mobile' view
    },
    isCommentPage: function () {
        return /\/comments\//i.test(window.location.href);
    },
    isMessagePage: function () {
        return /\/messaging\//i.test(window.location.href);
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
    currentProtocol: function () {
        return location.protocol;
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
var LinkExpando = function (hrefFilter, optionsObject) {
    var filter = hrefFilter;
    this.options = optionsObject;
    this.getFilter = function () {
        return filter;
    }
    this.getId = function (path) {
        var id = undefined;
        try {
            id = this.getFilter().exec(path)[1];//returns first matched group
        } catch (ex) {
            /*no-op*/
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
    if (target.data('text') === undefined) {
        target.data('text', target.text());
    }
    if (tagText) {
        target.html(UI.Common.htmlEscape(target.data('text')).concat('<span class=\'link-expando-type\'>', tagText, '</span>'))
    } else {
        //revert 
        target.text(UI.Common.htmlEscape(target.data('text')));
    }
}
LinkExpando.dataProp = function (target, prop, value) {
    if (value != null) {
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

    return function (options) {
        LinkExpando.call(this, /^([^\?]+(\.(jpg|jpeg|gif|giff|png)))$/i, options);
        this.autoLoadedCount = function () {
            return countAutoLoaded;
        }
        this.process = function (source) {

            //target action button
            var target = (this.options.targetFunc ? this.options.targetFunc($(source)) : $(source));
            var source = $(source);

            if (LinkExpando.isHooked(target)) {
                return;
            }
            var me = this;

            target.on('click', (function(event) {me.onClick(event)}));

            LinkExpando.isHooked(target, true);
            var type = UI.Common.fileExtension(source.prop('href')).toUpperCase();
            target.prop('title', type);
            if (options.setTags) {
                LinkExpando.setTag(target, type);
            }
            LinkExpando.dataProp(target, 'src', source.prop('href'));

            if (UI.ImageExpandoSettings.autoLoad) {
                this.loadImage(target, source.prop('href'));
            } else if (UI.ImageExpandoSettings.autoShow) {
                target.click();
            } 
        };
        this.onClick = function (event) {
            event.preventDefault();

            var target = this.options.targetFunc($(event.target));

            if (!LinkExpando.isVisible(target)) {
                //show
                if (LinkExpando.isLoaded(target)) {
                    LinkExpando.isVisible(target, true);
                    LinkExpando.toggle(this.options.destinationFunc(target), true);
                    if (this.options.toggle) {
                        this.options.toggle(target);
                    }
                } else {
                    //load
                    this.loadImage(target, LinkExpando.dataProp(target, 'src'));
                }
            } else {
                //hide
                LinkExpando.toggle(this.options.destinationFunc(target), false);
                if (this.options.toggle) {
                    this.options.toggle(target);
                }

                LinkExpando.isVisible(target, false);
            }
        };
        this.loadImage = function (target, href, autoLoading) {

            if (this.options.loading) {
                this.options.loading(target);
            }
            if (this.options.setTags) {
                LinkExpando.setTag(target, "loading");
            }
            LinkExpando.dataProp(target, 'src', href);

            //remove handler while loading
            target.off();
            target.on('click', function (e) { e.preventDefault(); }); //disable the link until loaded, prevent rapid clickers

            var img = new Image();
            img.onerror = function () {
                //can't determine what kind of error... could be 404, could be a working non-image source, etc.
                img.src = UI.Common.resolveUrl(UI.ImageExpandoSettings.errorImageUrl);
            }

            var me = this;
            img.onload = (function () {

                if (!this.complete) {
                    return;
                }

                var parent = target.parent();
                var destination = me.options.destinationFunc(target);

                var displayDiv = destination;

                var i = $(this);

                //Get natural dimensions. Since the image is not yet rendered, its dimensions are its natural ones
                var width = this.width, height = this.height;
                //Render image
                displayDiv.html(i);

                var desc = UI.Common.fileExtension(href).toUpperCase().concat(' · ', width.toString(), ' x ', height.toString());

                LinkExpando.setDirectLink(displayDiv, desc, href);
                target.prop('title', desc);


                //I HAVE NO IDEA WHY I HAVE TO DO THIS TO REMOVE THE width/height attributes of the image tag itself
                this.removeAttribute('width');
                this.removeAttribute('height');
                i.css("min-width", Math.min(width, 68));
                i.css("max-width", width);
                i.data("nwidth", width);
                //Hide drag shadow
                i.attr("draggable", false);
                //Hide right click menu as it is remapped to scale image
                i.attr("oncontextmenu", "return false");

                var touchData;
                i.on("mousedown", function (event) {
                    if (event.which === 3) {
                        //Right click, scale image to its natural size
                        i.css("width", width);
                        event.preventDefault();
                        return;
                    }
                    touchData = {
                        x: event.pageX,
                        origWidth: i.width()
                    };
                    $("body").on("mousemove", resizeListener);
                    $("body").on("mouseup", mouseUpListener);
                });
                var resizeListener = function (event) {
                    var deltaX = event.pageX - touchData.x;
                    i.css("width", touchData.origWidth + deltaX);
                    i.css("max-width", "");
                }
                var mouseUpListener = function () {
                    $("body").off("mousemove", resizeListener);
                    $("body").off("mouseup", mouseUpListener);
                };

                LinkExpando.isLoaded(target, true);

                //reestablish handler
                target.off();
                target.on('click', (function (event) {
                    if (!LinkExpando.isVisible(target)) {
                        //Reset image width
                        i.css("max-width", width);
                        i.css("width", "");
                    }
                    me.onClick(event);
                }));
                if (me.options.setTags) {
                    LinkExpando.setTag(target, UI.Common.fileExtension(href).toUpperCase());
                }
                if (!autoLoading) {
                    LinkExpando.isVisible(target, true);
                    if (me.options.toggle) {
                        me.options.toggle(target);
                    }
                    LinkExpando.toggle(displayDiv, true);
                }
                if (autoLoading && UI.ImageExpandoSettings.autoShow) {
                    target.click();
                }
            });
            img.src = href;
        }

    }
})();
ImageLinkExpando.prototype = new LinkExpando();
ImageLinkExpando.prototype.constructor = ImageLinkExpando;

/* HTML5 Video Expando */
var VideoLinkExpando = (function () {

    var vid = document.createElement('video');

    return function (urlRegEx) {
        LinkExpando.call(this, urlRegEx);

        this.isMP4Supported = function () {
            return this.isSupported('video/mp4', 'avc1.42E01E,mp4a.40.2');
        };
        this.isWEBMSupported = function () {
            return this.isSupported('video/webm', 'vp8.0,vorbis');
        };
        this.isSupported = function (type, codec) {
            if (vid.canPlayType) {
                var result = vid.canPlayType(type + ';' + (codec ? ' codecs="' + codec + '"' : ''));
                return ['probably', 'maybe'].indexOf(result) >= 0;
            }
            return false;
        };
        this.isVideoSupported = function () {
            return this.isMP4Supported() || this.isWEBMSupported();
        };
        this.embedVideo = function (source, videoProps, sources, description) {
            
            var item = $('<video/>', videoProps);
            var target = this.options.targetFunc(source);
            target.prop('title', description);
            

            if (sources.length > 0) {
                for (var i = 0; i < sources.length; i++) {
                    item.append($('<source/>', sources[i]));
                }
            } else {
                return;
            }

            var destination = this.options.destinationFunc(target);
            UI.Common.resizeTarget(item, false, destination.parent());

            destination.empty().append(item);

            if (this.options.setTags) {
                LinkExpando.setTag(target, description);
            }
            LinkExpando.setDirectLink(destination, description, source.prop('href'));

            LinkExpando.isLoaded(target, true);

            return destination;

        }
    }

})();
VideoLinkExpando.prototype = new ImageLinkExpando();
VideoLinkExpando.prototype.constructor = VideoLinkExpando;

var GfycatExpando = function (options) {
    LinkExpando.call(this, /gfycat\.com\/([A-Z]{1}[a-z]+[A-Z]{1}[a-z]+[A-Z]{1}[a-z]+)/, options);
    this.hook = function (source) {

        var target = this.options.targetFunc(source);
        target.prop('title', 'Gfycat');

        if (LinkExpando.isHooked(target)) {
            return;
        } else {
            LinkExpando.isHooked(target, true);
        }

        LinkExpando.dataProp(target, 'id', this.getId(source.prop('href')));

        var me = this;
        target.on('click', function (e) {

            e.preventDefault();
            //var target = $(e.target);
            if (!LinkExpando.isLoaded(target)) {
                if (me.options.loading) {
                    me.options.loading(target);
                }
                if (me.options.setTags) {
                    LinkExpando.setTag(target, "loading");
                }
                function fnError(result) {
                    //bail
                    if (me.options.setTags) {
                        LinkExpando.setTag(target, 'Error');
                    }
                    target.off('click');
                }
                me.getSourceInfo(LinkExpando.dataProp(target, 'id'), 
                    function (result) {

                        if (!result.gfyItem) {
                            fnError();
                            return;
                        }

                        if (me.isVideoSupported()) {
                            //vid
                            var div = me.embedVideo(source,
                                {
                                    'width': result.gfyItem.width,
                                    'height': result.gfyItem.height,
                                    'autoplay': 1,
                                    'loop': 1
                                },
                                [{
                                    'id': 'mp4gfycat',
                                    'src': result.gfyItem.mp4Url,
                                    'type': 'video/mp4'
                                },
                                {
                                    'id': 'webmgfycat',
                                    'src': result.gfyItem.webm,
                                    'type': 'video/webm'
                                }], 'Gfycat Video'
                            );

                            LinkExpando.isLoaded(target, true);
                            //LinkExpando.toggle(div, true);

                        } else {
                            //gif - load using default ImageLinkExpando logic
                            ImageLinkExpando.loadImage(target, result.gfyItem.gifUrl);
                        }

                    },
                    fnError
                );
            }
            me.options.destinationFunc(target).slideToggle();
            me.options.toggle(target);
        });
        if (me.options.setTags) {
            LinkExpando.setTag($(target), "Gfycat");
        }


    }
    this.getSourceInfo = function (id, fnCallback, fnErrorHandler) {
       
        try {
            $.ajax({
                url: UI.Common.currentProtocol() + '//gfycat.com/cajax/get/' + id,
                type: 'GET'
            }).done(fnCallback).fail(fnErrorHandler);
        } catch (e) {
            fnErrorHandler();
        }
    }

}
GfycatExpando.prototype = new VideoLinkExpando();
GfycatExpando.prototype.constructor = GfycatExpando;
GfycatExpando.prototype.process = function (target) {
    this.hook($(target));
}

var ImgurGifvExpando = function (options) {

    LinkExpando.call(this, /i\.imgur\.com\/([^"&?\/\.]*)\.gifv/i, options);
    this.getSrcUrl = function(id, extension) {
        return 'http://i.imgur.com/'.concat(id, extension);
    }
    this.hook = function (source) {


        var target = this.options.targetFunc(source);
        target.prop('title', 'Gifv');

        if (LinkExpando.isHooked(target)) {
            return;
        } else {
            LinkExpando.isHooked(target, true);
        }
        
        LinkExpando.dataProp(target, 'id', this.getId(source.prop('href')));

        var me = this;
        target.on('click', function (e) {

            e.preventDefault();

            //var target = me.options.targetFunc($(e.target));

            var id = me.getId(source.prop('href'));

            if (!LinkExpando.isLoaded(target)) {
               
                if (me.options.setTags) {
                    LinkExpando.setTag(target, "loading");
                }
                if (me.isVideoSupported()) {
                    //vid
                    var div = me.embedVideo(source,
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
                        }], 'Gifv Video'
                    );

                    LinkExpando.isLoaded(target, true);
                } else {
                    //kill it, it looks like imgur removes .gif files
                    target.off('click');
                    LinkExpando.setTag(target);
                }
            }
            me.options.destinationFunc(target).slideToggle();
            me.options.toggle(target);
        });
        if (me.options.setTags) {
            LinkExpando.setTag($(target), "Gifv");
        }
    }

}
ImgurGifvExpando.prototype = new VideoLinkExpando();
ImgurGifvExpando.prototype.constructor = ImgurGifvExpando;
ImgurGifvExpando.prototype.process = function (source) {
    this.hook($(source));
}



/* IFrameEmbedder */
var IFrameEmbedderExpando = function (urlRegEx, options) {
    LinkExpando.call(this, urlRegEx, options);
    this.defaultRatio = 0.5625;
    this.hook = function (source, description, iFrameSettings) {

        var target = this.options.targetFunc(source);

        if (LinkExpando.isHooked(target)) {
            return;
        } else {
            LinkExpando.isHooked(target, true);
        }
        
        var id = this.getId(source.prop('href'));
        if (!id) {
            return;
        }

        LinkExpando.dataProp(target, 'source', this.getSrcUrl(id));
        target.prop('title', description);
        
        var me = this;
        target.on('click',
            function (event) {
                event.preventDefault();

                var target = me.options.targetFunc($(event.target));
                var displayDiv = me.options.destinationFunc(target);
                if (!LinkExpando.isLoaded(target)) {

                    if (me.options.loading) {
                        me.options.loading(target);
                    }

                    //<iframe width="560" height="315" src="//www.youtube.com/embed/JUDSeb2zHQ0" frameborder="0" allowfullscreen></iframe>
                    iFrameSettings.src = LinkExpando.dataProp(target, 'source');
                    var iFrame = $('<iframe/>', iFrameSettings);
                    displayDiv.empty().html(iFrame);
                    LinkExpando.setDirectLink(displayDiv, description, source.prop('href'));
                    LinkExpando.isLoaded(target, true);

                    //displayDiv.insertAfter(target);
                    UI.Common.resizeTarget($('iframe', displayDiv), false, target.parent());
                }
                LinkExpando.isVisible(target, !LinkExpando.isVisible(target));
                me.options.toggle(target);
                me.options.destinationFunc(target).slideToggle(400, function () {
                    if (!LinkExpando.isVisible(target) && LinkExpando.isLoaded(target)) {
                        displayDiv.empty();
                        LinkExpando.isLoaded(target, false);
                    }
                });
            });
        if (me.options.setTags) {
            LinkExpando.setTag(target, description);
        }

    }
}
IFrameEmbedderExpando.prototype = new LinkExpando();
IFrameEmbedderExpando.prototype.constructor = IFrameEmbedderExpando;

/* YouTube */
var YouTubeExpando = function (options) {
    IFrameEmbedderExpando.call(this, /(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^"&?\/ ]{11})/i, options);
    this.getSrcUrl = function (id) { return '//www.youtube.com/embed/' + id; };
};
YouTubeExpando.prototype = new IFrameEmbedderExpando();
YouTubeExpando.prototype.constructor = YouTubeExpando;
YouTubeExpando.prototype.process = function (source) {
    var target = this.options.targetFunc($(source));

    var width = Math.min(560, UI.Common.availableWidth(target.parent()));

    this.hook($(source), 'YouTube', {
        width: width.toString(),
        height: (width * this.defaultRatio).toString(),
        frameborder: '0',
        allowfullscreen: true
    });
};

/* Imgur Album */
var ImgurAlbumExpando = function (options) {
    IFrameEmbedderExpando.call(this, /imgur\.com\/a\/(\w+)\/?/i, options);
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
var VimeoExpando = function (options) {
    IFrameEmbedderExpando.call(this, /vimeo\.com\/(?:.\*|.*\/)?([\d]+)\/?/i, options);
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

/* COUB */
var CoubExpando = function (options) {
    IFrameEmbedderExpando.call(this, /coub\.com\/(?:v|view|embed)\/(\w+)/i, options);
    this.getSrcUrl = function (id) { return '//coub.com/embed/' + id; };
};
CoubExpando.prototype = new IFrameEmbedderExpando();
CoubExpando.prototype.constructor = CoubExpando;
CoubExpando.prototype.process = function (target) {

    var width = Math.min(560, UI.Common.availableWidth($(target).parent()));

    //<iframe src="//coub.com/embed/cz1s" width="640" height="360" frameborder="0" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>
    this.hook($(target), 'Coub', {
        width: width.toString(),
        height: (width * this.defaultRatio),
        frameborder: '0',
        webkitallowfullscreen: 1,
        mozallowfullscreen: 1,
        allowfullscreen: 1
    });
};

/* SoundCloud */
var SoundCloudExpando = function () {
    var clientId = 'ab19f68dc1985a1b24752d987c91b7aa';
    IFrameEmbedderExpando.call(this, /xxx/i);
};
SoundCloudExpando.prototype = new IFrameEmbedderExpando();
SoundCloudExpando.prototype.constructor = SoundCloudExpando;
SoundCloudExpando.prototype.process = function (target) {
    //TODO
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

UI.SidebarHandler = function () {
    //Check if there is a sidebar on the current page
    if (!$(".side").exists()) {
        $("#show-menu-button").hide();
        return;
    }
    //The div that is the sidebar
    var sidebar = $(".side");
    //The button to shwo the sidebar when mobile
    var showMenuBtn = $("#show-menu-button");
    //The background to the sidebar
    var modalBg = $("#modal-background");
    //The body of the page
    var body = $("body");
    
    //Add a click listener
    showMenuBtn.on("click", function () {
        modalBg.toggleClass("show-mobile-sidebar");
        showMenuBtn.toggleClass("show-mobile-sidebar");
        sidebar.toggleClass("show-mobile-sidebar");
        body.toggleClass("show-mobile-sidebar");
    });

    modalBg.on("click", function () {
        hideSidebar();
    });

    function hideSidebar () {
        modalBg.toggleClass("show-mobile-sidebar", false);
        showMenuBtn.toggleClass("show-mobile-sidebar", false);
        sidebar.toggleClass("show-mobile-sidebar", false);
        body.toggleClass("show-mobile-sidebar", false);
    }

    //Media query listener to remove class when resized to desktop size
    var mql = window.matchMedia("(min-width: 870px)");
    mql.addListener(handleMediaQuery);

    function handleMediaQuery(mql) {
        if (mql.matches) {
            hideSidebar();
        }
    }
}


$(document).ready(function () {

    UI.Common.debug = false;
    //comment expandos
    var commentOptions = {
        targetFunc: function (source) {
            var anchor = source;
            if (anchor.is('span')) {
                anchor = source.parent();
            }
            return anchor;
        },
        destinationFunc: function (target) {
            var anchor = this.targetFunc(target);
            var container = target.next('.link-expando');
            if (container.length == 0) {
                var displayDiv = $('<div/>', {
                    class: 'link-expando',
                    style: 'display:none;'
                });
                displayDiv.insertAfter(target);
                container = target.next('.link-expando');
            }
            return container;
        },
        toggle: function (target) {
           /*no-op*/
        },
        setTags: true
    };
    UI.ExpandoManager.addExpando('.usertext-body > .md a:not(.link-expando-direct), .panel-message-body a:not(.link-expando-direct)',
        [
            new ImageLinkExpando(commentOptions),
            new YouTubeExpando(commentOptions),
            new VimeoExpando(commentOptions),
            new CoubExpando(commentOptions),
            new GfycatExpando(commentOptions),
            //new SoundCloudExpando,
            new ImgurAlbumExpando(commentOptions),
            new ImgurGifvExpando(commentOptions)
        ]);


    //Submission Expando Options
    var submissionOptions =  {
        targetFunc: function (source) {
            var container = source.parent().parent().find(".expando-button");
            if (container.length == 0) {
                //add <div class="expando-button collapsed selftext"></div>
                var displayDiv = $('<div/>', {
                    class: 'expando-button collapsed selftext'
                });
                displayDiv.insertAfter(source.parent())
                container = source.parent().parent().find(".expando-button");
            }
            return container;
        },
        destinationFunc: function (target) {
            var container = target.parent().find(".expando");
            if (container.length == 0) {
                var displayDiv = $('<div/>', {
                    class: 'expando collapsed link-expando',
                    style: 'display:none;'
                });
                target.parent().append(displayDiv);
                container = target.parent().find('.expando');
            }
            if (!container.hasClass('link-expando')) {
                container.addClass("link-expando");
            }
            return container;
        },
        toggle: function (target) {
            if (target.hasClass('loading')) {
                target.addClass('expanded');
                target.removeClass('loading');
                target.removeClass('collapsed');
            } else if (target.hasClass('collapsed')) {
                target.removeClass('collapsed');
                target.addClass('expanded');
            } else {
                target.removeClass('expanded');
                target.addClass('collapsed');
            }
        },
        loading: function (target) {
            target.addClass('loading');
            target.removeClass('collapsed');
            target.removeClass('expanded');
        },
        setTags: false
    };

    UI.ExpandoManager.addExpando('.submission .entry .title a:not(.link-expando-direct)',
        [
            new ImageLinkExpando(submissionOptions),
            new YouTubeExpando(submissionOptions),
            new VimeoExpando(submissionOptions),
            new GfycatExpando(submissionOptions),
            //new SoundCloudExpando,
            new ImgurAlbumExpando(submissionOptions),
            new ImgurGifvExpando(submissionOptions)
        ]);


    //if (UI.Common.isCommentPage() || UI.Common.isMessagePage()) {
        UI.ExpandoManager.execute();
    //}

    UI.Notifications.subscribe('iFrameLoaded', function (context) {
        var iframe = $('iframe', context);
        if (iframe) {
            UI.Common.resizeTarget(iframe, false, iframe.parent());
        }
    });

    //Reset image expando size on resize
    $(window).on('resize', function () {
        $(".link-expando img").each(function() {
            var naturalWidth = $(this).data("nwidth");
            $(this).css({ "width": "", "max-width": naturalWidth });
        });
    });

    UI.SidebarHandler();
});







