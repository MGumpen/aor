# AOR - Aviation Obstacle Registration
ASP.NET Core MVC Application with Docker & MariaDB
Laget for UiA i samarbeid med Norsk Luftambulanse og Kartverket

Gruppe 3, IT og informasjonssystemer, høsten 2025.

- Vi har en egen fil som viser hvordan vi bruker github: Github.md

- Forskjellige testingscenarier finner du i Testing.md

# Brukere i Web Applikasjonen: #
- Crew: crew@test.no Passord: Test123$
- Admin: admin@test.no Passord: Test123$
- Registerfører: reg@test.no Passord: Test123$

### Forutsetninger for å starte hele applikasjonen:
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for lokal utvikling)

### Start hele applikasjonen

```bash
# For å opprette docker containerne og starte alle services:
docker compose up -d --build

# Start alle services hvis du har containere fra før (database, web app, adminer):
docker compose up -d

# Åpne applikasjonen:
# - Web App: http://localhost:5001
# - Database Admin: http://localhost:8080

# Stopp alle services
docker compose down
```

### Lokal utvikling uten databasetilkobling

```bash
# 1. Start .NET applikasjonen lokalt
cd AOR
dotnet run

# 2. Åpne http://localhost:5001

#3. Stoppe .NET applikasjonen
    Trykk: Ctrl+C
```

## 🛠️ Database

### Connection String
- **Lokal utvikling:** `Server=localhost;Database=aor_db;Uid=aor_user;Pwd=Test123;Port=3306;`
- **Docker container:** `Server=mariadb;Database=aor_db;Uid=aor_user;Pwd=Test123;Port=3306;`

### Adminer (Database GUI)
Åpne http://localhost:8080:
- System: MySQL
- Server: mariadb
- Username: aor_user
- Password: Test123
- Database: aor_db

### Entity Framework Migrations

```bash
cd AOR

# Opprett ny migration
dotnet ef migrations add <MigrationName>

# Kjør migrations
dotnet ef database update

# Se migration status
dotnet ef migrations list
```

### Database tilgang via terminal

```bash
# Koble til MariaDB
docker exec -it aor-mariadb mariadb -u root -prootpassword123

# I MariaDB:
USE aor_db;
SHOW TABLES;
SELECT * FROM Advices;
```

##  Docker Commands

```bash
# Start alle services
docker compose up -d

# Se status
docker compose ps

# Se logs
docker compose logs -f aor-web
docker compose logs -f mariadb

# Rebuild web app
docker compose up -d --build aor-web

# Stopp og fjern alt (inkludert data)
docker compose down -v
```

## 🚨 Troubleshooting

**Web app unhealthy:**
```bash
docker logs aor-web
# Sjekk connection string i appsettings.json
```

**Database tilkobling feiler:**
```bash
# Sjekk at MariaDB kjører
docker compose ps

# Test tilkobling
docker exec -it aor-mariadb mariadb -u aor_user -pTest123 aor_db
```
