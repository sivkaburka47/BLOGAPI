namespace Blog.API.Models.DB
{
    public class Comment
    {
        public Guid id { get; set; }
    
        public string content { get; set; }
    
        public DateTime createTime { get; set; }
    
        public DateTime? modifiedDate { get; set; }
    
        public DateTime? deleteDate { get; set; }
        
        public Guid postId { get; set; }
        public Post post { get; set; }
        
        public Guid authorId { get; set; }
        public User author { get; set; }
        
        public Guid? parentCommentId { get; set; }
        public Comment? parentComment { get; set; } 
        
        public List<Comment> replies { get; set; } = new List<Comment>();
    }

}

