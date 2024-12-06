using Blog.API.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace Blog.API.Data
{
    public class BlogDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Like> Likes { get; set; }
        
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Community> Communities { get; set; }
        
        public DbSet<CommunityUser> CommunityUsers { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TokenBlackList> TokenBlackList { get; set; }
        
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<CommunityUser>(entity =>
            {
                entity.HasKey(e => new { e.userId, e.communityId });
            });
        }

    }
}