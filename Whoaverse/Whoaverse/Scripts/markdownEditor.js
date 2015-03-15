
/* Adds left and right markdown tags to each line of the selected text
 * textComponent:		the textarea
 * leftTag:				text to add at the left of each line of the selected text
 * rightTag:			text to add at the right of each line of the selected text
 */
function addTagsToEachSelectedTextLine(textComponent, leftTag, rightTag) {
    var selectedText = getSelectionArray(textComponent);
    var selectedTextLines = selectedText[1].split("\n");
    //Add text located before the selected text (which should not have been modified)
    textComponent.value = selectedText[0];
    //Add markdown to each line of the selected text
    for (i = 0; i < selectedTextLines.length; i++) {
        textComponent.value += addTagsToText(selectedTextLines[i], leftTag, rightTag) + "\n";
    }
    //Add text located after the selected text (which should not have been modified)
    textComponent.value += selectedText[2];
    //Temporary variable so that I can put the cursor at intended position
    var cursorPos = textComponent.value.length - rightTag.length - selectedText[2].length;
    //Set the focus back on the text component at the intended position
    setSelectionRange(textComponent, cursorPos, cursorPos);
}

/* Adds left and right markdown tags to the selected text
 * textComponent:		the textarea
 * leftTag:				text to add at the left of the selected text
 * rightTag:			text to add at the right of the selected text
 */
function addTagsToSelectedText(textComponent, leftTag, rightTag) {
    var selectedText = getSelectionArray(textComponent);
    //Add text located before the selected text (which should not have been modified)
    textComponent.value = selectedText[0];
    //Add markdown to each line of the selected text
    textComponent.value += addTagsToText(selectedText[1], leftTag, rightTag);
    //Add text located after the selected text (which should not have been modified)
    textComponent.value += selectedText[2];
    //Temporary variable so that I can put the cursor at intended position
    var cursorPos = textComponent.value.length - rightTag.length - selectedText[2].length;
    //Set the focus back on the text component at the intended position
    setSelectionRange(textComponent, cursorPos, cursorPos);
}

/* Adds left and right tags to the given text
 * text:				the text that will be manipulated
 * leftTag:				the tag that will be added at the left side of the text
 * rightTag:			the tag that will be added at the right side of the text
 */
function addTagsToText(text, leftTag, rightTag) {
    return leftTag + text + rightTag;
}

/* Gets selection array
 * textComponent:		the textarea
 * returns				an array with the following format: [text before selection, selected text, text after selection] if there is any selected text, [entire textarea text,'',''] if there isn't any selected text
 */
function getSelectionArray(textComponent) {
    var selectedText = [textComponent.value, '', ''];
    //Check if there is any selected text
    if (textComponent.selectionStart != undefined) {
        var selectedStartPos = textComponent.selectionStart;
        var selectedEndPos = textComponent.selectionEnd;
        //Get text on the left of the selected text
        selectedText[0] = textComponent.value.substring(0, selectedStartPos)
        //Get selected text
        selectedText[1] = textComponent.value.substring(selectedStartPos, selectedEndPos)
        //Get text on the right of the selected text
        selectedText[2] = textComponent.value.substring(selectedEndPos, textComponent.value.length)
    }
    return selectedText;
}

/* Focus on the text component and select the text between selectionStart and selectionEnd
 * textComponent:		the textarea
 * selectionStart:		start position of the selection
 * selectionEnd:		end position of the selection
 */
function setSelectionRange(textComponent, selectionStart, selectionEnd) {
    if (textComponent.setSelectionRange) {
        textComponent.focus();
        textComponent.setSelectionRange(selectionStart, selectionEnd);
    }
    else if (textComponent.createTextRange) {
        var range = textComponent.createTextRange();
        range.collapse(true);
        range.moveEnd('character', selectionEnd);
        range.moveStart('character', selectionStart);
        range.select();
    }
}

/*
 * Increments the number of columns to create in the table
 * columns:				the element that contains the number of columns that the table will have
 * numColumnsDisplay:	the element that displays the number of columns that the table will have
 */
function incColumns(columns, numColumnsDisplay) {
    columns.value++;
    updateNumColumnsDisplay(columns, numColumnsDisplay);
}

/*
 * Decrements the number of columns to create in the table
 * columns:				the element that contains the number of columns that the table will have
 * numColumnsDisplay:	the element that displays the number of columns that the table will have
 */
function decColumns(columns, numColumnsDisplay) {
    if (columns.value > 1) {
        columns.value--;
    }
    updateNumColumnsDisplay(columns, numColumnsDisplay);
}

/*
 * Updates the number of columns display
 * columns:				the element that contains the number of columns that the table will have
 * numColumnsDisplay:	the element that displays the number of columns that the table will have
 */
function updateNumColumnsDisplay(columns, numColumnsDisplay) {
    numColumnsDisplay.innerHTML = columns.value;
}

/*
 * Increments the number of rows to create in the table
 * rows:				the element that contains the number of rows that the table will have
 * numRowsDisplay:		the element that displays the number of rows that the table will have
 */
