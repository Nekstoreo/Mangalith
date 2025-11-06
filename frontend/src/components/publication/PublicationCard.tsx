'use client'

import { Badge } from '@/components/ui/badge'
import { Publication, PublicationStatus } from '@/services/publication/client'

const statusConfig: Record<
  PublicationStatus,
  { label: string; variant: 'default' | 'secondary' | 'destructive' | 'outline' }
> = {
  [PublicationStatus.Draft]: { label: 'Borrador', variant: 'secondary' },
  [PublicationStatus.InReview]: { label: 'En Revisión', variant: 'outline' },
  [PublicationStatus.NeedsRevision]: { label: 'Requiere Revisión', variant: 'outline' },
  [PublicationStatus.Published]: { label: 'Publicado', variant: 'default' },
  [PublicationStatus.Rejected]: { label: 'Rechazado', variant: 'destructive' },
  [PublicationStatus.Archived]: { label: 'Archivado', variant: 'secondary' },
  [PublicationStatus.UnderReview]: { label: 'Bajo Revisión', variant: 'outline' },
}

interface PublicationStatusBadgeProps {
  status: PublicationStatus
}

export function PublicationStatusBadge({ status }: PublicationStatusBadgeProps) {
  const config = statusConfig[status]
  return <Badge variant={config.variant}>{config.label}</Badge>
}

interface PublicationCardProps {
  publication: Publication
  onView?: () => void
  onEdit?: () => void
  onSubmit?: () => void
  onArchive?: () => void
}

export function PublicationCard({
  publication,
  onView,
  onEdit,
  onSubmit,
  onArchive,
}: PublicationCardProps) {
  const createdDate = new Date(publication.createdAtUtc).toLocaleDateString('es-ES')
  const canSubmit = publication.status === PublicationStatus.Draft
  const canArchive =
    publication.status === PublicationStatus.Published ||
    publication.status === PublicationStatus.Rejected

  return (
    <div className="border rounded-lg p-4 space-y-3">
      <div className="flex items-start justify-between">
        <div className="space-y-1 flex-1">
          <p className="text-sm text-muted-foreground">Manga ID: {publication.mangaId}</p>
          <p className="text-xs text-muted-foreground">ID: {publication.id}</p>
        </div>
        <PublicationStatusBadge status={publication.status} />
      </div>

      <div className="space-y-1 text-sm">
        <p>
          <span className="text-muted-foreground">Creado:</span> {createdDate}
        </p>
        {publication.submittedAtUtc && (
          <p>
            <span className="text-muted-foreground">Enviado:</span>{' '}
            {new Date(publication.submittedAtUtc).toLocaleDateString('es-ES')}
          </p>
        )}
        {publication.moderatorComments && (
          <p className="text-sm">
            <span className="text-muted-foreground">Comentarios:</span> {publication.moderatorComments}
          </p>
        )}
      </div>

      <div className="flex gap-2 flex-wrap pt-2">
        {onView && (
          <button
            onClick={onView}
            className="px-3 py-1 text-xs rounded border hover:bg-accent"
          >
            Ver
          </button>
        )}
        {canSubmit && onSubmit && (
          <button
            onClick={onSubmit}
            className="px-3 py-1 text-xs rounded bg-blue-500 text-white hover:bg-blue-600"
          >
            Enviar para Revisión
          </button>
        )}
        {onEdit && publication.status === PublicationStatus.Draft && (
          <button
            onClick={onEdit}
            className="px-3 py-1 text-xs rounded border hover:bg-accent"
          >
            Editar
          </button>
        )}
        {canArchive && onArchive && (
          <button
            onClick={onArchive}
            className="px-3 py-1 text-xs rounded border hover:bg-accent"
          >
            Archivar
          </button>
        )}
      </div>
    </div>
  )
}

interface PublicationListProps {
  publications: Publication[]
  onView?: (publication: Publication) => void
  onEdit?: (publication: Publication) => void
  onSubmit?: (publication: Publication) => void
  onArchive?: (publication: Publication) => void
  isLoading?: boolean
}

export function PublicationList({
  publications,
  onView,
  onEdit,
  onSubmit,
  onArchive,
  isLoading,
}: PublicationListProps) {
  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className="border rounded-lg p-4 space-y-2 animate-pulse">
            <div className="h-4 bg-muted rounded w-3/4" />
            <div className="h-4 bg-muted rounded w-1/2" />
          </div>
        ))}
      </div>
    )
  }

  if (publications.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        No hay publicaciones para mostrar
      </div>
    )
  }

  return (
    <div className="space-y-3">
      {publications.map((pub) => (
        <PublicationCard
          key={pub.id}
          publication={pub}
          onView={() => onView?.(pub)}
          onEdit={() => onEdit?.(pub)}
          onSubmit={() => onSubmit?.(pub)}
          onArchive={() => onArchive?.(pub)}
        />
      ))}
    </div>
  )
}
