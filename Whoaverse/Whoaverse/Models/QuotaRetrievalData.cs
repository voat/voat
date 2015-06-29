namespace Voat.Models
{
    public abstract class QuotaRetrievalData
    {
        public QuotaRetrievalData(string userName, string subverse, string url)
        {
            UserName = userName;
            Subverse = subverse;
            Url = url;
        }

        public string UserName { get; private set; }
        public string Subverse { get; private set; }
        public string Url { get; private set; }
    }
}