using API.DTO;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IAccountService accountService) : ControllerBase
    {
        [Authorize]
        [HttpGet("members")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await accountService.GetUsersAsync();
            return Ok(users);
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserDTO>> RegisterUser(RegisterDTO registerDTO)
        {
            var result = await accountService.RegisterAsync(registerDTO);
            return Ok(result);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var result = await accountService.LoginAsync(loginDTO);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("members/{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(string id)
        {
            var user = await accountService.GetUserAsync(id);
            return Ok(user);
        }
    }
}
