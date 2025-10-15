<p align="center">
  <img src="./assets/logos/logo.png" alt="Logo de Mangalith" width="200" height="200"/>
</p>

# <div align="center">Mangalith</div>

<div align="center">
  <h3>📖 Lector de manga open source completo y personalizable que democratiza la creación de plataformas de manga profesionales.</h3>
</div>

<div align="center">

  [![Next.js](https://img.shields.io/badge/Next.js-000000?style=for-the-badge&logo=next.js&logoColor=white)](https://nextjs.org/)
  [![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
  [![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
  [![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
  [![EF Core](https://img.shields.io/badge/EF_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://docs.microsoft.com/en-us/ef/core/)
  [![Docker](https://img.shields.io/badge/Docker-2CA5E0?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
</div>

---

## 📑 Tabla de Contenidos

- [Mangalith](#mangalith)
  - [📑 Tabla de Contenidos](#-tabla-de-contenidos)
  - [🌟 Visión del Proyecto](#-visión-del-proyecto)
  - [🎯 Filosofía](#-filosofía)
  - [🚀 Inicio Rápido](#-inicio-rápido)
  - [⚙️ Configuración de Entorno](#️-configuración-de-entorno)
    - [Variables de Entorno](#variables-de-entorno)
  - [🤝 Contribuciones](#-contribuciones)
  - [🛠️ Arquitectura](#️-arquitectura)
  - [📄 Licencia](#-licencia)

## 🌟 Visión del Proyecto

<div>

**Mangalith surge de la necesidad de democratizar la creación de plataformas de manga, eliminando las barreras técnicas que impiden a comunidades, grupos de traducción o individuos tener su propio sitio web profesional.**

</div>

## 🎯 Filosofía

<table>
  <tr>
    <td>
      <h3>💡 Simplicidad de Uso</h3>
      <ul>
        <li>Instalación en minutos, no horas</li>
        <li>Interfaz intuitiva para todos los niveles</li>
        <li>Sin experiencia en programación requerida</li>
      </ul>
    </td>
    <td>
      <h3>🔒 Independencia Tecnológica</h3>
      <ul>
        <li>Funciona completamente autónomo</li>
        <li>Sin servicios externos o APIs de terceros</li>
        <li>Control total de datos y contenido</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td>
      <h3>🎨 Flexibilidad Total</h3>
      <ul>
        <li>Desde bibliotecas personales hasta comunidades grandes</li>
        <li>Personalización visual y funcional completa</li>
        <li>Adaptable a cualquier escala y necesidad</li>
      </ul>
    </td>
    <td>
      <h3>🚀 Solución Turnkey</h3>
      <ul>
        <li>Plataforma completa lista para usar</li>
        <li>Sin desarrollo adicional requerido</li>
        <li>Deployment con un solo comando</li>
      </ul>
    </td>
  </tr>
</table>


## 🚀 Inicio Rápido

```bash
# Clonar el repositorio
git clone https://github.com/Nekstoreo/Mangalith.git
cd Mangalith

# Configurar variables de entorno
cp .env.example .env
cp backend/.env.example backend/.env
cp frontend/.env.example frontend/.env.local

# Iniciar base de datos
docker-compose up -d postgres

# Aplicar migraciones (primera vez)
cd backend/Mangalith.Api
dotnet ef database update --context MangalithDbContext --project ../Mangalith.Infrastructure
cd ../..

# Iniciar desarrollo
cd frontend && pnpm dev     # Para el frontend
cd backend/Mangalith.Api && dotnet run  # Para el backend

# 🌐 Frontend: http://localhost:3000
# 🔧 Backend: http://localhost:5000
# 🗄️ pgAdmin: http://localhost:5050 (opcional)
```

### Comandos Útiles

```bash
# Gestión de base de datos
docker-compose up -d postgres       # Iniciar PostgreSQL
docker-compose stop postgres        # Detener PostgreSQL
./backend/scripts/reset-database.sh # Resetear base de datos completa
docker-compose --profile admin up -d pgadmin  # Iniciar pgAdmin

# Desarrollo
cd frontend && pnpm dev              # Servidor de desarrollo frontend
cd backend/Mangalith.Api && dotnet run  # Servidor de desarrollo backend

# Construcción
cd frontend && pnpm build            # Construir frontend para producción
cd backend && dotnet build          # Construir backend

# Limpieza
docker-compose down -v              # Limpiar contenedores y volúmenes
```

## ⚙️ Configuración de Entorno

### Variables de Entorno

El proyecto utiliza archivos `.env` separados por servicio:

```bash
# Configuración de Docker/Infraestructura (raíz)
cp .env.example .env

# Configuración del Backend
cp backend/.env.example backend/.env

# Configuración del Frontend  
cp frontend/.env.example frontend/.env.local
```

**Variables por servicio:**

**Raíz (Docker Compose):**
- `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`: Configuración de PostgreSQL
- `PGADMIN_EMAIL`, `PGADMIN_PASSWORD`: Configuración de pgAdmin

**Backend:**
- `DATABASE_URL`: Cadena de conexión a PostgreSQL
- `JWT_SECRET_KEY`: Clave secreta para JWT (cambiar en producción)
- `ASPNETCORE_URLS`: URLs donde escucha el backend

**Frontend:**
- `NEXT_PUBLIC_API_URL`: URL de la API para el frontend

## 🤝 Contribuciones

<div align="center">

  [![Issues](https://img.shields.io/github/issues/Nekstoreo/Mangalith?style=for-the-badge)](https://github.com/Nekstoreo/Mangalith/issues)
  [![PRs](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=for-the-badge)](https://github.com/Nekstoreo/Mangalith/pulls)
  [![Discussions](https://img.shields.io/badge/Discussions-active-blue.svg?style=for-the-badge)](https://github.com/Nekstoreo/Mangalith/discussions)

</div>

¡Las contribuciones son siempre bienvenidas! Si quieres colaborar:

1. 🍴 Haz fork del proyecto
2. 🌿 Crea una rama para tu funcionalidad (`git checkout -b feature/nueva-funcionalidad`)
3. 📝 Realiza tus cambios y haz commit siguiendo [Conventional Commits](https://conventionalcommits.org/)
4. 📤 Sube tus cambios (`git push origin feature/nueva-funcionalidad`)
5. 🔄 Abre un Pull Request

Lee nuestras [Guías de Contribución](CONTRIBUTING.md) para más detalles.

## 🛠️ Arquitectura

- **Frontend**: Next.js 15 + TypeScript + Tailwind CSS
- **Backend**: .NET 9 + C# + Entity Framework Core
- **Base de Datos**: PostgreSQL con migraciones automáticas
- **Despliegue**: Docker (solo BD) + pnpm

## 📄 Licencia

Este proyecto está bajo la **Licencia Apache 2.0** que permite uso comercial y modificación libre. Consulta el archivo [LICENSE](LICENSE) para más detalles.

---
<div align="center">

  <i>🏗️ Actualmente en Desarrollo</i>
  <br/>
  <i>¿Te gusta este proyecto? Dale una ⭐️!</i>
  <br/>
  <i>Hecho con ❤️ para la comunidad.</i>
</div>

---
