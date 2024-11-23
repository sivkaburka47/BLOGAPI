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
        
        public DbSet<TokenBlackList> TokenBlackList { get; set; }
        
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        

    }
}