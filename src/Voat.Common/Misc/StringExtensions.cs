using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Voat.Common
{
    public static class StringExtensions
    {
        public static Dictionary<string, string> ToKeyValuePairs(this string value, string pairDelim = ";", string keyValueDelim = "=")
        {
            return ToKeyValuePairs(value, new string[] { pairDelim }, new string[] { keyValueDelim });
        }
        public static Dictionary<string, string> ToKeyValuePairs(this string value, string[] pairDelim, string[] keyValueDelim)
        {
            var keyValuePairs = value.Split(pairDelim, StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Split(keyValueDelim, StringSplitOptions.RemoveEmptyEntries))
                                .Where(x => x.Length == 2)
                                .ToDictionary(x => x.First().TrimSafe(), x => x.Last().TrimSafe());
            return keyValuePairs;
        }

        public static string NullIfEmpty(this string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return null;
            }
            return text;
        }

        public static bool IsTrimSafeNullOrEmpty(this string text)
        {
            return String.IsNullOrEmpty(text.TrimSafe());
        }
        public static string TrimSafe(this string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                return text.TrimWhiteSpace();
            }
            return text;
        }
        public static string TrimSafe(this string text, params string[] trimStrings)
        {
            if (!String.IsNullOrEmpty(text))
            {
                var trimmed = text.StripWhiteSpace();
                if (trimStrings != null && trimStrings.Length > 0)
                {
                    trimmed = trimStrings.Aggregate(trimmed, (result, trimString) => {
                        if (result.StartsWith(trimString))
                        {
                            result = result.Substring(trimString.Length, result.Length - trimString.Length);
                        }
                        if (result.EndsWith(trimString))
                        {
                            result = result.Substring(0, result.Length - trimString.Length);
                        }
                        return result;
                    });
                }
                return trimmed;

            }
            return text;
        }
        public static string ToNormalized(this string value, Normalization normalization)
        {
            if (!String.IsNullOrEmpty(value))
            {
                switch (normalization)
                {
                    case Normalization.Lower:
                        return value.ToLower();
                        break;
                    case Normalization.Upper:
                        return value.ToUpper();
                        break;
                }
            }
            return value;
        }

        public static string ToRelativePath(this string url)
        {
            if (!url.IsTrimSafeNullOrEmpty())
            {
                var uri = new Uri(url);
                return uri.AbsolutePath;
            }
            return url;
        }
        /// <summary>
        /// Reverses a string based on seperator. preview.voat.co becomes co.voat.preview
        /// </summary>
        /// <param name="content"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static string ReverseSplit(this string content, string seperator = ".")
        {
            if (!String.IsNullOrEmpty(content))
            {
                return String.Join(seperator, content.Split(new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries).Reverse());
            }
            return content;
        }
        public static IEnumerable<string> ToPathParts(this IEnumerable<string> relativePaths, IEnumerable<string> additionalParts = null)
        {
            List<string> parts = new List<string>();
            relativePaths.ToList().ForEach(x =>
            {
                parts.AddRange(x.ToPathParts());
            });
            if (additionalParts != null && additionalParts.Count() > 0)
            {
                parts.AddRange(additionalParts);
            }
            return parts.AsEnumerable();
        }

        public static IEnumerable<string> ToPathParts(this string relativePath)
        {
            relativePath = relativePath.TrimStart('~');
            var parts = relativePath.Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable();
            parts = parts.Select(x => x.TrimSafe()).Where(x => !String.IsNullOrEmpty(x)).ToList();
            return parts.AsEnumerable();
        }

        // credits to http://stackoverflow.com/questions/1613896/truncate-string-on-whole-words-in-net-c-sharp
        public static string TruncateAtWord(this string input, int length)
        {
            if (input == null || input.Length < length)
                return input;
            var iNextSpace = input.LastIndexOf(" ", length, StringComparison.Ordinal);
            return string.Format("{0}...", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
        }

        // check if a string contains unicode characters
        public static bool ContainsUnicode(this string stringToTest, bool includeUnprintableChars = true)
        {
            const int maxAnsiCode = 255;

            //Adding constraint for unprintable characters
            int minAnsiCode = (includeUnprintableChars ? 32 : 0);
            return stringToTest.Any(c => (c > maxAnsiCode || c < minAnsiCode));
        }

        public static string StripWhiteSpace(this string stringToClean)
        {
            var scrubbed = stringToClean;
            if (!String.IsNullOrEmpty(scrubbed))
            {
                scrubbed = Regex.Replace(scrubbed, @"\s{2,}", " ").Trim();
            }
            return scrubbed;
        }
        public static string TrimWhiteSpace(this string stringToClean)
        {
            var scrubbed = stringToClean;
            if (!String.IsNullOrEmpty(scrubbed))
            {
                scrubbed = Regex.Replace(scrubbed, @"^\s{1,}", "").Trim();
                scrubbed = Regex.Replace(scrubbed, @"\s{1,}$", "").Trim();
            }
            return scrubbed;
        }
        // string unicode characters from a string
        public static string StripUnicode(this string stringToClean, bool includeUnprintableChars = true, bool includeWhitespace = true)
        {
            var scrubbed = stringToClean;
            if (!String.IsNullOrEmpty(scrubbed))
            {
                scrubbed = Regex.Replace(scrubbed, String.Format(@"[^\u00{0}-\u00FF]", (includeUnprintableChars ? "20" : "00")), string.Empty).Trim();

                //remove sequential whitespace
                if (includeWhitespace)
                {
                    scrubbed = StripWhiteSpace(scrubbed);
                }
            }
            return scrubbed;
        }

        public static bool IsImageExtension(this string fileName)
        {
            var imageExtensions = new string[] { ".jpg", ".png", ".gif", ".jpeg" };
            var file = Path.GetExtension(fileName);
            return imageExtensions.Any(x => x.IsEqual(file));
        }
    }
}
