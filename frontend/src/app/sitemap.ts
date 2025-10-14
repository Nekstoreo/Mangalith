import { MetadataRoute } from 'next'

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const baseUrl = process.env.NEXT_PUBLIC_SITE_URL || 'https://mangalith.com'
  
  // Static pages
  const staticPages: MetadataRoute.Sitemap = [
    {
      url: baseUrl,
      lastModified: new Date(),
      changeFrequency: 'daily',
      priority: 1,
    },
    {
      url: `${baseUrl}/library`,
      lastModified: new Date(),
      changeFrequency: 'daily',
      priority: 0.8,
    },
    {
      url: `${baseUrl}/search`,
      lastModified: new Date(),
      changeFrequency: 'weekly',
      priority: 0.7,
    },
    {
      url: `${baseUrl}/about`,
      lastModified: new Date(),
      changeFrequency: 'monthly',
      priority: 0.5,
    },
    {
      url: `${baseUrl}/contact`,
      lastModified: new Date(),
      changeFrequency: 'monthly',
      priority: 0.5,
    },
    {
      url: `${baseUrl}/privacy`,
      lastModified: new Date(),
      changeFrequency: 'yearly',
      priority: 0.3,
    },
    {
      url: `${baseUrl}/terms`,
      lastModified: new Date(),
      changeFrequency: 'yearly',
      priority: 0.3,
    },
  ]

  // TODO: Add dynamic pages when backend is ready
  // This will be implemented in later phases when we have:
  // - Manga series pages
  // - Chapter pages  
  // - User profiles (public ones)
  // - Category/genre pages
  
  /*
  // Example of how to add dynamic manga pages:
  try {
    const mangaResponse = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/manga/public`)
    const mangaList = await mangaResponse.json()
    
    const mangaPages: MetadataRoute.Sitemap = mangaList.map((manga: any) => ({
      url: `${baseUrl}/manga/${manga.slug}`,
      lastModified: new Date(manga.updatedAt),
      changeFrequency: 'weekly' as const,
      priority: 0.6,
    }))
    
    const chapterPages: MetadataRoute.Sitemap = []
    for (const manga of mangaList) {
      const chaptersResponse = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/manga/${manga.id}/chapters`)
      const chapters = await chaptersResponse.json()
      
      chapters.forEach((chapter: any) => {
        chapterPages.push({
          url: `${baseUrl}/manga/${manga.slug}/chapter/${chapter.number}`,
          lastModified: new Date(chapter.updatedAt),
          changeFrequency: 'monthly' as const,
          priority: 0.4,
        })
      })
    }
    
    return [...staticPages, ...mangaPages, ...chapterPages]
  } catch (error) {
    console.error('Error generating dynamic sitemap:', error)
    return staticPages
  }
  */

  return staticPages
}