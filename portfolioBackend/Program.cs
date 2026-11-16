using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.Extensions.Hosting;
using portfolioBackend.data;
using portfolioBackend.Interfaces;
using portfolioBackend.Services;

namespace portfolioBackend;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddScoped<IBlogService, BlogService>();
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddOpenApi();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options => {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
            });
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });
        var app = builder.Build();
        
        if (!app.Environment.IsEnvironment("Testing"))
        {
            using var scope = app.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1. Rollen aanmaken
            foreach (var role in new[] { "Gast", "Editor", "Admin" })
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // 2. Alleen lokaal: Editor en Admin seeden uit user-secrets
            if (app.Environment.IsDevelopment())
            {
                var config = app.Configuration;

                async Task SeedUser(string? email, string? password, string role)
                {
                    if (email is null || password is null) return;
                    if (await users.FindByEmailAsync(email) != null) return;

                    var user = new IdentityUser { UserName = email, Email = email };
                    var result = await users.CreateAsync(user, password);
                    if (result.Succeeded) await users.AddToRoleAsync(user, role);
                }

                await SeedUser(config["Seed:EditorEmail"], config["Seed:EditorPassword"], "Editor");
                await SeedUser(config["Seed:AdminEmail"], config["Seed:AdminPassword"], "Admin");
            }
        }
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
        }

        app.UseHttpsRedirection();
        app.MapControllers();
        app.Run();
    }
}
