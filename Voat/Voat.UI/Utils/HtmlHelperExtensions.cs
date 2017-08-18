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

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Linq.Expressions;
using System.Text.Encodings.Web;

namespace Voat.UI.Utilities
{
    public static class HtmlHelperExtensions {

        public static HtmlString MarkdownEditorFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes) {

            var textareaString = htmlHelper.TextAreaFor(expression, htmlAttributes);

            var renderEditor = true;
            var writer = new System.IO.StringWriter();
            if (renderEditor)
            {
                var editor = htmlHelper.Partial("~/Views/Shared/_MarkdownEditor.cshtml");
                editor.WriteTo(writer, HtmlEncoder.Default);
            }

            textareaString.WriteTo(writer, HtmlEncoder.Default);
            return new HtmlString(writer.ToString());
        }
        
    }
}
