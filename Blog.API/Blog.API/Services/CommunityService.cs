using System.Security.Claims;
using Blog.API.Data;
using Blog.API.Middleware;
using Blog.API.Models.DB;
using Blog.API.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.API.Services;

public interface ICommunityService
{
    Task<List<CommunityDto>> GetCommunityList();
    Task<List<CommunityUserDto>> GetUserCommunityList(ClaimsPrincipal user);
    Task<CommunityFullDto> GetConcreteCommunity(Guid communityId);
    Task<PostPagedListDto> GetPostListInCommunity(Guid communityId, List<Guid> tags, PostSorting sorting, int page, int size, ClaimsPrincipal user);
    Task<Guid> CreatePost(CreatePostDto model, Guid communityId, ClaimsPrincipal user);
    Task<CommunityRole?> GetUserRoleInCommunity(Guid communityId, ClaimsPrincipal user);
    Task SubscribeToCommunity(Guid communityId, ClaimsPrincipal user);
    Task UnsubscribeFromCommunity(Guid communityId, ClaimsPrincipal user);
}

public class CommunityService : ICommunityService
{
    private readonly BlogDbContext _context;

    public CommunityService(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<List<CommunityDto>> GetCommunityList()
    {
        var communities = await _context.Communities
            .Select(c => new CommunityDto
            {
                id = c.id,
                createTime = c.createTime,
                name = c.name,
                description = c.description,
                isClosed = c.isClosed,
                subscribersCount = _context.CommunityUsers
                    .Count(cu => cu.communityId == c.id && cu.communityRoles.Contains(CommunityRole.Subscriber))
            })
            .ToListAsync();

        return communities;
    }

    public async Task<List<CommunityUserDto>> GetUserCommunityList(ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Guid.TryParse(userId, out var parsedId))
        {
            throw new UnauthorizedAccessException();
        }
        
        var userdb = await _context.Users
            .FirstOrDefaultAsync(d => d.id == parsedId);

        if (userdb == null)
            throw new KeyNotFoundException("User not found");

        var userCommunities = await _context.CommunityUsers
            .Where(cu => cu.userId == parsedId)
            .GroupBy(cu => cu.communityId)
            .Select(g => new CommunityUserDto
            {
                userId = parsedId,
                communityId = g.Key,
                CommunityRole = g.SelectMany(cu => cu.communityRoles).Min()
            })
            .ToListAsync();

        return userCommunities;
    }

    public async Task<CommunityFullDto> GetConcreteCommunity(Guid communityId)
    {
        var community = await _context.Communities
            .Where(c => c.id == communityId)
            .Include(c => c.communityUsers)
            .ThenInclude(cu => cu.user)
            .FirstOrDefaultAsync();

        if (community == null)
            throw new KeyNotFoundException($"Community with id={communityId} not found");

        var communityFullDto = new CommunityFullDto
        {
            id = community.id,
            createTime = community.createTime,
            name = community.name,
            description = community.description,
            isClosed = community.isClosed,
            subscribersCount = community.communityUsers.Count(cu => cu.communityRoles.Contains(CommunityRole.Subscriber)),
            administrators = community.communityUsers
                .Where(cu => cu.communityRoles.Contains(CommunityRole.Administrator))
                .Select(cu => new UserDTO
                {
                    id = cu.user.id,
                    createTime = cu.user.createTime,
                    fullName = cu.user.fullName,
                    birthDate = cu.user.birthDate,
                    gender = cu.user.gender,
                    email = cu.user.email,
                    phoneNumber = cu.user.phoneNumber
                })
                .ToList()
        };

        return communityFullDto;
    }

    public async Task<PostPagedListDto> GetPostListInCommunity(Guid communityId, List<Guid> tags, PostSorting sorting, int page, int size, ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Guid.TryParse(userId, out var parsedId))
        {
            throw new UnauthorizedAccessException();
        }
        
        var userdb = await _context.Users
            .FirstOrDefaultAsync(d => d.id == parsedId);

        if (userdb == null)
            throw new KeyNotFoundException("User not found");

        if (page < 1 || size < 1)
        {
            throw new ValidationAccessException("page or size must be greater than 0");
        }
        
        if (tags != null && tags.Any())
        {
            foreach (var tagId in tags)
            {
                var tagExists = await _context.Tags.AnyAsync(t => t.id == tagId);
                if (!tagExists)
                {
                    throw new KeyNotFoundException($"Tag with id={tagId} not found in database");
                }
            }
        }

        var community = await _context.Communities
            .Include(c => c.communityUsers)
            .FirstOrDefaultAsync(c => c.id == communityId);

        if (community == null)
            throw new KeyNotFoundException($"Community with id={communityId} not found");

        if (community.isClosed)
        {
            var isMember = community.communityUsers.Any(cu => cu.userId == parsedId);

            if (!isMember)
                throw new ForbiddenAccessException($"Access to closed community with id={communityId} is forbidden");
        }

        var query = _context.Posts
            .Include(p => p.tags)
            .Include(p => p.likes)
            .Include(p => p.author)
            .Where(post => post.communityId == communityId)
            .AsQueryable();

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
                query = query.OrderBy(post => post.likes.Count());
                break;
            case PostSorting.LikeDesc:
                query = query.OrderByDescending(post => post.likes.Count());
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
            author = post.author.fullName,
            communityId = post.communityId,
            communityName = community.name,
            addressId = post.addressId,
            likes = post.likes.Count(),
            hasLike = post.likes.Any(like => like.userId == parsedId),
            commentsCount = _context.Comments.Count(c => c.postId == post.id),
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

