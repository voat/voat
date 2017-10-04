using System;
using Microsoft.EntityFrameworkCore;

namespace Voat.Data.Models
{
    public abstract partial class VoatEntityContext : DbContext
    {

        public class UnintentionalCodeFirstException : Exception { }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //This code should remain independent of any backend 

            //CORE_PORT: Keys, will fix later and each table should have a ID INT PK
            modelBuilder.Entity<CommentRemovalLog>().HasKey(x => new { x.CommentID });
            modelBuilder.Entity<SubmissionRemovalLog>().HasKey(x => new { x.SubmissionID });
            //modelBuilder.Entity<SubmissionRemovalLog>().HasOne(x => x.Submission, );

            modelBuilder.Entity<DefaultSubverse>().HasKey(x => new { x.Subverse });
            modelBuilder.Entity<SessionTracker>().HasKey(x => new { x.SessionID, x.Subverse });
            modelBuilder.Entity<StickiedSubmission>().HasKey(x => new { x.SubmissionID, x.Subverse });
            modelBuilder.Entity<UserPreference>().HasKey(x => new { x.UserName });
            modelBuilder.Entity<ViewStatistic>().HasKey(x => new { x.SubmissionID, x.ViewerID });
            
        }
    }
}