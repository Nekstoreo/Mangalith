# Mangalith — Roadmap de Ejecución

> Planificación detallada desde el estado actual hasta v1.0, con sprints ejecutables y criterios de éxito claros.

---

## Estado Actual: Fase 0 Completa ✅

**Completado**:
- ✅ Estructura de proyectos (backend, frontend)
- ✅ Base de datos PostgreSQL configurada
- ✅ Docker Compose funcional
- ✅ Backend NestJS con arquitectura modular
- ✅ Autenticación JWT implementada
- ✅ CRUD de series y capítulos implementado
- ✅ Progreso de lectura implementado
- ✅ Sistema de subida de imágenes con Sharp
- ✅ Swagger/OpenAPI documentado
- ✅ Frontend Next.js con TypeScript y Tailwind

**Próximo paso**: Fase 1 - Lector de Manga + Panel Admin

---

## Filosofía del Roadmap

Este roadmap sigue principios pragmáticos:

**1. Iterativo e Incremental**
- Cada sprint produce algo demostrable
- No hay "sprints de infraestructura" sin features visibles
- El proyecto siempre está en estado "semi-funcional"

**2. MVP Primero, Pulido Después**
- Funcionalidad básica → Testing → Pulido
- No perfeccionar antes de validar
- Refactoring continuo es esperado y saludable

**3. Documentación Concurrente**
- Documentar mientras se desarrolla, no después
- Código autodocumentado cuando sea posible
- READMEs actualizados en cada feature significativa

**4. Timeboxing Flexible**
- Estimados en semanas, no días
- Si un sprint se extiende, está bien — ajustar, no estresarse
- El objetivo es aprendizaje y calidad, no velocidad

---

## Stack Tecnológico

| Componente | Tecnología |
|---|---|
| Frontend | Next.js 16+ con TypeScript |
| Backend | NestJS 11+ con TypeScript |
| Base de Datos | PostgreSQL 17+ |
| ORM | Prisma |
| Autenticación | Passport + JWT |
| Procesamiento de imágenes | Sharp |
| Documentación API | Swagger/OpenAPI |
| Contenedorización | Docker & Docker Compose |

---

## Fase 1: Lector de Manga + Panel Admin

**Objetivo**: Sistema funcional para leer manga y gestionar contenido desde interfaz web.

**Duración estimada**: 6-10 semanas  
**Prioridad**: Crítica (Core del producto)

### Sprint 1.1: Frontend - Listado y Detalle de Series (Semana 1-2)

**Objetivo**: Usuario puede navegar series disponibles en interfaz web.

**Tareas**:
- [ ] **Frontend - Configuración**
  - [ ] Setup de API client (Axios/Fetch)
  - [ ] Configurar variables de entorno (NEXT_PUBLIC_API_URL)
  - [ ] Generar tipos TypeScript desde Prisma schema o Swagger
- [ ] **Frontend - Componentes Base**
  - [ ] SeriesCard (componente reutilizable)
  - [ ] Button, Input, Card (componentes UI base)
- [ ] **Frontend - Páginas**
  - [ ] `app/(public)/page.tsx` - Homepage con listado de series
  - [ ] `app/(public)/series/[id]/page.tsx` - Detalle de serie
- [ ] **Frontend - State Management**
  - [ ] Custom hook: `useSeries`
  - [ ] Custom hook: `useSeriesDetail`
- [ ] **Backend - Ajustes**
  - [ ] Agregar campo genres/tags a Series (Prisma migration)
  - [ ] Endpoint de búsqueda por título (ya implementado con paginación)
- [ ] **Styling**
  - [ ] Layout responsive con TailwindCSS
  - [ ] Cards de series visuales y atractivos
  - [ ] Loading states y error handling UI

**Criterios de Éxito**:
- ✅ Homepage muestra todas las series con portadas
- ✅ Click en serie lleva a página de detalle
- ✅ Detalle muestra info de serie + lista de capítulos
- ✅ Interfaz se ve profesional en mobile y desktop

**Estimado**: 1.5-2 semanas

