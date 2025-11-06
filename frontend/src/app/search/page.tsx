'use client'

import { useEffect, useState } from 'react'
import { useSearchParams } from 'next/navigation'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { MangaList } from '@/components/manga/MangaCard'
import { useManga } from '@/hooks/useManga'
import { Search } from 'lucide-react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

export default function SearchPage() {
  const searchParams = useSearchParams()
  const initialQuery = searchParams.get('q') || ''
  
  const { mangas, loading, error, getPublicMangas, searchPublicMangas, clearError } = useManga()
  const [searchTerm, setSearchTerm] = useState(initialQuery)
  const [hasSearched, setHasSearched] = useState(false)

  useEffect(() => {
    if (initialQuery) {
      handleSearch(initialQuery)
    } else {
      // Load all public mangas initially
      getPublicMangas()
    }
  }, [initialQuery, getPublicMangas])

  const handleSearch = async (query: string = searchTerm) => {
    setHasSearched(true)
    if (query.trim()) {
      await searchPublicMangas(query.trim())
    } else {
      await getPublicMangas()
    }
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    handleSearch()
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-6xl mx-auto">
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-4">
            {hasSearched && searchTerm ? `Resultados para "${searchTerm}"` : 'Explorar Manga'}
          </h1>
          <p className="text-muted-foreground mb-6">
            {hasSearched && searchTerm 
              ? `Mostrando resultados de búsqueda para "${searchTerm}"`
              : 'Descubre manga publicado por nuestra comunidad'
            }
          </p>

          {/* Search Form */}
          <form onSubmit={handleSubmit} className="flex gap-4 max-w-2xl">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground w-4 h-4" />
              <Input
                placeholder="Buscar por título, autor, descripción..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
            <Button type="submit" disabled={loading}>
              {loading ? 'Buscando...' : 'Buscar'}
            </Button>
          </form>
        </div>

        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
            <div className="flex justify-between items-center">
              <p className="text-red-700">{error}</p>
              <button
                onClick={clearError}
                className="text-red-700 hover:text-red-900 text-sm font-medium"
              >
                Cerrar
              </button>
            </div>
          </div>
        )}

        {loading ? (
          <MangaList mangas={[]} isLoading={true} />
        ) : mangas.length > 0 ? (
          <div className="space-y-6">
            <div className="flex items-center justify-between">
              <h2 className="text-xl font-semibold">
                {hasSearched && searchTerm 
                  ? `${mangas.length} resultado${mangas.length !== 1 ? 's' : ''} encontrado${mangas.length !== 1 ? 's' : ''}`
                  : `Manga Disponible (${mangas.length})`
                }
              </h2>
            </div>
            
            <MangaList
              mangas={mangas.map(manga => ({
                id: manga.id,
                title: manga.title,
                alternativeTitle: manga.alternativeTitle,
                description: manga.description,
                author: manga.author,
                artist: manga.artist,
                year: manga.year,
                status: manga.status,
                coverImagePath: manga.coverImagePath,
                tags: manga.tags,
                genres: manga.genres,
                chapterCount: manga.chapterCount,
                viewCount: manga.viewCount,
                rating: manga.rating,
                ratingCount: manga.ratingCount,
                createdAtUtc: manga.createdAtUtc,
                updatedAtUtc: manga.updatedAtUtc,
                publicationStatus: manga.publicationStatus,
                contentRating: manga.contentRating,
                isNsfw: manga.isNsfw
              }))}
            />
          </div>
        ) : (
          <Card>
            <CardHeader>
              <CardTitle>
                {hasSearched && searchTerm ? 'Sin resultados' : 'Catálogo Público'}
              </CardTitle>
              <CardDescription>
                {hasSearched && searchTerm 
                  ? `No se encontraron mangas que coincidan con "${searchTerm}"`
                  : 'El catálogo público aparecerá aquí cuando los usuarios publiquen contenido'
                }
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="text-center py-12">
                <Search className="w-12 h-12 text-muted-foreground mx-auto mb-4" />
                <p className="text-muted-foreground mb-4">
                  {hasSearched && searchTerm 
                    ? 'Intenta con otros términos de búsqueda o explora todo el catálogo'
                    : 'Aún no hay manga publicado disponible para explorar'
                  }
                </p>
                {hasSearched && searchTerm && (
                  <Button 
                    variant="outline" 
                    onClick={() => {
                      setSearchTerm('')
                      setHasSearched(false)
                      getPublicMangas()
                    }}
                  >
                    Ver todo el catálogo
                  </Button>
                )}
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  )
}