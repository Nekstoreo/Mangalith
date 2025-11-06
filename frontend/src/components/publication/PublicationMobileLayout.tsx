'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Publication, PublicationStatus } from '@/services/publication/client'
import { PublicationStatusCard } from './PublicationStatus'
import { PublicationSubmission } from './PublicationSubmission'
import { PublicationHistory } from './PublicationHistory'
import { PublicationActions } from './PublicationActions'
import { 
  ChevronDown, 
  ChevronUp, 
  FileText, 
  Clock, 
  Settings,
  Eye
} from 'lucide-react'

interface PublicationMobileLayoutProps {
  publication: Publication
  onActionComplete?: (publication: Publication, action: string) => void
  className?: string
}

export function PublicationMobileLayout({ 
  publication, 
  onActionComplete,
  className 
}: PublicationMobileLayoutProps) {
  const [expandedSections, setExpandedSections] = useState<Record<string, boolean>>({
    status: true,
    actions: true,
    history: false,
    submission: false,
  })

  const toggleSection = (section: string) => {
    setExpandedSections(prev => ({
      ...prev,
      [section]: !prev[section]
    }))
  }

  const sections = [
    {
      id: 'status',
      title: 'Estado Actual',
      icon: FileText,
      component: (
        <PublicationStatusCard 
          publication={publication} 
          showDetails={true}
          className="border-0 shadow-none p-0"
        />
      ),
      alwaysShow: true,
    },
    {
      id: 'actions',
      title: 'Acciones Disponibles',
      icon: Settings,
      component: (
        <PublicationActions
          publication={publication}
          onActionComplete={onActionComplete}
          showAllActions={true}
          className="border-0 shadow-none p-0"
        />
      ),
      alwaysShow: true,
    },
    {
      id: 'submission',
      title: 'Enviar para Revisión',
      icon: Clock,
      component: (
        <PublicationSubmission
          publication={publication}
          onSubmissionComplete={(pub) => onActionComplete?.(pub, 'submit')}
          className="border-0 shadow-none p-0"
        />
      ),
      showWhen: publication.status === PublicationStatus.Draft || 
                publication.status === PublicationStatus.NeedsRevision,
    },
    {
      id: 'history',
      title: 'Historial',
      icon: Eye,
      component: (
        <PublicationHistory 
          publication={publication}
          showEstimatedTimes={true}
          className="border-0 shadow-none p-0"
        />
      ),
      alwaysShow: false,
    },
  ]

  const visibleSections = sections.filter(section => 
    section.alwaysShow || section.showWhen
  )

  return (
    <div className={`space-y-4 ${className}`}>
      {/* Header compacto para móvil */}
      <Card className="lg:hidden">
        <CardHeader className="pb-3">
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="text-base">Publicación</CardTitle>
              <p className="text-xs text-muted-foreground">
                ID: {publication.id.slice(0, 8)}...
              </p>
            </div>
            <Badge 
              variant={
                publication.status === PublicationStatus.Published ? 'default' :
                publication.status === PublicationStatus.Rejected ? 'destructive' :
                'outline'
              }
            >
              {publication.status === PublicationStatus.Draft && 'Borrador'}
              {publication.status === PublicationStatus.InReview && 'En Revisión'}
              {publication.status === PublicationStatus.NeedsRevision && 'Requiere Cambios'}
              {publication.status === PublicationStatus.Published && 'Publicado'}
              {publication.status === PublicationStatus.Rejected && 'Rechazado'}
              {publication.status === PublicationStatus.Archived && 'Archivado'}
              {publication.status === PublicationStatus.UnderReview && 'Bajo Revisión'}
            </Badge>
          </div>
        </CardHeader>
      </Card>

      {/* Secciones expandibles */}
      {visibleSections.map((section) => {
        const Icon = section.icon
        const isExpanded = expandedSections[section.id]
        
        return (
          <Card key={section.id}>
            <CardHeader 
              className="pb-2 cursor-pointer"
              onClick={() => toggleSection(section.id)}
            >
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Icon className="w-4 h-4" />
                  <CardTitle className="text-sm">{section.title}</CardTitle>
                </div>
                {isExpanded ? (
                  <ChevronUp className="w-4 h-4 text-muted-foreground" />
                ) : (
                  <ChevronDown className="w-4 h-4 text-muted-foreground" />
                )}
              </div>
            </CardHeader>
            
            {isExpanded && (
              <CardContent className="pt-0">
                {section.component}
              </CardContent>
            )}
          </Card>
        )
      })}

      {/* Información adicional en móvil */}
      <Card className="lg:hidden">
        <CardContent className="pt-4">
          <div className="grid grid-cols-2 gap-4 text-xs text-muted-foreground">
            <div>
              <span className="font-medium">Creado:</span>
              <br />
              {new Date(publication.createdAtUtc).toLocaleDateString('es-ES')}
            </div>
            {publication.submittedAtUtc && (
              <div>
                <span className="font-medium">Enviado:</span>
                <br />
                {new Date(publication.submittedAtUtc).toLocaleDateString('es-ES')}
              </div>
            )}
            <div>
              <span className="font-medium">Manga ID:</span>
              <br />
              {publication.mangaId.slice(0, 8)}...
            </div>
            {publication.reviewedAtUtc && (
              <div>
                <span className="font-medium">Revisado:</span>
                <br />
                {new Date(publication.reviewedAtUtc).toLocaleDateString('es-ES')}
              </div>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

interface PublicationMobileCardProps {
  publication: Publication
  onClick?: () => void
  className?: string
}

export function PublicationMobileCard({ 
  publication, 
  onClick,
  className 
}: PublicationMobileCardProps) {
  return (
    <Card 
      className={`cursor-pointer hover:bg-accent/50 transition-colors ${className}`}
      onClick={onClick}
    >
      <CardContent className="p-4">
        <div className="flex items-start justify-between mb-3">
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-1">
              <Badge 
                variant={
                  publication.status === PublicationStatus.Published ? 'default' :
                  publication.status === PublicationStatus.Rejected ? 'destructive' :
                  'outline'
                }
                className="text-xs"
              >
                {publication.status === PublicationStatus.Draft && 'Borrador'}
                {publication.status === PublicationStatus.InReview && 'En Revisión'}
                {publication.status === PublicationStatus.NeedsRevision && 'Requiere Cambios'}
                {publication.status === PublicationStatus.Published && 'Publicado'}
                {publication.status === PublicationStatus.Rejected && 'Rechazado'}
                {publication.status === PublicationStatus.Archived && 'Archivado'}
                {publication.status === PublicationStatus.UnderReview && 'Bajo Revisión'}
              </Badge>
            </div>
            
            <p className="text-sm text-muted-foreground truncate">
              Manga: {publication.mangaId.slice(0, 8)}...
            </p>
            <p className="text-xs text-muted-foreground">
              {new Date(publication.createdAtUtc).toLocaleDateString('es-ES')}
            </p>
          </div>
          
          <Button variant="ghost" size="sm">
            <Eye className="w-4 h-4" />
          </Button>
        </div>

        {publication.moderatorComments && (
          <div className="bg-muted p-2 rounded text-xs mb-2 line-clamp-2">
            <strong>Comentarios:</strong> {publication.moderatorComments}
          </div>
        )}

        <div className="flex justify-between items-center text-xs text-muted-foreground">
          <span>ID: {publication.id.slice(0, 8)}...</span>
          {publication.submittedAtUtc && (
            <span>
              Enviado: {new Date(publication.submittedAtUtc).toLocaleDateString('es-ES')}
            </span>
          )}
        </div>
      </CardContent>
    </Card>
  )
}