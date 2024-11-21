using Blog.API.Data;
using Blog.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Blog.API.Services;

public interface ITagService
{
    Task<List<TagDto>> GetTags();
}

public class TagService : ITagService
{
    private readonly BlogDbContext _context;
    
    public TagService(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<List<TagDto>> GetTags()
    {
        var tagDtos = await _context.Tags
            .Select(tag => new TagDto
            {
                id = tag.id,
                createTime = tag.createTime,
                name = tag.name
            })
            .ToListAsync();

        return tagDtos;
    }
}