---

### Sprint 1.2: Lector de Manga (Semana 3-5)

**Objetivo**: Lector funcional, visualmente profesional, con navegación fluida.

**Tareas**:
- [ ] **Frontend - Componentes del Lector**
  - [ ] MangaReader (componente principal)
  - [ ] PageViewer (renderiza imagen actual)
  - [ ] ChapterNavigation (botones anterior/siguiente/dropdown)
  - [ ] ProgressIndicator (página actual / total)
- [ ] **Frontend - Lógica del Lector**
  - [ ] Custom hook: `useReader` (gestiona estado de lectura)
  - [ ] Navegación con teclado (flechas, space)
  - [ ] (Opcional) Navegación con swipe en mobile
  - [ ] Precarga de próxima imagen
- [ ] **Frontend - UI/UX del Lector**
  - [ ] Modo fullscreen (ocultar header/footer)
  - [ ] Controles overlay (aparecen en hover/tap)
  - [ ] Indicador visual de página cargando
  - [ ] Animaciones suaves entre páginas
- [ ] **Frontend - Página del Lector**
  - [ ] `app/(public)/series/[id]/chapter/[chapterId]/page.tsx`
- [ ] **Backend - API para Lector**
  - [ ] `GET /api/chapters/{id}/pages` - Listar páginas de capítulo (ya existe)
  - [ ] Optimizar query (evitar N+1)
- [ ] **Testing**
  - [ ] Test de navegación entre páginas
  - [ ] Test de precarga de imágenes
  - [ ] Test de teclado shortcuts
- [ ] **Documentación**
  - [ ] GIF/video demo del lector funcionando

**Criterios de Éxito**:
- ✅ Usuario puede leer un capítulo completo sin fricción
- ✅ Navegación con teclado funciona perfectamente
- ✅ Interfaz se ve profesional y moderna
- ✅ Performance es fluida (transiciones <500ms)
- ✅ Funciona bien en mobile

**Estimado**: 2-3 semanas

**Nota**: Este es el showcase principal del proyecto. Invertir tiempo en UX aquí vale la pena.

---

### Sprint 1.3: Pulido de Fase 1 (Semana 6-7)

**Objetivo**: Fase 1 lista para demo, sin bugs críticos.

**Tareas**:
- [ ] **Bug Fixes**
  - [ ] Revisar y corregir bugs encontrados
  - [ ] Validar edge cases (series sin capítulos, capítulos sin páginas)
- [ ] **Performance**
  - [ ] Optimizar queries lentas (usar Prisma `EXPLAIN`)
  - [ ] Verificar que imágenes se sirven eficientemente
- [ ] **Testing Adicional**
  - [ ] Agregar tests de casos edge que faltaron
  - [ ] Verificar cobertura >60% en servicios críticos
- [ ] **Documentación**
  - [ ] Actualizar README con screenshots/GIFs
  - [ ] Verificar Swagger docs actualizados
- [ ] **UX Refinement**
  - [ ] Mejorar mensajes de error
  - [ ] Agregar loading states donde faltan
  - [ ] Verificar responsive en diferentes dispositivos

**Criterios de Éxito**:
- ✅ Fase 1 es demostrable sin errores
- ✅ Lector se siente pulido
- ✅ Documentación está actualizada

**Estimado**: 1-2 semanas

---

**Checkpoint Fase 1**: 
- Demo grabado del lector funcionando
- Git tag: `v0.5-alpha`
- GitHub Release con notas de lo completado

---

## Fase 2: Autenticación & Panel Admin

**Objetivo**: Sistema de usuarios, roles y panel administrativo funcional.

**Duración estimada**: 4-8 semanas  
**Prioridad**: Alta (Control de la plataforma)

### Sprint 2.1: Autenticación Frontend (Semana 8-9)

**Objetivo**: Usuarios pueden registrarse y hacer login desde la UI.

**Nota**: El backend de auth ya está implementado (JWT, bcrypt, Passport).

