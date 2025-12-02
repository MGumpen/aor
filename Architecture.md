Systemarkitektur for AOR

```mermaid
flowchart TD
    Browser["Browser\n(Bruker)"]
    WebApp["ASP.NET Core MVC\nControllers + Views + ViewModels"]
    Services["Tjenestelag\n(Forretningslogikk)"]
    EFCore["Entity Framework Core\nDbContext + Entities"]
    MariaDB["MariaDB\n(Docker container)"]

    Browser --> WebApp
    WebApp --> Services
    Services --> EFCore
    EFCore --> MariaDB
```