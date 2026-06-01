using System.ComponentModel.DataAnnotations;

namespace API.Authentication.DTO
{
    public class LoginDTO
    {
        [Required]
        public required string UserName { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
