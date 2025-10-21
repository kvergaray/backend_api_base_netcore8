## Auth API (Minimal API)

### Configuracion
- Edita `appsettings.json` y actualiza `ConnectionStrings:DefaultConnection` y `Jwt:Key`.
- Instala dependencias: `dotnet restore`.

### Ejecucion
- Inicia la API: `dotnet run`.
- Swagger disponible en `http://localhost:5287/swagger`.

### Endpoint
- `POST /api/auth/login`
- Request:

```json
{
  "email": "admin@local",
  "password": "P@ssw0rd!"
}
```

- Response `200 OK`:

```json
{
  "token": "<jwt>",
  "expiresIn": 3600,
  "user": {
    "id": 1,
    "roleId": 2,
    "name": "Doe",
    "firstName": "John",
    "email": "john@acme.com",
    "degreeId": 3,
    "phone": 9999999999,
    "cip": 12345678
  }
}
```

- Response `401 Unauthorized`:

```json
{
  "error": "Invalid credentials"
}
```

### Ejemplo curl

```bash
curl -s -X POST http://localhost:5287/api/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"admin@local\",\"password\":\"P@ssw0rd!\"}"
```
