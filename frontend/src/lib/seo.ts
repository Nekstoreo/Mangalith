import { Metadata } from 'next'

export interface SEOConfig {
  title: string
  description: string
  keywords?: string[]
  image?: string
  url?: string
  type?: 'website' | 'article' | 'book'
  publishedTime?: string
  modifiedTime?: string
  authors?: string[]
  section?: string
  tags?: string[]
}

const defaultSEO = {
  siteName: 'Mangalith',
  siteUrl: process.env.NEXT_PUBLIC_SITE_URL || 'https://mangalith.com',
  defaultTitle: 'Mangalith - Lector de Manga de Código Abierto',
  defaultDescription: 'Tu biblioteca personal de manga digital. Organiza, lee y administra tu colección de manga de forma sencilla y elegante. Plataforma de código abierto para lectores y grupos de scanlation.',
  defaultImage: '/images/og-default.jpg',
  twitterHandle: '@mangalith',
  locale: 'es_ES',
  alternateLocales: ['en_US'],
}

export function generateMetadata(config: SEOConfig): Metadata {
  const {
    title,
    description,
    keywords = [],
    image = defaultSEO.defaultImage,
    url,
    type = 'website',
    publishedTime,
    modifiedTime,
    authors = [],
    section,
    tags = []
  } = config

  const fullTitle = title === defaultSEO.defaultTitle 
    ? title 
    : `${title} | ${defaultSEO.siteName}`
  
  const fullUrl = url ? `${defaultSEO.siteUrl}${url}` : defaultSEO.siteUrl
  const fullImage = image.startsWith('http') ? image : `${defaultSEO.siteUrl}${image}`

  const metadata: Metadata = {
    title: fullTitle,
    description,
    keywords: keywords.length > 0 ? keywords.join(', ') : undefined,
    
    // Basic metadata
    robots: {
      index: process.env.NODE_ENV === 'production',
      follow: true,
      googleBot: {
        index: process.env.NODE_ENV === 'production',
        follow: true,
        'max-video-preview': -1,
        'max-image-preview': 'large',
        'max-snippet': -1,
      },
    },
    
    // Open Graph
    openGraph: {
      type,
      locale: defaultSEO.locale,
      alternateLocale: defaultSEO.alternateLocales,
      siteName: defaultSEO.siteName,
      title: fullTitle,
      description,
      url: fullUrl,
      images: [
        {
          url: fullImage,
          width: 1200,
          height: 630,
          alt: title,
        },
      ],
      ...(publishedTime && { publishedTime }),
      ...(modifiedTime && { modifiedTime }),
      ...(authors.length > 0 && { authors }),
      ...(section && { section }),
      ...(tags.length > 0 && { tags }),
    },
    
    // Twitter
    twitter: {
      card: 'summary_large_image',
      site: defaultSEO.twitterHandle,
      creator: defaultSEO.twitterHandle,
      title: fullTitle,
      description,
      images: [fullImage],
    },
    
    // Additional metadata
    alternates: {
      canonical: fullUrl,
      languages: {
        'es-ES': fullUrl,
        'en-US': fullUrl.replace('/es/', '/en/'),
      },
    },
    
    // App metadata
    applicationName: defaultSEO.siteName,
    generator: 'Next.js',
    referrer: 'origin-when-cross-origin',
    creator: 'Mangalith Team',
    publisher: 'Mangalith',
    
    // Format detection
    formatDetection: {
      email: false,
      address: false,
      telephone: false,
    },
  }

  return metadata
}

export function generateStructuredData(config: SEOConfig & { 
  breadcrumbs?: Array<{ name: string; url: string }> 
}) {
  const { title, description, image = defaultSEO.defaultImage, url, breadcrumbs } = config
  const fullUrl = url ? `${defaultSEO.siteUrl}${url}` : defaultSEO.siteUrl
  const fullImage = image.startsWith('http') ? image : `${defaultSEO.siteUrl}${image}`

  const structuredData: Record<string, unknown> = {
    '@context': 'https://schema.org',
    '@graph': [
      // Website
      {
        '@type': 'WebSite',
        '@id': `${defaultSEO.siteUrl}/#website`,
        url: defaultSEO.siteUrl,
        name: defaultSEO.siteName,
        description: defaultSEO.defaultDescription,
        potentialAction: [
          {
            '@type': 'SearchAction',
            target: {
              '@type': 'EntryPoint',
              urlTemplate: `${defaultSEO.siteUrl}/search?q={search_term_string}`,
            },
            'query-input': 'required name=search_term_string',
          },
        ],
      },
      
      // Organization
      {
        '@type': 'Organization',
        '@id': `${defaultSEO.siteUrl}/#organization`,
        name: defaultSEO.siteName,
        url: defaultSEO.siteUrl,
        logo: {
          '@type': 'ImageObject',
          url: `${defaultSEO.siteUrl}/images/logo.png`,
        },
        sameAs: [
          // Add social media URLs when available
        ],
      },
      
      // WebPage
      {
        '@type': 'WebPage',
        '@id': `${fullUrl}/#webpage`,
        url: fullUrl,
        name: title,
        description,
        isPartOf: {
          '@id': `${defaultSEO.siteUrl}/#website`,
        },
        about: {
          '@id': `${defaultSEO.siteUrl}/#organization`,
        },
        image: {
          '@type': 'ImageObject',
          url: fullImage,
        },
      },
    ],
  }

  // Add breadcrumbs if provided
  if (breadcrumbs && breadcrumbs.length > 0) {
    (structuredData['@graph'] as unknown[]).push({
      '@type': 'BreadcrumbList',
      itemListElement: breadcrumbs.map((crumb, index) => ({
        '@type': 'ListItem',
        position: index + 1,
        name: crumb.name,
        item: `${defaultSEO.siteUrl}${crumb.url}`,
      })),
    })
  }

  return structuredData
}

// Utility function to generate manga-specific structured data
export function generateMangaStructuredData(manga: {
  title: string
  description: string
  author?: string
  genre?: string[]
  status?: string
  image?: string
  url: string
  chapters?: number
  rating?: number
  publishedDate?: string
}) {
  const fullUrl = `${defaultSEO.siteUrl}${manga.url}`
  const fullImage = manga.image?.startsWith('http') 
    ? manga.image 
    : `${defaultSEO.siteUrl}${manga.image || defaultSEO.defaultImage}`

  return {
    '@context': 'https://schema.org',
    '@type': 'Book',
    name: manga.title,
    description: manga.description,
    url: fullUrl,
    image: fullImage,
    ...(manga.author && { author: { '@type': 'Person', name: manga.author } }),
    ...(manga.genre && { genre: manga.genre }),
    ...(manga.publishedDate && { datePublished: manga.publishedDate }),
    ...(manga.rating && { 
      aggregateRating: {
        '@type': 'AggregateRating',
        ratingValue: manga.rating,
        ratingCount: 1, // This should be actual count from database
      }
    }),
    ...(manga.chapters && { numberOfPages: manga.chapters }),
    bookFormat: 'EBook',
    inLanguage: 'es',
  }
}

export { defaultSEO }