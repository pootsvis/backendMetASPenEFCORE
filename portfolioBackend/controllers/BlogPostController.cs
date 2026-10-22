using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using portfolioBackend.data;
using portfolioBackend.models;

namespace portfolioBackend.controllers;
[Route("api/[controller]")]
[ApiController]
public class BlogPostController (ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts()
    {
        var blogPosts = await context.BlogPosts.ToListAsync();
        return Ok(blogPosts);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<BlogPost>> GetBlogPost(int id)
    {
        var blogPost = await context.BlogPosts.FindAsync(id);
        if (blogPost == null)
        {
            return NotFound();
        }
        return Ok(blogPost);
    }
    
    [HttpPost]
    public async Task<ActionResult<BlogPost>> CreateBlogPost(BlogPost blogPost)
    {
        context.BlogPosts.Add(blogPost);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBlogPost), new { id = blogPost.Id }, blogPost);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateBlogPost(int id, BlogPost updatedBlogPost)
    {
        var blogPost = await context.BlogPosts.FindAsync(id);
        if (blogPost == null)
        {
            return NotFound();
        }
        blogPost.Titel = updatedBlogPost.Titel;
        blogPost.Inhoud = updatedBlogPost.Inhoud;
        blogPost.PublicatieDatum = updatedBlogPost.PublicatieDatum;

        await context.SaveChangesAsync();
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBlogPost(int id)
    {
        var blogPost = await context.BlogPosts.FindAsync(id);
        if (blogPost == null)
        {
            return NotFound();
        }

        context.BlogPosts.Remove(blogPost);
        await context.SaveChangesAsync();

        return NoContent();
    }
}