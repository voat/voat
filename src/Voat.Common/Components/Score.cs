#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Voat.Common
{
    public class CountedScore : Score
    {
        private int _count = 0;
        public int Count { get => _count; set => _count = value; }

        public double Average
        {
            get
            {
                return Math.Round(Sum / (double)Count, _roundingDecimals);
            }
        }
        public double AverageUpVotes
        {
            get
            {
                return Math.Round(UpCount / (double)Count, _roundingDecimals);
            }
        }
        public double AverageDownVotes
        {
            get
            {
                return Math.Round(DownCount / (double)Count, _roundingDecimals);
            }
        }

    }

    /// <summary>
    /// A generic class to store up/down votes and up/down ccp and scp calculations.
    /// </summary>
    public class Score
    {
        protected short _roundingDecimals = 2;
        private int _up = 0;
        private int _down = 0;

        /// <summary>
        /// Total = UpCount + DownCount
        /// </summary>
        [JsonIgnore()]
        [IgnoreDataMember()]
        public int Total
        {
            get { return UpCount + DownCount; }
        }

        /// <summary>
        /// Sum = UpCount - DownCount
        /// </summary>
        public int Sum
        {
            get { return UpCount - DownCount; }
        }

        /// <summary>
        /// UpCount count
        /// </summary>
        public int UpCount
        {
            get { return _up; }
            set { _up = Math.Max(0, value); }
        }

        /// <summary>
        /// DownCount count
        /// </summary>
        public int DownCount
        {
            get { return _down; }
            set { _down = Math.Max(0, value); }
        }

        /// <summary>
        /// Ratio of UpCount to Total
        /// </summary>
        [JsonIgnore()]
        [IgnoreDataMember()]
        public double UpRatio
        {
            get { return (Total != 0 ? Math.Round((double)UpCount / (double)Total, _roundingDecimals) : 0); }
        }

        /// <summary>
        /// Ratio of DownCount to Total
        /// </summary>
        [JsonIgnore()]
        [IgnoreDataMember()]
        public double DownRatio
        {
            get { return (Total != 0 ? Math.Round((double)DownCount / (double)Total, _roundingDecimals) : 0); }
        }

        /// <summary>
        /// The ratio of UpCount to DownCount. 1 is an even distribution. If greater than 1: UpCount bias, less than 1: DownCount bias.
        /// </summary>
        [JsonIgnore()]
        [IgnoreDataMember()]
        public double Bias
        {
            get { return (DownCount != 0 ? Math.Round((double)UpCount / (double)DownCount, _roundingDecimals) : 1); }
        }

        /// <summary>
        /// Adds two Score objects together. Or us the + operator
        /// </summary>
        /// <param name="add">The Score to add</param>
        /// <returns></returns>
        public Score Combine(Score add)
        {
            if (add != null)
            {
                this.UpCount += add.UpCount;
                this.DownCount += add.DownCount;
            }
            return this;
        }

        public static Score operator +(Score s1, Score s2)
        {
            if (s1 != null)
            {
                return s1.Combine(s2);
            }
            else if (s2 != null)
            {
                return s2.Combine(s1);
            }
            return new Score();
        }
    }
}
