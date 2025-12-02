# AOR - Aviation Obstacle Registration
ASP.NET Core MVC Application with Docker & MariaDB
Laget for UiA i samarbeid med Norsk Luftambulanse og Kartverket

Gruppe 3, IT og informasjonssystemer, høsten 2025.

- Vi har en egen fil som viser hvordan vi bruker github: Github.md

- Forskjellige testingscenarier finner du i Testing.md

## Testbrukere i Web Applikasjonen (blir seedet til databasen ved oppstart):
- Crew: crew@test.no Passord: Test123$ Rolle(r): Crew
- Crew 2: crew2@test.no Passord: Test123$ Rolle(r): Crew, Admin
- Admin: admin@test.no Passord: Test123$ Rolle(r): Admin
- Registerfører: reg@test.no Passord: Test123$ Rolle(r): Registerfører

### Forutsetninger for å starte applikasjonen:
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) 

### Start applikasjonen

```bash
#Last ned docker desktop. https://www.docker.com/products/docker-desktop/

#Clone repo til din pc lokalt.

#Naviger i terminalen til der applikasjonsfilene er plassert.

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
## Contributors
- Vi har jobbet med en update branch. Det gjør at main branchen ikke inneholder riktig informasjon om hvem som har jobbet med 
prosjektet dersom man sjekker Insights og deretter Contributors på github, ettersom den bare viser commits til main. 
- Ettersom det stort sett er samme person som har laget PR og merget fra update til main, får denne personen veldig mange flere commits 
enn de andre i gruppen.
- For å få et mer riktig bilde av hvem som har bidratt til prosjektet, anbefaler vi å kjøre følgende kommando i terminalen:
```bash
git shortlog -sne
```
-Copilot står som Contributer. Den er hovedsakelig brukt til å gjøre endringer i CI/CD workflow filen, og ikke selve applikasjonsfilene.



# Teknisk

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
