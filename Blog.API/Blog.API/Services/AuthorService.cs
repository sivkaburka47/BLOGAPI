using Blog.API.Data;
using Blog.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Blog.API.Services;

public interface IAuthorService
{
    Task<List<AuthorDto>> GetAuthorList();
}

public class AuthorService : IAuthorService
{
    private readonly BlogDbContext _context;
    
    public AuthorService(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuthorDto>> GetAuthorList()
    {
        var authors = await _context.Users
            .Where(user => user.posts.Any())
            .Select(user => new AuthorDto
            {
                fullName = user.fullName,
                birthDate = user.birthDate,
                gender = user.gender,
                posts = user.posts.Count,
                likes = user.posts.Sum(post => post.likes.Count),
                created = user.createTime
            })
            .ToListAsync();

        return authors;
    }

}