using portfolioBackend.DTOs;
using portfolioBackend.models;

namespace portfolioBackend.Interfaces;

public interface IProjectService
{
    //<summary>
    // Get all projects
    //</summary>
    // <returns>all projects</returns>
    Task<List<Project>> GetAllProjects();
    
    //<summary>
    // Get a project by its ID
    //</summary>
    // <param name="id">The ID of the project</param>
    // <returns>The project</returns>
    Task<Project> GetProjectById(int id);
    
    //<summary>
    // Create a new project
    //</summary>
    // <param name="projectDto">The DTO for creating the project</param>
    // <returns>The created project</returns>
    Task<Project> CreateProject(CreateProjectDto projectDto);
    
    //<summary>
    // Update an existing project
    //</summary>
    // <param name="id">The ID of the project to update</param>
    // <param name="projectDto">The DTO for updating the project</param>
    // <returns>The updated project</returns>
    Task<Project> UpdateProject(int id, UpdateProjectDto projectDto);
    
    //<summary>
    // Delete a project
    //</summary>
    // <param name="id">The ID of the project to delete</param>
    // <returns>A boolean indicating whether the project was deleted</returns>
    Task<bool> DeleteProject(int id);
}