using OpenGraph_Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Configuration;

namespace Voat.Utilities
{
    public class HttpResourceOptions
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
        public bool AllowAutoRedirect { get; set; } = false;
        //SSRF guard: refuse to fetch urls whose host resolves to non-public addresses.
        //Turn off only for trusted, hard-coded endpoints (e.g. localhost test servers).
        public bool BlockNonPublicAddresses { get; set; } = true;

    }

    public sealed class HttpResource : IDisposable
    {
        private Uri _uri = null;
        private Uri _redirectedUri = null;
        private HttpResponseMessage _response;
        private MemoryStream _stream;
        private string _title = null;
        private string _contentString = null;
        private Uri _image = null;
        private HttpResourceOptions _options = new HttpResourceOptions();

        public HttpResponseMessage Response { get => _response; }
        public Stream Stream { get => _stream;  }
        
        public Uri Uri { get => _uri; }
        public Uri RedirectedUri { get => _redirectedUri; }
        public bool Redirected { get => _uri != _redirectedUri; }
        public HttpResourceOptions Options { get => _options; set => _options = value; }
        public IWebProxy Proxy { get; set; }

        public HttpResource(Uri uri, HttpResourceOptions options = null, IWebProxy proxy = null)
        {
            _uri = uri;
            if (options != null)
            {
                _options = options;
            }
            Proxy = proxy;
        }

        public HttpResource(string uri, HttpResourceOptions options = null) : this(new Uri(uri), options, null)
        {
        }
        /// <summary>
        /// This method makes the remote Http request, cowboy style.
        /// </summary>
        /// <param name="options">Options to use with remote request</param>
        /// <returns></returns>
        //TODO: This method needs to return a status
        public async Task<HttpStatusCode> GiddyUp(HttpMethod method = null, HttpContent content = null, HttpCompletionOption options = HttpCompletionOption.ResponseContentRead)
        {
            //Auto-redirect is always disabled at the handler level: when redirects are wanted
            //they are followed manually below so every hop can be checked against the SSRF
            //deny-list (a public url must not be able to redirect us into the internal network).
            var handler = new HttpClientHandler() {
                AllowAutoRedirect = false,
                Proxy = this.Proxy
            };

            using (var httpClient = new HttpClient(handler))
            {
                httpClient.Timeout = _options.Timeout;
                httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue($"Voat-OpenGraph-Parser", "2"));

                var target = Uri;
                const int maxRedirects = 5;
                for (int hop = 0; ; hop++)
                {
                    if (_options.BlockNonPublicAddresses && !await UrlUtility.IsUriSafeForServerFetch(target))
                    {
                        throw new InvalidOperationException($"Blocked request to non-public address: {target}");
                    }

                    switch (method?.Method.ToLower())
                    {
                        case "get":
                        case null:
                            _response = await httpClient.GetAsync(target, options);
                            break;
                        case "post":
                            _response = await httpClient.PostAsync(target, content);
                            break;
                        default:
                            throw new NotImplementedException($"{method?.Method} is currently not implemented");
                            break;
                    }

                    var isRedirect = (int)_response.StatusCode >= 300 && (int)_response.StatusCode < 400;
                    if (_options.AllowAutoRedirect && isRedirect && hop < maxRedirects)
                    {
                        var location = _response.Headers.Location;
                        if (location == null)
                        {
                            break;
                        }
                        target = location.IsAbsoluteUri ? location : new Uri(target, location);
                        _response.Dispose();
                        continue;
                    }
                    break;
                }

                _redirectedUri = _response.RequestMessage.RequestUri;

                if (options == HttpCompletionOption.ResponseContentRead)
                {
                    //Copy Response
                    _stream = new MemoryStream();
                    await _response.Content.CopyToAsync(_stream);
                    _stream.Seek(0, SeekOrigin.Begin);
                }
            }
            return _response.StatusCode;
        }
        public bool IsImage
        {
            get
            {
                return Uri == Image;
            }
        }
        public Uri Image
        {
            get
            {
                if (_image == null)
                {
                    //Check if this url is an image extension
                    if (Uri.ToString().IsImageExtension())
                    {
                        _image = _uri;
                        return _image;
                    }

                    EnsureReady();
                    //Check OpenGraph
                    var graph = OpenGraph.ParseHtml(ContentString);
                    if (graph.Image != null)
                    {
                        _image = graph.Image;
                        return _image;
                    }
                }
                return _image;
            }
        }
        public string Title
        {
            get
            {
                EnsureReady();

                if (_title == null)
                {
                    //Try Open Graph
                    var graph = OpenGraph.ParseHtml(ContentString);
                    if (!String.IsNullOrEmpty(graph.Title))
                    {
                        _title = WebUtility.HtmlDecode(graph.Title);
                    }
                    //Try Getting from Title
                    if (String.IsNullOrEmpty(_title))
                    {
                        var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                        htmlDocument.LoadHtml(ContentString);
                        var titleNode = htmlDocument.DocumentNode.Descendants("title").SingleOrDefault();
                        if (titleNode != null)
                        {
                            _title = WebUtility.HtmlDecode(titleNode.InnerText);
                        }
                    }
                }

                return _title;
            }
        }
        private void EnsureReady()
        {
            if (this.Response == null)
            {
                throw new InvalidOperationException("Request has not been processed");
            }
            else if (!this.Response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Request returned: {this.Response.StatusCode} {this.Response.ReasonPhrase}");
            }
        }
        private string ContentString
        {
            get
            {
                EnsureReady();
                if (_contentString == null)
                {
                    var reader = new StreamReader(this.Stream);
                    _contentString = reader.ReadToEnd();
                }
                return _contentString;
            }
        }


        public void Dispose()
        {
            _response?.Dispose();
            _stream?.Dispose();
        }
    }
}
