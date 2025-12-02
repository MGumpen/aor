Systemarkitektur for AOR

```mermaid
flowchart TD
    Browser["Browser (Bruker)"]
    WebApp["ASP.NET Core MVC"]
    Repos["Repositorys"]
    EFCore["Entity Framework Core, DbContext"]
    MariaDB["MariaDB (Docker container)"]

    Browser --> WebApp
    WebApp --> Repos
    Repos --> EFCore
    EFCore --> MariaDB
```