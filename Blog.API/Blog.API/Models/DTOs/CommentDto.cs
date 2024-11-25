using System.ComponentModel.DataAnnotations;

namespace Blog.API.Models.DTOs;

public class CommentDto
{
    [Required]
    public Guid id { get; set; }
    
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime createTime { get; set; }
    
    [Required]
    [MinLength(1)]
    public string content { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime? modifiedDate { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime? deleteDate { get; set; }
    
    [Required]
    public Guid authorId { get; set; }
    
    [Required]
    [MinLength(1)]
    public string author { get; set; }
    
    [Required]
    public int subComments { get; set; }
}