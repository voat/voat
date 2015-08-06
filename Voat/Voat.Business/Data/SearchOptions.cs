using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Voat.Common;
using Voat.Models;

namespace Voat.Data {

    /// <summary>
    /// Provide any of these Query string key/value pairs at any endpoint that supports the SearchOptions parsing to manipulate search query. WARNING: These features are not fully supported yet.
    /// </summary>
    public class SearchOptions {

        public const int MAX_COUNT = 50;

        private SortAlgorithm _sort = SortAlgorithm.Hot;
        private SortDirection _sortDirection = SortDirection.Default;
        private SortSpan _period = SortSpan.All;

        private DateTime? _startDate = null;
        private DateTime? _endDate = null;
        private int _count = 25;
        private int _currentIndex = 0;
        private int _page = 0;
        private string _search = null;
        private int _depth = -1;
        private IEnumerable<KeyValuePair<string, string>> _queryStrings = null;


        protected TimeSpan Duration(SortSpan span) {

            TimeSpan ts = TimeSpan.MaxValue;

            switch (span) { 
                case SortSpan.Hour:
                    ts = TimeSpan.FromHours(1);
                    break;
                case SortSpan.Day:
                    ts = TimeSpan.FromDays(1);
                    break;
                case SortSpan.Month:
                    ts = TimeSpan.FromDays(30);
                    break;
                case SortSpan.Week:
                    ts = TimeSpan.FromDays(7);
                    break;
                case SortSpan.Quarter:
                    ts = TimeSpan.FromDays(90);
                    break;
                case SortSpan.Year:
                    ts = TimeSpan.FromDays(365);
                    break;
            }

            return ts;

        }

        public static IList<KeyValuePair<string, string>> ParseQuery(string queryString, bool urlDecodeValues = true) {

            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();
            
            if (!String.IsNullOrEmpty(queryString)){

                string reducedQueryString = queryString.Trim();

                //a full url has been passed in
                if (queryString.Contains("?")) {
                    reducedQueryString = queryString.Substring(queryString.IndexOf("?") + 1, queryString.Length - queryString.IndexOf("?") - 1);
                }
                if (urlDecodeValues) {
                    reducedQueryString = HttpUtility.UrlDecode(reducedQueryString);
                }

                string[] s = reducedQueryString.Split('&');


                foreach (string pair in s){
                    if (pair.Contains('=')){
                        var keypair = pair.Split('=');            
                        pairs.Add(new KeyValuePair<string,string>(keypair[0], keypair[1]));
                    } else {
                        //filter out urls with no query strings and empty key pairs
                        if (!String.IsNullOrEmpty(pair) && !pair.ToLower().StartsWith("http")) {
                            pairs.Add(new KeyValuePair<string, string>(pair, ""));
                        }
                    }
                }

            }

            return pairs;        
        
        }

        public SearchOptions() {
            /*no-op*/
        }
        public SearchOptions(string queryString) : this(SearchOptions.ParseQuery(queryString)) {

        }

