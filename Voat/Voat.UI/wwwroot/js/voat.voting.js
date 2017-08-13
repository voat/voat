var voat = {
    voting: {
        //UI Stuff
        addOption: function (source) {
            $.ajax({
                type: 'GET',
                url: '/vote/element?type=VoteOption',
                success: function (data) {
                    $('div[data-voat-list="Options"]').append(data);
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

                }
            });
        },
        removeItem: function (source, itemClass) {
            var itemToRemove = $(source).parents(itemClass);
            itemToRemove.remove();
        },

        //BUILD JSON
        populateModelList: function (model, container, listName) {

            var selector = '*[data-voat-item="' + listName + '"]';
            var items = container.find(selector);

            for (var i = 0; i < items.length; i++) {
                model[i] = {};
                var listItem = $(items[i]);
                voat.voting.populateModel(model[i], listItem, listName, 1);
            }
        },
        populateModelFields: function (model, item, listName) {

            var fields = item.find('*[data-voat-field="' + listName + '"]');
            //var fields = voat.voting.filterLevel(allFields, '[data-voat-field]', item.parents('[data-voat-field]').length);
            $(fields).each(function () {
                var field = $(this);
                model[field.attr('name')] = field.val();
            });

        },
        populateModel: function (model, rootSelector, listName, level) {

            //populate fields
            voat.voting.populateModelFields(model, $(rootSelector), listName);

            //populate lists
            var allLists = $(rootSelector).find('*[data-voat-list]')
            var lists = voat.voting.filterLevel(allLists, '*[data-voat-list]', level);
            $(lists).each(function () {
                var list = $(this);
                var listName = list.attr('data-voat-list');
                model[listName] = [];
                var listModel = model[listName];
                voat.voting.populateModelList(listModel, list, listName)
            });
        },

        filterLevel: function (items, selector, nestLevel) {
            var filtered = [];
            //var currentNestLevel = $(selector).parents(selector).length;
            //relativeNestLevel += currentNestLevel;
            $(items).each(function () {
                var item = $(this);
                if (item.parents(selector).length == nestLevel) {
                    filtered.push(item);
                }
            })
            return filtered;
        },
        save: function () {
            //var r = $('[data-voat-item="vote"]').validate();

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

            //Mine
            //var model = {};
            //voat.voting.populateModel(model, '*[data-voat-item="vote"]', "vote", 0);
            // Fuzzy
            var model = toFuzzyModel(null, true, 0, false);
            //var puttsModel = model;


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
            $.ajax({
                type: 'POST',
                url: '/vote/save',
                data: JSON.stringify(model),
                contentType: "application/json",
                error: function () {
                    var args = arguments;
                },
                success: function (data) {
                    $("#container").html(data);
                    //document.open();
                    //document.write(data);
                    //document.close();
                }
            });

        }
    }
}
        //$('#saveVote').on('submit', function (e) {
        //    //e.preventDefault();
        //    $.ajax({
        //        url: 'submit.php',
        //        cache: false,
        //        type: 'POST',
        //        data: $('#formID').serialize(),
        //        success: function (json) {
        //            alert('all done');
        //        }
        //    });
        //});