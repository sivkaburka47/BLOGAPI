namespace Blog.API.Models.DTOs;

public class PostPagedListDto
{
    public List<PostDto>? posts { get; set; }
    public PageInfoModel pagination { get; set; }
}