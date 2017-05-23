#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

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
