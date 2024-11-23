namespace Blog.API.Models.DB
{
    public class Tag
    {
        public Guid id { get; set; }
    
        public DateTime createTime { get; set; }
    
        public string name { get; set; }
        
        public List<Post>? posts { get; set; }
    }
}

