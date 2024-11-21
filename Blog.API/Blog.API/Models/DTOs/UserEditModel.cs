using System.ComponentModel.DataAnnotations;

namespace Blog.API.Models.DTOs;

public class UserEditModel
{
    [Required]
    [EmailAddress]
    [MinLength(1)]
    public string email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(1)]
    public string fullName { get; set; } = string.Empty; 
    
    [DataType(DataType.DateTime)]
    public DateTime? birthDate { get; set; } 

    [Required]
    [EnumDataType(typeof(Gender))]
    public Gender gender { get; set; }
    
    [Phone]
    public string? phoneNumber { get; set; } 
}