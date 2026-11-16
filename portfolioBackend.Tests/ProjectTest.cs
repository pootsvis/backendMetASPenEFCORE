using System.Net;
using System.Net.Http.Json;

namespace portfolioBackend.Tests;

public class ProjectTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Url = "/api/project";

    // ---------- GET ----------
    [Fact]
    public async Task GetProjects_Anonymous_Returns200_AndContainsCreatedProject()
    {
        // Arrange
        var created = await TestData.CreateProjectAsync(factory, "Lijsttest");
        var client = await factory.CreateClientAsync();

        // Act
        var response = await client.GetAsync(Url);
        var list = await response.Content.ReadFromJsonAsync<List<ProjectResult>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(list!, p => p.Id == created.Id && p.Titel == "Lijsttest");
    }

    [Fact]
    public async Task GetProject_ExistingId_Returns200_WithCorrectData()
    {
        // Arrange
        var created = await TestData.CreateProjectAsync(factory, "Opvraagtest");
        var client = await factory.CreateClientAsync();

        // Act
        var response = await client.GetAsync($"{Url}/{created.Id}");
        var project = await response.Content.ReadFromJsonAsync<ProjectResult>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Opvraagtest", project!.Titel);
        Assert.Equal("Web", project.Categorie);
    }

    [Fact]
    public async Task GetProject_UnknownId_Returns404()
    {
        var client = await factory.CreateClientAsync();

        var response = await client.GetAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- POST ----------
    [Fact]
    public async Task CreateProject_Anonymous_Returns401()
    {
        var client = await factory.CreateClientAsync();

        var response = await client.PostAsJsonAsync(Url, TestData.NewProject());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProject_AsGast_Returns403()
    {
        var client = await factory.CreateClientAsync(ApiFactory.GastEmail);

        var response = await client.PostAsJsonAsync(Url, TestData.NewProject());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(ApiFactory.EditorEmail)]
    [InlineData(ApiFactory.AdminEmail)]
    public async Task CreateProject_AsEditorOrAdmin_Returns201_AndProjectIsStored(string email)
    {
        // Arrange
        var client = await factory.CreateClientAsync(email);

        // Act
        var response = await client.PostAsJsonAsync(Url, TestData.NewProject("Nieuw project"));
        var created = await response.Content.ReadFromJsonAsync<ProjectResult>();
        var fetched = await client.GetFromJsonAsync<ProjectResult>($"{Url}/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("Nieuw project", fetched!.Titel);
    }

    [Fact]
    public async Task CreateProject_InvalidInput_Returns400()
    {
        // Arrange: lege body, verplichte velden ontbreken
        var client = await factory.CreateClientAsync(ApiFactory.EditorEmail);

        // Act
        var response = await client.PostAsJsonAsync(Url, new { });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- PUT ----------
    [Fact]
    public async Task UpdateProject_Anonymous_Returns401()
    {
        var created = await TestData.CreateProjectAsync(factory);
        var client = await factory.CreateClientAsync();

        var response = await client.PutAsJsonAsync($"{Url}/{created.Id}", TestData.NewProject("Hack"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProject_AsGast_Returns403_AndDataUnchanged()
    {
        // Arrange
        var created = await TestData.CreateProjectAsync(factory, "Origineel");
        var gast = await factory.CreateClientAsync(ApiFactory.GastEmail);

        // Act
        var response = await gast.PutAsJsonAsync($"{Url}/{created.Id}", TestData.NewProject("Gewijzigd"));
        var fetched = await gast.GetFromJsonAsync<ProjectResult>($"{Url}/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("Origineel", fetched!.Titel);
    }

    [Theory]
    [InlineData(ApiFactory.EditorEmail)]
    [InlineData(ApiFactory.AdminEmail)]
    public async Task UpdateProject_AsEditorOrAdmin_Returns204_AndDataIsChanged(string email)
    {
        // Arrange
        var created = await TestData.CreateProjectAsync(factory, "Voor wijziging");
        var client = await factory.CreateClientAsync(email);

        // Act
        var response = await client.PutAsJsonAsync($"{Url}/{created.Id}", TestData.NewProject("Na wijziging"));
        var fetched = await client.GetFromJsonAsync<ProjectResult>($"{Url}/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Na wijziging", fetched!.Titel);
    }

    [Fact]
    public async Task UpdateProject_UnknownId_Returns404()
    {
        var client = await factory.CreateClientAsync(ApiFactory.EditorEmail);

        var response = await client.PutAsJsonAsync($"{Url}/999999", TestData.NewProject());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- DELETE ----------
    [Fact]
    public async Task DeleteProject_Anonymous_Returns401()
    {
        var created = await TestData.CreateProjectAsync(factory);
        var client = await factory.CreateClientAsync();

        var response = await client.DeleteAsync($"{Url}/{created.Id}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProject_AsGast_Returns403_AndProjectStillExists()
    {
        // Arrange
        var created = await TestData.CreateProjectAsync(factory);
        var gast = await factory.CreateClientAsync(ApiFactory.GastEmail);

        // Act
        var response = await gast.DeleteAsync($"{Url}/{created.Id}");
        var check = await gast.GetAsync($"{Url}/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, check.StatusCode);
    }

    [Theory]
    [InlineData(ApiFactory.EditorEmail)]
    [InlineData(ApiFactory.AdminEmail)]
    public async Task DeleteProject_AsEditorOrAdmin_Returns204_AndProjectIsGone(string email)
    {
        // Arrange
        var created = await TestData.CreateProjectAsync(factory);
        var client = await factory.CreateClientAsync(email);

        // Act
        var response = await client.DeleteAsync($"{Url}/{created.Id}");
        var check = await client.GetAsync($"{Url}/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, check.StatusCode);
    }

    [Fact]
    public async Task DeleteProject_UnknownId_Returns404()
    {
        var client = await factory.CreateClientAsync(ApiFactory.EditorEmail);

        var response = await client.DeleteAsync($"{Url}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}