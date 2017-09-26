using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Configuration;

namespace Voat.UI.Runtime
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            //{2} is area, {1} is controller,{0} is the action
            //"/Views/{2}/{1}/{0}.cshtml" 
            string[] locations = new string[] { };

            if (context.AreaName != null)
            {
                if (VoatSettings.Instance.AreaMaps.Values.Any(x => x.IsEqual(context.AreaName)))
                {
                    var keyValue = VoatSettings.Instance.AreaMaps.FirstOrDefault(x => x.Value.IsEqual(context.AreaName));
                    locations = new string[] { $"/Areas/{keyValue.Key}/Views/{{1}}/{{0}}.cshtml" };
                }
            }
            return locations.Union(viewLocations); //Add mvc default locations after ours
        }


        public void PopulateValues(ViewLocationExpanderContext context)
        {
            context.Values["customviewlocation"] = nameof(ViewLocationExpander);
        }
    }
}
