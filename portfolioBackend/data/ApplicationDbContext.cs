using Microsoft.EntityFrameworkCore;
using portfolioBackend.models;

namespace portfolioBackend.data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Project> Projects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<BlogPost>()
            .HasKey(b => b.Id); // set primary key for Blogpost entity
        
        modelBuilder.Entity<Project>()
            .HasKey(p => p.Id); // set primary key for Project entity
    }
}