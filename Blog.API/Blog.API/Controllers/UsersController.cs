

using Blog.API.Models.DTOs;
using Blog.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers
{

    [Route("api/account")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IAccountService _accountService;

        public UsersController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterModel model)
        {
            var response = await _accountService.RegisterAsync(model);
            return Ok(response);
        }
        
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponse>> Login(LoginCredentials model)
        {
            var response = await _accountService.LoginAsync(model);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var response = await _accountService.Logout(token, User);
            return Ok(response);
        }
        
        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<UserDTO>> getProfile()
        {
            var response = await _accountService.GetProfile(User);
            return Ok(response);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult> editProfile(UserEditModel userEdit)
        {
            var response = await _accountService.EditProfile(userEdit, User);
            return Ok(response);
        }
        
 

    }
}