using System.ComponentModel.DataAnnotations;

namespace Blog.API.Models.DTOs;

public class AuthorDto
{
    [Required]
    [MinLength(1)]
    public string fullName { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime? birthDate { get; set; }
    
    [Required]
    [EnumDataType(typeof(Gender))]
    public Gender gender { get; set; }
    
    public int posts { get; set; }
    
    public int likes { get; set; }
    
    public DateTime created { get; set; }
}