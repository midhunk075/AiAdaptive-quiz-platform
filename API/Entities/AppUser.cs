using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class AppUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }

        [Required]
        public required string UserName { get; set; }

        [Required]
        [EmailAddress]
        public required string EmailAddress { get; set; }

        [Required]
        public required byte[] PasswordHash { get; set; }

        [Required]
        public required byte[] PasswordSalt { get; set; }

        [Required]
        public required string Role { get; set; }
    }
}
