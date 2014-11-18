//CommentImageHandler.js - Version 0.1 - 11/13/2014
//NOTES: You must remove the following CSS from the stylesheets:
//.md img { display: none; }


var UI = UI || {};

UI.CommentImageHandler = {

	Execute: function (settings) {

		$(settings.selector).filter(function () { return settings.filter.test(this.href) }).each(function (i, x) {

			$(x).data("showing", false);

			$(x).click(function (event) {
				event.preventDefault();

				var target = $(this);
				var msettings = settings;

				if (!target.data("showing")) {
					//show
					if (target.data("loaded")) {
						target.data("showing", true);
						msettings.toggleFunction(target.next(), true);
					} else {

						var img = new Image();
						img.onload = function () {
							if (!this.complete) {
								return;
							}

							var anchorText = target.text();
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

							//I HAVE NO IDEA WHY I HAVE TO DO THIS TO REMOVE THE width/height attributes of the image tag itself
							i.css('width', width);
							i.css('height', height);
							this.removeAttribute("width");
							this.removeAttribute("height");

							if (width > msettings.maxSize || height > msettings.maxSize) {
								if (width >= height) {
									i.css("width", msettings.maxSize);
									i.css("height", "auto");

									i.data("origWidth", msettings.maxSize);
									i.data("origHeight", "auto");

								} else {
									i.css("width", "auto");
									i.css("height", msettings.maxSize);

									i.data("origWidth", "auto");
									i.data("origHeight", msettings.maxSize);
								}
								i.data("inFullMode", false);

								displayDiv.click(function () {
									var childImg = $(this).children("img");
									if (childImg.data('inFullMode')) {
										childImg.css("width", childImg.data('origWidth'));
										childImg.css("height", childImg.data('origHeight'));
										childImg.data('inFullMode', false);
									} else {
										childImg.css("width", "auto");
										childImg.css("height", "auto");
										childImg.data('inFullMode', true);
									}
								});
								displayDiv.css('cursor', 'pointer');
							}


							target.data("showing", true);
							target.data("loaded", true);
							msettings.toggleFunction(displayDiv, true);
						};
						img.src = target.attr("href");
					}

				} else {
					//hide
					msettings.toggleFunction(target.next(), false);
					target.data("showing", false);
				}


			});
			//kick it off if set to true
			if (settings.autoShow) {
				$(x).click();
			}
		});
	}
};

function CommentImageHandlerSettings() {

	this.autoShow = true; //if true then the click routine is run during event hookup
	this.selector = ".usertext-body > .md a"; //elements to find image anchor tags
	this.filter = /(.png$|.jpg$|.jpeg$|.gif$|.giff$)/i; //regex for href links to load
	this.maxSize = 250;

	this.toggleFunction = function (element, display) { //element (obj) to show/hide, display (bool) show/hide
		element.slideToggle();
	};
	//TODO: Settings that need implementing
	this.autoLoad = true; //this setting will preload all image links

}

$(document).ready(function () {

	var s = new CommentImageHandlerSettings();
	UI.CommentImageHandler.Execute(s);

});







