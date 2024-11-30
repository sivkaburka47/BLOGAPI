using System.Security.Claims;
using Blog.API.Data;
using Blog.API.Middleware;
using Blog.API.Models.DB;
using Blog.API.Models.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.API.Services;

public interface ICommentService
{
    Task<ActionResult> AddCommentToPost(Guid postId, CreateCommentDto model, ClaimsPrincipal user);
    Task<List<CommentDto>> GetCommentsTree(Guid commentId);
    Task<IActionResult> DeleteComment(Guid commentId, ClaimsPrincipal user);
    Task<IActionResult> EditComment(Guid commentId, UpdateCommentDto updateCommentDto, ClaimsPrincipal user);
}

public class CommentService : ICommentService
{
    private readonly BlogDbContext _context;
    
    public CommentService(BlogDbContext context)
    {
        _context = context;
    }
    
public async Task<ActionResult> AddCommentToPost(Guid postId, CreateCommentDto model, ClaimsPrincipal user)
{
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId == null || !Guid.TryParse(userId, out var parsedId))
    {
        throw new UnauthorizedAccessException();
    }

    var post = await _context.Posts
        .Include(p => p.community)
        .FirstOrDefaultAsync(p => p.id == postId);

    if (post == null)
        throw new KeyNotFoundException($"Post with id {postId} not found.");

    var community = post.community;

    if (community != null && community.isClosed)
    {
        var isMember = await _context.CommunityUsers
            .AnyAsync(cu => cu.communityId == community.id && cu.userId == parsedId);

        if (!isMember)
        {
            throw new ForbiddenAccessException($"User is not a member of the closed community id={community.id}");
        }
    }

    Comment? parentComment = null;
    if (model.parentId != null)
    {
        parentComment = await _context.Comments
            .FirstOrDefaultAsync(c => c.id == model.parentId.Value);

        if (parentComment == null)
            throw new KeyNotFoundException("Parent comment not found");

        if (parentComment.postId != postId)
            throw new ValidationAccessException("Parent comment is attached to another post");
    }

    var newComment = new Comment
    {
        id = Guid.NewGuid(),
        content = model.content,
        createTime = DateTime.UtcNow,
        postId = postId,
        authorId = parsedId,
        parentCommentId = model.parentId
    };

    _context.Comments.Add(newComment);
    await _context.SaveChangesAsync();
    return null;
}



    public async Task<List<CommentDto>> GetCommentsTree(Guid commentId)
    {
        var commentExists = await _context.Comments.AnyAsync(c => c.id == commentId);
        if (!commentExists)
        {
            throw new KeyNotFoundException($"Comment with id={commentId} not found");
        }
        
        var comments = await _context.Comments
            .AsNoTracking()
            .Include(c => c.author)
            .Where(c => c.parentCommentId == commentId)
            .Select(c => new CommentDto
            {
                id = c.id,
                createTime = c.createTime,
                content = c.content,
                modifiedDate = c.modifiedDate,
                deleteDate = c.deleteDate,
                authorId = c.authorId,
                author = c.author.fullName,
                subComments = c.replies.Count
            })
            .ToListAsync();

        async Task<List<CommentDto>> BuildDtos(List<CommentDto> comments)
        {
            var dtos = new List<CommentDto>();
            foreach (var comment in comments)
            {
                dtos.Add(comment);
                var subComments = await _context.Comments
                    .AsNoTracking()
                    .Include(c => c.author)
                    .Where(c => c.parentCommentId == comment.id)
                    .Select(c => new CommentDto
                    {
                        id = c.id,
                        createTime = c.createTime,
                        content = c.content,
                        modifiedDate = c.modifiedDate,
                        deleteDate = c.deleteDate,
                        authorId = c.authorId,
                        author = c.author.fullName,
                        subComments = c.replies.Count
                    })
                    .ToListAsync();

                dtos.AddRange(await BuildDtos(subComments));
            }
            return dtos;
        }

        var commentDtos = await BuildDtos(comments);

        commentDtos = commentDtos.OrderBy(c => c.createTime).ToList();

        return commentDtos;
    }
    
    public async Task<IActionResult> DeleteComment(Guid commentId, ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Guid.TryParse(userId, out var parsedId))
        {
            throw new UnauthorizedAccessException();
        }
        

        var comment = await _context.Comments
            .Include(c => c.replies)
            .Include(c => c.post)
            .ThenInclude(p => p.community)
            .FirstOrDefaultAsync(c => c.id == commentId);

        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment Id={commentId} not found");
        }

        var community = comment.post?.community;

        if (community != null && community.isClosed)
        {
            var isMember = await _context.CommunityUsers
                .AnyAsync(cu => cu.communityId == community.id && cu.userId == parsedId);

            if (!isMember)
            {
                throw new ForbiddenAccessException($"User is not a member of the closed community id={community.id}");
            }
        }
        
        if (comment.authorId != parsedId)
        {
            throw new ForbiddenAccessException("Not enough rights to delete this comment");
        }
        
        if (comment.replies.Any())
        {
            comment.content = "";
            comment.modifiedDate = DateTime.UtcNow;
            comment.deleteDate = DateTime.UtcNow;
        }
        else
        {
            _context.Comments.Remove(comment);
        }

        await _context.SaveChangesAsync();
        return null;
    }

    
    public async Task<IActionResult> EditComment(Guid commentId, UpdateCommentDto updateCommentDto, ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Guid.TryParse(userId, out var parsedId))
        {
            throw new UnauthorizedAccessException();
        }
    
        var comment = await _context.Comments
            .Include(c => c.post)
            .ThenInclude(p => p.community)
            .FirstOrDefaultAsync(c => c.id == commentId);

        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment Id={commentId} not found");
        }
        
        var community = comment.post?.community;

        if (community != null && community.isClosed)
        {
            var isMember = await _context.CommunityUsers
                .AnyAsync(cu => cu.communityId == community.id && cu.userId == parsedId);

            if (!isMember)
            {
                throw new ForbiddenAccessException($"User is not a member of the closed community id={community.id}");
            }
        }

        if (comment.authorId != parsedId)
        {
            throw new ForbiddenAccessException("Not enough rights to edit this comment");
        }
        
        comment.content = updateCommentDto.content;
        comment.modifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return null;
    }
}