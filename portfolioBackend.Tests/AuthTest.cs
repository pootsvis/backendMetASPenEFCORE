using System.Net;
using System.Net.Http.Json;

namespace portfolioBackend.Tests;

public class AuthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Register_ValidData_Returns200()
    {
        var client = await factory.CreateClientAsync();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { Email = "nieuw@test.nl", Password = ApiFactory.Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_NewUser_GetsGastRole_NotEditor()
    {
        // Arrange
        var client = await factory.CreateClientAsync();
        await client.PostAsJsonAsync("/api/auth/register",
            new { Email = "gastcheck@test.nl", Password = ApiFactory.Password });
        await client.PostAsJsonAsync("/api/auth/login",
            new { Email = "gastcheck@test.nl", Password = ApiFactory.Password });

        // Act: een Gast mag niet schrijven
        var response = await client.PostAsJsonAsync("/api/project", TestData.NewProject());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var client = await factory.CreateClientAsync();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { Email = ApiFactory.GastEmail, Password = ApiFactory.Password });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400()
    {
        var client = await factory.CreateClientAsync();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { Email = "zwak@test.nl", Password = "1" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_CorrectCredentials_Returns200_AndSetsAuthCookie()
    {
        var client = await factory.CreateClientAsync();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = ApiFactory.EditorEmail, Password = ApiFactory.Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies!, c => c.Contains(".AspNetCore.Identity.Application"));
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var client = await factory.CreateClientAsync();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = ApiFactory.EditorEmail, Password = "Fout1234!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}