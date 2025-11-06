'use client'

import { useEffect, useState } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { MangaList } from '@/components/manga/MangaCard'
import { useManga } from '@/hooks/useManga'
import { usePublication } from '@/hooks/usePublication'
import { Search, Plus, Upload } from 'lucide-react'
import Link from 'next/link'
import { Alert, AlertDescription } from '@/components/ui/alert'

export default function LibraryPage() {
  const { mangas, loading, error, getMyMangas, deleteManga, clearError } = useManga()
  const { submitForReview } = usePublication()
  const [searchTerm, setSearchTerm] = useState('')
  const [filteredMangas, setFilteredMangas] = useState(mangas)

  useEffect(() => {
    getMyMangas()
  }, [getMyMangas])

  useEffect(() => {
    if (searchTerm) {
      const filtered = mangas.filter(manga =>
        manga.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        manga.alternativeTitle?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        manga.author?.toLowerCase().includes(searchTerm.toLowerCase())
      )
      setFilteredMangas(filtered)
    } else {
      setFilteredMangas(mangas)
    }
  }, [mangas, searchTerm])

  const handleSubmitForReview = async (manga: any) => {
    try {
      if (manga.publication?.id) {
        await submitForReview(manga.publication.id)
        await getMyMangas() // Refresh the list
      }
    } catch (err) {
      console.error('Error submitting for review:', err)
    }
  }

  const handleDelete = async (manga: any) => {
    if (confirm(`¿Estás seguro de que quieres eliminar "${manga.title}"?`)) {
      try {
        await deleteManga(manga.id)
      } catch (err) {
        console.error('Error deleting manga:', err)
      }
    }
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="max-w-6xl mx-auto">
        <div className="mb-8">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h1 className="text-3xl font-bold mb-2">Mi Biblioteca</h1>
              <p className="text-muted-foreground">
                Gestiona tu colección personal de manga y sus publicaciones
              </p>
            </div>
            <Button asChild>
              <Link href="/upload">
                <Plus className="w-4 h-4 mr-2" />
                Subir Manga
              </Link>
            </Button>
          </div>

          {/* Search and filters */}
          <div className="flex gap-4 mb-6">
            <div className="relative flex-1 max-w-md">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground w-4 h-4" />
              <Input
                placeholder="Buscar en tu biblioteca..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
          </div>
        </div>

        {error && (
          <Alert className="mb-6 bg-red-50 border-red-200">
            <div className="flex justify-between items-center">
              <AlertDescription className="text-red-700">{error}</AlertDescription>
              <button
                onClick={clearError}
                className="text-red-700 hover:text-red-900 text-sm font-medium"
              >
                Cerrar
              </button>
            </div>
          </Alert>
        )}

        {loading ? (
          <MangaList mangas={[]} isLoading={true} />
        ) : filteredMangas.length > 0 ? (
          <div className="space-y-6">
            <div className="flex items-center justify-between">
              <h2 className="text-xl font-semibold">
                Tus Mangas ({filteredMangas.length})
              </h2>
            </div>
            
            <MangaList
              mangas={filteredMangas.map(manga => ({
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
              showActions={true}
              onSubmitForReview={handleSubmitForReview}
              onDelete={handleDelete}
            />
          </div>
        ) : (
          <Card>
            <CardHeader>
              <CardTitle>Biblioteca Personal</CardTitle>
              <CardDescription>
                Aquí aparecerán todos tus mangas subidos
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="text-center py-12">
                <Upload className="w-12 h-12 text-muted-foreground mx-auto mb-4" />
                <p className="text-muted-foreground mb-4">
                  {searchTerm 
                    ? `No se encontraron mangas que coincidan con "${searchTerm}"`
                    : 'Tu biblioteca está vacía. ¡Empieza subiendo algunos archivos!'
                  }
                </p>
                {!searchTerm && (
                  <Button asChild>
                    <Link href="/upload">
                      <Plus className="w-4 h-4 mr-2" />
                      Subir tu primer manga
                    </Link>
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