using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voat.Controllers;
using Voat.Utilities;

namespace Voat.UI.Areas.Admin.Controllers
{

    [Authorize(Roles = "GlobalAdmin,Admin,DelegateAdmin,GlobalBans,GlobalJanitor")]
    public class BaseAdminController : BaseController
    {
        public BaseAdminController() {
            
        }
    }
}
