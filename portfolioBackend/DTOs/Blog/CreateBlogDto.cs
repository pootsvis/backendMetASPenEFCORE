namespace portfolioBackend.DTOs.Blog;

public class CreateBlogDto
{
    public string Titel { get; set; }
    public string Inhoud { get; set; }
    public DateTime PublicatieDatum { get; set; }
}