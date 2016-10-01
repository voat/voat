﻿#region LICENSE 

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

#endregion 

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Voat.Common;
using Voat.Domain.Models;
using Voat.Models;
using Voat.Utilities;

namespace Voat.Data {


    //TODO: This needs to be abstracted into Submission search and Comment search
    /// <summary>
    /// Provide any of these Query string key/value pairs at any endpoint that supports the SearchOptions parsing to manipulate search query. WARNING: These features are not fully supported yet.
    /// </summary>
    public class SearchOptions {

        public const int MAX_COUNT = 50;
        
        private SortAlgorithm _sort = SortAlgorithm.Rank;
        private SortDirection _sortDirection = SortDirection.Default;
        private SortSpan _span = SortSpan.All;

        private DateTime? _startDate = null;
        private DateTime? _endDate = null;
        private int _count = 25;
        private int _currentIndex = 0;
        private int _page = 0;
        private string _phrase = null;
        private IEnumerable<KeyValuePair<string, string>> _queryStrings = null;
        private List<KeyValuePair<string, string>> _unknownPairs = new List<KeyValuePair<string, string>>();

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

        public SearchOptions(IEnumerable<KeyValuePair<string, string>> queryStrings)
        {
            //TODO: Make sure the querystrings passed into this method from controller are url decoded values
            if (queryStrings == null)
            {
                return;
            }

            this._queryStrings = queryStrings;
            //List<KeyValuePair<string, string>> unknownPairs = new List<KeyValuePair<string, string>>();

            foreach (var kp in queryStrings)
            {

                string value = kp.Value;

                switch (kp.Key.ToLower())
                {

                    case "period":
                    case "span":
                        SortSpan sortPer = SortSpan.All;
                        if (Enum.TryParse(value, true, out sortPer))
                        {
                            this._span = sortPer;
                        }
                        break;
                    case "sort":
                        SortAlgorithm sortAlg = SortAlgorithm.Rank;
                        if (Enum.TryParse(value, true, out sortAlg))
                        {
                            this.Sort = sortAlg;
                        }
                        break;
                    case "sortdirection":
                    case "direction":
                        SortDirection sortDir = SortDirection.Default;
                        if (Enum.TryParse(value, true, out sortDir))
                        {
                            this.SortDirection = sortDir;
                        }
                        break;
                    case "date":
                    case "startdate":
                    case "datestart":
                        DateTime startDate;
                        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out startDate))
                        {
                            this._startDate = startDate;
                        }
                        break;
                    //No longer supported
                    //case "enddate":
                    //case "dateend":
                    //    DateTime endDate;
                    //    if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out endDate)) {
                    //        this._endDate = endDate;
                    //    }
                    //    break;
                    case "count":
                        int count = 0;
                        if (Int32.TryParse(value, out count))
                        {
                            this.Count = count;
                        }
                        break;
                    case "index":
                    case "currentindex":
                        ////UNDONE: Don't think we want consumers controlling indexing, forceing all paging through the page querystring
                        //    int index = 0;
                        //    if (Int32.TryParse(value, out index))
                        //    {
                        //        this.Index = index;
                        //    }
                        break;
                    case "page":
                        int page = 0;
                        if (Int32.TryParse(value, out page))
                        {
                            this.Page = page;
                        }
                        break;
                    case "phrase":
                    case "search":
                    case "q":
                        this._phrase = (String.IsNullOrEmpty(value) ? "" : value.Trim());
                        break;
                    default:
                        _unknownPairs.Add(kp);
                        break;
                }
            }

            //process Period and Start End Times
            CalculateNewDateRange();
            ParseAdditionalKeyPairs(_unknownPairs);
        }

        private void CalculateNewDateRange()
        {
            if (this.Span != SortSpan.All)
            {
                if (!StartDate.HasValue)
                {
                    _startDate = Repository.CurrentDate;
                }
                //get date range based on span
                var range = _startDate.Value.Range(this.Span);
                this._startDate = range.Item1;
                this._endDate = range.Item2;
            }
        }

        /// <summary>
        /// Override this method if you extend from SearchOptions to handle all keypairs the SearchOptions base class didn't.
        /// </summary>
        /// <param name="keypairs"></param>
        protected virtual void ParseAdditionalKeyPairs(IEnumerable<KeyValuePair<string, string>> keypairs)
        {
            /*no-op*/
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
            get { return _span; }
            set {
                _span = value;
                CalculateNewDateRange();
            }
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
        /// The date for which to calculate a span.
        /// </summary>
        [JsonProperty("date")]
        [DataMember(Name = "startDate")]
        public DateTime? StartDate {
            get { return _startDate; }
            set {
                _startDate = value;
                CalculateNewDateRange();
            }
        }

        /// <summary>
        /// The end date for limiting search results.
        /// </summary>
        [JsonIgnore()] //currently we are going to force StartDate and Span in order to set this value rather than allow a range like this from the API. Too much room for abuse and causes caching issues.
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
        public string Phrase {
            get { return _phrase; }
            set {
                if (value == null) {
                    _phrase = "";
                } else {
                    _phrase = value.Trim(); 
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
        public override string ToString()
        {
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            if (StartDate.HasValue)
            {
                keyValues.Add("startDate", StartDate.Value.ToString("o"));
            }
            if (EndDate.HasValue)
            {
                keyValues.Add("endDate", EndDate.Value.ToString("o"));
            }
            if (SortDirection != SortDirection.Default)
            {
                keyValues.Add("direction", "reverse");
            }
            if (Span != SortSpan.All)
            {
                keyValues.Add("span", Span.ToString());
            }
            if (Page != 0)
            {
                keyValues.Add("page", Page.ToString());
            }
            if (Index != 0)
            {
                keyValues.Add("index", Index.ToString());
            }
            if (Sort != SortAlgorithm.Rank)
            {
                keyValues.Add("sort", Sort.ToString());
            }
            if (Count != 25)
            {
                keyValues.Add("count", Count.ToString());
            }
            if (!String.IsNullOrEmpty(Phrase))
            {
                keyValues.Add("phrase", Phrase);
            }
            
            return keyValues.OrderBy(x => x.Key).Concat(_unknownPairs.OrderBy(x => x.Key)).Aggregate("", (x, y) => String.Join(String.IsNullOrEmpty(x) ? "" : "&", x, String.Format("{0}={1}", y.Key, y.Value)));
        }
    }
}