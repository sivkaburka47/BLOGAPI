using System.ComponentModel.DataAnnotations;

namespace Blog.API.Models.DTOs;

public class TagDto
{
    [Required]
    public Guid id { get; set; }
    
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime createTime { get; set; }
    
    [Required]
    [MinLength(1)]
    public string name { get; set; }
}