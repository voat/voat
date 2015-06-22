namespace Voat.Models
{
    public class CombinedKarma
    {
        public CombinedKarma(int linkKarma, int commentKarma)
        {
            LinkKarma = linkKarma;
            CommentKarma = commentKarma;
        }

        public int LinkKarma { get; private set; }
        public int CommentKarma { get; private set; }
    }
}