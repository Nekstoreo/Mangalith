'use client'

import { Publication, PublicationStatus } from '@/services/publication/client'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { 
  Clock, 
  Upload, 
  CheckCircle, 
  XCircle, 
  AlertTriangle, 
  Archive, 
  Eye,
  User,
  Calendar
} from 'lucide-react'

interface PublicationHistoryProps {
  publication: Publication
  showEstimatedTimes?: boolean
  className?: string
}

interface TimelineEvent {
  date: string
  status: PublicationStatus
  label: string
  description?: string
  icon: React.ComponentType<{ className?: string }>
  color: string
  isCurrentStatus?: boolean
}

const statusIcons: Record<PublicationStatus, React.ComponentType<{ className?: string }>> = {
  [PublicationStatus.Draft]: Clock,
  [PublicationStatus.InReview]: Eye,
  [PublicationStatus.NeedsRevision]: AlertTriangle,
  [PublicationStatus.Published]: CheckCircle,
  [PublicationStatus.Rejected]: XCircle,
  [PublicationStatus.Archived]: Archive,
  [PublicationStatus.UnderReview]: AlertTriangle,
}

const statusColors: Record<PublicationStatus, string> = {
  [PublicationStatus.Draft]: 'text-gray-500',
  [PublicationStatus.InReview]: 'text-blue-500',
  [PublicationStatus.NeedsRevision]: 'text-orange-500',
  [PublicationStatus.Published]: 'text-green-500',
  [PublicationStatus.Rejected]: 'text-red-500',
  [PublicationStatus.Archived]: 'text-gray-500',
  [PublicationStatus.UnderReview]: 'text-yellow-500',
}

