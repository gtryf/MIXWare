using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MIXUI.Entities;

namespace MIXUI.Helpers
{
    public class DataContext : IdentityDbContext<AppUser>
    {
        public DataContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                .HasQueryFilter(u => u.IsEnabled);

            builder.Entity<File>()
                .Property(f => f.Type)
                .HasDefaultValue(FileType.Source)
                .HasConversion<string>();

            builder.Entity<Submission>()
                .Property(s => s.Status)
                .HasDefaultValue(SubmissionStatus.New)
                .HasConversion<string>();

            builder.Entity<Submission>()
                .Property<string>("ErrorsStr")
                .HasField("_errors");
            builder.Entity<Submission>()
                .Property<string>("WarningsStr")
                .HasField("_warnings");
        }

        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Submission> Submissions { get; set; }
    }
}
