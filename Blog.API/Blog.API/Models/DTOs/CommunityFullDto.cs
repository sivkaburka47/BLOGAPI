using System.ComponentModel.DataAnnotations;

namespace Blog.API.Models.DTOs;

public class CommunityFullDto
{
    [Required]
    public Guid id { get; set; }
    
    [Required]
    public DateTime createTime { get; set; }
    
    [Required]
    [MinLength(1)]
    public string name { get; set; }
    
    public string? description { get; set; }
    
    [Required] 
    public bool isClosed { get; set; } = false;

    [Required] 
    public int subscribersCount { get; set; } = 0;
    
    [Required] 
    public List<UserDTO> administrators { get; set; }
}
