# Mangalith — Roadmap

> Evolución estratégica de Mangalith desde fase inicial hasta v1.0. Este documento define las fases de desarrollo, requisitos funcionales y no funcionales, y criterios de éxito.

---

## Visión General

Mangalith busca ser el "WordPress del manga": una plataforma completa, autodesplegable y agnóstica que permita a cualquiera (desde entusiastas solitarios hasta comunidades) montar su propio sitio de manga sin barreras técnicas.

El camino hacia v1.0 está dividido en **4 fases estratégicas**:

```
Fase 0: Setup & Infraestructura (Fundación)
    ↓
Fase 1: Gestión de Contenido + Lector (Core)
    ↓
Fase 2: Autenticación & Panel Admin (Control)
    ↓
Fase 3: Pulido & Profesionalización (v1.0)
```

---

## Matriz de Requisitos

### Requisitos Funcionales Globales

| Requisito | Fase | Prioridad | Descripción |
|---|---|---|---|
| Subida y gestión de manga | 1 | **Alta** | Sistema de carga de archivos, procesamiento de imágenes, almacenamiento |
| Lector visualmente moderno | 1 | **Alta** | Interfaz intuitiva, navegación fluida, responsive |
| Autenticación de usuarios | 2 | **Alta** | Registro, login, gestión de sesiones |
| Sistema de roles | 2 | **Alta** | Admin, Uploader, Reader con permisos diferenciados |
| Panel administrativo | 2 | **Alta** | Gestión de contenido, usuarios, configuración del sitio |
| Lectura pública | 1 | **Alta** | Cualquiera puede leer sin estar logueado |
| Persistencia de progreso de lectura | 2 | **Media** | Guardar página actual y capítulos leídos por usuario |
| Búsqueda y filtrado de manga | 3 | **Media** | Búsqueda por título, autor, género, tags |
| Sistema de notificaciones | 3 | **Baja** | Notificar sobre nuevos capítulos (v1.0+) |

### Requisitos No Funcionales Globales

| Requisito | Fase | Prioridad | Descripción |
|---|---|---|---|
| Arquitectura limpia/hexagonal | 0 | **Alta** | Base sólida para mantenimiento y escalabilidad |
| Microservicios desacoplados | 0 | **Alta** | API REST independent, frontend agnóstico |
| Testing de lógica de negocio | 1-3 | **Alta** | Cobertura significativa sin obsesión exhaustiva |
| Seguridad en autenticación | 2 | **Alta** | Hashing de contraseñas, protección CSRF, validaciones |
| Documentación de API | 3 | **Media** | OpenAPI/Swagger para consumo externo |
| Performance básico | 3 | **Media** | Consultas optimizadas, caching donde sea obvio |
| Escalabilidad horizontal | 3 | **Baja** | Posibilidad de escalar, pero no crítico en v1.0 |

---

## Fase 0: Setup & Infraestructura

**Objetivo**: Establecer la base técnica sólida para todo lo que viene.

**Duración estimada**: 1-2 semanas

### Requisitos a Completar

**Funcionales**:
- [ ] Estructura de proyectos (backend, frontend, docker-compose)
- [ ] Base de datos configurada y migraciones iniciales
- [ ] API REST básica funcionando
- [ ] Frontend con setup de Next.js

**No Funcionales**:
- [ ] Arquitectura hexagonal implementada en backend
- [ ] Testing framework configurado (JUnit 5 + Mockito en backend, Jest en frontend)
- [ ] Docker Compose para desarrollo local
- [ ] Pipeline CI/CD básico (GitHub Actions)
- [ ] Logs y manejo de excepciones estructurado

### Detalles Técnicos

**Backend (Spring Boot)**:
- Structure: `domain`, `application`, `infrastructure`, `interfaces` (ports & adapters)
- Database: PostgreSQL con Flyway para migraciones
- Testing: JUnit 5, Mockito, TestContainers para BD

**Frontend (Next.js)**:
- Estructura de componentes clara
- TailwindCSS para styling
- Testing: Jest + React Testing Library (setup, pero sin cobertura masiva aún)

**DevOps**:
- Docker Compose: DB, backend, frontend
- GitHub Actions: Build y tests en cada push

### Criterios de Éxito

