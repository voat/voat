var voat = {
    voting: {
        //UI Stuff
        syncValidationChanges: function() {
            $("form").removeData("validator");
            $("form").removeData("unobtrusiveValidation");
            $.validator.unobtrusive.parse("form");
        },
        addOption: function (source) {
            $.ajax({
                type: 'GET',
                url: '/vote/element?type=VoteOption',
                success: function (data) {
                    $('div[data-voat-list="Options"]').append(data);
                    voat.voting.syncValidationChanges();
                }
            });
        },
        addRestriction: function (source) {
            var type = $("#vote-restriction-type").val();

            $.ajax({
                type: 'GET',
                url: '/vote/element?type=' + type,
                success: function (data) {
                    $('div[data-voat-list="Restrictions"]').append(data);
                    voat.voting.syncValidationChanges();
                }
            });
        },
        addOutcome: function (source) {

            var type = $(source).closest("[data-voat-item]").find("#vote-outcome-type").val();
            var caller = source;
            $.ajax({
                type: 'GET',
                url: '/vote/element?type=' + type,
                success: function (data) {

                    var optionItem = $(caller).closest('div[data-voat-item="Options"]');
                    var outcomeList = optionItem.find('div[data-voat-list="Outcomes"]');
                    outcomeList.append(data);
                    voat.voting.syncValidationChanges();
                }
            });
        },
        removeItem: function (source, itemClass) {
            var itemToRemove = $(source).parents(itemClass);
            itemToRemove.remove();
        },
        gimmieTheFormMEOW: function() {
            $("form").submit(function(x) {
                if ($(x.currentTarget).valid()) {
                    voat.voting.save();
                }
                return false;
            })
        },
        save: function () {

            function filterItemsByLevel(level) {
                return function () {
                    return $(this).parents('[data-voat-item]').length === level;
                };
            }
            function stripPathedName(name) {
                var split = name.split('.');
                if (split.length > 0) {
                    return split[split.length - 1];
                }
                return name;
            }
            function toFuzzyModel(parent, isItem, level, includeEmptyFields) {
                var selector, item,
                    object = isItem ? {} : [];
                if (parent === null) {
                    item = $('[data-voat-item]').filter(filterItemsByLevel(0));
                    //root - depends if you want root or not, we don't
                    /*object[item.data('vote-item')] = listToObject(item, true, 1);
                    return object;*/
                    return toFuzzyModel(item, true, 1);
                }
                if (isItem) {
                    selector = '*[data-voat-list],*[data-voat-field]';
                } else {
                    selector = '*[data-voat-item]';
                }
                var items = parent.find(selector).filter(filterItemsByLevel(level));
                if (isItem) {
                    for (var i = 0; i < items.length; i++) {
                        item = $(items[i]);
                        if (item.is('[data-voat-field]')) {
                            var val = item.val();
                            if (val != null && val != undefined && val.toString().length > 0 && !includeEmptyFields || includeEmptyFields) {
                                object[stripPathedName(item.attr('name'))] = val;
                            }
                        } else {
                            object[item.data('voat-list')] = toFuzzyModel(item, false, level, includeEmptyFields);
                        }
                    }
                } else {
                    for (var i = 0; i < items.length; i++) {
                        object.push(toFuzzyModel($(items[i]), true, level + 1, includeEmptyFields));
                    }
                }
                return object;
            }

            var model = toFuzzyModel(null, true, 0, false);
            
            //translate to Model Site Expects
            for (var i = 0; i < model.Restrictions.length; i++) {
                var item = model.Restrictions[i];
                var newItem = {};
                newItem.Type = item.Type;
                newItem.Options = JSON.stringify(item);
                model.Restrictions[i] = newItem;
            }
            for (var i = 0; i < model.Options.length; i++) {
                var item = model.Options[i];

                for (var i2 = 0; i2 < item.Outcomes.length; i2++) {
                    var item2 = item.Outcomes[i2];
                    var newItem = {};
                    newItem.Type = item2.Type;
                    newItem.Options = JSON.stringify(item2);
                    item.Outcomes[i2] = newItem;

                }
            }

            //Post
            $.ajax({
                type: 'POST',
                url: '/vote/save',
                data: JSON.stringify(model),
                contentType: "application/json",
                error: function () {
                    var args = arguments;
                },
                success: function (data) {
                    $("#container").replaceWith(data);
                    voat.voting.syncValidationChanges();
                    voat.voting.gimmieTheFormMEOW();
                }
            });

        }
    }
}
voat.voting.gimmieTheFormMEOW();
