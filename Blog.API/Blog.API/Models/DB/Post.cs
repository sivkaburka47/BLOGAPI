namespace Blog.API.Models.DB
{
    public class Post
    {
        public Guid id { get; set; }
        public DateTime createTime { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int readingTime { get; set; }
        public string? image { get; set; }
        public Guid authorId { get; set; }
        public User author { get; set; }
        public Guid? communityId { get; set; }
        public string? communityName { get; set; }
        public Guid? addressId { get; set; }
        public List<Like> likes { get; set; } = new List<Like>();
        public bool hasLike { get; set; } = false;
        public List<Tag>? tags { get; set; }
        public List<Comment> comments { get; set; } = new List<Comment>();

    }
}