✓ Puedo correr `docker-compose up -d` y tener todo funcional localmente  
✓ Un test de ejemplo pasa en backend y frontend  
✓ La arquitectura está clara y documentada (aunque simple)  

### Notas Personales / Técnicas

- **Para ti**: Este es el "scaffolding" aburrido pero crucial. Hazlo con cuidado, porque cambiar arquitectura después es costoso.
- **Consideración**: Usa Spring Boot Starters (Data JPA, Web) para no reinventar rueda, pero mantén la lógica separada en domain.
- **Testing**: No necesitas 100% cobertura aún, pero sí que entiendas dónde van los tests (domain logic sí, infraestructura puede esperar).

---

## Fase 1: Gestión de Contenido + Lector

**Objetivo**: Sistema funcional para subir manga y que otros lo lean sin friction.

**Duración estimada**: 6-10 semanas

### Requisitos a Completar

**Funcionales**:
- [ ] Modelo de datos: Series, Capítulos, Páginas, Metadatos
- [ ] API para crear/editar series y capítulos (admin)
- [ ] Sistema de carga de imágenes (múltiples formatos)
- [ ] Procesamiento de imágenes (resize, conversión, validación)
- [ ] Almacenamiento de archivos (local o S3 simple)
- [ ] **Lector visual moderno**
  - [ ] Visualización de páginas individual
  - [ ] Navegación entre capítulos (anterior/siguiente, listado)
  - [ ] Indicador de progreso
  - [ ] Responsive (mobile-first si es posible)
  - [ ] Modos de visualización (single page, double page, scroll)
- [ ] Listado público de series
- [ ] Página de detalle de serie con capítulos
- [ ] Lectura completamente pública (sin login requerido)

**No Funcionales**:
- [ ] Validaciones de contenido (formatos aceptados, tamaños, etc.)
- [ ] Manejo de errores en carga de archivos
- [ ] Tests de casos de uso principales (subir manga, leer manga)
- [ ] Optimización de imágenes (no servir originales 10MB)
- [ ] Caché básico de imágenes procesadas

### Detalles Técnicos

**Backend**:
- Entities: `Series`, `Chapter`, `Page`, `Image`
- Use Cases: `UploadMangaUseCase`, `GetSeriesDetailUseCase`, `GetChapterPagesUseCase`
- Image processing: ImageMagick o similar (vía library)
- Storage abstraction: Port para `ImageStorageRepository` (local/S3 intercambiable)
- Tests: Casos de uso principal con mocks, integración con BD de prueba

**Frontend**:
- Pages: `/`, `/series/:id`, `/series/:id/chapter/:chapterId/reader`
- Components: SeriesList, SeriesDetail, MangaReader
- Reader: Canvas/img tags, keyboard navigation, touch swipe (nice-to-have)
- Styling: Moderno, sin emojis, énfasis en UX limpia

**Data Models**:
```
Series {
  id, title, description, author, cover, createdAt, updatedAt
}

Chapter {
  id, seriesId, number, title, uploadDate
}

Page {
  id, chapterId, pageNumber, imageUrl, dimension
}
```

### Criterios de Éxito

✓ Alguien puede subir un manga (por API o admin)  
✓ Otro usuario ve el manga en el listado  
✓ Puede leer todos los capítulos con interfaz pulida  
✓ Funciona en mobile sin sentirse roto  
✓ Tests de lógica de negocio de manga pasan  

### Notas Personales / Técnicas

- **Prioridad visual**: El lector debe verse **profesional desde el día 1**. Este es el showcase principal de tu portafolio. Invierte tiempo en UX aquí.
- **Arquitectura**: La separación entre "subir manga" (admin) y "leer manga" (público) debe ser clara. Use JWT o sesiones simples para diferenciar después en Fase 2.
- **Storage**: Comienza con local (carpeta), es suficiente para v1.0. S3 es nice-to-have v1.1.
- **Testing**: Enfócate en "casos de uso" (UploadManga, ReadChapter) no en cada getter/setter. Mockea storage y BD.
- **Performance note**: No obsesiones con caching aún, pero sí con procesamiento eficiente de imágenes (no servir originales).

---

## Fase 2: Autenticación & Panel Admin

**Objetivo**: Sistema de usuarios y control completo para múltiples escenarios (personal, comunitario, etc.).

**Duración estimada**: 4-8 semanas

