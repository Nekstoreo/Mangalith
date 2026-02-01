<p align="center">
  <img src="./assets/logos/logo.png" alt="Logo de Mangalith" width="200" height="200"/>
</p>

# Mangalith

> Lector de manga open source completo y personalizable que democratiza la creación de plataformas de manga profesionales.

<div align="center">

[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
[![Java](https://img.shields.io/badge/Java-21+-ED8B00?logo=java&logoColor=white)](https://www.oracle.com/java/)
[![Spring Boot](https://img.shields.io/badge/Spring_Boot-3.x-6DB33F?logo=spring-boot&logoColor=white)](https://spring.io/projects/spring-boot)
[![Next.js](https://img.shields.io/badge/Next.js-15+-000000?logo=next.js&logoColor=white)](https://nextjs.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)

</div>

---

## Sobre el Proyecto

Mangalith es una plataforma web construida con una **interfaz visualmente moderna** que no sacrifica funcionalidad por estética. Cada componente está diseñado para ser intuitivo, responsivo y performante, brindando una experiencia de usuario pulida sin comprometer la estabilidad técnica.

El proyecto elimina las barreras que tradicionalmente han impedido que comunidades, grupos de traducción e individuos tengan su propio sitio web profesional dedicado a manga. Nace de la convicción de que no debería existir una brecha entre querer crear una plataforma de manga y poder hacerlo. Ya sea un archivo personal, un grupo de traducción o una comunidad completa, Mangalith se adapta a tus necesidades sin sacrificar ni calidad visual ni arquitectura técnica.

---

## Stack Tecnológico

| Componente | Tecnología |
|---|---|
| Frontend | Next.js 15+ con TypeScript |
| Backend | Spring Boot (Java 21+ LTS) |
| Base de Datos | PostgreSQL 15+ |
| ORM | Spring Data JPA / Hibernate |
| Contenedorización | Docker & Docker Compose |

---

## Características

### Lector Visualmente Moderno y Funcional
Un lector cuidadosamente diseñado con interfaz contemporánea que no sacrifica rendimiento. Incluye múltiples modos de visualización, navegación fluida entre capítulos y gestión de lectura persistente. La experiencia es tan pulida como rápida.

### Panel Administrativo Intuitivo
Interfaz limpia y moderna para subir, organizar y gestionar series, capítulos e imágenes sin requerir conocimiento técnico. Diseño responsive que funciona perfectamente en escritorio y tablet.

### Arquitectura Técnica Sólida
Plataforma completamente autónoma sin depender de servicios externos o APIs de terceros. **Control total** de datos y contenido, con una base técnica robusta que escala conforme creces.

### Escalabilidad desde el Inicio
Diseñada para crecer junto contigo. La arquitectura soporta desde un archivo personal hasta comunidades con miles de usuarios simultáneos sin perder elegancia visual ni rendimiento.

### Temas y Personalización Visual
Sistema de temas flexible que permite adaptar completamente la apariencia a tu marca. Los cambios se reflejan al instante en toda la plataforma, manteniendo coherencia visual en cada pantalla.

---

## Inicio Rápido

### Requisitos Previos

- Docker y Docker Compose
- Java 21+ (si ejecutas sin contenedores)
- Node.js 20+ (para desarrollo del frontend)
- PostgreSQL 15+ (si ejecutas sin contenedores)

### Instalación con Docker

La forma más rápida de comenzar:

```bash
git clone https://github.com/Nekstoreo/Mangalith.git
cd Mangalith
docker-compose up -d
```

Accede a la aplicación en **`http://localhost:3000`**

Las credenciales de administrador por defecto se proporcionan durante la instalación inicial.

### Instalación Local

Si prefieres ejecutar componentes localmente:

**Backend:**
```bash
cd backend
./mvnw spring-boot:run
```

**Frontend:**
```bash
cd frontend
npm install
npm run dev
```

**Base de datos:**
Configura una instancia de PostgreSQL y actualiza las variables de entorno con tus credenciales.

Para opciones avanzadas y configuración personalizada, consulta [INSTALL.md](./INSTALL.md).

---

## Uso

Una vez instalado, accede a la interfaz administrativa para:

- Crear series y capítulos
- Subir imágenes o archivos de manga
- Configurar metadatos y portadas
- Gestionar usuarios y permisos
- Personalizar la apariencia de tu plataforma

La documentación de usuario se encuentra en [USER_GUIDE.md](./docs/USER_GUIDE.md).

---

## Desarrollo y Contribuciones

### Flujo de Contribución

Las contribuciones son bienvenidas. Para contribuir:

1. Fork el repositorio
2. Crea una rama para tu funcionalidad (`git checkout -b feature/mi-funcionalidad`)
3. Realiza tus cambios y confirma (`git commit -m 'Agrega mi funcionalidad'`)
4. Envía un pull request

Esperamos que el código sea legible, bien documentado y que incluya tests cuando sea posible. Los pull requests serán revisados considerando calidad, alineación con el proyecto y adherencia a buenas prácticas.

### Configuración del Entorno de Desarrollo

```bash
# Clone y configure
git clone https://github.com/Nekstoreo/Mangalith.git
cd Mangalith

# Inicie los servicios de infraestructura
docker-compose -f docker-compose.dev.yml up -d

# Backend
cd backend && ./mvnw clean install

# Frontend
cd ../frontend && npm install
```

---

## Arquitectura

La plataforma está dividida en tres componentes principales:

**Backend API** — API REST construida con Spring Boot que gestiona la lógica de negocio, base de datos y autenticación.

**Frontend** — Aplicación de Next.js que proporciona la interfaz de usuario tanto para lectores como para administradores.

**Base de Datos** — PostgreSQL con esquema optimizado para consultas de lectura frecuentes.

La comunicación entre componentes es a través de APIs REST documentadas con OpenAPI/Swagger.

---

## Planes Futuros

El proyecto tiene una visión clara de evolución. Las próximas prioridades incluyen mejorar el rendimiento del lector para casos de uso masivo, expandir el sistema de metadatos para soportar cómics occidentales, e integrar notificaciones que mantengan a los usuarios informados de nuevos capítulos.

No hay un timeline específico asignado actualmente, ya que el desarrollo se ajusta según disponibilidad y feedback de la comunidad. Para detalles completos sobre características planeadas, versiones futuras y prioridades, consulta el [ROADMAP.md](./ROADMAP.md) dedicado.

---

## Comunidad y Soporte

| Canal | Descripción |
|---|---|
| **Issues** | [Reporte un problema](https://github.com/Nekstoreo/Mangalith/issues) |
| **Discussions** | [Únete a la conversación](https://github.com/Nekstoreo/Mangalith/discussions) |
| **Email** | [nestorg456k@outlook.com](mailto:nestorg456k@outlook.com) |

---

## Licencia y Conducta

Este proyecto está licenciado bajo **Apache License 2.0**. Consulta [LICENSE](./LICENSE) para más detalles.

Nos comprometemos a proporcionar un ambiente acogedor y respetuoso. Lee nuestro [Código de Conducta](./CODE_OF_CONDUCT.md) antes de participar.

---

## Agradecimientos

Este proyecto se construyó inspirándose en la pasión de comunidades de manga traducidas que merecen herramientas mejores. Gracias a cada persona que dedica tiempo a traducir y compartir historias.

---

<div align="center">

Hecho con dedicación para la comunidad manga.

</div>