**Tareas**:
- [ ] **Frontend - Auth Context**
  - [ ] AuthContext (gestiona usuario actual y token)
  - [ ] AuthProvider (wrap de la app)
  - [ ] Custom hook: `useAuth`
- [ ] **Frontend - Componentes**
  - [ ] LoginForm
  - [ ] RegisterForm
- [ ] **Frontend - Páginas**
  - [ ] `app/(auth)/login/page.tsx`
  - [ ] `app/(auth)/register/page.tsx`
- [ ] **Frontend - Persistencia**
  - [ ] Guardar JWT en localStorage (o cookie httpOnly en v1.1)
  - [ ] Incluir token en headers de requests (Authorization: Bearer)
- [ ] **Frontend - Protected Routes**
  - [ ] Middleware para verificar autenticación
  - [ ] Redirect a /login si no autenticado
- [ ] **Backend - Ajustes**
  - [ ] Endpoint `GET /api/auth/me` - Usuario actual
  - [ ] Role guards para endpoints protegidos

**Criterios de Éxito**:
- ✅ Usuario puede registrarse desde UI
- ✅ Usuario puede hacer login desde UI
- ✅ Token persiste entre sesiones
- ✅ Rutas protegidas redirigen a login si no autenticado

**Estimado**: 1.5-2 semanas

---

### Sprint 2.2: Sistema de Roles y Autorización (Semana 10)

**Objetivo**: Roles (Admin, Moderator, Reader) con permisos diferenciados.

**Nota**: El enum UserRole ya existe en Prisma.

**Tareas**:
- [ ] **Backend - Guards**
  - [ ] `RolesGuard` para verificar permisos por rol
  - [ ] Decorador `@Roles()` para endpoints
  - [ ] Verificación de roles en controllers
- [ ] **Backend - Application**
  - [ ] `UpdateUserRoleUseCase` (solo Admin)
  - [ ] Modificar endpoints existentes para verificar permisos
- [ ] **Frontend**
  - [ ] Mostrar/ocultar funcionalidades según rol
  - [ ] Verificar permisos antes de mostrar botones de admin
- [ ] **Testing**
  - [ ] Test: Reader no puede crear series
  - [ ] Test: Moderator puede crear series
  - [ ] Test: Solo Admin puede cambiar roles

**Criterios de Éxito**:
- ✅ Reader solo puede leer
- ✅ Moderator puede crear contenido
- ✅ Admin tiene control total
- ✅ Intentos de acceso no autorizado son rechazados

**Estimado**: 1 semana

---

### Sprint 2.3: Panel Administrativo - Gestión de Series (Semana 11-12)

**Objetivo**: Admin puede crear, editar y eliminar series desde UI.

**Tareas**:
- [ ] **Frontend - Layout Admin**
  - [ ] `app/(protected)/admin/layout.tsx`
  - [ ] Sidebar con navegación (Dashboard, Series, Chapters)
- [ ] **Frontend - Páginas Admin**
  - [ ] `app/(protected)/admin/page.tsx` - Dashboard (stats básicos)
  - [ ] `app/(protected)/admin/series/page.tsx` - Listado de series
  - [ ] `app/(protected)/admin/series/new/page.tsx` - Crear serie
  - [ ] `app/(protected)/admin/series/[id]/edit/page.tsx` - Editar serie
- [ ] **Frontend - Componentes Admin**
  - [ ] SeriesForm (crear/editar)
  - [ ] SeriesTable (con acciones: edit, delete)
  - [ ] ConfirmDialog (confirmar eliminación)
- [ ] **Backend - API**
  - [ ] `PATCH /api/series/{id}` - Actualizar serie (ya existe)
  - [ ] `DELETE /api/series/{id}` - Eliminar serie (ya existe)
  - [ ] Proteger endpoints con RolesGuard
- [ ] **Testing**
  - [ ] Test de edición de serie
  - [ ] Test de eliminación en cascada (chapters, pages)

**Criterios de Éxito**:
- ✅ Admin puede crear serie con formulario
- ✅ Admin puede editar serie existente
- ✅ Admin puede eliminar serie (con confirmación)
- ✅ UI de admin es funcional y clara

