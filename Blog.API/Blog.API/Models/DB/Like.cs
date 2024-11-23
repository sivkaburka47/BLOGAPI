namespace Blog.API.Models.DB
{
    public class Like
    {
        public Guid id { get; set; } 
        
        public Guid postId { get; set; }
        public Post post { get; set; }

        public Guid userId { get; set; }
        public User user { get; set; }
    }

}