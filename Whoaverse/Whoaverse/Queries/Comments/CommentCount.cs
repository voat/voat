namespace Voat.Queries.Comments
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;

    public static class CommentCount
    {
        public static Task<int> GetCommentCountAsync(this IQueryable<Comment> query, int submissionId)
        {
            return query.Where(c => c.MessageId == submissionId && c.Name != "deleted").CountAsync();
        }
    }
}