function incRows(rows, numRowsDisplay) {
    rows.value++;
    updateNumRowsDisplay(rows, numRowsDisplay);
}

/*
 * Decrements the number of rows to create in the table
 * rows:				the element that contains the number of rows that the table will have
 * numRowsDisplay:		the element that displays the number of rows that the table will have
 */
function decRows(rows, numRowsDisplay) {
    if (rows.value > 1) {
        rows.value--;
    }
    updateNumRowsDisplay(rows, numRowsDisplay);
}

/*
 * Updates the number of rows display
 * rows:				the element that contains the number of rows that the table will have
 * numRowsDisplay:		the element that displays the number of rows that the table will have
 */
function updateNumRowsDisplay(rows, numRowsDisplay) {
    numRowsDisplay.innerHTML = rows.value;
}

/*
 * Creates the markdown for the table
 * textComponent:		the textarea
 * numColumns:			the number of columns that the table will have
 * numRows:				the number of rows that the table will have
 */
function createTable(textComponent, numColumns, numRows) {
    var selectedText = getSelectionArray(textComponent);
    if (numColumns >= 1 && numRows >= 1) {
        //Add text located before the selected text (which should not have been modified)
        textComponent.value = selectedText[0];
        //Add table markdown
        //Create first row (columns title)
        for (c = 0; c < numColumns; c++) {
            textComponent.value += "Title |";
        }
        textComponent.value += "\n";
        for (c = 0; c < numColumns; c++) {
            textComponent.value += "--- |";
        }
        textComponent.value += "\n";
        //Create next rows (columns content)
        for (r = 0; r < numRows; r++) {
            for (c = 0; c < numColumns; c++) {
                textComponent.value += "Text |";
            }
            textComponent.value += "\n";
        }
        //Add text located after the selected text (which should not have been modified)
        textComponent.value += selectedText[2];
        //Temporary variable so that I can put the cursor at intended position
        var cursorPos = textComponent.value.length - selectedText[2].length;
        //Set the focus back on the text component at the intended position
        setSelectionRange(textComponent, cursorPos, cursorPos);
    } else {
        alert('Error: Unable to create table! Invalid number of columns/rows.');
    }
}

/*
 * Creates the markdown for the hyperlink
 * textComponent:		the textarea
 * url:					the hyperlink's target
 */
function addHyperlink(textComponent, url) {
    if (url != "" && url !== null) {
		if (getSelectionArray(textComponent)[1] == ""){
			//No text selected, add "Title Here" to help user understand the markdown
			addTagsToSelectedText(textComponent, '[Title Here','](' + url + ')');
		} else {
			addTagsToSelectedText(textComponent, '[','](' + url + ')');
		}
    }
    textComponent.focus();
}

var markdown = new MarkdownDeep.Markdown();
markdown.ExtraMode = true;
markdown.SafeMode = true;

//Replace all matches with markdown
function renderMarkdown(outputString) {
    var searchRules = [
        {
            //MUST BE RUN FIRST
            "name": "URL",
            "regex": /((?:\W|^)(?:ht|f)tp(?:s?)\:\/\/[0-9a-zA-Z](?:[-.\w]*[0-9a-zA-Z])*(?::(0-9)*)*(?:\/?)(?:[a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_=]*))/g,
            "prefix": "",
            "suffix": ""
        },
        {
            "name": "User Mentions",
            "regex": /(?:\W|^)(?:(?:@|\/u\/)([a-zA-Z0-9-_]+))\b/g,
            "prefix": "/user/",
            "suffix": ""
        },
        {
            "name": "Subverse Mentions",
            "regex": /(?:\W|^)\/v\/([a-zA-Z0-9]+)/g,
            "prefix": "/v/",
            "suffix": ""
        },
        {
            "name": "Subreddit Mentions",
            "regex": /(?:\W|^)\/r\/([a-zA-Z0-9]+)/g,
            "prefix": "https://reddit.com/r/",
            "suffix": ""
        }
    ];
    for (var i = 0; i < searchRules.length; i++) {
        var currentRule = searchRules[i];
        outputString = outputString.replace(currentRule.regex, function () {
            return "[" + arguments[0] + "](" + currentRule.prefix + arguments[1] + currentRule.suffix + ")";
        });
    }
    return markdown.Transform(outputString);
}

function updatePreview(textarea) {
    textarea = $(textarea);
    var panel = textarea.siblings(".panel");
    var preview = panel.find(".md-preview");
    if (!textarea || !preview || !panel) { return; }
    var htmlString = renderMarkdown(textarea.val());
    preview.html(htmlString);
    if (htmlString) {
        panel.show();
    } else {
        panel.hide();
    }
}
//Initialize the preview for every box.
//Some browsers, like Chrome preserve entries through navigation.
//This ensures the preview is visible.
$(document).ready(function() {
    $("textarea[oninput]").each(function() {
        updatePreview(this);
    });
});