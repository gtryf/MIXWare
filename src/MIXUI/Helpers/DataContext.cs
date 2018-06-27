using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MIXUI.Entities;

namespace MIXUI.Helpers
{
    public class DataContext : IdentityDbContext<AppUser>
    {
        public DataContext(DbContextOptions options) : base(options) { }

        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            AddTimestamps();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries().Where(x => x.Entity is EntityBase && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((EntityBase)entity.Entity).CreatedUtc = DateTime.UtcNow;
                }

                ((EntityBase)entity.Entity).UpdatedUtc = DateTime.UtcNow;
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                .HasQueryFilter(u => u.IsEnabled);

            builder.Entity<File>()
                .Property(f => f.Type)
                .HasDefaultValue(FileType.Source)
                .HasConversion<string>();
            builder.Entity<File>()
                .Property(f => f.UpdatedUtc)
                .HasDefaultValueSql("current_timestamp");
            builder.Entity<File>()
                .Property(f => f.CreatedUtc)
                .HasDefaultValueSql("current_timestamp");

            builder.Entity<Submission>()
                .Property(s => s.Status)
                .HasDefaultValue(SubmissionStatus.New)
                .HasConversion<string>();
            builder.Entity<Submission>()
                .Property(f => f.UpdatedUtc)
                .HasDefaultValueSql("current_timestamp");
            builder.Entity<Submission>()
                .Property(f => f.CreatedUtc)
                .HasDefaultValueSql("current_timestamp");
            builder.Entity<Submission>()
                .Property<string>("ErrorsStr")
                .HasField("_errors");
            builder.Entity<Submission>()
                .Property<string>("WarningsStr")
                .HasField("_warnings");

            builder.Entity<Workspace>()
                .Property(f => f.UpdatedUtc)
                .HasDefaultValueSql("current_timestamp");
            builder.Entity<Workspace>()
                .Property(f => f.CreatedUtc)
                .HasDefaultValueSql("current_timestamp");
        }

        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Submission> Submissions { get; set; }
    }
}
