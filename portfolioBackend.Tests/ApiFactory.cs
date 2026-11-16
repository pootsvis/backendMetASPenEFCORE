using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore.Infrastructure;
using portfolioBackend.data;

namespace portfolioBackend.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    public const string Password = "Test1234!"; // alleen voor de in-memory testdatabase
    public const string GastEmail = "gast@test.nl";
    public const string EditorEmail = "editor@test.nl";
    public const string AdminEmail = "admin@test.nl";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            // Echte (Postgres) database vervangen door SQLite in-memory
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

            _connection.Open();
            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_connection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        Seed(host.Services).GetAwaiter().GetResult();
        return host;
    }

    private static async Task Seed(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        foreach (var role in new[] { "Gast", "Editor", "Admin" })
            await roles.CreateAsync(new IdentityRole(role));

        await CreateUser(users, GastEmail, "Gast");
        await CreateUser(users, EditorEmail, "Editor");
        await CreateUser(users, AdminEmail, "Admin");
    }

    private static async Task CreateUser(UserManager<IdentityUser> users, string email, string role)
    {
        var user = new IdentityUser { UserName = email, Email = email };
        await users.CreateAsync(user, Password);
        await users.AddToRoleAsync(user, role);
    }

    /// <summary>Client met cookie-ondersteuning; ingelogd als email, of anoniem bij null.</summary>
    public async Task<HttpClient> CreateClientAsync(string? email = null)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });

        if (email != null)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login",
                new { Email = email, Password });
            response.EnsureSuccessStatusCode();
        }
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}