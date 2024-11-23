using Blog.API.Models.DB;
using Blog.API.Models.DTOs;
using Blog.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers
{
    [Route("api/post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }
    
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<PostPagedListDto>> GetList(
            [FromQuery] List<Guid> tags,
            [FromQuery] string? author,
            [FromQuery] int min,
            [FromQuery] int max,
            [FromQuery] PostSorting sorting,
            [FromQuery] bool onlyMyCommunities = false,
            [FromQuery] int page = 1,
            [FromQuery] int size = 5)
        {
            var postPagedList = await _postService.GetList(tags, author, min, max, sorting, onlyMyCommunities, page, size, User);
            return Ok(postPagedList);
        }
    }
}