### Requisitos a Completar

**Funcionales**:
- [ ] Registro de usuarios (email, username, password)
- [ ] Login / Logout
- [ ] Sesiones y tokens (JWT o sesiones server)
- [ ] Sistema de roles: Admin, Uploader, Reader
- [ ] Panel administrativo funcional
  - [ ] Dashboard (overview)
  - [ ] Gestión de usuarios (CRUD, cambio de rol)
  - [ ] Gestión de series/capítulos (editar, eliminar)
  - [ ] Configuración del sitio (nombre, descripción, etc.)
  - [ ] Logs/auditoría (quién hizo qué)
- [ ] Autorización por ruta (solo admin ve admin panel)
- [ ] Persistencia de progreso de lectura (qué página lees, capítulos leídos)
- [ ] Perfil de usuario (editar datos básicos)

**No Funcionales**:
- [ ] Hashing seguro de contraseñas (bcrypt)
- [ ] Protección CSRF en formularios
- [ ] Validaciones robustas en login/registro
- [ ] Tests de autenticación y autorización
- [ ] Rate limiting en login (brute force protection)
- [ ] Manejo seguro de tokens (expiración, refresh)

### Detalles Técnicos

**Backend**:
- Entities: `User`, `Role`, `UserProgress`
- Use Cases: `RegisterUserUseCase`, `LoginUseCase`, `UpdateUserRoleUseCase`
- Security: Spring Security, JWT o sesiones (elige uno)
- Tests: Autorización correcta, tokens válidos, permisos por rol

**Frontend**:
- Pages: `/auth/login`, `/auth/register`, `/admin/*`
- Components: LoginForm, RegisterForm, AdminDashboard, UserManager, etc.
- State management: Contexto de usuario + token en localStorage (considerar seguridad luego)
- Protected routes: Solo accessible si logueado y rol correcto

**Roles & Permisos**:
```
Admin: CRUD todo, gestión de usuarios, configuración
Uploader: Crear/editar sus propias series, subir capítulos
Reader: Leer, guardar progreso, ver su perfil
```

### Criterios de Éxito

✓ Un usuario se registra, loguea, ve panel admin  
✓ Admin puede crear usuarios y asignar roles  
✓ Uploader puede subir manga, Reader no  
✓ Progreso de lectura persiste entre sesiones  
✓ Tests de seguridad básica pasan  

### Notas Personales / Técnicas

- **Autenticación**: Spring Security es potente pero complejo. Comienza simple, agrega complejidad si necesitas.
- **JWT vs Sesiones**: JWT es más "moderna" y stateless (mejor para APIs/microservicios). Sesiones son más simples. Elige según comfort.
- **Admin Panel**: No necesita ser bonito aún, solo funcional. Enfócate en lógica, UI puede pulirse en Fase 3.
- **Testing**: Este es donde testing importa mucho. Valida que un Reader no pueda deletear series, un Uploader no pueda cambiar su rol, etc.
- **Portafolio angle**: Aquí demuestras conocimiento de seguridad y RBAC. Documenta bien.

---

## Fase 3: Pulido & Profesionalización (v1.0)

**Objetivo**: Convertir MVP funcional en v1.0 profesional, listo para usar en producción.

**Duración estimada**: 3-6 semanas

### Requisitos a Completar

**Funcionales**:
- [ ] Búsqueda y filtrado de manga (por título, autor, género)
- [ ] Sistema de tags/categorías en series
- [ ] Página "Acerca de" / "Créditos" configurables
- [ ] Feedback de validaciones clara (errores visibles)
- [ ] Función de "continuar leyendo" en homepage
- [ ] Soporte para modo oscuro (nice-to-have)

**No Funcionales**:
- [ ] Optimización de queries (índices, eager loading)
- [ ] Caching de datos frecuentes (Redis opcional, pero considera)
- [ ] Documentación de API (OpenAPI/Swagger)
- [ ] Documentación de usuario (guías de instalación, uso)
- [ ] Tests adicionales (cobertura ~60-70%, no obsesión)
- [ ] Validaciones robustas en todas partes
- [ ] UX refinement (animaciones suaves, transiciones, loading states)
- [ ] Performance audit (Lighthouse, queries lentas)
- [ ] Seguridad adicional (headers de seguridad, input sanitization)
- [ ] Deploy documentation (guía de instalación en producción)

