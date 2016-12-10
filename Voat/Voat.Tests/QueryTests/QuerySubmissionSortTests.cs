using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voat.Data;
using Voat.Domain.Models;
using Voat.Domain.Query;
using Voat.Tests.Repository;

namespace Voat.Tests.QueryTests
{
    [TestClass]
    public class QuerySubmissionSortTests : BaseUnitTest
    {
        private static string subverse = "sort";

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            VoatDataInitializer.CreateSorted(subverse);
        }

        
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Sort")]
        [TestCategory("Query.Submission.Sort")]
        public void Search_Sort_RelativeRank()
        {
            var s = new SearchOptions();
            s.Sort = SortAlgorithm.RelativeRank;
            var q = new QuerySubmissions(subverse, s);
            var r = q.ExecuteAsync().Result;
            VerifySort(s, r);
        }
        //Need a way to test this, right now we don't have comments on these entries
        //[TestMethod]
        //[TestCategory("Query")]
        //[TestCategory("Submission")]
        //[TestCategory("Sort")]
        //[TestCategory("Query.Submission.Sort")]
        //public void Search_Sort_Active()
        //{
        //    var s = new SearchOptions();
        //    s.Sort = SortAlgorithm.Active;
        //    var q = new QuerySubmissions(subverse, s);
        //    var r = q.Execute().Result;
        //    VerifySort(s, r);
        //}
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Sort")]
        [TestCategory("Query.Submission.Sort")]
        public void Search_Sort_Rank() {
            var s = new SearchOptions();
            s.Sort = SortAlgorithm.Rank;
            var q = new QuerySubmissions(subverse, s);
            var r = q.ExecuteAsync().Result;
            VerifySort(s, r);
        }

        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Sort")]
        [TestCategory("Query.Submission.Sort")]
        public void Search_Sort_New()
        {
            var s = new SearchOptions();
            s.Sort = SortAlgorithm.New;
            var q = new QuerySubmissions(subverse, s);
            var r = q.ExecuteAsync().Result;
            VerifySort(s, r);
        }
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Sort")]
        [TestCategory("Query.Submission.Sort")]
        public void Search_Sort_Top()
        {
            var s = new SearchOptions();
            s.Sort = SortAlgorithm.Top;
            var q = new QuerySubmissions(subverse, s);
            var r = q.ExecuteAsync().Result;
            VerifySort(s, r);
        }
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Sort")]
        [TestCategory("Query.Submission.Sort")]
        public void Search_Sort_Bottom()
        {
            var s = new SearchOptions();
            s.Sort = SortAlgorithm.Bottom;
            var q = new QuerySubmissions(subverse, s);
            var r = q.ExecuteAsync().Result;
            VerifySort(s, r);
        }

        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Sort")]
        [TestCategory("Query.Submission.Sort")]
        public void Search_Sort_Viewed()
        {
            var s = new SearchOptions();
            s.Sort = SortAlgorithm.Viewed;
            var q = new QuerySubmissions(subverse, s);
            var r = q.ExecuteAsync().Result;
            VerifySort(s, r);
        }
        [TestMethod]
        [TestCategory("Query")]
        [TestCategory("Submission")]
        [TestCategory("Sort")]
        [TestCategory("Query.Submission.Sort")]
        public void Search_Sort_Intensity()
        {
            var s = new SearchOptions();
            s.Sort = SortAlgorithm.Intensity;
            var q = new QuerySubmissions(subverse, s);
            var r = q.ExecuteAsync().Result;
            VerifySort(s, r);
        }

        private void VerifySort(SearchOptions options, IEnumerable<Submission> results)
        {

            if (results == null || results.Count() == 0)
            {
                Assert.Fail("Sort Result set is null or empty");
            }

            var f = new Func<double, double, bool, bool>((x, y, ascending) => {

                Debug.Print(String.Format("x: {0}, y:{0}", x, y));

                if (ascending)
                {
                    return x <= y;
                }
                else
                {
                    return x >= y;
                }
            });

            Submission previousSubmission = null;
            double previous = 0;
            double current = 0;
            bool asc = true;

            foreach (var currentSubmission in results)
            {
                if (previousSubmission != null)
                {
                    switch (options.Sort)
                    {
                        case SortAlgorithm.Rank:
                            asc = options.SortDirection == SortDirection.Reverse;
                            current = (double)currentSubmission.Rank;
                            previous = (double)previousSubmission.Rank;
                            break;
                        case SortAlgorithm.Bottom:
                            asc = options.SortDirection == SortDirection.Reverse;
                            current = (double)currentSubmission.DownCount;
                            previous = (double)previousSubmission.DownCount;
                            break;
                        case SortAlgorithm.Top:
                            asc = options.SortDirection == SortDirection.Reverse;
                            current = (double)currentSubmission.UpCount;
                            previous = (double)previousSubmission.UpCount;
                            break;
                        case SortAlgorithm.New:
                            asc = options.SortDirection == SortDirection.Reverse;
                            current = (double)currentSubmission.CreationDate.Ticks;
                            previous = (double)previousSubmission.CreationDate.Ticks;
                            break;
                        case SortAlgorithm.Intensity:
                            asc = options.SortDirection == SortDirection.Reverse;
                            current = (double)currentSubmission.UpCount + currentSubmission.DownCount;
                            previous = (double)previousSubmission.UpCount + previousSubmission.DownCount;
                            break;
                        case SortAlgorithm.Viewed:
                            asc = options.SortDirection == SortDirection.Reverse;
                            current = (double)currentSubmission.Views;
                            previous = (double)previousSubmission.Views;
                            break;
                        case SortAlgorithm.RelativeRank:
                            asc = options.SortDirection == SortDirection.Reverse;
                            current = (double)currentSubmission.RelativeRank;
                            previous = (double)previousSubmission.RelativeRank;
                            break;
                        default:
                            throw new NotImplementedException("No case for " + options.Sort.ToString());
                            break;

                    }
                    Assert.IsTrue(f(previous, current, asc), String.Format("Submission {0} was out of order compared to submission {1} for sort {2}", previousSubmission.ID, currentSubmission.ID, options.Sort));
                }
                previousSubmission = currentSubmission;
            }
        }
    }
}
