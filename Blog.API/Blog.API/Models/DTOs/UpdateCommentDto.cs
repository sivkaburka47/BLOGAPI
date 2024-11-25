using System.ComponentModel.DataAnnotations;

namespace Blog.API.Models.DTOs;

public class UpdateCommentDto
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string content { get; set; }
}