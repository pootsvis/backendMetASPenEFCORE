using System.Text.Json.Serialization;

namespace portfolioBackend.models;

public class BlogPost
{
  [JsonIgnore]
  public int? Id { get; set; }
  public string Titel { get; set; }
  public string Inhoud { get; set; }
  public DateTime? PublicatieDatum { get; set; }
}