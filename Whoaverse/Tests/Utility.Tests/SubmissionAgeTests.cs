namespace Utility.Tests
{
    using System;
    using System.Globalization;
    using System.Threading;
    using Voat.Utils;
    using Xunit;

    [Trait("Category", "Unit tests")]
    public class SubmissionAgeTests
    {
        [Fact]
        public void Calc_SubmissionAge_1second()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromSeconds(1));
            Assert.Equal("1 second", result);
        }

        [Fact]
        public void Calc_SubmissionAge_30seconds()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromSeconds(30));
            Assert.Equal("30 seconds", result);
        }

        [Fact]
        public void Calc_SubmissionAge_1minute()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromMinutes(1));
            Assert.Equal("1 minute", result);
        }

        [Fact]
        public void Calc_SubmissionAge_2minutes()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromMinutes(2));
            Assert.Equal("2 minutes", result);
        }

        [Fact]
        public void Calc_SubmissionAge_1Day()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromDays(1));
            Assert.Equal("1 day", result);
        }

        [Fact]
        public void Calc_SubmissionAge_2Day()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromDays(2));
            Assert.Equal("2 days", result);
        }

        [Fact]
        public void Calc_SubmissionAge_8months()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromDays(8 * 31));
            Assert.Equal("8 months", result);
        }

        [Fact]
        public void Calc_SubmissionAge_1year()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromDays(370));
            Assert.Equal("1 year", result);
        }

        [Fact]
        public void Calc_SubmissionAge_1_5years()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromDays(18 * 30));
            Assert.Equal("1.5 years", result);
        }

        [Fact]
        public void Calc_SubmissionAge_2years()
        {
            string result = Submissions.CalcSubmissionAge(TimeSpan.FromDays(369 * 2));
            Assert.Equal("2 years", result);
        }

        [Fact]
        public void Calc_SubmissionAgeDouble()
        {
            DateTime testDate = DateTime.Now.AddDays(-143);
            double result = Submissions.CalcSubmissionAgeDouble(testDate);
            Assert.Equal(3432, Math.Round(result, 1));
        }
    }
}