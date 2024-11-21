using System.ComponentModel.DataAnnotations;

namespace Blog.API.Models.DTOs;

public class UserDTO
{
    [Required]
    public Guid id { get; set; }
        
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime createTime { get; set; } 

    [Required]
    [MinLength(1)]
    public string fullName { get; set; } = string.Empty; 
    
    [DataType(DataType.DateTime)]
    public DateTime? birthDate { get; set; } 

    [Required]
    [EnumDataType(typeof(Gender))]
    public Gender gender { get; set; }

    [Required]
    [EmailAddress]
    [MinLength(1)]
    public string email { get; set; } = string.Empty;
    
    [Phone]
    public string? phoneNumber { get; set; } 
}