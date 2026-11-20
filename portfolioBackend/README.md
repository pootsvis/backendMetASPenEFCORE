# Portfolio Backend

Backend voor mijn portfoliosite (WPFW, Opdracht 3 en 4). Een REST API voor het beheren van **projecten** en **blogposts** (CRUD), gebouwd met ASP.NET Core en Entity Framework Core. In opdracht 4 is de API beveiligd met **ASP.NET Core Identity** (cookie-authenticatie, rollen Gast / Editor / Admin) en voorzien van een **xUnit-testproject**.

## Technische stack

| Onderdeel | Keuze |
|---|---|
| Framework | ASP.NET Core Web API op **.NET 10** |
| ORM | Entity Framework Core (code-first, met migraties) |
| Database | **PostgreSQL 16** (via Docker, provider: Npgsql) |
| Authenticatie | ASP.NET Core Identity met EF Core, authenticatiecookie |
| Autorisatie | Rollen: `Gast`, `Editor`, `Admin` (`[Authorize(Roles = "Editor,Admin")]`) |
| Tests | xUnit + `WebApplicationFactory` + SQLite in-memory |
| API-documentatie | Swagger / OpenAPI (Swashbuckle) |

## Codestructuur

De API is **controller-based** en volgt REST-principes. De logica zit in een **servicelaag** die via dependency injection wordt aangeboden. Flow van een request: **Controller → Service (via interface) → ApplicationDbContext → PostgreSQL**.

```
backendMetASPenEFCORE/
├── compose.yaml                  # PostgreSQL-container voor lokale ontwikkeling
├── .env.example                  # voorbeeld van de benodigde variabelen (zonder echte waarden)
├── portfolioBackend.sln
├── portfolioBackend/             # de API
│   ├── controllers/              # AuthController, ProjectController, BlogPostController
│   ├── Services/                 # BlogService, ProjectService
│   ├── Interfaces/               # IBlogService, IProjectService
│   ├── DTOs/                     # Create/Update/Response-DTO's
│   ├── models/                   # BlogPost, Project
│   ├── data/                     # ApplicationDbContext (IdentityDbContext)
│   ├── Migrations/               # EF Core-migraties (incl. Identity)
│   └── Program.cs
└── portfolioBackend.Tests/       # xUnit-testproject
    ├── ApiFactory.cs             # test-host met SQLite in-memory en geseede testgebruikers
    ├── TestModels.cs             # testdata en hulpmethodes
    ├── AuthTest.cs               # registratie en login
    ├── ProjectTest.cs            # CRUD-routes projecten
    └── BlogPostTest.cs           # CRUD-routes blogposts
```

## Vereisten

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/products/docker-desktop/) (voor de database)
- De EF Core CLI-tool:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Configuratie (geen geheimen in de repository)

Wachtwoorden en connection strings staan **niet** in de repository. Ze worden lokaal ingesteld.

### 1. Database-gegevens voor Docker (`.env`)

Kopieer `.env.example` naar `.env` (dit bestand staat in `.gitignore`) en vul zelf waarden in:

```
POSTGRES_USER=appuser
POSTGRES_PASSWORD=<kies-een-wachtwoord>
POSTGRES_DB=appdb
```

### 2. Connection string en seed-gebruikers (user-secrets)

Voer uit in de map van de repository (gebruik hetzelfde wachtwoord als in `.env`):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=appdb;Username=appuser;Password=<hetzelfde-wachtwoord>" --project portfolioBackend

