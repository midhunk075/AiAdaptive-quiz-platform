using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class LoginDTO
    {
        [Required]
        public required string UserName { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
