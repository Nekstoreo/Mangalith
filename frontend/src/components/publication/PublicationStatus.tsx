'use client'

import { Badge } from '@/components/ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Publication, PublicationStatus as Status } from '@/services/publication/client'
import { Clock, CheckCircle, XCircle, AlertTriangle, Archive, Eye } from 'lucide-react'

const statusConfig: Record<
  Status,
  {
    label: string
    variant: 'default' | 'secondary' | 'destructive' | 'outline'
    icon: React.ComponentType<{ className?: string }>
    color: string
    description: string
    userMessage: string
  }
> = {
  [Status.Draft]: {
    label: 'Borrador',
    variant: 'secondary',
    icon: Clock,
    color: 'text-gray-600',
    description: 'La publicación está en modo borrador',
    userMessage: 'Tu publicación está guardada como borrador. Puedes editarla y enviarla para revisión cuando esté lista.',
  },
  [Status.InReview]: {
    label: 'En Revisión',
    variant: 'outline',
    icon: Eye,
    color: 'text-blue-600',
    description: 'Un moderador está revisando la publicación',
    userMessage: 'Tu publicación está siendo revisada por nuestro equipo de moderación. Te notificaremos cuando tengamos una decisión.',
  },
  [Status.NeedsRevision]: {
    label: 'Requiere Revisión',
    variant: 'outline',
    icon: AlertTriangle,
    color: 'text-orange-600',
    description: 'La publicación necesita cambios antes de ser aprobada',
    userMessage: 'Tu publicación necesita algunos cambios. Revisa los comentarios del moderador y realiza las correcciones necesarias.',
  },
  [Status.Published]: {
    label: 'Publicado',
    variant: 'default',
    icon: CheckCircle,
    color: 'text-green-600',
    description: 'La publicación está disponible públicamente',
    userMessage: '¡Felicidades! Tu publicación ha sido aprobada y ahora está disponible para todos los usuarios.',
  },
  [Status.Rejected]: {
    label: 'Rechazado',
    variant: 'destructive',
    icon: XCircle,
    color: 'text-red-600',
    description: 'La publicación ha sido rechazada',
    userMessage: 'Tu publicación ha sido rechazada. Revisa los comentarios del moderador para entender los motivos.',
  },
  [Status.Archived]: {
    label: 'Archivado',
    variant: 'secondary',
    icon: Archive,
    color: 'text-gray-600',
    description: 'La publicación ha sido archivada',
    userMessage: 'Esta publicación ha sido archivada y ya no está disponible públicamente.',
  },
  [Status.UnderReview]: {
    label: 'Bajo Revisión',
    variant: 'outline',
    icon: AlertTriangle,
    color: 'text-yellow-600',
    description: 'La publicación está siendo revisada por reportes',
    userMessage: 'Tu publicación está siendo revisada debido a reportes de la comunidad. Te notificaremos sobre el resultado.',
  },
}

interface PublicationStatusProps {
  status: Status
  className?: string
  showIcon?: boolean
}

export function PublicationStatusBadge({ status, className, showIcon = false }: PublicationStatusProps) {
  const config = statusConfig[status]
  const Icon = config.icon

  return (
    <Badge variant={config.variant} className={className}>
      {showIcon && <Icon className="w-3 h-3" />}
      {config.label}
    </Badge>
  )
}

interface PublicationStatusCardProps {
  publication: Publication
  showDetails?: boolean
  className?: string
}

export function PublicationStatusCard({ 
  publication, 
  showDetails = true, 
  className 
}: PublicationStatusCardProps) {
  const config = statusConfig[publication.status]
  const Icon = config.icon

  return (
    <Card className={className}>
      <CardHeader className="pb-3">
        <CardTitle className="flex items-center gap-2 text-base">
          <Icon className={`w-4 h-4 ${config.color}`} />
          Estado de Publicación
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex items-center justify-between">
          <PublicationStatusBadge status={publication.status} />
          <span className="text-xs text-muted-foreground">
            {new Date(publication.createdAtUtc).toLocaleDateString('es-ES')}
          </span>
        </div>

        {showDetails && (
          <Alert>
            <AlertDescription>
              {config.userMessage}
            </AlertDescription>
          </Alert>
        )}

        {publication.moderatorComments && (
          <div className="space-y-2">
            <h4 className="text-sm font-medium">Comentarios del Moderador:</h4>
            <div className="bg-muted p-3 rounded-md text-sm">
              {publication.moderatorComments}
            </div>
          </div>
        )}

        {publication.rejectionReason && (
          <div className="space-y-2">
            <h4 className="text-sm font-medium text-red-600">Motivo del Rechazo:</h4>
            <div className="bg-red-50 border border-red-200 p-3 rounded-md text-sm text-red-700">
              {publication.rejectionReason}
            </div>
          </div>
        )}

        {showDetails && (
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs text-muted-foreground">
            <div>
              <span className="font-medium">Creado:</span>{' '}
              {new Date(publication.createdAtUtc).toLocaleString('es-ES')}
            </div>
            {publication.submittedAtUtc && (
              <div>
                <span className="font-medium">Enviado:</span>{' '}
                {new Date(publication.submittedAtUtc).toLocaleString('es-ES')}
              </div>
            )}
            {publication.reviewedAtUtc && (
              <div>
                <span className="font-medium">Revisado:</span>{' '}
                {new Date(publication.reviewedAtUtc).toLocaleString('es-ES')}
              </div>
            )}
            <div>
              <span className="font-medium">ID:</span> {publication.id.slice(0, 8)}...
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  )
}

// Export the existing component for backward compatibility
export { PublicationStatusBadge as PublicationStatus }