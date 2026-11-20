using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    
    [Authorize(Roles = "Editor,Admin")]
    [HttpPost]
    public async Task<ActionResult<BlogPost>> CreateBlogPost(CreateBlogDto blogPost)
    {
        var createdBlogPost = await blogService.CreateBlog(blogPost);
        return CreatedAtAction(nameof(GetBlogPost), new { id = createdBlogPost.Id }, createdBlogPost);
    }

    [Authorize(Roles = "Editor,Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateBlogPost(int id, UpdateBlogDto updatedBlogPost)
    {
        var blogPost = await blogService.GetBlogById(id);
        if (blogPost == null)
        {
            return NotFound();
        }

        await blogService.UpdateBlog(id, updatedBlogPost);
        return NoContent();
    }
    
    [Authorize(Roles = "Editor,Admin")]
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