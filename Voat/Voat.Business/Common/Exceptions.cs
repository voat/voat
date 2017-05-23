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

namespace Voat.Common
{
    [Serializable]
    abstract public class VoatException : Exception
    {
        private string _friendlyType = "VoatException";

        public string FriendlyType
        {
            get
            {
                return _friendlyType;
            }
        }

        public VoatException(string friendlyType, string message)
            : base(message)
        {
            _friendlyType = friendlyType;
        }

        public VoatException(string friendlyType, string message, Exception innerExeption)
            : base(message, innerExeption)
        {
            _friendlyType = friendlyType;
        }

        public VoatException(string friendlyType, string format, params object[] args)
            : this(friendlyType, String.Format(format, args))
        {
        }
    }

    [Serializable]
    public class VoatRuleExeception : VoatException
    {
        public VoatRuleExeception()
            : this("Rule Exception")
        {
            /*no-op*/
        }

        public VoatRuleExeception(string message)
            : base("Rule", message)
        {
            /*no-op*/
        }

        public VoatRuleExeception(string format, params object[] args) : this(String.Format(format, args))
        {
        }
    }

    [Serializable]
    public class VoatSecurityException : VoatException
    {
        public VoatSecurityException()
            : this("Not authorized for action")
        {
            /*no-op*/
        }

        public VoatSecurityException(string message)
            : base("Security", message)
        {
            /*no-op*/
        }

        public VoatSecurityException(string format, params object[] args) : this(String.Format(format, args))
        {
        }
    }

    [Serializable]
    public class VoatValidationException : VoatException
    {
        public VoatValidationException()
            : this("A Voat validation exception has been encountered.")
        {
            /*no-op*/
        }

        public VoatValidationException(string message)
            : base("Validation", message)
        {
            /*no-op*/
        }

        public VoatValidationException(string format, params object[] args)
            : this(String.Format(format, args))
        {
            /*no-op*/
        }
    }
    [Serializable]
    public class VoatNotFoundException : VoatException
    {
        public VoatNotFoundException()
            : this("A Voat not found exception has been encountered.")
        {
            /*no-op*/
        }

        public VoatNotFoundException(string message)
            : base("NotFound", message)
        {
            /*no-op*/
        }

        public VoatNotFoundException(string format, params object[] args)
            : this(String.Format(format, args))
        {
            /*no-op*/
        }
    }
}
