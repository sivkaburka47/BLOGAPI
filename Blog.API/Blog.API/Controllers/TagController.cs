using Blog.API.Models.DTOs;
using Blog.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }
        
        [AllowAnonymous]
        [HttpGet("tag")]
        public async Task<ActionResult<List<TagDto>>> GetTags()
        {
            var tags = await _tagService.GetTags();
            
            return Ok(tags);
        }
    }
}