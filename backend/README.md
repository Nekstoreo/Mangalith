# Mangalith Backend

Base del servidor backend construido con ASP.NET Core 9 siguiendo principios de arquitectura limpia.

## Características

- API modular con capas `Domain`, `Application`, `Infrastructure` y `Api`.
- Autenticación JWT con endpoints de registro y login.
- Validaciones con DataAnnotations y FluentValidation.
- Manejo centralizado de errores con respuestas JSON consistentes.
- Logging estructurado con Serilog.
- Configuración de CORS, HTTPS por defecto y rate limiting básico.

## Requisitos previos

- [.NET 9 SDK](https://dotnet.microsoft.com/) (versión preview según fecha de este repositorio).
- Certificado de desarrollo HTTPS configurado (`dotnet dev-certs https --trust`).

## Configuración

1. Copia `backend/Mangalith.Api/appsettings.json` a `appsettings.Development.json` si necesitas claves personalizadas.
2. Ajusta el secreto JWT (`Jwt:SecretKey`) y los orígenes permitidos en el archivo de configuración o mediante variables de entorno.

## Ejecución

```bash
cd backend
# Restaurar dependencias
dotnet restore
# Compilar (workaround: ejecutar sin restaurar para evitar fallos del SDK preview)
dotnet build --no-restore /m:1
# Levantar la API
dotnet run --no-build --project Mangalith.Api
```

La API estará disponible en `https://localhost:5001` (HTTPS) y `http://localhost:5000` (HTTP). El endpoint de salud `GET /health` y la documentación Swagger en `/swagger` están habilitados en desarrollo.

## Endpoints clave

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/profile/me` (requiere JWT en el header `Authorization: Bearer <token>`)

## Variables de entorno sugeridas

- `ASPNETCORE_ENVIRONMENT=Development`
- `Jwt__SecretKey=<secreto de al menos 64 caracteres>`
- `Cors__AllowedOrigins__0=https://tu-dominio.com`

## Próximos pasos

- Integrar almacenamiento persistente para usuarios.
- Añadir pruebas automatizadas de integración.
- Incorporar migraciones y configuración para base de datos.
