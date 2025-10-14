import Head from 'next/head'
import { SEOConfig, generateStructuredData } from '@/lib/seo'

interface SEOHeadProps extends SEOConfig {
  breadcrumbs?: Array<{ name: string; url: string }>
  structuredData?: Record<string, unknown>
}

export function SEOHead({ 
  title, 
  description, 
  image, 
  url, 
  breadcrumbs,
  structuredData,
  ...rest 
}: SEOHeadProps) {
  const defaultStructuredData = generateStructuredData({
    title,
    description,
    image,
    url,
    breadcrumbs,
    ...rest
  })

  const finalStructuredData = structuredData || defaultStructuredData

  return (
    <Head>
      {/* Additional meta tags that aren't covered by Next.js metadata */}
      <meta name="theme-color" content="#000000" />
      <meta name="msapplication-TileColor" content="#000000" />
      <meta name="apple-mobile-web-app-capable" content="yes" />
      <meta name="apple-mobile-web-app-status-bar-style" content="default" />
      <meta name="apple-mobile-web-app-title" content="Mangalith" />
      
      {/* Preconnect to external domains for performance */}
      <link rel="preconnect" href="https://fonts.googleapis.com" />
      <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="anonymous" />
      
      {/* Structured Data */}
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: JSON.stringify(finalStructuredData) }}
      />
    </Head>
  )
}

export default SEOHead