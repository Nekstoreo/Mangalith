'use client'

import { Card, CardContent, CardHeader } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { PublicationStatusBadge } from '@/components/publication/PublicationStatus'
import { Eye, Edit, Trash2, Upload } from 'lucide-react'
import Image from 'next/image'
import Link from 'next/link'

export interface MangaData {
  id: string
  title: string
  alternativeTitle?: string
  description?: string
  author?: string
  artist?: string
  year?: number
  status: string
  coverImagePath?: string
  tags?: string
  genres?: string
  chapterCount: number
  viewCount: number
  rating: number
  ratingCount: number
  createdAtUtc: string
  updatedAtUtc: string
  publicationStatus?: string
  contentRating?: string
  isNsfw: boolean
}

interface MangaCardProps {
  manga: MangaData
  showActions?: boolean
  onEdit?: (manga: MangaData) => void
  onDelete?: (manga: MangaData) => void
  onSubmitForReview?: (manga: MangaData) => void
  className?: string
}

export function MangaCard({
  manga,
  showActions = false,
  onEdit,
  onDelete,
  onSubmitForReview,
  className
}: MangaCardProps) {
  const canSubmitForReview = manga.publicationStatus === 'Draft' || manga.publicationStatus === 'NeedsRevision'
  const canEdit = manga.publicationStatus === 'Draft' || manga.publicationStatus === 'NeedsRevision' || manga.publicationStatus === 'Rejected'

  return (
    <Card className={`overflow-hidden hover:shadow-lg transition-shadow ${className}`}>
      <div className="relative">
        {manga.coverImagePath ? (
          <Image
            src={manga.coverImagePath}
            alt={manga.title}
            width={300}
            height={400}
            className="w-full h-48 object-cover"
          />
        ) : (
          <div className="w-full h-48 bg-muted flex items-center justify-center">
            <span className="text-muted-foreground">Sin portada</span>
          </div>
        )}
        
        {manga.isNsfw && (
          <Badge variant="destructive" className="absolute top-2 right-2">
            NSFW
          </Badge>
        )}
        
        {manga.publicationStatus && (
          <div className="absolute top-2 left-2">
            <PublicationStatusBadge status={manga.publicationStatus as any} />
          </div>
        )}
      </div>

      <CardHeader className="pb-2">
        <div className="space-y-1">
          <h3 className="font-semibold text-lg line-clamp-2">
            <Link 
              href={`/manga/${manga.id}`}
              className="hover:text-primary transition-colors"
            >
              {manga.title}
            </Link>
          </h3>
          {manga.alternativeTitle && (
            <p className="text-sm text-muted-foreground line-clamp-1">
              {manga.alternativeTitle}
            </p>
          )}
        </div>
      </CardHeader>

      <CardContent className="space-y-3">
        {manga.description && (
          <p className="text-sm text-muted-foreground line-clamp-3">
            {manga.description}
          </p>
        )}

        <div className="flex flex-wrap gap-1 text-xs text-muted-foreground">
          {manga.author && (
            <span>Por: {manga.author}</span>
          )}
          {manga.year && (
            <span>• {manga.year}</span>
          )}
        </div>

        <div className="flex items-center justify-between text-sm">
          <div className="flex items-center gap-4">
            <span className="flex items-center gap-1">
              <Eye className="w-4 h-4" />
              {manga.viewCount.toLocaleString()}
            </span>
            <span>{manga.chapterCount} cap.</span>
            {manga.rating > 0 && (
              <span>★ {manga.rating.toFixed(1)}</span>
            )}
          </div>
          
          {manga.contentRating && (
            <Badge variant="outline" className="text-xs">
              {manga.contentRating}
            </Badge>
          )}
        </div>

        {showActions && (
          <div className="flex gap-2 pt-2 border-t">
            <Button
              variant="outline"
              size="sm"
              asChild
              className="flex-1"
            >
              <Link href={`/manga/${manga.id}`}>
                <Eye className="w-4 h-4 mr-1" />
                Ver
              </Link>
            </Button>
            
            {canEdit && onEdit && (
              <Button
                variant="outline"
                size="sm"
                onClick={() => onEdit(manga)}
              >
                <Edit className="w-4 h-4" />
              </Button>
            )}
            
            {canSubmitForReview && onSubmitForReview && (
              <Button
                variant="default"
                size="sm"
                onClick={() => onSubmitForReview(manga)}
              >
                <Upload className="w-4 h-4" />
              </Button>
            )}
            
            {onDelete && manga.publicationStatus !== 'Published' && (
              <Button
                variant="outline"
                size="sm"
                onClick={() => onDelete(manga)}
                className="text-destructive hover:text-destructive"
              >
                <Trash2 className="w-4 h-4" />
              </Button>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

interface MangaListProps {
  mangas: MangaData[]
  isLoading?: boolean
  showActions?: boolean
  onEdit?: (manga: MangaData) => void
  onDelete?: (manga: MangaData) => void
  onSubmitForReview?: (manga: MangaData) => void
  className?: string
}

export function MangaList({
  mangas,
  isLoading = false,
  showActions = false,
  onEdit,
  onDelete,
  onSubmitForReview,
  className
}: MangaListProps) {
  if (isLoading) {
    return (
      <div className={`grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 ${className}`}>
        {Array.from({ length: 8 }).map((_, i) => (
          <Card key={i} className="overflow-hidden">
            <div className="w-full h-48 bg-muted animate-pulse" />
            <CardHeader>
              <div className="h-4 bg-muted animate-pulse rounded" />
              <div className="h-3 bg-muted animate-pulse rounded w-2/3" />
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                <div className="h-3 bg-muted animate-pulse rounded" />
                <div className="h-3 bg-muted animate-pulse rounded" />
                <div className="h-3 bg-muted animate-pulse rounded w-1/2" />
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    )
  }

  if (mangas.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-muted-foreground">No se encontraron mangas</p>
      </div>
    )
  }

  return (
    <div className={`grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 ${className}`}>
      {mangas.map((manga) => (
        <MangaCard
          key={manga.id}
          manga={manga}
          showActions={showActions}
          onEdit={onEdit}
          onDelete={onDelete}
          onSubmitForReview={onSubmitForReview}
        />
      ))}
    </div>
  )
}