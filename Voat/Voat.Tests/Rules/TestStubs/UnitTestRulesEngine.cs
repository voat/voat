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

using Voat.RulesEngine;

namespace Voat.Tests.Rules
{
    public class UnitTestRulesEngine : Voat.Rules.VoatRulesEngine
    {
        public UnitTestRulesEngine(bool initialize = false) : base(/*new ThreadIsolatedContextHandler<VoatRequestContext>()*/)
        {
            _engine = this;
            if (initialize)
            {
                Initialize(RuleSection.Instance.Configuration);
            }
            Instance = this;
        }
    }
}
