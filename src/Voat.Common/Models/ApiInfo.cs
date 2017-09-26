using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Common.Models
{
    public class ApiInfo
    {
        public string EndPoint { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }

        public bool IsValid
        {
            get => !String.IsNullOrEmpty(EndPoint) && !String.IsNullOrEmpty(PublicKey) && !String.IsNullOrEmpty(PrivateKey);
        }
    }
}
