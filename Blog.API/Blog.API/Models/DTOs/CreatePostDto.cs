using System.ComponentModel.DataAnnotations;

namespace Blog.API.Models.DTOs;

public class CreatePostDto
{
    [Required]
    [StringLength(1000, MinimumLength = 5)]
    public string title { get; set; }

    [Required]
    [StringLength(5000, MinimumLength = 5)]
    public string description { get; set; }

    [Required]
    public int readingTime { get; set; }

    [Url]
    [MaxLength(1000)]
    public string? image { get; set; }

    public Guid? addressId { get; set; }

    [Required]
    [MinLength(1)]
    public List<Guid> tags { get; set; }
}