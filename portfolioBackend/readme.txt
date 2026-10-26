# Portfolio Backend

Backend voor mijn portfoliosite (WPFW, Opdracht 3). Een REST API voor het beheren van **projecten** en **blogposts** (CRUD), gebouwd met ASP.NET Core en Entity Framework Core.

## Technische stack

| Onderdeel | Keuze |
---------------------
| Framework | ASP.NET Core Web API op **.NET 10** |
| ORM | Entity Framework Core (code-first, met migraties) |
| Database | **PostgreSQL 16** (via Docker, provider: Npgsql) |
| API-documentatie | Swagger / OpenAPI (Swashbuckle) |

## API-stijl en codestructuur

De API is **controller-based** (`AddControllers()` + `MapControllers()`) en volgt REST-principes: resources (projecten en blogposts) worden via HTTP-methodes (GET, POST, PUT, DELETE) benaderd. De logica zit niet in de controllers maar in een aparte **servicelaag** die via dependency injection wordt aangeboden. Aanvragen en antwoorden lopen via **DTO's**, zodat de databasemodellen niet direct naar buiten komen.

```
portfolioBackend/
├── compose.yaml                 # PostgreSQL-container voor lokale ontwikkeling
├── portfolioBackend.sln
└── portfolioBackend/
    ├── controllers/             # HTTP-endpoints (BlogPostController, ProjectController)
    ├── Services/                # Businesslogica (BlogService, ProjectService)
    ├── Interfaces/              # Contracten voor de services (IBlogService, IProjectService)
    ├── DTOs/
    │   ├── Blog/                # CreateBlogDto, UpdateBlogDto, BlogResponse
    │   └── Project/             # CreateProjectDto, UpdateProjectDto, ProjectResponse
    ├── models/                  # Entiteiten (BlogPost, Project)
    ├── data/                    # ApplicationDbContext
    ├── Migrations/              # EF Core-migraties
    ├── Program.cs               # Startup: DI, EF Core, Swagger, middleware
    ├── appsettings.json         # Configuratie incl. connection string
    └── Dockerfile
```

Flow van een request: **Controller → Service (via interface) → ApplicationDbContext → PostgreSQL**.

## Vereisten

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/products/docker-desktop/) (voor de database)
- De EF Core CLI-tool:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Database configureren en aanmaken

1. **Start PostgreSQL** vanuit de root van de repository:
   ```bash
   docker compose up -d
   ```
   Dit start een `postgres:16`-container met:

   | Instelling | Waarde |
   -----------------------
   | Database | `appdb` |
   | Gebruiker | `appuser` |
   | Wachtwoord | `apppassword` |
   | Poort | `5432` |

   De data blijft bewaard in de Docker-volume `pgdata`.

2. **Controleer de connection string** in `portfolioBackend/appsettings.json`. Deze komt overeen met de container hierboven:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=appdb;Username=appuser;Password=apppassword"
   }
   ```
   De wachtwoorden zijn alleen bedoeld voor lokale ontwikkeling.

3. **Maak de database en tabellen aan** met de bestaande migraties:
   ```bash
   cd portfolioBackend
   dotnet ef database update
   ```

## De applicatie starten

```bash
cd portfolioBackend
dotnet run
```

De poort staat in de console-output (en in `Properties/launchSettings.json`). In de Development-omgeving is de Swagger UI beschikbaar op:

```
https://localhost:<poort>/swagger
```

Hier zie je alle endpoints en kun je ze direct uitproberen.

## Database stoppen

```bash
docker compose down        # stopt de container, data blijft bewaard
```