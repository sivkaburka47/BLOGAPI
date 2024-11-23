using System.ComponentModel.DataAnnotations;

namespace Blog.API.Models.DTOs;

public class PostFullDto
{
    [Required]
    public Guid id { get; set; }
    
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime createTime { get; set; }
    
    [Required]
    [MinLength(1)]
    public string title { get; set; }
    
    [Required]
    [MinLength(1)]
    public string description { get; set; }
    
    [Required]
    public int readingTime { get; set; }
    
    public string? image { get; set; }
    
    [Required]
    public Guid authorId { get; set; }
    
    [Required]
    [MinLength(1)]
    public string author { get; set; }
    
    public Guid? communityId { get; set; }
    
    public string? communityName { get; set; }
    
    public Guid? addressId { get; set; }

    [Required] 
    public int likes { get; set; } = 0;

    [Required] 
    public bool hasLike { get; set; } = false;
    
    [Required]
    public int commentsCount { get; set; } = 0;
    
    public List<TagDto>? tags { get; set; }
    
}