**Estimado**: 1.5-2 semanas

---

### Sprint 2.4: Panel Administrativo - Gestión de Capítulos e Imágenes (Semana 13-14)

**Objetivo**: Admin puede crear capítulos y subir imágenes desde UI.

**Tareas**:
- [ ] **Frontend - Páginas Admin**
  - [ ] `app/(protected)/admin/chapters/new/page.tsx` - Crear capítulo
  - [ ] `app/(protected)/admin/chapters/[id]/edit/page.tsx` - Editar capítulo
- [ ] **Frontend - Componentes Admin**
  - [ ] ChapterForm (crear/editar)
  - [ ] ImageUploader (drag & drop, preview)
  - [ ] ImageGallery (reordenar páginas)
- [ ] **Backend - API**
  - [ ] `POST /api/chapters/{id}/pages` - Subir páginas (multipart)
  - [ ] `DELETE /api/pages/{id}` - Eliminar página
  - [ ] Reordenar páginas (update page_number)
- [ ] **Backend - Uploads**
  - [ ] Mejorar servicio de uploads para batch upload
  - [ ] Validación de formatos y tamaños
- [ ] **Testing**
  - [ ] Test de upload múltiple de imágenes
  - [ ] Test de reordenamiento de páginas

**Criterios de Éxito**:
- ✅ Admin puede crear capítulo con imágenes
- ✅ Drag & drop funciona correctamente
- ✅ Preview de imágenes antes de subir
- ✅ Reordenamiento de páginas es intuitivo

**Estimado**: 1.5-2 semanas

---

### Sprint 2.5: Panel Administrativo - Gestión de Usuarios (Semana 15)

**Objetivo**: Admin puede gestionar usuarios y roles.

**Tareas**:
- [ ] **Frontend - Páginas Admin**
  - [ ] `app/(protected)/admin/users/page.tsx` - Listado de usuarios
- [ ] **Frontend - Componentes Admin**
  - [ ] UsersTable (con acciones: cambiar rol, eliminar)
  - [ ] RoleSelector (dropdown para cambiar rol)
- [ ] **Backend - API**
  - [ ] `GET /api/users` - Listar usuarios (solo Admin)
  - [ ] `PUT /api/users/{id}/role` - Cambiar rol
  - [ ] `DELETE /api/users/{id}` - Eliminar usuario
- [ ] **Backend - Application**
  - [ ] `GetUsersListUseCase`
  - [ ] `DeleteUserUseCase`
- [ ] **Testing**
  - [ ] Test: Solo Admin puede listar usuarios
  - [ ] Test: Cambio de rol persiste correctamente

**Criterios de Éxito**:
- ✅ Admin ve lista de todos los usuarios
- ✅ Admin puede cambiar rol de cualquier usuario
- ✅ Admin puede eliminar usuarios

**Estimado**: 1 semana

---

### Sprint 2.6: Progreso de Lectura (Semana 16)

**Objetivo**: Usuarios registrados pueden guardar progreso de lectura.

**Nota**: El backend de reading progress ya está implementado.

**Tareas**:
- [ ] **Frontend - Lector**
  - [ ] Modificar useReader para guardar progreso automáticamente
  - [ ] Mostrar indicador de "continuar leyendo" en homepage
  - [ ] Integrar con endpoint `POST /api/reader/progress`
- [ ] **Frontend - Profile**
  - [ ] `app/(protected)/profile/page.tsx` - Ver historial de lectura
  - [ ] Mostrar series leídas con última página
- [ ] **Backend - Ajustes**
  - [ ] Endpoint `GET /api/reader/progress` (ya existe)
  - [ ] Agregar endpoint para "última serie leída"
- [ ] **Testing**
  - [ ] Test: Progreso se guarda al cambiar página
  - [ ] Test: Usuario vuelve y continúa donde quedó

**Criterios de Éxito**:
- ✅ Usuario lee capítulo, progreso se guarda automáticamente
- ✅ Usuario cierra sesión y vuelve, continúa donde quedó
- ✅ Homepage muestra "continuar leyendo" correctamente

