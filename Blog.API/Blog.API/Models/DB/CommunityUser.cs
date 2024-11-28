using Blog.API.Models.DTOs;

namespace Blog.API.Models.DB
{
    public class CommunityUser
    {
        public Guid userId { get; set; }
        public User user { get; set; }

        public Guid communityId { get; set; }
        public Community community { get; set; }

        public List<CommunityRole> communityRoles { get; set; } = new List<CommunityRole>();
    }

}

