# Convenciones de Commits y Branching

Mangalith sigue convenciones estrictas para mantener un historial de commits limpio, profesional y fácil de navegar. Esto es crucial tanto para desarrollo personal como para visibilidad en portafolio.

---

## Estructura de Ramas

Las ramas siguen un patrón jerárquico basado en fases del roadmap:

```
feat/fase-x/caracteristica
├── feat/fase-0/arquitectura-hexagonal
├── feat/fase-0/docker-setup
├── feat/fase-1/modelo-datos-manga
├── feat/fase-1/lector-ui-moderna
├── feat/fase-2/autenticacion-jwt
└── feat/fase-2/panel-admin
```

### Nomenclatura

**Prefijo**: `feat/` (para features principales dentro de fases)
- También válidos: `fix/`, `refactor/`, `docs/`, `test/` si es necesario

**Fase**: `fase-0`, `fase-1`, `fase-2`, `fase-3`

**Característica**: nombre descriptivo en kebab-case (minúsculas, guiones, sin espacios)

### Ejemplo de Flujo

```bash
# Trabajar en una característica de Fase 1
git checkout -b feat/fase-1/lector-ui-moderna

# Commitear cambios (ver formato semántico abajo)
git commit -m "feat(frontend): implementar lector single-page"
git commit -m "fix(reader): corregir navegación con touch en mobile"

# Al terminar: Push y PR a main
git push origin feat/fase-1/lector-ui-moderna
# Crear Pull Request en GitHub
```

---

## Formato de Commits Semánticos

Todos los commits siguen **Conventional Commits** con estructura:

```
tipo(alcance): descripción breve
```

### Tipos

- **feat**: Nueva funcionalidad o feature
- **fix**: Corrección de bug
- **refactor**: Cambio de código que no agrega feature ni corrige bug
- **test**: Agregar o modificar tests
- **docs**: Cambios en documentación
- **chore**: Cambios en build, dependencias, config (no código de producto)
- **perf**: Mejoras de performance
- **ci**: Cambios en CI/CD

### Alcance

Especifica **qué área del código** afecta el commit:

**Backend**:
- `domain`: lógica de negocio
- `application`: casos de uso
- `infrastructure`: persistencia, external services
- `api`: endpoints REST

**Frontend**:
- `components`: componentes React
- `pages`: páginas
- `context`: state management
- `styles`: styling

**Transversal**:
- `docker`: configuración Docker
- `config`: configuración general
- `ci`: pipelines GitHub Actions

### Descripción

Imperativo, presente, conciso:
- ✅ "implementar validación de email"
- ✅ "agregar test para caso de uso login"
- ❌ "implementada validación" (pasado)
- ❌ "validaciones" (muy vago)
- ❌ "stuff" (sin significado)

---

## Ejemplos Realistas

### Fase 1: Gestión de Contenido

```bash
# Setup inicial
git commit -m "feat(domain): crear entidades Series, Chapter, Page"
git commit -m "feat(infrastructure): implementar repositorio de imágenes"
git commit -m "feat(application): crear use case UploadMangaUseCase"
git commit -m "test(domain): tests para lógica de validación de manga"
git commit -m "feat(api): endpoint POST /api/series para crear serie"

# Frontend del lector
git commit -m "feat(components): crear componente MangaReader"
git commit -m "feat(pages): página de lectura /series/:id/chapter/:chapterId/reader"
git commit -m "fix(reader): corregir scroll en mobile"
git commit -m "perf(components): optimizar re-renders de MangaReader con useMemo"
```

### Fase 2: Autenticación

```bash
git commit -m "feat(domain): crear entidad User con password hashing"
git commit -m "feat(application): crear use case RegisterUserUseCase"
git commit -m "test(application): tests para validación de contraseña fuerte"
git commit -m "feat(api): endpoint POST /api/auth/register"
git commit -m "feat(infrastructure): implementar JWT token generation"
git commit -m "refactor(api): extraer validación de usuario a Value Object"
```

### Mantenimiento

```bash
git commit -m "docs: actualizar ROADMAP con progreso de Fase 1"
git commit -m "chore(deps): actualizar Spring Boot a 3.2.1"
git commit -m "ci: agregar test stage a GitHub Actions"
```

---

## Merging a Main

Cuando termines una característica en su rama `feat/fase-x/...`:

1. **Asegúrate que tests pasen** localmente
2. **Rebase** si es necesario: `git rebase main`
3. **Push** a la rama: `git push origin feat/fase-x/caracteristica`
4. **Crea PR en GitHub** con descripción clara
5. **Merge a main** una vez validado

### Descripción de PR (Template opcional)

```markdown
## Descripción
Breve descripción de qué implementa

## Fase
Fase X del roadmap

## Cambios
- Cambio 1
- Cambio 2

## Testing
Cómo probaste esto localmente

## Notas
Cualquier cosa importante para revisar
```

---

## Resumen Rápido

| Aspecto | Regla |
|---|---|
| **Rama** | `feat/fase-x/descripcion-kebab-case` |
| **Commit** | `tipo(alcance): descripción imperativa` |
| **Tipos** | feat, fix, refactor, test, docs, chore, perf, ci |
| **Alcance** | domain, application, infrastructure, api, components, pages, etc. |
| **Descripción** | Presente, imperativo, conciso, sin punto final |
| **Frecuencia** | Atómico: un cambio lógico por commit |

---

Mantén estas convenciones desde el inicio. **Un historial limpio es una forma de respeto al proyecto — y a ti mismo.**
