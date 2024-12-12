using System.Security.Claims;
using Blog.API.Context;
using Blog.API.Data;
using Blog.API.Middleware;
using Blog.API.Models.DB;
using Blog.API.Models.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
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

        Task<Guid> CreatePost(CreatePostDto model, ClaimsPrincipal user);
        Task<PostFullDto> GetConcretePost(Guid postId, ClaimsPrincipal user);

        Task<IActionResult> LikeConcretePost(Guid postId, ClaimsPrincipal user);
        Task<IActionResult> DeleteLikeConcretePost(Guid postId, ClaimsPrincipal user);
    }

    public class PostService : IPostService
    {
        private readonly BlogDbContext _context;
        private readonly MyDbContext _contextSec;

        public PostService(BlogDbContext context, MyDbContext contextSec)
        {
            _context = context;
            _contextSec = contextSec;
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
            Guid? parsedId = null;
            
            //�������� ����������� �� ������������
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsed))
            {
                parsedId = parsed;
                var userdb = await _context.Users.FirstOrDefaultAsync(d => d.id == parsedId);
                if (userdb == null)
                    throw new KeyNotFoundException("User not found");
            }
            
            //��������� �������� � �� �������
            if (page < 1 || size < 1)
            {
                throw new ValidationAccessException("page or size must be greater than 0");
            }
            //�������� ������������� ����� � ��
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

            var query = _context.Posts
                .Include(p => p.tags)
                .Include(p => p.likes)
                .Include(p => p.author)
                .Include(p => p.community)
                .ThenInclude(c => c.communityUsers)
                .AsQueryable();
            
            //������ �� ������ ���� �� �� ����
            if (!string.IsNullOrEmpty(author))
            {
                query = query.Include(p => p.author)
                    .Where(post => post.author.fullName.Contains(author));
            }
            
            //������ �� ������� ������
            if (min > 0)
            {
                query = query.Where(post => post.readingTime >= min);
            }

            if (max > 0)
            {
                query = query.Where(post => post.readingTime <= max);
            }
            
            //������ �� �����
            if (tags != null && tags.Any())
            {
                query = query.Where(post => post.tags.Any(tag => tags.Contains(tag.id)));
            }
            
            //������ �� �����������
            if (parsedId != null)
            {
                if (onlyMyCommunities)
                {
                    query = query.Where(post =>
                        post.community != null &&
                        post.community.communityUsers.Any(cu => cu.userId == parsedId));
                }
                else
                {
                    query = query.Where(post =>
                        post.community == null || !post.community.isClosed ||
                        post.community.communityUsers.Any(cu => cu.userId == parsedId));
                }
            }
            else
            {
                //���� ������������ �� �����������
                query = query.Where(post => post.community == null || !post.community.isClosed);
            }
            
            //���������� ��������� �������������
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
                communityName = post.communityId != null ? post.community.name : null,
                addressId = post.addressId,
                likes = post.likes.Count(),
                hasLike = parsedId != null && post.likes.Any(like => like.userId == parsedId),
                commentsCount = _context.Comments.Count(c => c.postId == post.id),
                tags = post.tags?.Select(tag => new TagDto
                {
                    id = tag.id,
                    createTime = tag.createTime,
                    name = tag.name
                }).ToList()
            }).ToList();
            
            if (parsedId == null)
            {
                foreach (var post in postsdb)
                {
                    post.hasLike = false;
                }
            }


            var pageInfo = new PageInfoModel
            {
                size = size,
                count = (int)Math.Ceiling((double)totalItems / size),
                current = page
            };
            

            //�������� ��� ������� �������� �� ��������� ����� ��������� �������
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

        public async Task<Guid> CreatePost(CreatePostDto model, ClaimsPrincipal user)
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

            if (model.readingTime <= 0)
            {
                throw new ValidationAccessException("ReadingTime must be grater than 0.");
            }
            
            //�������� ������������� addressId � ��
            if (model.addressId != null)
            {

                var addressChain = new List<SearchAddressModel>();

                var addrObj = await _contextSec.AsAddrObjs
                    .FirstOrDefaultAsync(a => a.Objectguid == model.addressId);

                long objectId;

                if (addrObj != null)
                {
                    objectId = addrObj.Objectid;
                }
                else
                {
                    var houseObj = await _contextSec.AsHouses
                        .FirstOrDefaultAsync(h => h.Objectguid == model.addressId);

                    if (houseObj == null)
                    {
                        throw new KeyNotFoundException($"Object with id={model.addressId} not found");
                    }

                    objectId = houseObj.Objectid;

                    addressChain.Add(new SearchAddressModel
                    {
                        objectId = (int)houseObj.Objectid,
                        objectGuid = houseObj.Objectguid,
                        text = houseObj.Housenum,
                        objectLevel = GarAddressLevel.Building,
                        objectLevelText = AddressLevel.LevelsDictionary[GarAddressLevel.Building]
                    });
                }

                var hierarchy = await _contextSec.AsAdmHierarchies
                    .FirstOrDefaultAsync(h => h.Objectid == objectId);

                if (hierarchy == null)
                {
                    throw new KeyNotFoundException($"Object with id={model.addressId} not found");
                }
            }
            
            var post = new Post
            {
                createTime = DateTime.UtcNow,
                title = model.title,
                description = model.description,
                readingTime = model.readingTime,
                image = model.image,
                authorId = userdb.id,
                author = userdb,
                addressId = model.addressId,
                tags = new List<Tag>(),
                comments = new List<Comment>()
            };

            var existingTags = await _context.Tags
                .Where(t => model.tags.Contains(t.id))
                .ToListAsync();

            //���������� ������������� ����� � ��
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

        public async Task<PostFullDto> GetConcretePost(Guid postId, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? parsedId = null;
            
            //�������� ����������� �� ������������
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsed))
            {
                parsedId = parsed;
                var userdb = await _context.Users.FirstOrDefaultAsync(d => d.id == parsedId);
                if (userdb == null)
                    throw new KeyNotFoundException("User not found");
            }


            var post = await _context.Posts
                .Include(p => p.tags)
                .Include(p => p.likes)
                .Include(p => p.author)
                .Include(p => p.community)
                .ThenInclude(c => c.communityUsers)
                .Include(p => p.comments)
                .ThenInclude(c => c.author)
                .FirstOrDefaultAsync(p => p.id == postId);
            
            //�������� ��� ���� ���������� � ��
            if (post == null)
            {
                throw new KeyNotFoundException($"Post with id {postId} not found.");
            }

            //�������� �� ����� ������������
            if (post.community != null && post.community.isClosed)
            {
                if (parsedId == null)
                {
                    throw new ForbiddenAccessException(
                        $"Access to closed community with id={post.community.id} is forbidden");
                }
                
                var isMember = post.community.communityUsers.Any(cu => cu.userId == parsedId);

                if (!isMember)
                    throw new ForbiddenAccessException(
                        $"Access to closed community with id={post.community.id} is forbidden");
            }


            var postFullDto = new PostFullDto
            {
                id = post.id,
                title = post.title,
                createTime = post.createTime,
                description = post.description,
                readingTime = post.readingTime,
                image = post.image,
                authorId = post.authorId,
                author = post.author.fullName,
                communityId = post.communityId,
                communityName = post.communityId != null ? post.community.name : null,
                addressId = post.addressId,
                likes = post.likes.Count(),
                hasLike = parsedId != null ? post.likes.Any(like => like.userId == parsedId) : false,
                commentsCount = post.comments.Count(),
                tags = post.tags.Select(t => new TagDto
                {
                    id = t.id,
                    createTime = t.createTime,
                    name = t.name
                }).ToList(),
                comments = post.comments
                    .Where(c => c.parentCommentId == null)
                    .OrderBy(c => c.createTime)
                    .Select(c => new CommentDto
                    {
                        id = c.id,
                        createTime = c.createTime,
                        content = c.content,
                        modifiedDate = c.modifiedDate,
                        deleteDate = c.deleteDate,
                        authorId = c.authorId,
                        author = c.author.fullName,
                        subComments = post.comments.Count(sub => sub.parentCommentId == c.id)
                    }).ToList()
            };

            return postFullDto;
        }


        public async Task<IActionResult> LikeConcretePost(Guid postId, ClaimsPrincipal user)
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


            var post = await _context.Posts
                .Include(p => p.likes)
                .Include(p => p.community)
                .ThenInclude(c => c.communityUsers)
                .FirstOrDefaultAsync(p => p.id == postId);

            //�������� ��� ����� ���� ����������
            if (post == null)
            {
                throw new KeyNotFoundException($"Post with id {postId} not found.");
            }

            //�������� ���� ������������
            if (post.community != null && post.community.isClosed)
            {
                var isMember = post.community.communityUsers.Any(cu => cu.userId == parsedId);

                if (!isMember)
                    throw new ForbiddenAccessException(
                        $"Access to closed community with id={post.community.id} is forbidden");
            }

            //�������� ��� ���� ��� ����� �� �����
            var existingLike = post.likes.FirstOrDefault(like => like.userId == parsedId);

            if (existingLike != null)
            {
                throw new ValidationAccessException("Like on this post already set by user");
            }

            var like = new Like
            {
                postId = post.id,
                userId = userdb.id
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            return null;
        }

        public async Task<IActionResult> DeleteLikeConcretePost(Guid postId, ClaimsPrincipal user)
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

            var post = await _context.Posts
                .Include(p => p.likes)
                .Include(p => p.community)
                .ThenInclude(c => c.communityUsers)
                .FirstOrDefaultAsync(p => p.id == postId);
            
            //�������� ���������� �� ���� � ��
            if (post == null)
            {
                throw new KeyNotFoundException($"Post with id {postId} not found.");
            }

            //�������� ���� ������������
            if (post.community != null && post.community.isClosed)
            {
                var isMember = post.community.communityUsers.Any(cu => cu.userId == parsedId);

                if (!isMember)
                    throw new ForbiddenAccessException(
                        $"Access to closed community with id={post.community.id} is forbidden");
            }

            //�������� ��� ���� ��� ����� �� �����
            var existingLike = post.likes.FirstOrDefault(like => like.userId == parsedId);

            if (existingLike == null)
            {
                throw new ValidationAccessException("Like on this post not found for the user.");
            }

            _context.Likes.Remove(existingLike);
            await _context.SaveChangesAsync();

            return null;
        }
    }
}