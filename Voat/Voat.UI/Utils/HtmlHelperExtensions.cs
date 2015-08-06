using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.WebPages;

namespace Voat.UI.Utilities {
    public static class HtmlHelperExtensions {

        public static MvcHtmlString MarkdownEditorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes) {
            
            var textareaString = TextAreaExtensions.TextAreaFor(htmlHelper, expression, htmlAttributes);

            var renderEditor = true; 

            if (renderEditor) {
                var editor = htmlHelper.Partial("_MarkdownEditor");
                return new MvcHtmlString(editor.ToString() + textareaString);
            } else {
                return textareaString;
            }

        }
      
    }
}