**Estimado**: 1 semana

---

**Checkpoint Fase 2**: 
- Sistema completo de usuarios funcionando
- Panel admin demostrable
- Git tag: `v0.8-beta`
- GitHub Release

---

## Fase 3: Pulido & Profesionalización (v1.0)

**Objetivo**: Transformar MVP funcional en v1.0 lista para producción.

**Duración estimada**: 3-6 semanas  
**Prioridad**: Alta (Calidad para lanzamiento)

### Sprint 3.1: Búsqueda y Filtrado (Semana 17-18)

**Objetivo**: Usuarios pueden buscar y filtrar series fácilmente.

**Tareas**:
- [ ] **Backend - Search**
  - [ ] Implementar búsqueda por título (Prisma `contains` case-insensitive)
  - [ ] (Opcional v1.1) Full-text search con pg_trgm
  - [ ] Endpoint: `GET /api/series?search=one+piece` (ya existe)
- [ ] **Backend - Filtering**
  - [ ] Agregar campo genres a Series (Prisma migration)
  - [ ] Endpoint: `GET /api/series?genre=shonen`
- [ ] **Frontend - Search UI**
  - [ ] Barra de búsqueda en header
  - [ ] Página de resultados de búsqueda
  - [ ] Filtros por género (checkboxes)
- [ ] **Testing**
  - [ ] Test de búsqueda (encuentra resultados correctos)
  - [ ] Test de filtrado por género

**Criterios de Éxito**:
- ✅ Usuario puede buscar series por título
- ✅ Usuario puede filtrar por género
- ✅ Búsqueda es suficientemente rápida (<500ms)

**Estimado**: 1.5-2 semanas

---

### Sprint 3.2: Optimización de Performance (Semana 19)

**Objetivo**: Aplicación es rápida y eficiente.

**Tareas**:
- [ ] **Backend - DB Optimization**
  - [ ] Revisar queries lentas (usar Prisma `EXPLAIN`)
  - [ ] Agregar índices faltantes
  - [ ] Optimizar relaciones con `include` selectivo
- [ ] **Backend - Caching**
  - [ ] Implementar Redis cache para listado de series
  - [ ] Cache de imágenes procesadas
- [ ] **Frontend - Performance**
  - [ ] Lazy loading de imágenes (Next.js Image)
  - [ ] Code splitting de páginas pesadas
  - [ ] Optimizar bundle size (analizar con @next/bundle-analyzer)
- [ ] **Testing**
  - [ ] Lighthouse audit (objetivo: >70 mobile, >85 desktop)
  - [ ] Load testing básico (opcional)

**Criterios de Éxito**:
- ✅ Lighthouse score >70 en mobile
- ✅ Queries principales <200ms
- ✅ Homepage carga en <2 segundos

**Estimado**: 1 semana

---

### Sprint 3.3: Documentación y Deployment (Semana 20-21)

**Objetivo**: Proyecto está listo para que otros lo instalen y usen.

**Tareas**:
- [ ] **Documentación de API**
  - [ ] Verificar Swagger/OpenAPI completo (ya configurado)
  - [ ] Anotar todos los endpoints con descripciones
  - [ ] Swagger UI accesible en `/api/docs`
- [ ] **Documentación de Usuario**
  - [ ] Crear INSTALL.md detallado
  - [ ] Guía de uso del panel admin
  - [ ] Troubleshooting común
- [ ] **Deployment**
  - [ ] Optimizar Dockerfiles (multi-stage builds, ya implementado)
  - [ ] Crear docker-compose.prod.yml
  - [ ] Script de setup inicial (crear usuario admin)
  - [ ] Documentar variables de entorno
- [ ] **Seguridad**
  - [ ] Revisar headers de seguridad (Helmet, ya configurado)
  - [ ] Configurar rate limiting (@nestjs/throttler)
  - [ ] Sanitización de inputs
- [ ] **CI/CD**
  - [ ] GitHub Actions: build y tests automáticos
  - [ ] (Opcional) Deploy automático a staging

