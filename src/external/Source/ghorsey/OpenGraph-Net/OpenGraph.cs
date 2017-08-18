
namespace OpenGraph_Net
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using HtmlAgilityPack;

    /// <summary>
    /// Represents Open Graph meta data parsed from HTML
    /// </summary>
    public class OpenGraph : IDictionary<string, string>
    {
        /// <summary>
        /// The required meta
        /// </summary>
        private static readonly string[] RequiredMeta = { "title", "type", "image", "url" };

        /// <summary>
        /// The open graph data
        /// </summary>
        private readonly IDictionary<string, string> openGraphData;

        /// <summary>
        /// The local alternatives
        /// </summary>
        private IList<string> localAlternatives;

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type of open graph document.</value>
        public string Type { get; private set; }

        /// <summary>
        /// Gets the title of the open graph document.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the image for the open graph document.
        /// </summary>
        /// <value>The image.</value>
        public Uri Image { get; private set; }

        /// <summary>
        /// Gets the URL for the open graph document
        /// </summary>
        /// <value>The URL.</value>
        public Uri Url { get; private set; }

        /// <summary>
        /// Gets the original URL used to generate this graph
        /// </summary>
        /// <value>The original URL.</value>
        public Uri OriginalUrl { get; private set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="OpenGraph" /> class from being created.
        /// </summary>
        private OpenGraph()
        {
            this.openGraphData = new Dictionary<string, string>();
            this.localAlternatives = new List<string>();
        }

        /// <summary>
        /// Makes the graph.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="type">The type.</param>
        /// <param name="image">The image.</param>
        /// <param name="url">The URL.</param>
        /// <param name="description">The description.</param>
        /// <param name="siteName">Name of the site.</param>
        /// <param name="audio">The audio.</param>
        /// <param name="video">The video.</param>
        /// <param name="locale">The locale.</param>
        /// <param name="localeAlternate">The locale alternate.</param>
        /// <param name="determiner">The determiner.</param>
        /// <returns><see cref="OpenGraph"/></returns>
        public static OpenGraph MakeGraph(
            string title,
            string type,
            string image,
            string url,
            string description = "",
            string siteName = "",
            string audio = "",
            string video = "",
            string locale = "",
            IList<string> localeAlternate = null,
            string determiner = "")
        {
            var graph = new OpenGraph
            {
                Title = title,
                Type = type,
                Image = new Uri(image, UriKind.Absolute),
                Url = new Uri(url, UriKind.Absolute)
            };

            graph.openGraphData.Add("title", title);
            graph.openGraphData.Add("type", type);
            graph.openGraphData.Add("image", image);
            graph.openGraphData.Add("url", url);

            if (!string.IsNullOrWhiteSpace(description))
            {
                graph.openGraphData.Add("description", description);
            }

            if (!string.IsNullOrWhiteSpace(siteName))
            {
                graph.openGraphData.Add("site_name", siteName);
            }

            if (!string.IsNullOrWhiteSpace(audio))
            {
                graph.openGraphData.Add("audio", audio);
            }

            if (!string.IsNullOrWhiteSpace(video))
            {
                graph.openGraphData.Add("video", video);
            }

            if (!string.IsNullOrWhiteSpace(locale))
            {
                graph.openGraphData.Add("locale", locale);
            }

            if (!string.IsNullOrWhiteSpace(determiner))
            {
                graph.openGraphData.Add("determiner", determiner);
            }

            if (localeAlternate != null)
            {
                graph.localAlternatives = localeAlternate;
            }

            return graph;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var doc = new HtmlDocument();

            foreach (var itm in this.openGraphData)
            {
                var meta = doc.CreateElement("meta");
                meta.Attributes.Add("property", "og:" + itm.Key);
                meta.Attributes.Add("content", itm.Value);
                doc.DocumentNode.AppendChild(meta);
            }

            foreach (var itm in this.localAlternatives)
            {
                var meta = doc.CreateElement("meta");
                meta.Attributes.Add("property", "og:locale:alternate");
                meta.Attributes.Add("content", itm);
                doc.DocumentNode.AppendChild(meta);
            }

            return doc.DocumentNode.InnerHtml;
        }

        /// <summary>
        /// Downloads the HTML of the specified URL and parses it for open graph content.
        /// </summary>
        /// <param name="url">The URL to download the HTML from.</param>
        /// <param name="userAgent">The user agent to use when downloading content.  The default is <c>"facebookexternalhit"</c> which is required for some site (like amazon) to include open graph data.</param>
        /// <param name="validateSpecifiction">if set to <c>true</c> <see cref="OpenGraph"/> will validate against the specification.</param>
        /// <returns>
        ///   <see cref="OpenGraph" />
        /// </returns>
        public static OpenGraph ParseUrl(string url, string userAgent = "facebookexternalhit", bool validateSpecifiction = false)
        {
            Uri uri = new Uri(url);
            return ParseUrl(uri, userAgent, validateSpecifiction);
        }


        /// <summary>
        /// Parses the URL asynchronous.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="userAgent">The user agent.</param>
        /// <param name="validateSpecifiction">if set to <c>true</c> [validate specifiction].</param>
        /// <returns><see cref="Task{OpenGraph}"/></returns>
        public static Task<OpenGraph> ParseUrlAsync(string url, string userAgent = "facebookexternalhit", bool validateSpecifiction = false)
        {
            Uri uri = new Uri(url);
            return ParseUrlAsync(uri, userAgent, validateSpecifiction);
        }

        /// <summary>
        /// Parses the URL asynchronous.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="userAgent">The user agent.</param>
        /// <param name="validateSpecification">if set to <c>true</c> [validate specification].</param>
        /// <returns><see cref="Task{OpenGraph}"/></returns>
        public static async Task<OpenGraph> ParseUrlAsync(Uri url, string userAgent = "facebookexternalhit", bool validateSpecification = false)
        {
            OpenGraph result = new OpenGraph { OriginalUrl = url };

            HttpDownloader downloader = new HttpDownloader(url, null, userAgent);
            string html = await downloader.GetPageAsync();

            return ParseHtml(result, html, validateSpecification);
        }

        /// <summary>
        /// Downloads the HTML of the specified URL and parses it for open graph content.
        /// </summary>
        /// <param name="url">The URL to download the HTML from.</param>
        /// <param name="userAgent">The user agent to use when downloading content.  The default is <c>"facebookexternalhit"</c> which is required for some site (like amazon) to include open graph data.</param>
        /// <param name="validateSpecification">if set to <c>true</c> verify that the document meets the required attributes of the open graph specification.</param>
        /// <returns><see cref="OpenGraph"/></returns>
        public static OpenGraph ParseUrl(Uri url, string userAgent = "facebookexternalhit", bool validateSpecification = false)
        {
            OpenGraph result = new OpenGraph { OriginalUrl = url };

            HttpDownloader downloader = new HttpDownloader(url, null, userAgent);
            string html = downloader.GetPage();

            return ParseHtml(result, html, validateSpecification);
        }

        /// <summary>
        /// Parses the HTML for open graph content.
        /// </summary>
        /// <param name="content">The HTML to parse.</param>
        /// <param name="validateSpecification">if set to <c>true</c> verify that the document meets the required attributes of the open graph specification.</param>
        /// <returns><see cref="OpenGraph"/></returns>
        public static OpenGraph ParseHtml(string content, bool validateSpecification = false)
        {
            OpenGraph result = new OpenGraph();
            return ParseHtml(result, content, validateSpecification);
        }

        /// <summary>
        /// Parses the HTML.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="content">The content.</param>
        /// <param name="validateSpecification">if set to <c>true</c> [validate specification].</param>
        /// <returns><see cref="OpenGraph"/></returns>
        /// <exception cref="OpenGraph_Net.InvalidSpecificationException">The parsed HTML does not meet the open graph specification</exception>
        private static OpenGraph ParseHtml(OpenGraph result, string content, bool validateSpecification = false)
        {
            int indexOfClosingHead = Regex.Match(content, "</head>").Index;
            string toParse = content.Substring(0, indexOfClosingHead + 7);

            toParse = toParse + "<body></body></html>\r\n";

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(toParse);

            HtmlNodeCollection allMeta = document.DocumentNode.SelectNodes("//meta");
            var urlPropertyPatterns = new[] { "image", "url^"};
            var openGraphMetaTags = from meta in allMeta ?? new HtmlNodeCollection(null)
                                    where (meta.Attributes.Contains("property") && meta.Attributes["property"].Value.StartsWith("og:")) ||
                                    (meta.Attributes.Contains("name") && meta.Attributes["name"].Value.StartsWith("og:"))
                                    select meta;

            foreach (HtmlNode metaTag in openGraphMetaTags)
            {
                string value = GetOpenGraphValue(metaTag);
                string property = GetOpenGraphKey(metaTag);
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (result.openGraphData.ContainsKey(property))
                {
                    continue;
                }

                foreach (var urlPropertyPattern in urlPropertyPatterns)
                {
                    if (Regex.IsMatch(property, urlPropertyPattern))
                    {
                        value = HtmlDecodeUrl(value);
                        break;
                    }
                }
                result.openGraphData.Add(property, value);
            }

            string type;
            result.openGraphData.TryGetValue("type", out type);
            result.Type = type ?? string.Empty;

            string title;
            result.openGraphData.TryGetValue("title", out title);
            result.Title = title ?? string.Empty;

            try
            {
                string image;
                result.openGraphData.TryGetValue("image", out image);
                result.Image = new Uri(image ?? string.Empty);
            }
            catch (UriFormatException)
            {
                // do nothing
            }
            catch (ArgumentException)
            {
                // do nothing
            }

            try
            {
                string url;
                result.openGraphData.TryGetValue("url", out url);
                result.Url = new Uri(url ?? string.Empty);
            }
            catch (UriFormatException)
            {
                // do nothing
            }
            catch (ArgumentException)
            {
                // do nothing
            }

            if (validateSpecification)
            {
                foreach (string required in RequiredMeta)
                {
                    if (!result.ContainsKey(required))
                    {
                        throw new InvalidSpecificationException("The parsed HTML does not meet the open graph specification");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Safes the HTML decode URL.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The string</returns>
        private static string HtmlDecodeUrl(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            // naive attempt
            var patterns = new Dictionary<string, string>
            {
                ["&amp;"] = "&",
            };


            foreach (var key in patterns)
            {
                value = value.Replace(key.Key, key.Value);
            }

            return value;

        }
        /// <summary>
        /// Gets the open graph key.
        /// </summary>
        /// <param name="metaTag">The meta tag.</param>
        /// <returns>Returns the key stored from the meta tag</returns>
        private static string GetOpenGraphKey(HtmlNode metaTag)
        {
            if (metaTag.Attributes.Contains("property"))
            {
                return CleanOpenGraphKey(metaTag.Attributes["property"].Value);
            }

            return CleanOpenGraphKey(metaTag.Attributes["name"].Value);
        }

        /// <summary>
        /// Cleans the open graph key.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>strips the <c>og:</c> namespace from the value</returns>
        private static string CleanOpenGraphKey(string value)
        {
            return value.Replace("og:", string.Empty).ToLower(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the open graph value.
        /// </summary>
        /// <param name="metaTag">The meta tag.</param>
        /// <returns>Returns the value from the meta tag</returns>
        private static string GetOpenGraphValue(HtmlNode metaTag)
        {
            if (!metaTag.Attributes.Contains("content"))
            {
                return string.Empty;
            }

            return metaTag.Attributes["content"].Value;
        }

        #region IDictionary<string,string> Members

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="OpenGraph_Net.ReadOnlyDictionaryException">Cannot change a read only dictionary</exception>
        public void Add(string key, string value)
        {
            throw new ReadOnlyDictionaryException();
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(string key)
        {
            return this.openGraphData.ContainsKey(key);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        public ICollection<string> Keys => this.openGraphData.Keys;

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>false</c></returns>
        /// <exception cref="OpenGraph_Net.ReadOnlyDictionaryException">Cannot change a read only dictionary</exception>
        public bool Remove(string key)
        {
            throw new ReadOnlyDictionaryException();
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if the value was successfully set; otherwise false</returns>
        public bool TryGetValue(string key, out string value)
        {
            return this.openGraphData.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        public ICollection<string> Values => this.openGraphData.Values;

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>returns the open graph value at the specified key</returns>
        /// <exception cref="OpenGraph_Net.ReadOnlyDictionaryException">Cannot modify a read-only collection</exception>
        public string this[string key]
        {
            get
            {
                if (!this.openGraphData.ContainsKey(key))
                {
                    return string.Empty;
                }

                return this.openGraphData[key];
            }

            set
            {
                throw new ReadOnlyDictionaryException();
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,string>> Members

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <exception cref="OpenGraph_Net.ReadOnlyDictionaryException">Cannot change a read only dictionary</exception>
        public void Add(KeyValuePair<string, string> item)
        {
            throw new ReadOnlyDictionaryException();
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <exception cref="OpenGraph_Net.ReadOnlyDictionaryException">Cannot change a read only dictionary</exception>
        public void Clear()
        {
            throw new ReadOnlyDictionaryException();
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(KeyValuePair<string, string> item)
        {
            return this.openGraphData.Contains(item);
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            this.openGraphData.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public int Count => this.openGraphData.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>true</returns>
        public bool IsReadOnly => true;

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Returns false</returns>
        /// <exception cref="OpenGraph_Net.ReadOnlyDictionaryException">Cannot change a read only dictionary</exception>
        public bool Remove(KeyValuePair<string, string> item)
        {
            throw new ReadOnlyDictionaryException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,string>> Members

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator for the key value pairs</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.openGraphData.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)this.openGraphData).GetEnumerator();
        }

        #endregion     
    }
}
