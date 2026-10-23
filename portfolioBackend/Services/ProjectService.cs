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

    public async Task<Project> GetProjectById(int id)
    {
        Project? project = await context.Projects.FindAsync(id);
        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        return project;
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

        project.Titel = projectDto.Titel;
        project.Beschrijving = projectDto.Beschrijving;
        project.Categorie = projectDto.Categorie;
        project.GithubUrl = projectDto.GithubUrl;
        project.datum = projectDto.datum;

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