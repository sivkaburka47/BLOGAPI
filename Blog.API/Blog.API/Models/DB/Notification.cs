namespace Blog.API.Models.DB
{
    public class Notification
    {
        public Guid id { get; set; }
        public Guid postId { get; set; }
        public string subscriberEmail { get; set; }
        public DateTime createdAt { get; set; }
        public string errorMessage { get; set; }
        public bool isPermanentFailure { get; set; }

    }
}
