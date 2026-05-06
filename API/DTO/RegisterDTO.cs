using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class RegisterDTO
    {
        [Required]
        public required string UserName { get; set; }

        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress]
        public required string EmailAddress { get; set; }

        [Required]
        public required string Password { get; set; }

        [Required]
        public string Role { get; set; } = "Student";
    }
}
