using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Common
{
    public class PathOptions
    {
        public static PathOptions Default
        {
            get
            {
                return new PathOptions();
            }
        }
        public static PathOptions EnsureValid(PathOptions options)
        {
            if (options == null)
            {
                return Default;
            }
            return options;
        }
        public PathOptions() { }

        public PathOptions(bool fullyQualified, bool provideProtocol, string forceDomain = null)
        {
            this.FullyQualified = fullyQualified;
            this.ProvideProtocol = provideProtocol;
            this.ForceDomain = forceDomain;
        }

        public Normalization Normalization { get; set; } = Normalization.None;
        public bool EscapeUrl { get; set; } = true;
        public string ForceDomain { get; set; } = null;
        public bool FullyQualified { get; set; } = true;
        public bool ProvideProtocol { get; set; } = true;
    }
}