**Criterios de Éxito**:
- ✅ Alguien externo puede instalar siguiendo INSTALL.md
- ✅ API está documentada con Swagger
- ✅ Docker images están optimizados
- ✅ CI pipeline pasa en cada push

**Estimado**: 1.5-2 semanas

---

### Sprint 3.4: Testing Final y Bug Fixes (Semana 22)

**Objetivo**: v1.0 sin bugs conocidos, coverage razonable.

**Tareas**:
- [ ] **Testing Exhaustivo**
  - [ ] Revisar cobertura de tests (objetivo >60%)
  - [ ] Agregar tests faltantes en áreas críticas
  - [ ] Tests E2E de flujos principales (opcional con Playwright)
- [ ] **Bug Hunting**
  - [ ] Probar en diferentes browsers (Chrome, Firefox, Safari)
  - [ ] Probar en mobile real (iOS y Android)
  - [ ] Registrar y corregir bugs encontrados
- [ ] **UX Final**
  - [ ] Revisar mensajes de error (claros y útiles)
  - [ ] Verificar animaciones y transiciones
  - [ ] Accessibility audit básico
- [ ] **Code Review**
  - [ ] Revisar código propio (refactoring menor)
  - [ ] Eliminar TODOs y código comentado
  - [ ] Verificar consistencia de estilo

**Criterios de Éxito**:
- ✅ No hay bugs críticos conocidos
- ✅ Cobertura de tests >60%
- ✅ Funciona en major browsers y mobile

**Estimado**: 1 semana

---

### Sprint 3.5: Release v1.0 (Semana 23)

**Objetivo**: Lanzamiento oficial de v1.0.

**Tareas**:
- [ ] **Preparación de Release**
  - [ ] Changelog completo (CHANGELOG.md)
  - [ ] README actualizado con screenshots/GIFs
  - [ ] LICENSE verificado (Apache 2.0)
  - [ ] CODE_OF_CONDUCT.md
- [ ] **Git & GitHub**
  - [ ] Tag: `v1.0.0`
  - [ ] GitHub Release con binaries (Docker images)
  - [ ] GitHub Discussions habilitado
- [ ] **Comunicación**
  - [ ] Post en LinkedIn/Twitter sobre el lanzamiento
  - [ ] (Opcional) Post en Reddit r/selfhosted
  - [ ] (Opcional) Submit a Hacker News Show HN
- [ ] **Post-Launch**
  - [ ] Monitorear issues reportados
  - [ ] Responder preguntas de usuarios

**Criterios de Éxito**:
- ✅ v1.0.0 está etiquetado en GitHub
- ✅ Release notes están publicados
- ✅ Proyecto es público y accesible

**Estimado**: Preparación ~3 días, lanzamiento 1 día

---

**Checkpoint v1.0**: 
- 🎉 Mangalith v1.0 lanzado oficialmente
- Git tag: `v1.0.0`
- GitHub Release público
- Proyecto listo para portfolio y contribuciones

---

## Métricas de Éxito del Proyecto

### Métricas Técnicas

| Métrica | Objetivo v1.0 | Método de Medición |
|---|---|---|
| Cobertura de tests | >60% en lógica crítica | Jest coverage |
| Lighthouse Score (Mobile) | >70 | Chrome DevTools |
| Lighthouse Score (Desktop) | >85 | Chrome DevTools |
| Tiempo de carga homepage | <2 segundos | Network tab |
| Tiempo de transición entre páginas | <500ms | User experience |
| API response time (p95) | <200ms | Logs/monitoring |

### Métricas de Calidad

| Aspecto | Criterio | Verificación |
|---|---|---|
| Documentación | Completa y actualizada | README claro, INSTALL funciona |
| Código limpio | Legible y mantenible | Code review propio |
| Seguridad | OWASP Top 10 considerado | Security checklist |
| Accesibilidad | Básica (WCAG AA) | Lighthouse audit |
| Responsive | Mobile, Tablet, Desktop | Test en dispositivos reales |

