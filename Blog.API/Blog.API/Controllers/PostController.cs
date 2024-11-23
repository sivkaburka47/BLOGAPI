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

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Guid>> CreatePost(CreatePostDto model)
        {
            var postId = await _postService.CreatePost(model, User);
            return Ok(postId);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<PostFullDto>> GetConcretePost( Guid id)
        {
            var post = await _postService.GetConcretePost(id, User);
            return Ok(post);
        }
        
        [Authorize]
        [HttpPost("{postId}/like")]
        public async Task<ActionResult> LikeConcretePost( Guid postId)
        {
            var response = await _postService.LikeConcretePost(postId, User);
            return Ok(response);
        }
        
        [Authorize]
        [HttpDelete("{postId}/like")]
        public async Task<ActionResult> DeleteLikeConcretePost( Guid postId)
        {
            var response = await _postService.DeleteLikeConcretePost(postId, User);
            return Ok(response);
        }
    }
}