export function PublicationHistory({ 
  publication, 
  showEstimatedTimes = true,
  className 
}: PublicationHistoryProps) {
  const events: TimelineEvent[] = []

  // Construir timeline basado en fechas
  if (publication.createdAtUtc) {
    events.push({
      date: publication.createdAtUtc,
      status: PublicationStatus.Draft,
      label: 'Publicación Creada',
      description: 'La publicación fue creada como borrador',
      icon: statusIcons[PublicationStatus.Draft],
      color: statusColors[PublicationStatus.Draft],
    })
  }

  if (publication.submittedAtUtc) {
    events.push({
      date: publication.submittedAtUtc,
      status: PublicationStatus.InReview,
      label: 'Enviado para Revisión',
      description: 'La publicación fue enviada al equipo de moderación',
      icon: statusIcons[PublicationStatus.InReview],
      color: statusColors[PublicationStatus.InReview],
    })
  }

  if (publication.reviewedAtUtc) {
    let statusLabel = 'Revisado'
    let description = 'La publicación fue revisada por un moderador'
    
    if (publication.status === PublicationStatus.Published) {
      statusLabel = 'Publicación Aprobada'
      description = 'La publicación fue aprobada y está disponible públicamente'
    } else if (publication.status === PublicationStatus.Rejected) {
      statusLabel = 'Publicación Rechazada'
      description = 'La publicación fue rechazada por el moderador'
    } else if (publication.status === PublicationStatus.NeedsRevision) {
      statusLabel = 'Requiere Cambios'
      description = 'El moderador solicita cambios antes de la aprobación'
    }

    events.push({
      date: publication.reviewedAtUtc,
      status: publication.status,
      label: statusLabel,
      description: publication.moderatorComments || description,
      icon: statusIcons[publication.status],
      color: statusColors[publication.status],
      isCurrentStatus: true,
    })
  }

  // Calcular tiempo estimado para próximos pasos
  const getEstimatedNextStep = () => {
    if (publication.status === PublicationStatus.InReview && publication.submittedAtUtc) {
      const submittedDate = new Date(publication.submittedAtUtc)
      const estimatedReviewDate = new Date(submittedDate.getTime() + (3 * 24 * 60 * 60 * 1000)) // 3 días
      const now = new Date()
      
      if (estimatedReviewDate > now) {
        return {
          label: 'Revisión Estimada',
          date: estimatedReviewDate.toLocaleDateString('es-ES'),
          description: 'Tiempo estimado para completar la revisión (1-3 días hábiles)',
        }
      }
    }
    return null
  }

  const estimatedNextStep = showEstimatedTimes ? getEstimatedNextStep() : null

  return (
    <Card className={className}>
      <CardHeader className="pb-3">
        <CardTitle className="flex items-center gap-2 text-base">
          <Calendar className="w-4 h-4" />
          Historial de la Publicación
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-4">
          {events.length === 0 ? (
            <Alert>
              <Clock className="w-4 h-4" />
              <AlertDescription>
                No hay historial disponible para esta publicación.
              </AlertDescription>
            </Alert>
          ) : (
            <>
              {events.map((event, index) => {
                const Icon = event.icon
                const isLast = index === events.length - 1
                
                return (
                  <div key={index} className="flex gap-3">
                    <div className="flex flex-col items-center">
                      <div className={`w-8 h-8 rounded-full border-2 flex items-center justify-center ${
                        event.isCurrentStatus 
                          ? 'bg-primary border-primary text-primary-foreground' 
                          : 'bg-background border-border'
                      }`}>
                        <Icon className={`w-4 h-4 ${event.isCurrentStatus ? 'text-primary-foreground' : event.color}`} />
                      </div>
                      {!isLast && <div className="w-0.5 h-8 bg-border mt-2" />}
                    </div>
                    
                    <div className="flex-1 pb-4">
                      <div className="flex flex-col sm:flex-row sm:items-center gap-1 sm:gap-2 mb-2">
                        <Badge 
                          variant={event.isCurrentStatus ? "default" : "outline"}
                          className="w-fit"
                        >
                          {event.label}
                        </Badge>
                        <span className="text-xs text-muted-foreground flex items-center gap-1">
                          <Clock className="w-3 h-3" />
                          {new Date(event.date).toLocaleString('es-ES')}
                        </span>
                      </div>
                      
                      {event.description && (
                        <p className="text-sm text-muted-foreground leading-relaxed">
                          {event.description}
                        </p>
                      )}
                      
                      {event.isCurrentStatus && publication.rejectionReason && (
                        <div className="mt-2 p-2 bg-red-50 border border-red-200 rounded text-sm text-red-700">
                          <strong>Motivo:</strong> {publication.rejectionReason}
                        </div>
                      )}
                    </div>
                  </div>
                )
              })}

              {/* Próximo paso estimado */}
              {estimatedNextStep && (
                <div className="flex gap-3 opacity-60">
                  <div className="flex flex-col items-center">
                    <div className="w-8 h-8 rounded-full border-2 border-dashed border-muted-foreground/30 flex items-center justify-center">
                      <Clock className="w-4 h-4 text-muted-foreground" />
                    </div>
                  </div>
                  
                  <div className="flex-1 pb-4">
                    <div className="flex flex-col sm:flex-row sm:items-center gap-1 sm:gap-2 mb-2">
                      <Badge variant="outline" className="w-fit border-dashed">
                        {estimatedNextStep.label}
                      </Badge>
                      <span className="text-xs text-muted-foreground">
                        {estimatedNextStep.date}
                      </span>
                    </div>
                    
                    <p className="text-sm text-muted-foreground">
                      {estimatedNextStep.description}
                    </p>
                  </div>
                </div>
              )}
            </>
          )}
        </div>

        {/* Información adicional */}
        <div className="border-t pt-3 space-y-2">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs text-muted-foreground">
            <div className="flex items-center gap-1">
              <User className="w-3 h-3" />
              <span>ID: {publication.id.slice(0, 8)}...</span>
            </div>
            <div className="flex items-center gap-1">
              <Calendar className="w-3 h-3" />
              <span>Manga: {publication.mangaId.slice(0, 8)}...</span>
            </div>
          </div>
          
          {publication.status === PublicationStatus.InReview && (
            <Alert>
              <Eye className="w-4 h-4" />
              <AlertDescription className="text-xs">
                Tu publicación está en cola de revisión. Recibirás una notificación cuando sea procesada.
              </AlertDescription>
            </Alert>
          )}
        </div>
      </CardContent>
    </Card>
  )
}
