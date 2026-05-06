using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class UserDTO
    {
        [Required]
        public required string FirstName { get; set; }
        [Required] 
        public required string LastName { get; set; }
        [Required]
        public required string UserName { get; set; }

        [Required]
        public required string EmailAddress { get; set; }

        [Required]
        public required string Role { get; set; }

        public string? Token { get; set; }
    }
}
