using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using portfolioBackend.data;
using portfolioBackend.DTOs.Blog;
using portfolioBackend.Interfaces;
using portfolioBackend.models;

namespace portfolioBackend.controllers;
[Route("api/[controller]")]
[ApiController]
public class BlogPostController (IBlogService blogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts()
    {
        var blogPosts = await blogService.GetAllBlogs();
        return Ok(blogPosts);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<BlogPost>> GetBlogPost(int id)
    {
        var blogPost = await blogService.GetBlogById(id);
        if (blogPost == null)
        {
            return NotFound();
        }
        return Ok(blogPost);
    }
    
    [HttpPost]
    public async Task<ActionResult<BlogPost>> CreateBlogPost(CreateBlogDto blogPost)
    {
        var createdBlogPost = await blogService.CreateBlog(blogPost);
        return CreatedAtAction(nameof(GetBlogPost), new { id = createdBlogPost.Id }, createdBlogPost);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateBlogPost(int id, UpdateBlogDto updatedBlogPost)
    {
        var blogPost = await blogService.GetBlogById(id);
        if (blogPost == null)
        {
            return NotFound();
        }
        blogPost.Titel = updatedBlogPost.Titel;
        blogPost.Inhoud = updatedBlogPost.Inhoud;
        blogPost.PublicatieDatum = updatedBlogPost.PublicatieDatum;

        await blogService.UpdateBlog(id, updatedBlogPost);
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBlogPost(int id)
    {
        var blogPost = await blogService.GetBlogById(id);
        if (blogPost == null)
        {
            return NotFound();
        }

        await blogService.DeleteBlog(id);
        return NoContent();
    }
}