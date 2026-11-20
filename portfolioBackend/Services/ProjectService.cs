using Microsoft.EntityFrameworkCore;
using portfolioBackend.data;
using portfolioBackend.DTOs;
using portfolioBackend.Interfaces;
using portfolioBackend.models;

namespace portfolioBackend.Services;

public class ProjectService(
    ApplicationDbContext context
) : IProjectService
{
    public async Task<List<Project>> GetAllProjects()
    {
        return await context.Projects.ToListAsync();
    }

    public async Task<Project?> GetProjectById(int id)
    {
        return await context.Projects.FindAsync(id);
    }

    public async Task<Project> CreateProject(CreateProjectDto projectDto)
    {
        Project project = new()
        {
            Titel = projectDto.Titel,
            Beschrijving = projectDto.Beschrijving,
            Categorie = projectDto.Categorie,
            GithubUrl = projectDto.GithubUrl,
            datum = projectDto.datum
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        return project;
    }

    public async Task<Project> UpdateProject(int id, UpdateProjectDto projectDto)
    {
        Project? project = await context.Projects.FindAsync(id);
        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        if (projectDto.Titel != null) project.Titel = projectDto.Titel;
        if (projectDto.Beschrijving != null) project.Beschrijving = projectDto.Beschrijving;
        if (projectDto.Categorie != null) project.Categorie = projectDto.Categorie;
        if (projectDto.GithubUrl != null) project.GithubUrl = projectDto.GithubUrl;
        if (projectDto.datum.HasValue) project.datum = projectDto.datum.Value;


        context.Projects.Update(project);
        await context.SaveChangesAsync();

        return project;
    }

    public async Task<bool> DeleteProject(int id)
    {
        Project? project = await context.Projects.FindAsync(id);
        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        context.Projects.Remove(project);
        await context.SaveChangesAsync();

        return true;
    }
}