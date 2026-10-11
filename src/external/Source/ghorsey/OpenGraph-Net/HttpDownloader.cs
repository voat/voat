
namespace OpenGraph_Net
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    //
    /// <summary>
    /// Http Downloader
    /// </summary>
    /// <remarks>
    /// http://stackoverflow.com/a/2700707
    /// Modernization: rewritten from the obsolete WebRequest/HttpWebRequest API (SYSLIB0014)
    /// to HttpClient. Encoding sniffing and meta-charset re-encode behavior is preserved.
    /// </remarks>
    public class HttpDownloader
    {
        private static readonly HttpClient Client = CreateClient();

        private readonly string referer;
        private readonly string userAgent;

        /// <summary>
        /// Gets or sets the encoding.
        /// </summary>
        /// <value>
        /// The encoding.
        /// </value>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        /// <value>
        /// The headers.
        /// </value>
        public HttpResponseHeaders Headers { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public Uri Url { get; set; }

        private static HttpClient CreateClient()
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            return new HttpClient(handler);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDownloader"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="referer">The referer.</param>
        /// <param name="userAgent">The user agent.</param>
        public HttpDownloader(Uri url, string referer, string userAgent)
        {
            this.Encoding = Encoding.GetEncoding("ISO-8859-1");
            this.Url = url;
            this.userAgent = userAgent;
            this.referer = referer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDownloader"/> class.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="referer">The referer.</param>
        /// <param name="userAgent">The user agent.</param>
        public HttpDownloader(string url, string referer, string userAgent) : this(new Uri(url), referer, userAgent)
        {
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <returns>The content of the page</returns>
        public string GetPage()
        {
            return this.GetPageAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets the page asynchronosly
        /// </summary>
        /// <returns>
        /// The content of the page
        /// </returns>
        public async Task<string> GetPageAsync()
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, this.Url))
            {
                if (!string.IsNullOrEmpty(this.referer))
                {
                    request.Headers.TryAddWithoutValidation("Referer", this.referer);
                }
                if (!string.IsNullOrEmpty(this.userAgent))
                {
                    request.Headers.TryAddWithoutValidation("User-Agent", this.userAgent);
                }

                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip,deflate");

                using (HttpResponseMessage response = await Client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    this.Headers = response.Headers;
                    if (response.RequestMessage != null && response.RequestMessage.RequestUri != null)
                    {
                        this.Url = response.RequestMessage.RequestUri;
                    }
                    return await this.ProcessContent(response);
                }
            }
        }
        /// <summary>
        /// Processes the content.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        private async Task<string> ProcessContent(HttpResponseMessage response)
        {
            this.SetEncodingFromHeader(response);

            Stream s = await response.Content.ReadAsStreamAsync();

            if (s == null)
            {
                throw new InvalidOperationException("Response stream came back as null");
            }

            MemoryStream memStream = new MemoryStream();
            await s.CopyToAsync(memStream);
            s.Close();
            string html;
            memStream.Position = 0;
            using (StreamReader r = new StreamReader(memStream, this.Encoding))
            {
                html = r.ReadToEnd().Trim();
                html = this.CheckMetaCharSetAndReEncode(memStream, html);
            }

            return html;
        }

        /// <summary>
        /// Sets the encoding from header.
        /// </summary>
        /// <param name="response">The response.</param>
        private void SetEncodingFromHeader(HttpResponseMessage response)
        {
            string charset = null;
            if (response.Content.Headers.ContentType != null && !string.IsNullOrEmpty(response.Content.Headers.ContentType.CharSet))
            {
                charset = response.Content.Headers.ContentType.CharSet.Trim('\'', '"');
            }
            else if (response.Content.Headers.ContentType != null)
            {
                Match m = Regex.Match(response.Content.Headers.ContentType.ToString(), @";\s*charset\s*=\s*(?<charset>.*)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    charset = m.Groups["charset"].Value.Trim('\'', '"');
                }
            }
            if (!string.IsNullOrEmpty(charset))
            {
                try
                {
                    this.Encoding = Encoding.GetEncoding(charset);
                }
                // ReSharper disable once UncatchableException
                catch (ArgumentException)
                {
                }
            }
        }

        /// <summary>
        /// Checks the meta character set and re encode.
        /// </summary>
        /// <param name="memStream">The memory stream.</param>
        /// <param name="html">The HTML.</param>
        /// <returns></returns>
        private string CheckMetaCharSetAndReEncode(Stream memStream, string html)
        {
            Match m = new Regex(@"<meta\s+.*?charset\s*=\s*?""?(?<charset>[A-Za-z0-9_-]+?)""", RegexOptions.Singleline | RegexOptions.IgnoreCase).Match(html);
            ////Match m = new Regex(@"<meta\s+.*?charset\s*=\s*(?<charset>[A-Za-z0-9_-]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase).Match(html);
            if (m.Success)
            {
                string charset = m.Groups["charset"].Value.ToLower();
                if ((charset == "unicode") || (charset == "utf-16"))
                {
                    charset = "utf-8";
                }

                try
                {
                    Encoding metaEncoding = Encoding.GetEncoding(charset);
                    if (!this.Encoding.Equals(metaEncoding))
                    {
                        memStream.Position = 0L;
                        StreamReader recodeReader = new StreamReader(memStream, metaEncoding);
                        html = recodeReader.ReadToEnd().Trim();
                        recodeReader.Close();
                    }
                }
                // ReSharper disable once UncatchableException
                catch (ArgumentException)
                {
                }
            }

            return html;
        }
    }
}
