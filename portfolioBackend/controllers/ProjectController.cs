using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using portfolioBackend.data;
using portfolioBackend.DTOs;
using portfolioBackend.Interfaces;
using portfolioBackend.models;

namespace portfolioBackend.controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectController(
    IProjectService projectService
    ) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        var projects = await projectService.GetAllProjects();
        return Ok(projects);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProject(int id)
    {
        var project = await projectService.GetProjectById(id);
        if (project == null)
        {
            return NotFound();
        }
        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(CreateProjectDto project)
    {
        var createdProject = await projectService.CreateProject(project);
        return CreatedAtAction(nameof(GetProjects), new { id = createdProject.Id }, createdProject);
    }
    
    [HttpPut("{id}")]   
    public async Task<ActionResult> UpdateProject(int Id , UpdateProjectDto UpdatedProject)
    {
        var project = await projectService.GetProjectById(Id);
        if (project == null)
        {
            return NotFound();
        }
        await projectService.UpdateProject(Id, UpdatedProject);
        return NoContent();
    }
     
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProject(int id)
    {
        var project = await projectService.GetProjectById(id);
        if (project == null)
        {
            return NotFound();
        }

        await projectService.DeleteProject(id);
        return NoContent();
    }
}