        public SearchOptions(IEnumerable<KeyValuePair<string, string>> queryStrings) {
            //TODO: Make sure the querystrings passed into this method from controller are url decoded values
            if (queryStrings == null) {
                return;            
            }

            this._queryStrings = queryStrings;

            foreach (var kp in queryStrings) {

                string value = kp.Value;

                switch (kp.Key.ToLower()) {

                    case "period":
                    case "span":
                        SortSpan sortPer = SortSpan.All;
                        if (Enum.TryParse(value, true, out sortPer)) {
                            this.Span = sortPer;
                        }
                        break;
                    case "sort":
                        SortAlgorithm sortAlg = SortAlgorithm.Hot;
                        if (Enum.TryParse(value, true, out sortAlg)) {
                            this.Sort = sortAlg;
                        }
                        break;
                    case "sortdirection":
                    case "direction":
                        SortDirection sortDir = SortDirection.Default;
                        if (Enum.TryParse(value, true, out sortDir)) {
                            this.SortDirection = sortDir;
                        }
                        break;
                    case "startdate":
                    case "datestart":
                        DateTime startDate;
                        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out startDate)) {
                            this._startDate = startDate;
                        }
                        break;
                    case "enddate":
                    case "dateend":
                        DateTime endDate;
                        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out endDate)) {
                            this._endDate = endDate;
                        }
                        break;
                    case "count":
                        int count = 0;
                        if (Int32.TryParse(value, out count)) {
                            this.Count = count;
                        }
                        break;
                    case "index":
                    case "currentindex":
                        int index = 0;
                        if (Int32.TryParse(value, out index)) {
                            this.Index = index;
                        }
                        break;
                    case "page":
                        int page = 0;
                        if (Int32.TryParse(value, out page)) {
                            this.Page = page;
                        }
                        break;
                    case "depth":
                        int depth;
                        if (int.TryParse(value, out depth)) {
                            this.Depth = depth;
                        }
                        break;
                    case "search":
                        this._search = (String.IsNullOrEmpty(value) ? "" : value.Trim());
                        break;
                }
            }

            //process Period and Start End Times
            if (this.Span != SortSpan.All) {
                if (this.EndDate.HasValue) {
                    //assume this was provided so we will use this date as the start date
                    this.StartDate = this.EndDate.Value.Subtract(Duration(this.Span));
                } else {
                    this.StartDate = DateTime.UtcNow.Subtract(Duration(this.Span));
                    this.EndDate = DateTime.UtcNow;
                }
                
            }

        }

        public static SearchOptions Default {
            get {
                return new SearchOptions();
            }
        }
        
        /// <summary>
        /// The span of time your search encompases.  Specify the text value in querystring.
        /// </summary>
        [JsonProperty("span")]
        [DataMember(Name = "span")]
        public SortSpan Span {
            get { return _period; }
            set { _period = value; }
        }

        /// <summary>
        /// The sort algorithm used to order search results. Specify the text value in querystring.
        /// </summary>
        [JsonProperty("sort")]
        [DataMember(Name = "sort")]
        public SortAlgorithm Sort {
            get { return _sort; }
            set { _sort = value; }
        }
        /// <summary>
        /// The sort order requested.  Specify the text value in querystring.
        /// </summary>
        [JsonProperty("direction")]
        [DataMember(Name = "direction")]
        public SortDirection SortDirection {
            get { return _sortDirection; }
            set { _sortDirection = value; }
        }

        /// <summary>
        /// The start date for limiting search results.
        /// </summary>
        [JsonProperty("startDate")]
        [DataMember(Name = "startDate")]
        public DateTime? StartDate {
            get { return _startDate; }
            set { _startDate = value; }
        }

        /// <summary>
        /// The end date for limiting search results. This value is overridden if <paramref name="Span">span</paramref> is provided.
        /// </summary>
        [JsonProperty("endDate")]
        [DataMember(Name = "endDate")]
        public DateTime? EndDate {
            get { return _endDate; }
            set { _endDate = value; }
        }

        /// <summary>
        /// The number of search records requested. Max Value is 50.
        /// </summary>
        [JsonProperty("count")]
        [DataMember(Name = "count")]
        public int Count {
            get { return _count; }
            set {
                if (value <= MAX_COUNT) {
                    if (value <= 0) {
                        throw new VoatValidationException("Count must be a value greater than zero.");
                    }
                    _count = value;
                } else {
                    _count = MAX_COUNT;
                }
                RecalculateIndex();
            }
        }
        /// <summary>
        /// The current index to start from for search results. This value is a paging index.
        /// </summary>
        [JsonProperty("index")]
        [DataMember(Name = "index")]
        public int Index {
            get { return _currentIndex; }
            set {
                if (value < 0) {
                    throw new VoatValidationException("Index can not be a negative value.");
                } else {
                    _currentIndex = value;
                    RecalculateIndex();
                }
            }
        }
        /// <summary>
        /// [NEW] The page in which to retrieve. This value simply overriddes 'Index' and calculates it for you. How nice are we? Fairly nice I must say. Paging starts on page 1 not page 0.
        /// </summary>
        [JsonProperty("page")]
        [DataMember(Name = "page")]
        public int Page {
            get {
                return _page; 
            }
            set {
                if (value <= 0) {
                    throw new VoatValidationException("Page must be greater than zero. Paging starts on page 1, not page 0.");
                } else {
                    _page = value;
                    RecalculateIndex();
                }
             }
        }


        /// <summary>
        /// The search value to match for submissions or comments.
        /// </summary>
        [JsonProperty("search")]
        [DataMember(Name = "search")]
        public string Search {
            get { return _search; }
            set {
                if (value == null) {
                    _search = "";
                } else {
                    _search = value.Trim(); 
                }
            }
        }
        /// <summary>
        /// Specifies the depth of comment tree to retrieve. Used only for comment queries.
        /// </summary>
        [JsonProperty("depth")]
        [DataMember(Name = "depth")]
        public int Depth {
            get { return _depth; }
            set {
                if (value >= -1 && value < 10) {
                    _depth = value;
                }
            }
        }
        /// <summary>
        /// Represents the original querystring arguments the Search class was constructed with.
        /// </summary>
        [JsonIgnore()]
        [IgnoreDataMember()]
        public IList<KeyValuePair<string, string>> QueryStrings {
            get { return _queryStrings.ToList(); }
        }

        private void RecalculateIndex() {
            if (Page > 0) {
                //adjust friendly page count to zero based
                _currentIndex = (Count * (Page - 1)) + (Page - 1);
            }
        }
    }
}