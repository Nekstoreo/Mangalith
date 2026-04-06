# Mangalith Backend

Backend API construido con **NestJS** + **Prisma** + **PostgreSQL**.

## Stack Tecnológico

- **Framework**: NestJS 11
- **Lenguaje**: TypeScript 5.7
- **ORM**: Prisma 6
- **Base de Datos**: PostgreSQL 17
- **Autenticación**: JWT ( Passport )
- **Documentación**: Swagger/OpenAPI
- **Validación**: class-validator + class-transformer
- **Procesamiento de imágenes**: Sharp

## Estructura del Proyecto

```
src/
├── modules/
│   ├── auth/          # Autenticación (JWT)
│   ├── manga/         # Series, capítulos, páginas
│   ├── reader/        # Progreso de lectura
│   └── uploads/       # Manejo de archivos
├── config/
│   └── prisma/        # Configuración de Prisma
├── common/
│   ├── filters/       # Filtros de excepciones
│   ├── interceptors/  # Interceptores
│   └── guards/        # Guards de autenticación
└── main.ts            # Entry point
```

## Módulos

### Auth
- Registro de usuarios (`POST /api/auth/register`)
- Login (`POST /api/auth/login`)
- JWT Bearer authentication

### Manga
- Series CRUD (`/api/series`)
- Capítulos con paginación (`/api/chapters`)
- Relaciones: Series → Chapters → Pages

### Reader
- Progreso de lectura por usuario
- Guarda última página leída
- Historial de lectura

### Uploads
- Subida de imágenes
- Conversión automática a WebP
- Optimización con Sharp

## Inicio Rápido

### Desarrollo Local

```bash
# Instalar dependencias
npm install

# Configurar variables de entorno
cp .env.example .env
# Editar .env con tus credenciales de DB

# Generar cliente Prisma
npx prisma generate

# Ejecutar migraciones
npx prisma migrate dev

# Iniciar servidor en modo desarrollo
npm run start:dev
```

### Docker

```bash
# Desde la raíz del proyecto
docker-compose up -d
```

## Scripts Disponibles

```bash
npm run start:dev      # Desarrollo con hot-reload
npm run build          # Compilar para producción
npm run start:prod     # Ejecutar compilado
npm run db:generate    # Generar cliente Prisma
npm run db:migrate     # Crear/ aplicar migraciones
npm run db:studio      # Abrir Prisma Studio
npm run db:deploy      # Deploy migraciones en producción
```

## API Documentation

Una vez iniciado el servidor, accede a Swagger UI:
- **URL**: http://localhost:3001/api/docs

## Variables de Entorno

| Variable | Descripción | Default |
|----------|-------------|---------|
| `DATABASE_URL` | URL de conexión PostgreSQL | - |
| `JWT_SECRET` | Clave secreta para JWT | - |
| `JWT_EXPIRATION` | Tiempo de expiración del token | 7d |
| `PORT` | Puerto del servidor | 3001 |
| `UPLOAD_DIR` | Directorio para uploads | uploads |
| `CORS_ORIGIN` | Origen permitido para CORS | * |

## Decisiones Arquitectónicas

### ¿Por qué NestJS?
- **Estructura opinionada**: Evita código spaghetti
- **Inyección de dependencias**: Testing y organización
- **Decoradores**: Código limpio y declarativo
- **TypeScript first**: Type safety completo

### ¿Por qué Prisma?
- **Type-safe queries**: Autocompletado en el IDE
- **Migraciones automáticas**: Schema → DB sincronizado
- **Prisma Studio**: UI para explorar datos
- **Performance**: Query engine optimizado

### ¿Por qué Monolito Modular?
- **Simpleza**: Un deploy, una base de datos
- **Límites claros**: Cada módulo es independiente
- **Escalable**: Puede extraerse a microservicios si crece
- **Costo de infraestructura bajo**

## Licencia

Apache License 2.0 - Ver [LICENSE](../LICENSE)