    public async Task<Guid> CreatePost(CreatePostDto model, Guid communityId, ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Guid.TryParse(userId, out var parsedId))
        {
            throw new UnauthorizedAccessException();
        }
        
        var userdb = await _context.Users
            .FirstOrDefaultAsync(d => d.id == parsedId);

        if (userdb == null)
            throw new KeyNotFoundException("User not found");

        var community = await _context.Communities
            .Include(c => c.communityUsers)
            .FirstOrDefaultAsync(c => c.id == communityId);

        if (community == null)
            throw new KeyNotFoundException($"Community with id={communityId} not found");
        
        var existingUser = await _context.CommunityUsers
            .FirstOrDefaultAsync(cu => cu.communityId == communityId && cu.userId == parsedId);

        if (existingUser == null || !existingUser.communityRoles.Contains(CommunityRole.Administrator))
        {
            throw new ForbiddenAccessException($"User Id={parsedId} is not able to post in community Id={communityId}");
        }

        var post = new Post
        {
            createTime = DateTime.UtcNow,
            title = model.title,
            description = model.description,
            readingTime = model.readingTime,
            image = model.image,
            authorId = parsedId,
            author = userdb,
            communityId = communityId,
            addressId = model.addressId,
            tags = new List<Tag>(),
            comments = new List<Comment>()
        };

        var existingTags = await _context.Tags
            .Where(t => model.tags.Contains(t.id))
            .ToListAsync();

        var missingTagIds = model.tags.Except(existingTags.Select(t => t.id)).ToList();
        if (missingTagIds.Any())
        {
            var firstMissingTagId = missingTagIds.First();
            throw new KeyNotFoundException($"Tag Id={firstMissingTagId} does not found");
        }

        post.tags.AddRange(existingTags);

        _context.Posts.Add(post);

        await _context.SaveChangesAsync();

        return post.id;
    }

    public async Task<CommunityRole?> GetUserRoleInCommunity(Guid communityId, ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Guid.TryParse(userId, out var parsedId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        
        var userdb = await _context.Users
            .FirstOrDefaultAsync(d => d.id == parsedId);

        if (userdb == null)
            throw new KeyNotFoundException("User not found");

        var community = await _context.Communities
            .Include(c => c.communityUsers)
            .FirstOrDefaultAsync(c => c.id == communityId);

        if (community == null)
            throw new KeyNotFoundException($"Community with id={communityId} not found");

        var userRoles = await _context.CommunityUsers
            .Where(cu => cu.communityId == communityId && cu.userId == parsedId)
            .Select(cu => cu.communityRoles)
            .FirstOrDefaultAsync();

        if (userRoles == null || !userRoles.Any())
        {
            return null;
        }

        var userRole = userRoles.Min();

        return userRole;
    }

    public async Task SubscribeToCommunity(Guid communityId, ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Guid.TryParse(userId, out var parsedId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        
        var userdb = await _context.Users
            .FirstOrDefaultAsync(d => d.id == parsedId);

        if (userdb == null)
            throw new KeyNotFoundException("User not found");

        var community = await _context.Communities
            .FirstOrDefaultAsync(c => c.id == communityId);

        if (community == null)
            throw new KeyNotFoundException($"Community with id={communityId} not found");

        var existingUser = await _context.CommunityUsers
            .FirstOrDefaultAsync(cu => cu.communityId == communityId && cu.userId == parsedId);

        if (existingUser != null && existingUser.communityRoles.Contains(CommunityRole.Subscriber))
        {
            throw new ValidationAccessException(
                $"User with id={parsedId} already subscribed to the community with id={communityId}.");
        }

        if (existingUser != null)
        {
            existingUser.communityRoles.Add(CommunityRole.Subscriber);
        }
        else
        {
            _context.CommunityUsers.Add(new CommunityUser
            {
                userId = parsedId,
                communityId = communityId,
                communityRoles = new List<CommunityRole> { CommunityRole.Subscriber }
            });
        }

        await _context.SaveChangesAsync();
    }


    public async Task UnsubscribeFromCommunity(Guid communityId, ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Guid.TryParse(userId, out var parsedId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        
        var userdb = await _context.Users
            .FirstOrDefaultAsync(d => d.id == parsedId);

        if (userdb == null)
            throw new KeyNotFoundException("User not found");

        var community = await _context.Communities
            .FirstOrDefaultAsync(c => c.id == communityId);

        if (community == null)
            throw new KeyNotFoundException($"Community with id={communityId} not found");

        var existingUser = await _context.CommunityUsers
            .FirstOrDefaultAsync(cu => cu.communityId == communityId && cu.userId == parsedId);

        if (existingUser == null || !existingUser.communityRoles.Contains(CommunityRole.Subscriber))
        {
            throw new ValidationAccessException(
                $"User with id={parsedId} not subscribed to the community with id={communityId}.");
        }

        existingUser.communityRoles.Remove(CommunityRole.Subscriber);

        if (existingUser.communityRoles.Count == 0)
        {
            _context.CommunityUsers.Remove(existingUser);
        }

        await _context.SaveChangesAsync();
    }
}