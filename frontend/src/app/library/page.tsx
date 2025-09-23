"use client"

import { useState } from "react"
import { MainLayout } from "@/components/layout"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Badge } from "@/components/ui/badge"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import {
  BookOpen,
  Search,
  Grid3X3,
  List,
  Star,
  Upload,
  Plus
} from "lucide-react"
import Link from "next/link"

interface Manga {
  id: string
  title: string
  author: string
  description: string
  coverImage?: string
  status: 'ongoing' | 'completed' | 'hiatus' | 'cancelled'
  genres: string[]
  chapters: number
  lastReadChapter?: number
  rating: number
  isFavorite: boolean
}

export default function LibraryPage() {
  const [searchQuery, setSearchQuery] = useState('')
  const [sortBy, setSortBy] = useState('title')
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid')
  const [showFavorites, setShowFavorites] = useState(false)

  // Mock data - This would come from your API
  const mockManga: Manga[] = [
    {
      id: '1',
      title: 'One Piece',
      author: 'Eiichiro Oda',
      description: 'La aventura de Monkey D. Luffy y su tripulación en busca del tesoro legendario One Piece.',
      status: 'ongoing',
      genres: ['Aventura', 'Acción', 'Comedia'],
      chapters: 1090,
      lastReadChapter: 1050,
      rating: 4.8,
      isFavorite: true,
    },
    {
      id: '2',
      title: 'Attack on Titan',
      author: 'Hajime Isayama',
      description: 'Humanidad lucha contra titanes gigantes en un mundo post-apocalíptico.',
      status: 'completed',
      genres: ['Acción', 'Drama', 'Fantasía'],
      chapters: 139,
      lastReadChapter: 139,
      rating: 4.9,
      isFavorite: true,
    },
    {
      id: '3',
      title: 'Demon Slayer',
      author: 'Koyoharu Gotouge',
      description: 'La historia de Tanjiro Kamado y su hermana Nezuko convertida en demonio.',
      status: 'completed',
      genres: ['Acción', 'Sobrenatural', 'Drama'],
      chapters: 205,
      lastReadChapter: 180,
      rating: 4.7,
      isFavorite: false,
    },
  ]

  const filteredManga = mockManga.filter(manga => {
    const matchesSearch = manga.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
                         manga.author.toLowerCase().includes(searchQuery.toLowerCase())
    const matchesFavorites = !showFavorites || manga.isFavorite
    return matchesSearch && matchesFavorites
  })

  const getStatusColor = (status: Manga['status']) => {
    switch (status) {
      case 'ongoing': return 'bg-green-500'
      case 'completed': return 'bg-blue-500'
      case 'hiatus': return 'bg-yellow-500'
      case 'cancelled': return 'bg-red-500'
      default: return 'bg-gray-500'
    }
  }

  const getStatusText = (status: Manga['status']) => {
    switch (status) {
      case 'ongoing': return 'En Emisión'
      case 'completed': return 'Completado'
      case 'hiatus': return 'En Pausa'
      case 'cancelled': return 'Cancelado'
      default: return status
    }
  }

  return (
    <MainLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold">Mi Biblioteca</h1>
            <p className="text-muted-foreground">
              Organiza y administra tu colección de manga
            </p>
          </div>
          <div className="flex items-center space-x-2">
            <Link href="/upload">
              <Button>
                <Plus className="mr-2 h-4 w-4" />
                Subir Manga
              </Button>
            </Link>
          </div>
        </div>

        {/* Search and Filters */}
        <Card>
          <CardContent className="pt-6">
            <div className="flex flex-col md:flex-row gap-4">
              <div className="flex-1">
                <div className="relative">
                  <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground h-4 w-4" />
                  <Input
                    placeholder="Buscar por título o autor..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="pl-10"
                  />
                </div>
              </div>

              <div className="flex items-center space-x-2">
                <Select value={sortBy} onValueChange={setSortBy}>
                  <SelectTrigger className="w-40">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="title">Título</SelectItem>
                    <SelectItem value="author">Autor</SelectItem>
                    <SelectItem value="rating">Calificación</SelectItem>
                    <SelectItem value="chapters">Capítulos</SelectItem>
                  </SelectContent>
                </Select>

                <Button
                  variant={viewMode === 'grid' ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => setViewMode('grid')}
                >
                  <Grid3X3 className="h-4 w-4" />
                </Button>

                <Button
                  variant={viewMode === 'list' ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => setViewMode('list')}
                >
                  <List className="h-4 w-4" />
                </Button>

                <Button
                  variant={showFavorites ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => setShowFavorites(!showFavorites)}
                >
                  <Star className="h-4 w-4" />
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Library Content */}
        {filteredManga.length === 0 ? (
          <Card>
            <CardContent className="py-12">
              <div className="text-center">
                <BookOpen className="h-16 w-16 text-muted-foreground mx-auto mb-4" />
                <h3 className="text-lg font-semibold mb-2">
                  {searchQuery ? 'No se encontraron mangas' : 'Tu biblioteca está vacía'}
                </h3>
                <p className="text-muted-foreground mb-6">
                  {searchQuery
                    ? 'Intenta con otros términos de búsqueda'
                    : 'Comienza subiendo tu primer manga a la biblioteca'
                  }
                </p>
                {!searchQuery && (
                  <Link href="/upload">
                    <Button>
                      <Upload className="mr-2 h-4 w-4" />
                      Subir Primer Manga
                    </Button>
                  </Link>
                )}
              </div>
            </CardContent>
          </Card>
        ) : (
          <div className={
            viewMode === 'grid'
              ? 'grid gap-6 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4'
              : 'space-y-4'
          }>
            {filteredManga.map((manga) => (
              <Card key={manga.id} className="group hover:shadow-lg transition-shadow">
                <CardHeader className="pb-3">
                  <div className="aspect-[3/4] bg-muted rounded-md mb-3 flex items-center justify-center">
                    <BookOpen className="h-12 w-12 text-muted-foreground" />
                  </div>

                  <div className="flex items-start justify-between">
                    <div className="flex-1 min-w-0">
                      <CardTitle className="text-lg truncate group-hover:text-primary transition-colors">
                        {manga.title}
                      </CardTitle>
                      <CardDescription className="text-sm">
                        por {manga.author}
                      </CardDescription>
                    </div>

                    {manga.isFavorite && (
                      <Star className="h-5 w-5 text-yellow-500 fill-current flex-shrink-0 ml-2" />
                    )}
                  </div>
                </CardHeader>

                <CardContent className="pt-0">
                  <div className="space-y-3">
                    <div className="flex items-center gap-2">
                      <div className={`w-2 h-2 rounded-full ${getStatusColor(manga.status)}`} />
                      <span className="text-xs text-muted-foreground">
                        {getStatusText(manga.status)}
                      </span>
                    </div>

                    <div className="flex flex-wrap gap-1">
                      {manga.genres.slice(0, 3).map((genre) => (
                        <Badge key={genre} variant="secondary" className="text-xs">
                          {genre}
                        </Badge>
                      ))}
                      {manga.genres.length > 3 && (
                        <Badge variant="outline" className="text-xs">
                          +{manga.genres.length - 3}
                        </Badge>
                      )}
                    </div>

                    <div className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">
                        {manga.chapters} capítulos
                      </span>
                      <span className="font-medium">
                        ★ {manga.rating}
                      </span>
                    </div>

                    {manga.lastReadChapter && (
                      <div className="text-xs text-muted-foreground">
                        Último leído: Capítulo {manga.lastReadChapter}
                      </div>
                    )}

                    <div className="flex space-x-2 pt-2">
                      <Button size="sm" className="flex-1">
                        <BookOpen className="mr-1 h-3 w-3" />
                        Leer
                      </Button>
                      <Button size="sm" variant="outline">
                        <Star className={`h-3 w-3 ${manga.isFavorite ? 'fill-current' : ''}`} />
                      </Button>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>
    </MainLayout>
  )
}
