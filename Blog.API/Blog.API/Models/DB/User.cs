using Blog.API.Models.DTOs;

namespace Blog.API.Models.DB
{
    public class User
    {
        public Guid id { get;  set; }
        public string fullName { get; set; }
        public DateTime? birthDate { get; set; }
        public DateTime createTime { get; set; }
        public Gender gender { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string passwordHash { get; set; }
        
    }
}