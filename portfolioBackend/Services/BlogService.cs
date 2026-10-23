using Microsoft.EntityFrameworkCore;
using portfolioBackend.data;
using portfolioBackend.DTOs.Blog;
using portfolioBackend.Interfaces;
using portfolioBackend.models;

namespace portfolioBackend.Services;

public class BlogService(
    ApplicationDbContext context
) : IBlogService
{
    public async Task<List<BlogPost>> GetAllBlogs()
    {
        return await context.BlogPosts.ToListAsync();
    }

    public async Task<BlogPost> GetBlogById(int id)
    {
        BlogPost? blogPost = await context.BlogPosts.FindAsync(id);
        if (blogPost == null)
        {
            throw new InvalidOperationException("Blog post not found");
        }
        return blogPost;
    }

    public async Task<BlogPost> CreateBlog(CreateBlogDto blogDto)
    {
        BlogPost blogPost = new()
        {
            Titel = blogDto.Titel,
            Inhoud = blogDto.Inhoud,
            PublicatieDatum = blogDto.PublicatieDatum
        };
        context.BlogPosts.Add(blogPost);
        await context.SaveChangesAsync();
        return blogPost;
    }

    public async Task<BlogPost> UpdateBlog(int id, UpdateBlogDto blogDto)
    {
        BlogPost? blogPost = await context.BlogPosts.FindAsync(id);
        if (blogPost == null)
        {
            throw new InvalidOperationException("Blog post not found");
        }
        blogPost.Titel = blogDto.Titel;
        blogPost.Inhoud = blogDto.Inhoud;
        blogPost.PublicatieDatum = blogDto.PublicatieDatum;
        context.BlogPosts.Update(blogPost);
        await context.SaveChangesAsync();
        return blogPost;
    }

    public async Task<bool> DeleteBlog(int id)
    {
        BlogPost? blogPost = await context.BlogPosts.FindAsync(id);
        if (blogPost == null)
        {
            throw new InvalidOperationException("Blog post not found");
        }
        context.BlogPosts.Remove(blogPost);
        await context.SaveChangesAsync();
        return true;
    }
}