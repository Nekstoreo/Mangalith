# Mangalith — Estrategia del Proyecto

> Definición de la arquitectura, modelo de negocio y filosofía de operación de Mangalith.

---

## 1. Filosofía del Proyecto

### Low Profile & Sostenibilidad
- Priorizar la supervivencia a largo plazo sobre el crecimiento agresivo.
- Costos operativos mínimos para reducir la dependencia de ingresos masivos.
- Evitar llamar la atención innecesaria de entidades legales o corporativas.

### Community First
- El sitio es una herramienta para la comunidad latina de manga.
- Transparencia sobre costos y limitaciones.
- Feedback directo con usuarios y grupos de traducción.

---

## 2. Modelo de Negocio

### Monetización Pasiva (No Invasiva)
- **Anuncios Discretos**: Banners pequeños o anuncios entre páginas/capítulos.
  - ❌ Prohibido: Pop-ups, interstitials, auto-play con sonido, contenido malicioso.
  - ✅ Permitido: Banners estáticos, patrocinios directos de grupos/tiendas.
- **Donaciones**: Criptomonedas (Bitcoin, etc.) para permitir apoyo anónimo.
  - Sin plataformas que requieran identidad real (PayPal, Stripe) para proteger al operador.

### Prohibiciones Estrictas
- ❌ **No Suscripciones**: No queremos manejar datos de pago recurrentes ni crear barreras de entrada.
- ❌ **No Venta de Datos**: El usuario no es el producto.
- ❌ **No Paywalls**: Todo el contenido es accesible gratuitamente.

---

## 3. Arquitectura de Imágenes (Regla de Oro)

### No Alojar Imágenes Propias
- **Mangalith es un Directorio/Agregador**, no un host de contenido.
- Nunca almacenar archivos de imágenes (.jpg, .png, .webp) en nuestros servidores.
- Solo guardar metadatos: Títulos, descripciones, IDs y **URLs externas**.

### Estrategia de Carga: Hotlinking Directo
- El frontend solicita imágenes directamente desde la fuente externa (ej. MangaDex, Imgchest, servidores de scanlators).
- **Beneficio**: Costo de ancho de banda = $0 para nosotros. El proveedor externo paga el tráfico.
- **CDN**: Aprovechar que fuentes como MangaDex usan Cloudflare para servir contenido rápido y optimizado.

### Política de Proxy
- ❌ **No usar Proxy por defecto**: No intermediar el tráfico de imágenes para no consumir recursos de nuestro VPS.
- ⚠️ **Excepción**: Solo considerar proxy bajo demanda si una fuente bloquea hotlinking y es crítica, priorizando siempre fuentes amigables.

---

## 4. Privacidad y Datos (Zero-Knowledge)

### Sin Registro de Usuarios
- No almacenar emails, contraseñas ni perfiles personales.
- Eliminar el riesgo de fugas de datos sensibles.

### Progreso Local
- El historial de lectura se guarda en el `localStorage` del navegador del usuario.
- Opcional: Exportar/Importar progreso via JSON para cambio de dispositivo.

### Analytics Anónimos
- Usar herramientas que no trackeen identidad (ej. Plausible, Umami, logs anonimizados).
- Métricas permitidas: Páginas vistas, capítulos populares, países (sin IPs completas).
- Métricas prohibidas: Identidad del usuario, comportamiento individual rastreable.

---

## 5. Manejo Legal

### Postura de Agregador
- Mangalith no crea ni posee el contenido. Solo indexa enlaces públicos.
- Términos de Servicio claros: "Plataforma de indexación de terceros".

### Proceso DMCA
- Formulario visible y accesible para reportes de copyright.
- Política de "Notice and Takedown": Retirar enlaces reportados válidamente en <24h.
- No confrontar, solo cumplir para mantener el hosting seguro.

### Infraestructura
- VPS estándar (inicialmente $5-10/mes).
- Si el riesgo aumenta, considerar proveedores "Offshore" o "Bulletproof" (Países Bajos, etc.), aunque con la estrategia actual de no-hosting, un VPS normal debería ser suficiente.

---

## 6. Stack Tecnológico (Resumen)

| Componente | Decisión | Razón |
|---|---|---|
| **Frontend** | Next.js | Performance, SEO, SSR/SSG. |
| **Backend** | NestJS | Arquitectura modular, mantenible. |
| **Base de Datos** | PostgreSQL | Robustez para metadatos y relaciones. |
| **Imágenes** | Hotlinking Externo | Costo cero, sin responsabilidad de storage. |
| **Hosting** | VPS Económico | Bajo perfil, costos mínimos. |

---

**Última actualización**: Abril 2026  
**Estado**: Documento vivo. Cualquier cambio en la estrategia debe discutirse y actualizarse aquí.
