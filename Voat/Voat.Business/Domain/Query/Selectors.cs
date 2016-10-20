namespace Voat.Domain.Query
{
    //    public static class QueryProcessor
    //    {
    //        public static Action<Data.Models.Submission> AnonSubmission = new Action<Data.Models.Submission>(x => { if (x.IsAnonymized) x.UserName = x.ID.ToString(); });
    //        public static Action<Comment> AnonComment = new Action<Comment>(x => { if (x.IsAnonymized) x.UserName = x.ID.ToString(); });
    //        public static Action<usp_CommentTree_Result> AnonCommentTree = new Action<usp_CommentTree_Result>(x => { if (x.IsAnonymized) x.UserName = x.ID.ToString(); });
    //    }

    //    public static class QuerySelectors
    //    {
    //        public static readonly Func<Subverse, domain.SubverseInformation> ToSubverseInformation =
    //           new Func<Subverse, domain.SubverseInformation>(x =>
    //                new domain.SubverseInformation()
    //                {
    //                    Name = x.Name,
    //                    Title = x.Title,
    //                    Description = x.Description,
    //                    CreationDate = x.CreationDate,
    //                    SubscriberCount = (x.SubscriberCount == null ? 0 : x.SubscriberCount.Value),
    //                    RatedAdult = x.IsAdult,
    //                    Sidebar = x.SideBar,
    //                    Type = x.Type
    //                });
    //    }
}
