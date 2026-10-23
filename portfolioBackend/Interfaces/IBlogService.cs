using portfolioBackend.DTOs.Blog;
using portfolioBackend.models;

namespace portfolioBackend.Interfaces;

public interface IBlogService
{
    ///<summary>
    /// Get all blogs
    /// </summary>
    /// <returns>all blogs</returns>
    Task<List<BlogPost>> GetAllBlogs();
    
    ///<summary>
    /// Get a blog by its ID
    /// </summary>
    /// <param name="id">The ID of the blog to retrieve</param>
    /// <returns>The blog with the specified ID</returns>
    Task<BlogPost> GetBlogById(int id);
    
    ///<summary>
    /// Create a new blog post
    /// </summary>
    /// <param name="blogDto">The data for the new blog post</param>
    /// <returns>The created blog post</returns>
    Task<BlogPost> CreateBlog(CreateBlogDto blogDto);
    
    ///<summary>
    /// Update an existing blog post
    /// </summary>
    /// <param name="id">The ID of the blog post to update</param>
    /// <param name="blogDto">The updated data for the blog post</param>
    /// <returns>The updated blog post</returns>
    Task<BlogPost> UpdateBlog(int id, UpdateBlogDto blogDto);
    
    ///<summary>
    /// Delete a blog post
    /// </summary>
    /// <param name="id">The ID of the blog post to delete</param>
    /// <returns>A boolean indicating whether the deletion was successful</returns>
    Task<bool> DeleteBlog(int id);
}