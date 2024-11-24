using Blog.API.Models.DTOs;
using Blog.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers
{
    [Route("api/author")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private IAuthorService _authorService;

        
        public AuthorController(IAuthorService authorService)
        {
            _authorService = authorService;
        }
        
        [AllowAnonymous]
        [HttpGet("list")]
        public async Task<ActionResult<List<AuthorDto>>> GetAuthorList()
        {
            var authorList = await _authorService.GetAuthorList();
            
            return Ok(authorList);
        }
    }
}