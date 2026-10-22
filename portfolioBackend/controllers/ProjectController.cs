using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using portfolioBackend.data;
using portfolioBackend.models;

namespace portfolioBackend.controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        var projects = await context.Projects.ToListAsync();
        return Ok(projects);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProject(int id)
    {
        var project = await context.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(Project project)
    {
        context.Projects.Add(project);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProjects), new { id = project.Id }, project);
    }
    
    [HttpPut("{id}")]   
    public async Task<ActionResult> UpdateProject(int Id , Project UpdatedProject)
    {
        var project = await context.Projects.FindAsync(Id);
        if (project == null)
        {
            return NotFound();
        }
        project.Titel = UpdatedProject.Titel;
        project.Beschrijving = UpdatedProject.Beschrijving;
        project.Categorie = UpdatedProject.Categorie;
        project.GithubUrl = UpdatedProject.GithubUrl;
        project.datum = UpdatedProject.datum;
        
        await context.SaveChangesAsync();
        return NoContent();
    }
     
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProject(int id)
    {
        var project = await context.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        context.Projects.Remove(project);
        await context.SaveChangesAsync();

        return NoContent();
    }
}