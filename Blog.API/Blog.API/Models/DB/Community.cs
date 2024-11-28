namespace Blog.API.Models.DB
{
    public class Community
    {
        public Guid id { get; set; }
        
        public DateTime createTime { get; set; }
        
        public string name { get; set; }
        
        public string? description { get; set; }
        
        public bool isClosed { get; set; } = false;
        
        public List<CommunityUser> communityUsers { get; set; } = new List<CommunityUser>();
        
        public List<Post> posts { get; set; } = new List<Post>();
    }
}


