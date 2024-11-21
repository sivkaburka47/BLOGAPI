using System.ComponentModel.DataAnnotations;

namespace Blog.API.Models.DTOs
{
    public class UserRegisterModel
    {
        [Required]
        [MinLength(1)]
        public string fullName { get; set; }

        [Required]
        [MinLength(6)]
        public string password { get; set; }

        [Required]
        [EmailAddress]
        public string email { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? birthDate { get; set; }

        [Required]
        [EnumDataType(typeof(Gender))]
        public Gender gender { get; set; }

        [Phone]
        public string phoneNumber { get; set; }
    }
}