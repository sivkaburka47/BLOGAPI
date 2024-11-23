using System.Security.Claims;
using Blog.API.Data;
using Blog.API.Middleware;
using Blog.API.Models.DB;
using Blog.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.API.Services
{
    public interface IPostService
    {
        Task<PostPagedListDto> GetList(
            [FromQuery] List<Guid> tags,
            [FromQuery] string? author,
            [FromQuery] int min,
            [FromQuery] int max,
            [FromQuery] PostSorting sorting,
            [FromQuery] bool onlyMyCommunities,
            [FromQuery] int page,
            [FromQuery] int size,
            ClaimsPrincipal user);
    }

    public class PostService : IPostService
    {
        private readonly BlogDbContext _context;

        public PostService(BlogDbContext context)
        {
            _context = context;
        }


        public async Task<PostPagedListDto> GetList(
            [FromQuery] List<Guid> tags,
            [FromQuery] string? author,
            [FromQuery] int min,
            [FromQuery] int max,
            [FromQuery] PostSorting sorting,
            [FromQuery] bool onlyMyCommunities,
            [FromQuery] int page,
            [FromQuery] int size,
            ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var parsedId))
            {
                throw new UnauthorizedAccessException();
            }

            var userdb = await _context.Users
                .FirstOrDefaultAsync(d => d.id == parsedId);
            
            if (page < 1 || size < 1)
            {
                throw new ValidationAccessException("page or size must be greater than 0");
            }
            
            var query = _context.Posts
                .Include(p => p.tags)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(post => post.author == author);
            }
            
            if (min > 0)
            {
                query = query.Where(post => post.readingTime >= min);
            }

            if (max > 0)
            {
                query = query.Where(post => post.readingTime <= max);
            }
            
            if (tags != null && tags.Any())
            {
                query = query.Where(post => post.tags.Any(tag => tags.Contains(tag.id)));
            }
            
            switch (sorting)
            {
                case PostSorting.CreateDesc:
                    query = query.OrderByDescending(post => post.createTime);
                    break;
                case PostSorting.CreateAsc:
                    query = query.OrderBy(post => post.createTime);
                    break;
                case PostSorting.LikeAsc:
                    query = query.OrderBy(post => post.likes);
                    break;
                case PostSorting.LikeDesc:
                    query = query.OrderByDescending(post => post.likes);
                    break;
            }
            
            var totalItems = await query.CountAsync();
            
            var posts = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
            
            var postsdb = posts.Select(post => new PostDto
            {
                id = post.id,
                createTime = post.createTime,
                title = post.title,
                description = post.description,
                readingTime = post.readingTime,
                image = post.image,
                authorId = post.authorId,
                author = post.author,
                communityId = post.communityId,
                communityName = post.communityName,
                addressId = post.addressId,
                likes = post.likes,
                hasLike = post.hasLike,
                commentsCount = post.commentsCount,
                tags = post.tags?.Select(tag => new TagDto
                {
                    id = tag.id,
                    createTime = tag.createTime,
                    name = tag.name
                }).ToList()
            }).ToList();
            
            var pageInfo = new PageInfoModel
            {
                size = size,
                count = (int)Math.Ceiling((double)totalItems / size),
                current = page
            };
            
            if (pageInfo.current > pageInfo.count)
            {
                throw new ValidationAccessException("current page must be less than page count");
            }

            return new PostPagedListDto
            {
                posts = postsdb,
                pagination = pageInfo
            };
        }
    }
}