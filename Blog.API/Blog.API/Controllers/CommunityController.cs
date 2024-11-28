using System.ComponentModel.DataAnnotations;
using Blog.API.Models.DTOs;
using Blog.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers
{
    [Route("api/community")]
    [ApiController]
    public class CommunityController : ControllerBase
    {
        private ICommunityService _communityService;

        
        public CommunityController(ICommunityService communityService)
        {
            _communityService = communityService;
        }
        
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<CommunityDto>>> GetCommunityList()
        {
            var communityList = await _communityService.GetCommunityList();
            
            return Ok(communityList);
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<ActionResult<List<CommunityUserDto>>> GetUserCommunityList()
        {
            var communityList = await _communityService.GetUserCommunityList(User);
            
            return Ok(communityList);
        }
        
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<List<CommunityUserDto>>> GetConcreteCommunity(Guid id)
        {
            var communityList = await _communityService.GetConcreteCommunity(id);
            
            return Ok(communityList);
        }
        
        [Authorize]
        [HttpGet("{id}/post")]
        public async Task<ActionResult<PostPagedListDto>> GetList(
            Guid id,
            [FromQuery] List<Guid> tags,
            [FromQuery] PostSorting sorting,
            [FromQuery] int page = 1,
            [FromQuery] int size = 5)
        {
            var postPagedList = await _communityService.GetPostListInCommunity(id, tags, sorting, page, size, User);
            return Ok(postPagedList);
        }
        
        [Authorize]
        [HttpPost("{id}/post")]
        public async Task<ActionResult<Guid>> CreatePost(Guid id, CreatePostDto model)
        {
            var postId = await _communityService.CreatePost(model, id, User);
            return Ok(postId);
        }
        
        [Authorize]
        [HttpGet("{id}/role")]
        public async Task<IActionResult> GetUserRoleInCommunity(Guid id)
        {
            var role = await _communityService.GetUserRoleInCommunity(id, User);
            if (role == null)
            {
                return Ok("null");
            }
            
            return Ok(role);
        }

        [Authorize]
        [HttpPost("{id}/subscribe")]
        public async Task<IActionResult> SubscribeToCommunity(Guid id)
        {
            await _communityService.SubscribeToCommunity(id, User);
            return Ok();
        }
        
        [Authorize]
        [HttpDelete("{id}/unsubscribe")]
        public async Task<IActionResult> UnsubscribeToCommunity(Guid id)
        {
            await _communityService.UnsubscribeFromCommunity(id, User);
            return Ok();
        }
    }
}