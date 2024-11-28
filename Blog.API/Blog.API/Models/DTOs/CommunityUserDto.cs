using System.ComponentModel.DataAnnotations;

namespace Blog.API.Models.DTOs;

public class CommunityUserDto
{
    [Required]
    public Guid userId { get; set; }
    
    [Required]
    public Guid communityId { get; set; }
    
    [Required]
    public CommunityRole CommunityRole { get; set; }
}


