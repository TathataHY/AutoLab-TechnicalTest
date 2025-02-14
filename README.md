# AutoLab - Sistema de Gestión de Vehículos

Sistema de gestión de vehículos desarrollado con .NET 9, que permite registrar y administrar información de vehículos a nivel mundial.

## Requisitos Previos

- SQL Server 2019 o superior
- .NET 9 SDK
- Visual Studio 2022 o superior

## Configuración del Proyecto

1. Clonar el repositorio:
```bash
git clone https://github.com/tu-usuario/autolab.git
cd autolab
```

2. Configurar la cadena de conexión:
   - En `src/Backend/AutoLab.API/appsettings.json`
   - En `src/Frontend/AutoLab.Web/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AutoLab;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

3. Crear la base de datos:
```bash
cd src/Backend/AutoLab.API
dotnet ef database update
```

4. Configurar el API Token de Countries:
   - Registrarse en https://api.countrystatecity.in/
   - Obtener el API Token
   - Agregar el token en `src/Backend/AutoLab.API/appsettings.json`:

```json
{
  "CountryApi": {
    "Token": "tu-token-aquí"
  }
}
```

## Ejecución del Proyecto

1. Iniciar la API:
```bash
cd src/Backend/AutoLab.API
dotnet run
```

2. En otra terminal, iniciar el Frontend:
```bash
cd src/Frontend/AutoLab.Web
dotnet run
```

3. Acceder a:
   - Frontend: https://localhost:7001
   - API Swagger: https://localhost:7000/swagger

## Estructura del Proyecto

```
src/
├── Backend/
│   ├── AutoLab.API/           # API REST
│   ├── AutoLab.Application/   # Lógica de aplicación
│   ├── AutoLab.Domain/        # Entidades y reglas de negocio
│   └── AutoLab.Infrastructure/# Acceso a datos y servicios externos
└── Frontend/
    └── AutoLab.Web/          # Aplicación MVC
```

## Tecnologías Utilizadas

- Backend:
  - ASP.NET Core 9 Web API
  - Entity Framework Core
  - Clean Architecture
  - SQL Server

- Frontend:
  - ASP.NET Core 9 MVC
  - Bootstrap 5
  - jQuery DataTables
  - Select2

## Base de Datos

La base de datos se crea utilizando Entity Framework Core Code First. Las migraciones están incluidas en el proyecto y se aplicarán automáticamente al ejecutar el comando update.

## Creación de la Base de Datos

### Usando Entity Framework Core (Recomendado)

1. La base de datos se crea automáticamente usando Code First. Ejecutar:
```bash
cd src/Backend/AutoLab.API
dotnet ef database update
```

### Creación Manual (SQL Server)

Si prefieres crear la base de datos manualmente, ejecuta el siguiente script:

```sql
CREATE DATABASE AutoLab;
GO

USE AutoLab;
GO

CREATE TABLE Vehicles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Country NVARCHAR(100) NOT NULL,
    Brand NVARCHAR(100) NOT NULL,
    Model NVARCHAR(100) NOT NULL,
    Year INT NOT NULL,
    LicensePlate NVARCHAR(8) NOT NULL,
    VinCode NVARCHAR(17) NOT NULL,
    CONSTRAINT CK_Year CHECK (Year >= 1900 AND Year <= YEAR(GETDATE())),
    CONSTRAINT CK_LicensePlate CHECK (LEN(LicensePlate) >= 6 AND LEN(LicensePlate) <= 8),
    CONSTRAINT CK_VinCode CHECK (LEN(VinCode) >= 14 AND LEN(VinCode) <= 17)
);
GO

-- Índices para mejorar el rendimiento de las búsquedas
CREATE INDEX IX_Vehicles_Country ON Vehicles(Country);
CREATE INDEX IX_Vehicles_Brand ON Vehicles(Brand);
CREATE INDEX IX_Vehicles_LicensePlate ON Vehicles(LicensePlate);
CREATE INDEX IX_Vehicles_VinCode ON Vehicles(VinCode);
GO
```

### Configuración de la Cadena de Conexión

1. En `appsettings.json` de ambos proyectos (API y Web):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AutoLab;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Notas Importantes

- La base de datos implementa las mismas validaciones que el dominio:
  - Año entre 1900 y el año actual
  - Patente entre 6 y 8 caracteres
  - Código VIN entre 14 y 17 caracteres
- Se crearon índices para optimizar las búsquedas frecuentes
- Las migraciones de Entity Framework mantienen estas mismas restricciones

## Características

- CRUD completo de vehículos
- Validaciones en cliente y servidor
- Búsqueda de países con autocompletado
- Listado con filtros y paginación
- Manejo global de errores
- Arquitectura limpia y mantenible

## Autor

Andre Huaman Yovera

## Licencia

MIT