dotnet user-secrets set "Seed:EditorEmail" "editor@lokaal.nl" --project portfolioBackend
dotnet user-secrets set "Seed:EditorPassword" "<kies-een-wachtwoord>" --project portfolioBackend
dotnet user-secrets set "Seed:AdminEmail" "admin@lokaal.nl" --project portfolioBackend
dotnet user-secrets set "Seed:AdminPassword" "<kies-een-ander-wachtwoord>" --project portfolioBackend
```

Wachtwoorden moeten minimaal 8 tekens lang zijn en voldoen aan de standaardregels van Identity (hoofdletter, kleine letter, cijfer en speciaal teken).

## Database starten en migraties toepassen

1. **Start PostgreSQL** vanuit de root van de repository:
   ```bash
   docker compose up -d
   ```
   De data blijft bewaard in de Docker-volume `pgdata`.

2. **Pas de migraties toe** (dit maakt de tabellen voor projecten, blogposts én Identity aan):
   ```bash
   dotnet ef database update --project portfolioBackend
   ```
   De migraties staan in `portfolioBackend/Migrations/` (`initialcreate` en `AddIdentity`).

## De applicatie starten

```bash
cd portfolioBackend
dotnet run
```

De poort staat in de console-output (en in `Properties/launchSettings.json`). In de Development-omgeving is de Swagger UI beschikbaar op:

```
https://localhost:<poort>/swagger
```

Bij het opstarten (alleen buiten de testomgeving) worden de rollen `Gast`, `Editor` en `Admin` aangemaakt als ze nog niet bestaan. In Development worden bovendien een Editor- en een Admin-gebruiker aangemaakt uit de user-secrets hierboven.

## Gebruikers en rollen

### Registreren en inloggen

| Route | Beschrijving |
|---|---|
| `POST /api/auth/register` | Maakt een gebruiker aan. Body: `{ "email": "...", "password": "..." }`. Elke nieuwe gebruiker krijgt **altijd de rol `Gast`**. Publieke registratie kent nooit Editor of Admin toe. |
| `POST /api/auth/login` | Logt in. Bij succes zet de API een authenticatiecookie (`.AspNetCore.Identity.Application`). Bij fout: `401`. |

In Swagger UI wordt de cookie na het inloggen automatisch meegestuurd bij volgende requests.

### Een gewone gebruiker maken

Registreer via `POST /api/auth/register` met een eigen e-mailadres en wachtwoord. Deze gebruiker heeft de rol `Gast`.

### Editor en Admin beschikbaar maken

Er zijn twee manieren, beide lokaal:

**Optie A: seed bij het opstarten (Development).** Stel de `Seed:*`-user-secrets in zoals hierboven beschreven. Bij `dotnet run` worden deze gebruikers aangemaakt, met de rol `Editor` en `Admin`.

**Optie B: een bestaande gebruiker handmatig een rol geven.** Open een SQL-sessie in de database-container:

```bash
docker exec -it postgres-db psql -U appuser -d appdb
```

Geef de gebruiker de rol `Editor` (vervang het e-mailadres, in hoofdletters):

```sql
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM "AspNetUsers" u, "AspNetRoles" r
WHERE u."NormalizedEmail" = 'NAAM@VOORBEELD.NL' AND r."Name" = 'Editor';
```

Voor Admin gebruik je `r."Name" = 'Admin'`. Wil je de rol `Gast` weghalen, dan kan dat met een `DELETE` op `"AspNetUserRoles"` voor dezelfde gebruiker en rol.

> **Let op:** na een wijziging van een rol moet de gebruiker **opnieuw inloggen**. De rollen staan namelijk in de authenticatiecookie die bij het inloggen wordt uitgegeven.

## Autorisatie en routes

Alle `GET`-routes zijn publiek. `POST`, `PUT` en `DELETE` vereisen een aangemelde gebruiker met de rol `Editor` of `Admin`. De controle gebeurt in de API zelf.

| Situatie | Resultaat |
|---|---|
| Schrijfverzoek zonder aanmelding | `401 Unauthorized` |
| Aangemelde gebruiker zonder Editor- of Admin-rol (Gast) | `403 Forbidden` |
| Editor of Admin | toegestaan |

### Overzicht van de routes

| # | Route | Wie heeft toegang | Verwachte resultaten |
|---|---|---|---|
| 1 | `GET /api/project` | iedereen | `200` |
| 2 | `GET /api/project/{id}` | iedereen | `200`, onbekend id: `404` |
| 3 | `POST /api/project` | Editor, Admin | `201`; geen login: `401`; Gast: `403`; ongeldige invoer: `400` |
| 4 | `PUT /api/project/{id}` | Editor, Admin | `204`; geen login: `401`; Gast: `403`; onbekend id: `404` |
| 5 | `DELETE /api/project/{id}` | Editor, Admin | `204`; geen login: `401`; Gast: `403`; onbekend id: `404` |
| 6 | `GET /api/blogpost` | iedereen | `200` |
| 7 | `GET /api/blogpost/{id}` | iedereen | `200`, onbekend id: `404` |
| 8 | `POST /api/blogpost` | Editor, Admin | `201`; geen login: `401`; Gast: `403`; ongeldige invoer: `400` |
| 9 | `PUT /api/blogpost/{id}` | Editor, Admin | `204`; geen login: `401`; Gast: `403`; onbekend id: `404` |
| 10 | `DELETE /api/blogpost/{id}` | Editor, Admin | `204`; geen login: `401`; Gast: `403`; onbekend id: `404` |
| – | `POST /api/auth/register` | iedereen | `200`; dubbel e-mailadres of zwak wachtwoord: `400` |
| – | `POST /api/auth/login` | iedereen | `200` + cookie; foute gegevens: `401` |

## Tests uitvoeren

Het xUnit-testproject (`portfolioBackend.Tests`) zit in dezelfde solution en test de API **via HTTP** met `WebApplicationFactory` en de echte Identity-configuratie. De tests gebruiken een **SQLite in-memory database**, dus ze zijn geïsoleerd van de gewone PostgreSQL-database en je hebt de Docker-container er niet voor nodig. De `ApiFactory` maakt per testrun de rollen en drie testgebruikers aan (Gast, Editor, Admin). De testwachtwoorden daarin zijn dummywaarden voor deze tijdelijke database.

Alle tests uitvoeren, vanuit de root van de repository:

```bash
dotnet test
```

Met een coverage-rapport (Cobertura-bestand in `TestResults/`):

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Elke test heeft herkenbare **Arrange / Act / Assert**-onderdelen. Voor `POST`, `PUT` en `DELETE` controleren de tests ook wat er met de gegevens gebeurt, bijvoorbeeld met een `GET` na het aanmaken, wijzigen of verwijderen.

### Route- en testmatrix

De testnamen hebben de vorm `<Actie><Resource>_<Situatie>_<Verwacht resultaat>`. Hieronder staat per route welke tests er zijn (projecten in `ProjectTests`, blogposts in `BlogPostTests`).

| Route | Tests |
|---|---|
| `GET` lijst | `GetProjects_Anonymous_Returns200_AndContainsCreatedProject`, `GetBlogPosts_Anonymous_Returns200_AndContainsCreatedPost` |
| `GET` op id | `GetProject_ExistingId_Returns200_WithCorrectData`, `GetProject_UnknownId_Returns404` (en `GetBlogPost_...`) |
| `POST` | `CreateProject_Anonymous_Returns401`, `CreateProject_AsGast_Returns403`, `CreateProject_AsEditorOrAdmin_Returns201_AndProjectIsStored` (Editor én Admin), `CreateProject_InvalidInput_Returns400` (en `CreateBlogPost_...`) |
| `PUT` | `UpdateProject_Anonymous_Returns401`, `UpdateProject_AsGast_Returns403_AndDataUnchanged`, `UpdateProject_AsEditorOrAdmin_Returns204_AndDataIsChanged` (Editor én Admin), `UpdateProject_UnknownId_Returns404` (en `UpdateBlogPost_...`) |
| `DELETE` | `DeleteProject_Anonymous_Returns401`, `DeleteProject_AsGast_Returns403_AndProjectStillExists`, `DeleteProject_AsEditorOrAdmin_Returns204_AndProjectIsGone` (Editor én Admin), `DeleteProject_UnknownId_Returns404` (en `DeleteBlogPost_...`) |
| Registratie | `Register_ValidData_Returns200`, `Register_NewUser_GetsGastRole_NotEditor`, `Register_DuplicateEmail_Returns400`, `Register_WeakPassword_Returns400` |
| Login | `Login_CorrectCredentials_Returns200_AndSetsAuthCookie`, `Login_WrongPassword_Returns401` |

## Database stoppen

```bash
docker compose down        # stopt de container, data blijft bewaard
docker compose down -v     # stopt de container en verwijdert de data
```

## Voorbereiding op de frontend

De backend gebruikt cookie-authenticatie via de Identity-login (`POST /api/auth/login`). Een latere SPA (React) kan zich daarmee aanmelden; de API geeft `401` en `403` terug in plaats van redirects naar een loginpagina.
