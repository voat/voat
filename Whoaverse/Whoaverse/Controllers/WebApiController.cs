/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
*/

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Whoaverse.Models;

namespace Whoaverse.Controllers
{
    public class WebApiController : ApiController
    {
        private whoaverseEntities db = new whoaverseEntities();
        
        // GET api/defaultsubverses
        [System.Web.Http.HttpGet]
        public IEnumerable<string> DefaultSubverses()
        {
            var listOfDefaultSubverses = db.Defaultsubverses.OrderBy(s => s.position).ToList();

            List<string> tmpList = new List<string>();
            foreach (var item in listOfDefaultSubverses)
            {
                tmpList.Add(item.name);
            }

            return tmpList;
        }

    }
}