using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voat
{
    public static class Extensions
    {
        public static string GetFirstErrorMessage(this System.Web.Mvc.ModelStateDictionary modelState)
        {
            var message = ErrorMessages(modelState).FirstOrDefault();
            return message;
        }
        private static IEnumerable<string> ErrorMessages(System.Web.Mvc.ModelStateDictionary modelState)
        {
            foreach (var kp in modelState)
            {
                foreach (var e in kp.Value.Errors)
                {
                    if (!String.IsNullOrEmpty(e.ErrorMessage))
                    {
                        yield return e.ErrorMessage;
                    }
                }
            }
        }
    }
}