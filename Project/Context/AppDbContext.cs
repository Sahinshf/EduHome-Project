using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project.Models.Common;

namespace Project.Context
{
    public class AppDbContext : IdentityDbContext <AppUser>
    {
        public AppDbContext ( DbContextOptions<AppDbContext> options) :base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseCategory> CourseCategory { get; set; }

        public DbSet<Event> Events { get; set; }
        public DbSet<Speaker> Speakers { get; set; }
        public DbSet<EventSpeaker> EventsSpeaker { get; set; }

        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<SocialMedia> SocialMedias { get; set; }

        public DbSet<Blog> Blogs { get; set; }  

        public DbSet<Subscribe> Subscribes { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var item in entries)
            {
                switch (item.State)
                {
                    case EntityState.Unchanged:
                        item.Entity.CreatedAt = DateTime.UtcNow;
                        item.Entity.ModifiedAt = DateTime.UtcNow;
                        break;

                    case EntityState.Modified:
                        item.Entity.ModifiedAt = DateTime.UtcNow;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
