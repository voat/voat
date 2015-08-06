using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Common
{

    abstract public class VoatException : Exception {

        private string _friendlyType = "VoatException";

        public string FriendlyType {
            get {
                return _friendlyType;
            }
        }

        public VoatException(string friendlyType, string message)
            : base(message) {
            _friendlyType = friendlyType;
        }
        public VoatException(string friendlyType, string message, Exception innerExeption)
            : base(message, innerExeption) {
            _friendlyType = friendlyType;
        }
        public VoatException(string friendlyType, string format, params object[] args)
            : this(friendlyType, String.Format(format, args)) {
        }
    }
    
    
    public class VoatRuleExeception : VoatException {

        public VoatRuleExeception()
            : this("Rule Exception") {
            /*no-op*/
        }
        public VoatRuleExeception(string message)
            : base("Rule", message) {
            /*no-op*/
        }

        public VoatRuleExeception(string format, params object[] args) : this(String.Format(format, args)) { }
    }
    public class VoatSecurityException : VoatException {

        public VoatSecurityException()
            : this("Not authorized for action") {
            /*no-op*/
        }
        public VoatSecurityException(string message)
            : base("Security", message) {
            /*no-op*/
        }

        public VoatSecurityException(string format, params object[] args) : this(String.Format(format, args)) { }
    }

    public class VoatValidationException : VoatException {

        public VoatValidationException()
            : this("A Voat validation exception has been encountered.") {
            /*no-op*/
        }

        public VoatValidationException(string message)
            : base("Validation", message) {
            /*no-op*/
        }

        public VoatValidationException(string format, params object[] args)
            : this(String.Format(format, args)) {
            /*no-op*/
        }
    }

    public class VoatNotFoundException : VoatException {

        public VoatNotFoundException()
            : this("A Voat not found exception has been encountered.") {
            /*no-op*/
        }

        public VoatNotFoundException(string message)
            : base("NotFound", message) {
            /*no-op*/
        }

        public VoatNotFoundException(string format, params object[] args)
            : this(String.Format(format, args)) {
            /*no-op*/
        }
    }

}
