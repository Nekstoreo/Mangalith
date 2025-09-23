# Guías de Contribución

¡Gracias por tu interés en contribuir a Mangalith! Todas las contribuciones son bienvenidas, ya sean reportes de bugs, mejoras de código, documentación o sugerencias de características.

## 📋 Código de Conducta

Por favor, lee nuestro [Código de Conducta](CODE_OF_CONDUCT.md) antes de contribuir. Todas las interacciones en este proyecto deben seguir estas guías para mantener un ambiente positivo y productivo.

## 🚀 Cómo Contribuir

### 1. Haz Fork del Proyecto

1. Ve al repositorio principal en GitHub
2. Haz clic en el botón "Fork" en la esquina superior derecha
3. Clona tu fork localmente:
   ```bash
   git clone https://github.com/Nekstoreo/Mangalith.git
   cd mangalith
   ```

### 2. Configura el Entorno de Desarrollo

1. Asegúrate de tener Docker y Docker Compose instalados
2. Copia el archivo de ejemplo de variables de entorno:
   ```bash
   cp .env.example .env
   ```
3. Inicia los servicios:
   ```bash
   docker-compose up -d
   ```

### 3. Crea una Rama para tu Contribución

```bash
git checkout -b feature/nombre-de-tu-caracteristica
# o
git checkout -b fix/descripcion-de-tu-fix
# o
git checkout -b docs/actualizacion-de-documentacion
```

### 4. Haz tus Cambios

- Sigue las convenciones de código existentes
- Asegúrate de que tu código sea legible y bien documentado
- Escribe tests si es apropiado
- Actualiza la documentación si es necesario

### 5. Commit tus Cambios

Usamos [Conventional Commits](https://conventionalcommits.org/) para mantener un historial limpio:

```bash
git commit -m "feat: añadir nueva funcionalidad de búsqueda"
git commit -m "fix: corregir error en la carga de imágenes"
git commit -m "docs: actualizar README con instrucciones de instalación"
```

**Tipos de commit:**
- `feat:` - Nueva característica
- `fix:` - Corrección de bug
- `docs:` - Cambios en documentación
- `style:` - Cambios de formato (sin lógica)
- `refactor:` - Refactorización de código
- `test:` - Añadir tests
- `chore:` - Mantenimiento del proyecto

### 6. Push y Crea un Pull Request

```bash
git push origin feature/nombre-de-tu-caracteristica
```

1. Ve a tu fork en GitHub
2. Haz clic en "Compare & pull request"
3. Describe tus cambios claramente
4. Espera la revisión del equipo

## 🎯 Prioridades de Desarrollo

Consulta nuestro [roadmap detallado](ROADMAP.md) para ver las características planificadas y el estado actual del proyecto.

### Características Principales en Desarrollo
- Sistema de carga de archivos de manga
- Lector básico con navegación fluida
- Gestión de usuarios y autenticación
- Sistema de búsqueda y filtrado

## 🐛 Reportar Bugs

Si encuentras un bug, por favor:

1. Revisa los [Issues existentes](https://github.com/Nekstoreo/Mangalith/issues) para ver si ya fue reportado
2. Si no existe, crea un nuevo Issue con:
   - Descripción clara del problema
   - Pasos para reproducirlo
   - Comportamiento esperado vs actual
   - Información del entorno (OS, navegador, etc.)
   - Capturas de pantalla si es relevante

## 💡 Sugerir Características

¡Nos encanta escuchar nuevas ideas! Para sugerir una característica:

1. Crea un Issue con el label "enhancement"
2. Describe claramente la funcionalidad propuesta
3. Explica por qué sería útil
4. Proporciona ejemplos de uso si es posible

## 🔧 Configuración de Desarrollo

### Requisitos
- Node.js 18+
- Docker y Docker Compose
- Git

### Scripts Disponibles
```bash
# Desarrollo con Docker
docker-compose up -d

# Instalación de dependencias (si trabajas directamente con Node.js)
npm install
# o
pnpm install
# o
yarn install

# Ejecutar en modo desarrollo
npm run dev

# Ejecutar tests
npm run test

# Linting
npm run lint
```

## 📝 Estilo de Código

- Usa TypeScript para todos los archivos
- Sigue las convenciones de ESLint
- Usa nombres descriptivos para variables y funciones
- Comenta el código complejo
- Mantén las líneas por debajo de 100 caracteres

## 🧪 Tests

- Escribe tests para nuevas funcionalidades
- Asegúrate de que los tests existentes pasen
- Usa Jest para testing unitario
- Considera testing de integración para características críticas

## 📚 Documentación

- Actualiza la documentación junto con el código
- Usa un lenguaje claro y conciso
- Incluye ejemplos cuando sea apropiado
- Mantén la documentación sincronizada con el código

## 🙏 Reconocimiento

¡Gracias por contribuir a Mangalith! Tu ayuda nos ayuda a crear una mejor plataforma para la comunidad de manga.

---

**¿Preguntas?** No dudes en abrir un [Discussion](https://github.com/Nekstoreo/Mangalith/discussions) si necesitas ayuda o tienes preguntas sobre cómo contribuir.