### Métricas de Aprendizaje Personal

| Objetivo | Evidencia |
|---|---|
| Dominio de NestJS | Código implementado con módulos, guards, interceptors |
| Experiencia Full-Stack Moderna | Proyecto funcionando end-to-end |
| Prácticas DevOps | CI/CD funcional, Docker deployment |
| Portfolio Destacable | Proyecto demostrable en entrevistas |

---

## Gestión de Tiempo y Expectativas

### Estimado Total

- **Fase 1**: 6-10 semanas
- **Fase 2**: 4-8 semanas
- **Fase 3**: 3-6 semanas
- **Total**: 13-24 semanas (3-6 meses)

### Realidad del Side Project

**Disponibilidad realista**:
- Semanas productivas: 10-15 horas
- Semanas ocupadas: 5-8 horas
- Semanas sin avance: ocurrirán, está bien

**Promedio esperado**: ~10 horas/semana

**Timeline realista con variabilidad**:
- Optimista: 4 meses
- Realista: 5-6 meses
- Pesimista: 8 meses

**Importante**: No estresarse con timeline. El objetivo es aprendizaje y calidad, no velocidad.

---

## Estrategia de Commits y Versionado

### Convención de Commits

Seguir [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add series creation endpoint
fix: resolve image upload validation bug
docs: update API documentation
refactor: simplify UserService logic
test: add unit tests for CreateSeriesUseCase
chore: update dependencies
```

### Versionado Semántico

- **v0.x.y**: Pre-release (Fases 0-2)
- **v1.0.0**: Primera release estable
- **v1.x.y**: Patches y features menores post-v1.0

### Branches

- `main`: Código estable (protegido)
- `develop`: Integración de features (protegido)
- `feature/*`: Desarrollo de features
- `fix/*`: Bug fixes

**Workflow**:
1. Feature branch desde `develop`
2. Desarrollo + tests
3. PR a `develop` (self-review)
4. Merge a `develop`
5. Periódicamente, `develop` → `main` (release)

---

## Plan de Comunicación del Progreso

### GitHub

- **Issues**: Trackear bugs y features
- **Milestones**: Uno por fase
- **Projects**: Kanban board (opcional)
- **Releases**: Al finalizar cada fase

### Personal

- **Dev Log** (opcional): Blog posts sobre decisiones técnicas
- **Demos**: Videos cortos en LinkedIn/Twitter cada milestone
- **Portfolio**: Mantener sección actualizada

---

## Próximos Pasos Inmediatos

### Esta Semana

1. ✅ Revisar y aprobar documentación de planificación
2. ✅ Migrar backend de Java/Spring Boot a Node.js/NestJS
3. ⬜ Comenzar Sprint 1.1: Frontend - Listado de Series

### Este Mes

- Completar Sprint 1.1 (Frontend listado y detalle)
- Primera demo interna del frontend conectado al backend

### Este Trimestre

- Completar Fase 1
- Demo del lector funcionando
- Tag `v0.5-alpha`

---

## Notas Finales

### Para Mantener Motivación

- **Celebrar pequeños wins**: Cada feature completada es progreso
- **No comparar con otros**: Tu timeline es único
- **Documentar aprendizajes**: Lo que aprendes vale tanto como el código
- **Mostrar progreso**: Shares en redes generan feedback positivo

### Señales de que Debes Ajustar

- Sprint toma >3 semanas más de lo estimado → Simplificar feature
- Frustración constante → Pair programming virtual o pedir feedback
- Pérdida de interés → Trabajar en feature más motivante temporalmente

### Principio Fundamental

> "Hecho es mejor que perfecto. Perfecto puede venir después."

No esperar a que algo esté perfecto para avanzar. Iterar es parte del proceso.

---

**Última actualización**: Abril 2026  
**Estado actual**: Fase 0 Completa ✅, Backend NestJS implementado, Fase 1 en preparación  
**Próxima revisión**: Al completar cada sprint

Este roadmap es un documento vivo. Actualizar conforme avanzan los sprints y ajustar estimados según realidad.
