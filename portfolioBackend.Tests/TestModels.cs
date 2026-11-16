using System.Net.Http.Json;

namespace portfolioBackend.Tests;

public record ProjectResult(int Id, string Titel, string Beschrijving, string Categorie, string GithubUrl);
public record BlogResult(int Id, string Titel, string Inhoud);

public static class TestData
{
    public static object NewProject(string titel = "Testproject") => new
    {
        Titel = titel,
        Beschrijving = "Een beschrijving",
        Categorie = "Web",
        GithubUrl = "https://github.com/test/test"
    };

    public static object NewBlog(string titel = "Testblog") => new
    {
        Titel = titel,
        Inhoud = "Inhoud van de blogpost"
    };

    // Maakt als Editor een project aan en geeft het terug (voor tests die een bestaand record nodig hebben)
    public static async Task<ProjectResult> CreateProjectAsync(ApiFactory factory, string titel = "Testproject")
    {
        var editor = await factory.CreateClientAsync(ApiFactory.EditorEmail);
        var response = await editor.PostAsJsonAsync("/api/project", NewProject(titel));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProjectResult>())!;
    }

    public static async Task<BlogResult> CreateBlogAsync(ApiFactory factory, string titel = "Testblog")
    {
        var editor = await factory.CreateClientAsync(ApiFactory.EditorEmail);
        var response = await editor.PostAsJsonAsync("/api/blogpost", NewBlog(titel));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<BlogResult>())!;
    }
}