namespace portfolioBackend.models;

public class Project
{
    public int Id { get; set; }
    public string Titel { get; set; }
    public string Beschrijving { get; set; }
    public string Categorie { get; set; }
    public string GithubUrl { get; set; }
    public DateTime datum { get; set; }
}