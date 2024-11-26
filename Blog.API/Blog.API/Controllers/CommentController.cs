using Blog.API.Models.DTOs;
using Blog.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private ICommentService _commentService;

        
        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }
        
        [AllowAnonymous]
        [HttpGet("comment/{id}/tree")]
        public async Task<ActionResult<List<CommentDto>>> GetCommentsTree(Guid id)
        {
            var commentsTree = await _commentService.GetCommentsTree(id);
            
            return Ok(commentsTree);
        }
        
        [Authorize]
        [HttpPost("post/{id}/comment")]
        public async Task<ActionResult> AddCommentToPost(Guid id, CreateCommentDto model)
        {
            var response = await _commentService.AddCommentToPost(id, model, User);
            return Ok(response);
        }

        [Authorize]
        [HttpPut("comment/{id}")]
        public async Task<IActionResult> EditComment(Guid id, UpdateCommentDto updateCommentDto)
        {
            var response = await _commentService.EditComment(id, updateCommentDto, User);
            return Ok(response);
        }
        

        [Authorize]
        [HttpDelete("comment/{id}")]
        public async Task<ActionResult> DeleteComment(Guid id)
        {
            var response = await _commentService.DeleteComment(id, User);
            return Ok(response);
        }
    }
}