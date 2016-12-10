namespace Voat.Data
{
    public class AGGREGATE_SUBVERSE
    {
        public const string ALL = "_all";
        public const string FRONT = "_front";
        public const string ANY = "_any";
        public const string DEFAULT = "_default";

        public static bool IsAggregate(string subverse)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(subverse))
            {
                switch (subverse.ToLower())
                {
                    case AGGREGATE_SUBVERSE.ALL:
                    case AGGREGATE_SUBVERSE.FRONT:
                    case AGGREGATE_SUBVERSE.DEFAULT:
                    case AGGREGATE_SUBVERSE.ANY:
                    case "all":
                        result = true;
                        break;
                }
            }

            return result;
        }
    }
}
