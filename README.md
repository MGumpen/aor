AOR
Aviation Obstacle Registration

It og informasjonssystemer
Gruppe 3, høst 2025

# AOR - ASP.NET Core Application

## Docker Setup

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
