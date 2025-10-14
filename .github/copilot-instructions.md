# Instrucciones de Mangalith para Agentes de IA

## Visión General del Proyecto
Mangalith es una plataforma de lectura de manga de código abierto diseñada para democratizar el hosting de manga. Es un monorepo con un **frontend Next.js 15** (TypeScript, React 19, shadcn/ui) y un **backend .NET 9** (C# , Clean Architecture). El proyecto se encuentra actualmente en la Fase 1 (Fundación) - la autenticación está implementada, pero la persistencia en base de datos (EF Core + PostgreSQL) aún no está integrada.

## Arquitectura y Estructura

### Backend: Clean Architecture (.NET 9)
```
backend/
├── Mangalith.Api/          # Punto de entrada, controladores, middleware
├── Mangalith.Application/  # Lógica de negocio, servicios, validadores
├── Mangalith.Infrastructure/ # Aspectos externos (JWT, repositorios)
└── Mangalith.Domain/       # Entidades core (User, futuro: Manga, Chapter)
```

**Flujo de Dependencias**: `Api → Application → Domain ← Infrastructure`

- **Domain**: Entidades puras con setters privados y métodos factory (ej. `User.cs`)
- **Application**: Servicios (ej. `AuthService`), validadores (FluentValidation), contratos
- **Infrastructure**: `JwtProvider`, `Pbkdf2PasswordHasher`, repositorios en memoria (serán EF Core)
- **Api**: Controladores, middleware (`ExceptionHandlingMiddleware`), configuración en Program.cs

**Patrones Clave**:
- **Inyección de Dependencias**: Cada capa tiene un `DependencyInjection.cs` con métodos de extensión (ej. `AddApplication()`, `AddInfrastructure()`)
- **Manejo de Excepciones**: Las excepciones custom heredan de `AppException` (ValidationAppException, UnauthorizedAppException, ConflictAppException) y son capturadas por `ExceptionHandlingMiddleware` para retornar JSON `ErrorResponse` consistente
- **Validación**: FluentValidation para requests (ej. `RegisterRequestValidator` enforcea complejidad de contraseña)
- **Formato de Respuesta**: Los controladores retornan respuestas tipadas (`AuthResponse`) envueltas en `ApiResponse<T>` o `ErrorResponse` con JSON en camelCase

### Frontend: Next.js App Router (Next.js 15)
```
frontend/src/
├── app/           # Páginas y layouts (App Router)
├── components/    # Componentes React (basados en shadcn/ui)
├── stores/        # Stores de Zustand (auth.ts, preferences.ts)
├── services/      # Cliente API, fetching de datos
└── lib/           # Utilidades
```

**Patrones Clave**:
- **Gestión de Estado**: Zustand con middleware `persist` para auth (almacena token en localStorage)
- **Cliente API**: Clase `ApiClient` personalizada en `services/api/client.ts` con interceptores de axios:
  - Request: Agrega automáticamente `Authorization: Bearer <token>` desde Zustand
  - Response: 401 dispara logout + redirección a `/auth/login`
- **Formularios**: React Hook Form + Zod para validación
- **Flujo de Auth**: `middleware.ts` define rutas protegidas pero delega a validaciones client-side (tokens en localStorage)
- **Estilos**: Tailwind CSS v4, componentes shadcn/ui, fuentes Geist

## Flujo de Desarrollo

### Configuración del Entorno
1. **Copiar variables de entorno**: `cp env.example .env` (actualizar `JWT_SECRET`, credenciales de BD)
2. **Iniciar PostgreSQL**: `docker-compose up -d` (usa postgres:16)
3. **Instalar dependencias**: `pnpm install` (raíz instala concurrentemente frontend/backend)
4. **Ejecutar servidores de desarrollo**: `pnpm dev` (corre frontend:3000 y backend vía concurrently)

### Desarrollo Backend
- **Build**: `cd backend && dotnet build --no-restore /m:1` (workaround para problemas de .NET 9 preview)
- **Run**: `dotnet run --no-build --project Mangalith.Api` (escucha en https://localhost:5001)
- **Endpoints**: Swagger en `/swagger` (solo dev), health check en `/health`
- **Base de Datos**: AÚN NO IMPLEMENTADA - actualmente usa `InMemoryUserRepository` en la capa Infrastructure

### Desarrollo Frontend
- **Servidor dev**: `pnpm dev` (Turbopack habilitado, puerto 3000)
- **Build**: `pnpm build --turbopack`
- **Linting**: `pnpm lint` (ESLint 9)

## Convenciones Críticas

### Backend
1. **Controladores**: Retornan `IActionResult` con códigos de estado explícitos (`Created()`, `Ok()`, `BadRequest()`). Usar `[FromBody]` para tipos complejos, `[AllowAnonymous]` para rutas públicas
2. **Servicios**: Aceptan `CancellationToken` para operaciones async (ej. `RegisterAsync(request, cancellationToken)`)
3. **Nombres**: Las rutas son lowercase (`/api/auth/register`). Los DTOs usan propiedades PascalCase pero se serializan a camelCase
4. **Configuración JWT**: `Jwt:SecretKey` debe tener ≥32 caracteres. Se configura en `appsettings.json` y se vincula a `JwtSettings` vía options pattern
5. **CORS**: Configurado vía array `Cors:AllowedOrigins` en appsettings. Array vacío = permitir todos los orígenes (modo dev)
6. **Logging**: Serilog configurado en Program.cs con logging estructurado

### Frontend
1. **Llamadas API**: Siempre usar instancia `ApiClient`, nunca axios directo. Los errores se manejan automáticamente por interceptores
2. **Estado de Auth**: Acceder vía hook `useAuthStore()`. `login()` establece user+token, `logout()` limpia y redirige
3. **Rutas Protegidas**: Definir en array `protectedRoutes` de `middleware.ts`. Las validaciones de auth ocurren client-side vía Zustand
4. **Variables de Entorno**: Prefijar variables públicas con `NEXT_PUBLIC_` (ej. `NEXT_PUBLIC_API_URL`)
5. **Estructura de Componentes**: Usar primitivos shadcn/ui, extender con composiciones custom en `components/`

### Flujo de Git
- **Mensajes de Commit**: Seguir Conventional Commits (`feat:`, `fix:`, `docs:`, `refactor:`, etc.)
  - **Plantilla**:
    ```
    <tipo>[opcional ámbito]: <descripción>

    [cuerpo opcional]
    ```
  
- **Branches**: `feature/descripcion`, `fix/descripcion`, o `docs/descripcion`
- **Branch Actual**: `develop` (main es default pero inactivo)

## Comunicación Cross-Stack

### Flujo de Autenticación
1. Frontend hace POST a `/api/auth/login` con email/password
2. Backend valida con `LoginRequestValidator`, hashea password vía `IPasswordHasher`, genera JWT vía `IJwtProvider`
3. Retorna `AuthResponse { user, token }` con 200 OK
4. Frontend almacena en Zustand (persistido en localStorage), lo agrega a todos los requests subsiguientes vía interceptor
5. Los endpoints protegidos verifican JWT vía atributo `[Authorize]` (configurado en Program.cs con JwtBearer)

### Manejo de Errores
- **Backend**: Lanza excepciones custom (ej. `throw new UnauthorizedAppException("invalid_credentials", "Invalid email or password")`)
- **Middleware**: Captura y mapea a status HTTP + `ErrorResponse { code, message, errors? }`
- **Frontend**: El interceptor extrae el error, lo almacena en estado `error` de Zustand, lo muestra vía toast/UI

## Trabajo Próximo (Fases de ROADMAP.md)
- **Fase 1 (En Progreso)**: Integración de base de datos con EF Core, migraciones, seeding
- **Fase 2**: Sistema de carga de archivos para manga (CBZ/CBR/ZIP), extracción de imágenes
- **Fase 3**: Sistema de roles (Admin, Moderador, Uploader, Lector), flujo de publicación
- **Fase 4**: Componente lector de manga, tracking de progreso, bookmarks

## Al Trabajar en Este Proyecto
- **Cambios en BD**: Actualmente en memoria. Al agregar EF Core, actualizar `IUserRepository` en Infrastructure y crear migraciones
- **Nuevos Endpoints API**: Agregar a controladores, definir contratos en capa Application, registrar validadores si es necesario
- **Formularios Frontend**: Usar React Hook Form + Zod, mostrar errores desde objeto `ErrorResponse.errors`
- **Cambios de Auth**: Actualizar tanto `AuthService` (backend) como `useAuthStore` (frontend) en sincronía
- **Estilos**: Seguir patrones existentes de shadcn/ui, usar utilidades de Tailwind, mantener soporte de dark mode (next-themes)
- **Tests**: Aún no implementados - referirse a ROADMAP para objetivos futuros de testing de integración

## Archivos Clave de Referencia
- Punto de entrada backend: `backend/Mangalith.Api/Program.cs`
- Lógica de auth: `backend/Mangalith.Application/Services/AuthService.cs`
- Manejo de excepciones: `backend/Mangalith.Api/Middleware/ExceptionHandlingMiddleware.cs`
- Cliente API frontend: `frontend/src/services/api/client.ts`
- Store de auth: `frontend/src/stores/auth.ts`
- Template de entorno: `env.example`
