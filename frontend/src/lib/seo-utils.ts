import { SEOConfig } from './seo'

// Utility functions for generating SEO configurations for different page types

export function generateHomePageSEO(): SEOConfig {
  return {
    title: "Mangalith - Lector de Manga de Código Abierto",
    description: "Tu biblioteca personal de manga digital. Organiza, lee y administra tu colección de manga de forma sencilla y elegante. Plataforma de código abierto para lectores y grupos de scanlation.",
    keywords: [
      "manga", "lector manga", "biblioteca digital", "manga online", 
      "scanlation", "CBZ", "CBR", "manga reader", "código abierto",
      "biblioteca personal", "manga digital", "lector web"
    ],
    type: "website",
    url: "/"
  }
}

export function generateLibraryPageSEO(): SEOConfig {
  return {
    title: "Biblioteca de Manga - Mangalith",
    description: "Explora nuestra extensa biblioteca de manga digital. Encuentra tus series favoritas, descubre nuevos títulos y disfruta de la mejor experiencia de lectura online.",
    keywords: [
      "biblioteca manga", "catálogo manga", "manga online", "leer manga",
      "series manga", "manga digital", "colección manga"
    ],
    type: "website",
    url: "/library"
  }
}

export function generateSearchPageSEO(query?: string): SEOConfig {
  const baseTitle = "Buscar Manga - Mangalith"
  const title = query ? `Resultados para "${query}" - Mangalith` : baseTitle
  
  return {
    title,
    description: query 
      ? `Resultados de búsqueda para "${query}". Encuentra manga por título, autor, género y más en Mangalith.`
      : "Busca en nuestra extensa biblioteca de manga. Encuentra series por título, autor, género, año y más criterios.",
    keywords: [
      "buscar manga", "encontrar manga", "manga por género", "manga por autor",
      "búsqueda avanzada", "filtros manga"
    ],
    type: "website",
    url: query ? `/search?q=${encodeURIComponent(query)}` : "/search"
  }
}

export function generateMangaPageSEO(manga: {
  title: string
  description: string
  author?: string
  genres?: string[]
  status?: string
  slug: string
  image?: string
  chapters?: number
  rating?: number
  publishedDate?: string
}): SEOConfig {
  const keywords = [
    "manga", manga.title, "leer manga online", "manga digital"
  ]
  
  if (manga.author) keywords.push(manga.author)
  if (manga.genres) keywords.push(...manga.genres)
  
  return {
    title: `${manga.title} - Leer Manga Online | Mangalith`,
    description: `Lee ${manga.title} online en Mangalith. ${manga.description.substring(0, 120)}...`,
    keywords,
    type: "article",
    url: `/manga/${manga.slug}`,
    image: manga.image,
    authors: manga.author ? [manga.author] : undefined,
    tags: manga.genres,
    publishedTime: manga.publishedDate
  }
}

export function generateChapterPageSEO(manga: {
  title: string
  slug: string
}, chapter: {
  number: number
  title?: string
  publishedDate?: string
}): SEOConfig {
  const chapterTitle = chapter.title 
    ? `${manga.title} - Capítulo ${chapter.number}: ${chapter.title}`
    : `${manga.title} - Capítulo ${chapter.number}`
  
  return {
    title: `${chapterTitle} | Mangalith`,
    description: `Lee el Capítulo ${chapter.number} de ${manga.title} online en Mangalith. Experiencia de lectura optimizada para todos los dispositivos.`,
    keywords: [
      "manga", manga.title, `capítulo ${chapter.number}`, "leer manga online",
      "manga digital", "lector web"
    ],
    type: "article",
    url: `/manga/${manga.slug}/chapter/${chapter.number}`,
    publishedTime: chapter.publishedDate,
    section: "Manga Chapter"
  }
}

export function generateUserProfileSEO(user: {
  username: string
  displayName?: string
  bio?: string
  mangaCount?: number
}): SEOConfig {
  const displayName = user.displayName || user.username
  
  return {
    title: `${displayName} - Perfil de Usuario | Mangalith`,
    description: user.bio 
      ? `Perfil de ${displayName} en Mangalith. ${user.bio.substring(0, 120)}...`
      : `Perfil de ${displayName} en Mangalith. Descubre su biblioteca y actividad en la plataforma.`,
    keywords: [
      "perfil usuario", "biblioteca personal", displayName, "manga usuario",
      "colección manga", "actividad manga"
    ],
    type: "article",
    url: `/user/${user.username}`
  }
}

export function generateGenrePageSEO(genre: string): SEOConfig {
  return {
    title: `Manga de ${genre} - Mangalith`,
    description: `Descubre los mejores manga de ${genre} en Mangalith. Explora nuestra colección de series de ${genre.toLowerCase()} y encuentra tu próxima lectura favorita.`,
    keywords: [
      "manga", genre.toLowerCase(), `manga ${genre.toLowerCase()}`, "género manga",
      "series manga", "leer manga online"
    ],
    type: "website",
    url: `/genre/${genre.toLowerCase().replace(/\s+/g, '-')}`
  }
}

// Utility for generating breadcrumbs
export function generateBreadcrumbs(path: string): Array<{ name: string; url: string }> {
  const segments = path.split('/').filter(Boolean)
  const breadcrumbs = [{ name: 'Inicio', url: '/' }]
  
  let currentPath = ''
  
  for (const segment of segments) {
    currentPath += `/${segment}`
    
    // Map segments to readable names
    let name = segment
    switch (segment) {
      case 'library':
        name = 'Biblioteca'
        break
      case 'search':
        name = 'Buscar'
        break
      case 'manga':
        name = 'Manga'
        break
      case 'user':
        name = 'Usuario'
        break
      case 'genre':
        name = 'Género'
        break
      case 'chapter':
        name = 'Capítulo'
        break
      default:
        // Capitalize and replace hyphens with spaces
        name = segment.replace(/-/g, ' ').replace(/\b\w/g, l => l.toUpperCase())
    }
    
    breadcrumbs.push({ name, url: currentPath })
  }
  
  return breadcrumbs
}