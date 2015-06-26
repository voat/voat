namespace Voat.Models
{
    using System;

    public class NotificationCountModel : IEquatable<NotificationCountModel>
    {
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(NotificationCountModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CommentReplies == other.CommentReplies && PostReplies == other.PostReplies && PrivateMessages == other.PrivateMessages;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NotificationCountModel) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CommentReplies;
                hashCode = (hashCode*397) ^ PostReplies;
                hashCode = (hashCode*397) ^ PrivateMessages;
                return hashCode;
            }
        }

        public NotificationCountModel(int commentReplies, int postReplies, int privateMessages)
        {
            CommentReplies = commentReplies;
            PostReplies = postReplies;
            PrivateMessages = privateMessages;
        }

        public NotificationCountModel() : this(0, 0, 0) { }

        public int CommentReplies { get; private set; }
        public int PostReplies { get; private set; }
        public int PrivateMessages { get; private set; }
        public int Total { get { return CommentReplies + PostReplies + PrivateMessages; } }
    }
}