### Detalles Técnicos

**Backend**:
- Optimizaciones: Índices en BD, lazy loading donde corresponda
- API docs: Anotaciones OpenAPI, Swagger UI
- Logging: Información útil, no spam
- Monitoreo: Preparación para observabilidad (no necesario implementar aún)

**Frontend**:
- Performance: Code splitting, lazy loading de componentes
- UX: States claros (loading, error, success), transiciones suaves
- Accessibility: Alt text, labels en forms, navegación por teclado
- Documentación: README con screenshots, guías de desarrollo

**Deployment**:
- Docker: Imágenes optimizadas, multi-stage builds
- Documentación de instalación clara
- Variables de entorno bien documentadas
- Script de setup inicial (base de datos, admin user)

### Criterios de Éxito

✓ Plataforma se siente pulida y profesional  
✓ Documentación permite que alguien externo lo instale sin preguntar  
✓ API está documentada y es usable  
✓ Tests críticos pasan y cobertura es razonable  
✓ Performance es aceptable (Lighthouse 70+)  
✓ Seguridad básica validada (OWASP top 10)  

### Notas Personales / Técnicas

- **Porfolio angle**: Este es donde brillas. Documentación, testing, performance = profesionalismo.
- **Versioning**: Considera implementar semantic versioning desde aquí. v1.0.0 es un hito.
- **Roadmap futuro**: Documenta qué queda para v1.1, v2.0 (notificaciones, recomendaciones, etc.).
- **Tech debt**: Antes de release, lista qué decidiste "no hacer" en v1.0 y por qué. Muestra pensamiento estratégico.

---

## Fuera del Scope — v1.0

Estas características son deliberadamente pospuestas para después de v1.0:

- **Notificaciones de nuevos capítulos** (requiere queue/scheduler)
- **Sistema de recomendaciones inteligente** (requiere ML/algorithms)
- **Integración con Discord** (externa, complicada en seguridad)
- **Modo offline del lector** (requiere service workers, local storage robusto)
- **Análisis y estadísticas detalladas** (requiere agregaciones complejas)
- **Soporte para cómics occidentales** (requiere adjusts en metadata/modelo)
- **Comentarios en capítulos** (requiere moderation, spam filtering)
- **Sistema de puntuaciones/reviews** (requiere agregaciones)
- **API pública para terceros** (requiere rate limiting, OAuth, etc.)

Estas van en **v1.1+** cuando v1.0 esté estable y validado.

---

## Notas Finales

### Para tu Desarrollo Personal

- **No te presiones con timeline**: Este es un side project. Si una fase toma 3 semanas en lugar de 6, genial. Si toma 12, está bien.
- **Refactoriza mientras avanzas**: La arquitectura hexagonal permite cambios internos sin quebrar external APIs. Úsalo.
- **Aprende en el camino**: Cada fase te enseña algo. Fase 0 = arquitectura, Fase 1 = lógica + frontend, Fase 2 = seguridad, Fase 3 = polish.
- **Comunica progreso**: Si quieres, crea releases en GitHub para cada fase. `v0.1-alpha`, `v0.5-beta`, `v1.0` son hitos visibles.

### Para tu Portafolio

- **Haz visibles los commits**: Commits atómicos, mensajes claros. Los recruiters ven el historio.
- **Documenta decisiones**: Por qué elegiste Spring Boot sobre .NET, Next.js sobre Vue, hexagonal sobre layered.
- **Muestra testing**: No necesita ser extremo, pero ve sus tests y entiende por qué los casos principales están cubiertos.
- **Refuerza v1.0**: Cuando llegues, v1.0 es el punto de venta. "Plataforma lista para producción" suena mejor que "proyecto en desarrollo".

### Próximos Pasos

1. Comienza con **Fase 0**: Setup limpio es inversión que vale.
2. Planifica sprints internos si lo deseas (ej: "Semana 1: BD + API basica", "Semana 2: First component en frontend").
3. Revisa este roadmap cada 2 semanas. Ajusta si la realidad lo requiere.
4. Cuando termines una fase, marca hitos en GitHub (milestones, releases).

---

<div align="center">

**Mangalith v1.0: El WordPress del manga. Hecho con arquitectura, pasión y propósito.**

</div>
