namespace portfolioBackend.DTOs.Blog;

public class UpdateBlogDto
{
    public string? Titel { get; set; }
    public string? Inhoud { get; set; }
    public DateTime? PublicatieDatum { get; set; }
}