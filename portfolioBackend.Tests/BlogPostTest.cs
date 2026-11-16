using System.Net;
using System.Net.Http.Json;

namespace portfolioBackend.Tests;

public class BlogPostTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Url = "/api/blogpost";

    [Fact]
    public async Task GetBlogPosts_Anonymous_Returns200_AndContainsCreatedPost()
    {
        var created = await TestData.CreateBlogAsync(factory, "Lijstblog");
        var client = await factory.CreateClientAsync();

        var response = await client.GetAsync(Url);
        var list = await response.Content.ReadFromJsonAsync<List<BlogResult>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(list!, b => b.Id == created.Id);
    }

    [Fact]
    public async Task GetBlogPost_ExistingId_Returns200_WithCorrectData()
    {
        var created = await TestData.CreateBlogAsync(factory, "Enkele blog");
        var client = await factory.CreateClientAsync();

        var response = await client.GetAsync($"{Url}/{created.Id}");
        var blog = await response.Content.ReadFromJsonAsync<BlogResult>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Enkele blog", blog!.Titel);
    }

    [Fact]
    public async Task GetBlogPost_UnknownId_Returns404()
    {
        var client = await factory.CreateClientAsync();

        var response = await client.GetAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateBlogPost_Anonymous_Returns401()
    {
        var client = await factory.CreateClientAsync();
        var response = await client.PostAsJsonAsync(Url, TestData.NewBlog());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateBlogPost_AsGast_Returns403()
    {
        var client = await factory.CreateClientAsync(ApiFactory.GastEmail);
        var response = await client.PostAsJsonAsync(Url, TestData.NewBlog());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(ApiFactory.EditorEmail)]
    [InlineData(ApiFactory.AdminEmail)]
    public async Task CreateBlogPost_AsEditorOrAdmin_Returns201_AndPostIsStored(string email)
    {
        var client = await factory.CreateClientAsync(email);

        var response = await client.PostAsJsonAsync(Url, TestData.NewBlog("Nieuwe blog"));
        var created = await response.Content.ReadFromJsonAsync<BlogResult>();
        var fetched = await client.GetFromJsonAsync<BlogResult>($"{Url}/{created!.Id}");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Nieuwe blog", fetched!.Titel);
    }

    [Fact]
    public async Task CreateBlogPost_InvalidInput_Returns400()
    {
        var client = await factory.CreateClientAsync(ApiFactory.EditorEmail);
        var response = await client.PostAsJsonAsync(Url, new { });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBlogPost_Anonymous_Returns401()
    {
        var created = await TestData.CreateBlogAsync(factory);
        var client = await factory.CreateClientAsync();
        var response = await client.PutAsJsonAsync($"{Url}/{created.Id}", TestData.NewBlog("Hack"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBlogPost_AsGast_Returns403()
    {
        var created = await TestData.CreateBlogAsync(factory);
        var client = await factory.CreateClientAsync(ApiFactory.GastEmail);
        var response = await client.PutAsJsonAsync($"{Url}/{created.Id}", TestData.NewBlog("Hack"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(ApiFactory.EditorEmail)]
    [InlineData(ApiFactory.AdminEmail)]
    public async Task UpdateBlogPost_AsEditorOrAdmin_Returns204_AndDataIsChanged(string email)
    {
        var created = await TestData.CreateBlogAsync(factory, "Voor");
        var client = await factory.CreateClientAsync(email);

        var response = await client.PutAsJsonAsync($"{Url}/{created.Id}", TestData.NewBlog("Na"));
        var fetched = await client.GetFromJsonAsync<BlogResult>($"{Url}/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Na", fetched!.Titel);
    }

    [Fact]
    public async Task UpdateBlogPost_UnknownId_Returns404()
    {
        var client = await factory.CreateClientAsync(ApiFactory.EditorEmail);
        var response = await client.PutAsJsonAsync($"{Url}/999999", TestData.NewBlog());
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBlogPost_Anonymous_Returns401()
    {
        var created = await TestData.CreateBlogAsync(factory);
        var client = await factory.CreateClientAsync();
        var response = await client.DeleteAsync($"{Url}/{created.Id}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBlogPost_AsGast_Returns403()
    {
        var created = await TestData.CreateBlogAsync(factory);
        var client = await factory.CreateClientAsync(ApiFactory.GastEmail);
        var response = await client.DeleteAsync($"{Url}/{created.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(ApiFactory.EditorEmail)]
    [InlineData(ApiFactory.AdminEmail)]
    public async Task DeleteBlogPost_AsEditorOrAdmin_Returns204_AndPostIsGone(string email)
    {
        var created = await TestData.CreateBlogAsync(factory);
        var client = await factory.CreateClientAsync(email);

        var response = await client.DeleteAsync($"{Url}/{created.Id}");
        var check = await client.GetAsync($"{Url}/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, check.StatusCode);
    }

    [Fact]
    public async Task DeleteBlogPost_UnknownId_Returns404()
    {
        var client = await factory.CreateClientAsync(ApiFactory.EditorEmail);
        var response = await client.DeleteAsync($"{Url}/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}