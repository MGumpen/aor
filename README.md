# AOR - Aviation Obstacle Registration
**ASP.NET Core MVC Application with Docker & MariaDB**

It og informasjonssystemer  
Gruppe 3, høst 2025

## 🚀 Quick Start

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Running with Docker Compose (Recommended)

1. **Start hele stack-en:**
   ```bash
   docker compose up -d
   ```

2. **Åpne applikasjonen:**
   - Web App: http://localhost:5000
   - Database Admin (Adminer): http://localhost:8080

3. **Stopp hele stack-en:**
   ```bash
   docker compose down
   ```

### Running lokalt for utvikling

1. **Start kun database:**
   ```bash
   docker compose up -d mariadb
   ```

2. **Start .NET applikasjonen:**
   ```bash
   cd AOR
   dotnet run
   ```

3. **Applikasjonen kjører på:**
   - http://localhost:5242

## 🛠️ Database Management

### Entity Framework Migrations

```bash
# Opprett ny migration
dotnet ef migrations add <MigrationName>

# Kjør migrations
dotnet ef database update

# Se migration status
dotnet ef migrations list

# Fjern siste migration (hvis ikke appliert)
dotnet ef migrations remove
```

### Database Tilkobling

**Lokal utvikling:**
- Server: localhost:3307
- Database: aor_db
- Bruker: aor_user
- Passord: aor_password123

**Docker container:**
- Server: mariadb:3306
- Database: aor_db
- Bruker: aor_user
- Passord: aor_password123

### Adminer (Database GUI)

Åpne http://localhost:8080 når Docker kjører:
- System: MySQL
- Server: mariadb
- Username: aor_user
- Password: aor_password123
- Database: aor_db

## 📁 Project Structure

```
AOR/
├── Controllers/        # MVC Controllers
├── Models/            # Data Models  
├── Views/             # Razor Views
├── Data/              # Entity Framework DbContext
├── wwwroot/           # Static files (CSS, JS, images)
├── Migrations/        # EF Core migrations
├── appsettings.json   # App configuration
└── Dockerfile         # Container definition

docker-compose.yml     # Multi-container orchestration
```

## 🐳 Docker Commands

```bash
# Start alle services
docker compose up -d

# Start kun database
docker compose up -d mariadb

# Se logs for specific service
docker compose logs -f aor-web
docker compose logs -f mariadb

# Rebuild og restart app
docker compose up -d --build aor-web

# Stopp og fjern alt
docker compose down -v  # -v fjerner volumes også
```

## 🔧 Development Tips

1. **Hot Reload:** Koden oppdateres automatisk når du endrer filer
2. **Database Changes:** Lag migration etter schema-endringer
3. **Environment Variables:** Bruk appsettings.Development.json for lokale innstillinger
4. **Logging:** EF Core SQL-spørringer logges i Development-modus

## 🚨 Troubleshooting

**Port konflikter:**
- MariaDB bruker port 3307 (ikke standard 3306)
- Web app bruker port 5000 i Docker, 5242 lokalt

**Database tilkobling feiler:**
```bash
# Sjekk at MariaDB kjører
docker compose ps

# Se database logs
docker compose logs mariadb

# Test tilkobling
docker exec -it aor-mariadb mysql -u aor_user -p aor_db
```

**Migration problemer:**
```bash
# Reset database completely
docker compose down -v
docker compose up -d mariadb
dotnet ef database update
```

Dette prosjektet inkluderer enkel Docker-konfiguration for deployment og utvikling.

### Forutsetninger

- Docker Desktop installert på macOS
- Docker Compose (inkludert i Docker Desktop)

### Kjøre applikasjonen med Docker

#### Enkelt kommando:
```bash
./run-docker.sh
```

#### Manuelle kommandoer:
```bash
# Bygg og start containerne
docker-compose up --build

# Kjør i bakgrunnen
docker-compose up --build -d

# Se logger
docker-compose logs -f

# Stopp containerne
docker-compose down
```

### Tilgang til applikasjonen

- **HTTP:** http://localhost:5000

### Nyttige Docker-kommandoer

```bash
# Se kjørende containere
docker ps

# Se alle images
docker images

# Fjern alle stoppede containere og ubrukte images
docker system prune

# Bygg på nytt uten cache
docker-compose build --no-cache

# Få tilgang til container shell
docker-compose exec aor-app bash
```

### Feilsøking

- Sjekk at Docker Desktop kjører
- Kontroller at port 5000 ikke er i bruk
- Se container-logger: `docker-compose logs aor-app`

## Filstruktur

- `Dockerfile` - Hovedkonfiguration for Docker image
- `docker-compose.yml` - Docker Compose konfigurasjon
- `.dockerignore` - Filer som skal ekskluderes fra Docker build
- `run-docker.sh` - Skript for å kjøre applikasjonen enkelt
>>>>>>> f639697 (Lagt til dockerfile og docker-compose ved hjelp av AI)
