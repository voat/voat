using System;

namespace Voat.Caching
{
    public interface ICacheable
    {
        CachePolicy CachingPolicy { get; }
    }

    public class CachePolicy
    {
        private TimeSpan _timeSpan = TimeSpan.Zero;

        /// <summary>
        /// Specifies a caching policy
        /// </summary>
        /// <param name="duration">The duration item remains in cache</param>
        /// <param name="refetchLimit">The number of times a cached item is refreshed. Never (-1), Forever (0), or a specific number of times</param>
        /// <param name="isSliding">Is cache duration renewed upon access of cached item (Not Currently Implemented)</param>
        public CachePolicy(TimeSpan duration, int refetchLimit = -1, bool isSliding = false)
        {
            this.Duration = duration;
            this.RefetchLimit = refetchLimit;
            this.IsSliding = isSliding;
        }

        public TimeSpan Duration
        {
            get
            {
                return _timeSpan;
            }

            protected set
            { //force policy to be passed in on creation
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException("Cache durations must be TimeSpan.Zero or greater");
                }

                //I also don't like cache TimeSpans if they are TimeSpan.MaxValue, these shouldn't be inserted with an
                //expiration but rather with none. Maybe handle this in CacheLayer abstraction.
                _timeSpan = value;
            }
        }

        public bool IsSliding { get; protected set; }

        public bool IsValid { get { return this.Duration > TimeSpan.Zero; } }

        //I don't know if we will support this logic, maybe we can in conjunction with Redis but Redis
        //has no expiration callback logic like .NET does, so it will have to be a combination of both
        //techniques and then we will need have a Master vs. Slave setting because we don't want slave webservers
        //recaching on top of a master. I'm planning on implementing this but don't know if I want redis handling
        //expirations or if we do it internally.
        public int RefetchLimit { get; protected set; }

        public static CachePolicy None { get { return new CachePolicy(TimeSpan.Zero); } }

        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj != null)
            {
                var comparePolicy = obj as CachePolicy;
                if (comparePolicy != null)
                {
                    result = comparePolicy.Duration == this.Duration && comparePolicy.RefetchLimit == this.RefetchLimit;
                }
            }
            return result;
        }

        public static bool operator ==(CachePolicy x, CachePolicy y)
        {
            var result = Object.ReferenceEquals(x, y);

            if (!result)
            {
                if (!Object.ReferenceEquals(x, null))
                {
                    result = x.Equals(y);
                }
                else if (!Object.ReferenceEquals(y, null))
                {
                    result = y.Equals(x);
                }
            }

            return result;
        }


        public static bool operator !=(CachePolicy x, CachePolicy y)
        {
            return !(x == y);
        }

    }
}
