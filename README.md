# Backend API Base (.NET 8)

Plantilla base para construir APIs REST sobre .NET 8 con autenticación JWT, validaciones con FluentValidation y soporte para múltiples motores de base de datos. El objetivo es servir como punto de partida neutro para proyectos que requieran autenticación y gestión de usuarios, sin incluir lógica de negocio específica.

## Características principales

- Arquitectura por capas: `Domain`, `Application`, `Infrastructure`, `Web`.
- Autenticación con JWT usando `Microsoft.AspNetCore.Authentication.JwtBearer`.
- Validaciones de entrada con FluentValidation.
- Repositorio de usuarios basado en ADO.NET con compatibilidad para MySQL, SQL Server, PostgreSQL u Oracle.
- Servicios auxiliares para generación y actualización de contraseñas cifradas con BCrypt.
- Documentación interactiva con Swagger (Swashbuckle).

## Requisitos previos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) o superior.
- Motor de base de datos compatible (MySQL, SQL Server, PostgreSQL u Oracle).
- Editor/IDE de preferencia (Visual Studio, Rider, VS Code).

## Configuración

### Variables de JWT

En `appsettings.json` (o variables de entorno equivalentes) define la sección `Jwt`:

```json
"Jwt": {
  "Key": "<clave-secreta-de-al-menos-32-caracteres>",
  "Issuer": "backend-api-base",
  "Audience": "backend-api-base-clients",
  "ExpiresMinutes": 60
}
```

### Cadena de conexión y proveedor

Selecciona el motor con el valor `DatabaseProvider` (MySql, SqlServer, PostgreSql u Oracle). Puedes definir una cadena genérica `ConnectionStrings:DefaultConnection` o una específica por proveedor:

```json
"DatabaseProvider": "MySql",
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=users_db;User Id=root;Password=secret;",
  "MySql": "Server=localhost;Port=3306;Database=users_db;User Id=root;Password=secret;"
}
```

Si existe una cadena específica para el proveedor actual, tendrá prioridad sobre `DefaultConnection`.

## Ejecución local

1. Restaurar dependencias:
   ```bash
   dotnet restore
   ```
2. Compilar:
   ```bash
   dotnet build
   ```
3. Ejecutar la API:
   ```bash
   dotnet run
   ```

La API se expone por defecto en `https://localhost:5001` y `http://localhost:5000`. Swagger UI estará disponible en `/swagger`.

## Endpoints disponibles

- `POST /api/auth/login`: autentica a un usuario y retorna un token JWT.

  - Request ejemplo:
    ```json
    {
      "username": "demo.user",
      "password": "P@ssw0rd!"
    }
    ```
  - Response ejemplo:
    ```json
    {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "expiresIn": 3600
    }
    ```

- `POST /api/auth/password`: genera una nueva contraseña segura para el usuario indicado y actualiza su hash en la base de datos.
  - Request ejemplo:
    ```json
    {
      "userId": 1,
      "length": 12
    }
    ```
  - Response ejemplo:
    ```json
    {
      "userId": 1,
      "password": "A7s!q9Lm$2Hp"
    }
    ```

> **Nota:** El repositorio de usuarios espera una tabla `users` (con variantes por proveedor) cuyos campos principales coinciden con la entidad `Domain/Entities/User.cs`.

## Estructura del proyecto

- `Domain`: Entidades y contratos compartidos.
- `Application`: Casos de uso, DTOs, validaciones y servicios de aplicación.
- `Infrastructure`: Implementación de repositorios, proveedores de datos y servicios de seguridad.
- `Web`: Controladores, endpoints y composición de la aplicación.

## Próximos pasos sugeridos

- Integrar migraciones o scripts de base de datos acordes al motor elegido.
- Agregar pruebas unitarias/integración para servicios críticos (autenticación, repositorio).
- Conectar la API con proveedores de identidad externos o políticas de autorización según sea necesario.
