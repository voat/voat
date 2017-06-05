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

//CORE_PORT: Unsupported
//[assembly: OwinStartup(typeof(Startup))]
namespace Voat
{
    public partial class Startup
    {

        //CORE_PORT: Changes in setup
        public void Configuration(object app)
        {
            throw new NotImplementedException("Core port problem");
        }
        /*
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            if (!Settings.SignalRDisabled)
            {
                // wire up SignalR
                var config = new HubConfiguration()
                {
                    EnableDetailedErrors = false,
                    EnableJSONP = false,
                    EnableJavaScriptProxies = true
                };
                app.MapSignalR(config);
            }
        }
        */
    }
}
