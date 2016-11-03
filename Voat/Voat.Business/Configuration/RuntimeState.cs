#region LICENSE

/*

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All portions of the code written by Voat, Inc. are Copyright(c) Voat, Inc.

    All Rights Reserved.

*/

#endregion LICENSE

using System;

namespace Voat
{
    /// <summary>
    /// This setting specifies which mode the runtime is set to.
    /// </summary>
    public enum RuntimeStateSetting
    {
        /// <summary>
        /// Api is disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Api is in a read-only state.
        /// </summary>
        Read = 1,

        /// <summary>
        /// Api is in a write-only state.
        /// </summary>
        Write = 2,

        /// <summary>
        /// Api is fully enabled.
        /// </summary>
        Enabled = Read | Write,

        /// <summary>
        /// Api is fully enabled.
        /// </summary>
        ReadWrite = Read | Write,
    }

    public static class RuntimeState
    {
        /// <summary>
        /// The key name in the <AppSettings> section
        /// </summary>
        public const string API_CONFIG_KEY_NAME = "runtimeState";

        private static RuntimeStateSetting _setting = RuntimeStateSetting.Enabled;

        static RuntimeState()
        {
            Refresh();
        }

        public static event EventHandler<RuntimeStateSetting> OnStateChanged;

        public static RuntimeStateSetting Current
        {
            get
            {
                return _setting;
            }
        }
        public static RuntimeStateSetting Parse(string setting)
        {
            var value = RuntimeStateSetting.Disabled;

            if (String.IsNullOrEmpty(setting))
            {
                value = RuntimeStateSetting.Enabled; //by default keep enabled
            }
            else
            {
                RuntimeStateSetting configSetting = RuntimeStateSetting.Disabled;

                //Parse enum value
                if (Enum.TryParse(setting, true, out configSetting))
                {
                    value = configSetting;
                }
                else
                {
                    //Support "true" and "false" values in the web.config and map them to Enabled and Disabled
                    bool enabled = false;
                    if (Boolean.TryParse(setting, out enabled))
                    {
                        value = (enabled ? RuntimeStateSetting.Enabled : RuntimeStateSetting.Disabled);
                    }
                    else
                    {
                        value = RuntimeStateSetting.Disabled;
                    }
                }
            }

            return value;
        }
        public static void Refresh(string setting)
        {
            var _current = _setting;
            
            _setting = Parse(setting);

            if (_current != _setting)
            {
                if (OnStateChanged != null)
                {
                    OnStateChanged(typeof(RuntimeState), _setting);
                }
            }

        }
        public static void Refresh()
        {
            var setting = System.Configuration.ConfigurationManager.AppSettings[API_CONFIG_KEY_NAME];
            Refresh(setting);
        }
    }
}
