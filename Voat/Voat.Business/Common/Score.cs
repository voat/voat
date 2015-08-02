using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Voat.Common
{

    /// <summary>
    /// A generic class to store up/down votes and up/down ccp and scp calculations.
    /// </summary>
    public class Score {

        private short _roundingDecimals = 2;
        private int _up = 0;
        private int _down = 0;

        /// <summary>
        /// Total = UpCount + DownCount
        /// </summary>
        [JsonIgnore()]
        [IgnoreDataMember()]
        public int Total {
            get {
                return UpCount + DownCount;
            }
        }

        /// <summary>
        /// Sum = UpCount - DownCount
        /// </summary>
        [JsonProperty("sum")]
        [DataMember(Name = "sum")]
        public int Sum {
            get {
                return UpCount - DownCount;
            }
        }

        /// <summary>
        /// UpCount count
        /// </summary>

        [JsonProperty("upCount")]
        [DataMember(Name = "upCount")]
        public int UpCount { get { return _up; } set { _up = Math.Max(0, value); } }

        /// <summary>
        /// DownCount count
        /// </summary>

        [JsonProperty("downCount")]
        [DataMember(Name = "downCount")]
        public int DownCount { get { return _down; } set { _down = Math.Max(0, value); } }

        /// <summary>
        /// Ratio of UpCount to Total
        /// </summary>
        [JsonIgnore()]
        [IgnoreDataMember()]
        public double UpRatio { get { return (Total != 0 ? Math.Round((double)UpCount / (double)Total, _roundingDecimals) : 0); } }
        
        /// <summary>
        /// Ratio of DownCount to Total
        /// </summary>
        [JsonIgnore()]
        [IgnoreDataMember()]
        public double DownRatio { get { return (Total != 0 ? Math.Round((double)DownCount / (double)Total, _roundingDecimals) : 0); } }

        /// <summary>
        /// The ratio of UpCount to DownCount. 1 is an even distribution. If greater than 1: UpCount bias, less than 1: DownCount bias.
        /// </summary>
        [JsonIgnore()]
        [IgnoreDataMember()]
        public double Bias { get { return (DownCount != 0 ? Math.Round((double)UpCount / (double)DownCount, _roundingDecimals) : 1); } }


        /// <summary>
        /// Adds two Score objects together. Or us the + operator
        /// </summary>
        /// <param name="add">The Score to add</param>
        /// <returns></returns>
        public Score Combine(Score add) {
            if (add != null) {

                this.UpCount += add.UpCount;
                this.DownCount += add.DownCount;

            }
            return this;
        }
        public static Score operator+(Score s1, Score s2){
            if (s1 != null) {
                return s1.Combine(s2);
            } else if (s2 != null) {
                return s2.Combine(s1);
            }
            return new Score();
        }
    }

}
