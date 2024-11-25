using System.ComponentModel.DataAnnotations;

namespace Blog.API.Models.DTOs;

public class CreateCommentDto
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string content { get; set; }
    
    public Guid? parentId { get; set; }
}