'use client'

import { useState, useEffect } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Publication, PublicationStatus } from '@/services/publication/client'
import { usePublication } from '@/hooks/usePublication'
import { PublicationStatusCard } from './PublicationStatus'
import { PublicationSubmission } from './PublicationSubmission'
import { PublicationHistory } from './PublicationHistory'
import { PublicationActions } from './PublicationActions'
import { 
  FileText, 
  Plus, 
  Filter, 
  RefreshCw,
  BarChart3,
  Search
} from 'lucide-react'

interface PublicationDashboardProps {
  userId?: string
  showCreateButton?: boolean
  className?: string
}

export function PublicationDashboard({ 
  userId, 
  showCreateButton = true,
  className 
}: PublicationDashboardProps) {
  const {
    publications,
    loading,
    error,
    totalCount,
    currentPage,
    getMyPublications,
    getPublicationsByStatus,
    clearError,
  } = usePublication()

  const [selectedPublication, setSelectedPublication] = useState<Publication | null>(null)
  const [statusFilter, setStatusFilter] = useState<PublicationStatus | 'all'>('all')
  const [currentPageLocal, setCurrentPageLocal] = useState(1)
  const [viewMode, setViewMode] = useState<'list' | 'detail'>('list')

  useEffect(() => {
    loadPublications()
  }, [statusFilter, currentPageLocal])

  const loadPublications = async () => {
    try {
      if (statusFilter === 'all') {
        await getMyPublications(currentPageLocal, 10)
      } else {
        await getPublicationsByStatus(statusFilter, currentPageLocal, 10)
      }
    } catch (err) {
      console.error('Error loading publications:', err)
    }
  }

  const handleRefresh = () => {
    loadPublications()
  }

  const handlePublicationSelect = (publication: Publication) => {
    setSelectedPublication(publication)
    setViewMode('detail')
  }

  const handleActionComplete = (publication: Publication, action: string) => {
    // Actualizar la publicación seleccionada si es la misma
    if (selectedPublication?.id === publication.id) {
      setSelectedPublication(publication)
    }
    
    // Recargar la lista
    loadPublications()

    // Manejar acciones específicas
    if (action === 'edit') {
      // TODO: Navegar a editor
      console.log('Navigate to editor for:', publication.id)
    } else if (action === 'view') {
      // TODO: Navegar a vista pública
      console.log('Navigate to public view for:', publication.id)
    }
  }

  const getStatusCounts = () => {
    const counts: Record<string, number> = {
      all: totalCount,
      [PublicationStatus.Draft]: 0,
      [PublicationStatus.InReview]: 0,
      [PublicationStatus.Published]: 0,
      [PublicationStatus.NeedsRevision]: 0,
      [PublicationStatus.Rejected]: 0,
      [PublicationStatus.Archived]: 0,
    }

    publications.forEach(pub => {
      counts[pub.status] = (counts[pub.status] || 0) + 1
    })

    return counts
  }

  const statusCounts = getStatusCounts()

  const statusOptions = [
    { value: 'all', label: 'Todas', count: statusCounts.all },
    { value: PublicationStatus.Draft, label: 'Borradores', count: statusCounts[PublicationStatus.Draft] },
    { value: PublicationStatus.InReview, label: 'En Revisión', count: statusCounts[PublicationStatus.InReview] },
    { value: PublicationStatus.Published, label: 'Publicadas', count: statusCounts[PublicationStatus.Published] },
    { value: PublicationStatus.NeedsRevision, label: 'Requieren Cambios', count: statusCounts[PublicationStatus.NeedsRevision] },
    { value: PublicationStatus.Rejected, label: 'Rechazadas', count: statusCounts[PublicationStatus.Rejected] },
    { value: PublicationStatus.Archived, label: 'Archivadas', count: statusCounts[PublicationStatus.Archived] },
  ]

  if (viewMode === 'detail' && selectedPublication) {
    return (
      <div className={`space-y-6 ${className}`}>
        {/* Header con botón de regreso */}
        <div className="flex items-center gap-4">
          <Button
            variant="outline"
            onClick={() => {
              setViewMode('list')
              setSelectedPublication(null)
            }}
          >
            ← Volver a la Lista
          </Button>
          <div>
            <h2 className="text-lg font-semibold">Detalles de Publicación</h2>
            <p className="text-sm text-muted-foreground">
              ID: {selectedPublication.id.slice(0, 8)}...
            </p>
          </div>
        </div>

        {/* Grid de componentes de detalle */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="space-y-6">
            <PublicationStatusCard 
              publication={selectedPublication} 
              showDetails={true}
            />
            
            <PublicationActions
              publication={selectedPublication}
              onActionComplete={handleActionComplete}
              showAllActions={true}
            />
          </div>

          <div className="space-y-6">
            <PublicationHistory 
              publication={selectedPublication}
              showEstimatedTimes={true}
            />

            {(selectedPublication.status === PublicationStatus.Draft || 
              selectedPublication.status === PublicationStatus.NeedsRevision) && (
              <PublicationSubmission
                publication={selectedPublication}
                onSubmissionComplete={(pub) => handleActionComplete(pub, 'submit')}
              />
            )}
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className={`space-y-6 ${className}`}>
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h2 className="text-2xl font-bold flex items-center gap-2">
            <FileText className="w-6 h-6" />
            Mis Publicaciones
          </h2>
          <p className="text-muted-foreground">
            Gestiona tus publicaciones de manga y su estado de revisión
          </p>
        </div>
        
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={handleRefresh}
            disabled={loading}
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            Actualizar
          </Button>
          
          {showCreateButton && (
            <Button>
              <Plus className="w-4 h-4 mr-2" />
              Nueva Publicación
            </Button>
          )}
        </div>
      </div>

      {/* Error Alert */}
      {error && (
        <Alert variant="destructive">
          <AlertDescription className="flex justify-between items-center">
            <span>{error}</span>
            <Button
              variant="ghost"
              size="sm"
              onClick={clearError}
            >
              Cerrar
            </Button>
          </AlertDescription>
        </Alert>
      )}

      {/* Filtros */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="flex items-center gap-2 text-base">
            <Filter className="w-4 h-4" />
            Filtros
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-2">
            {statusOptions.map((option) => (
              <button
                key={option.value}
                onClick={() => {
                  setStatusFilter(option.value as PublicationStatus | 'all')
                  setCurrentPageLocal(1)
                }}
                className={`px-3 py-1.5 rounded-md text-sm transition-colors flex items-center gap-2 ${
                  statusFilter === option.value
                    ? 'bg-primary text-primary-foreground'
                    : 'border hover:bg-accent'
                }`}
              >
                {option.label}
                {option.count > 0 && (
                  <Badge variant="secondary" className="text-xs">
                    {option.count}
                  </Badge>
                )}
              </button>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Lista de Publicaciones */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="flex items-center justify-between">
            <span className="flex items-center gap-2">
              <BarChart3 className="w-4 h-4" />
              Publicaciones
              {totalCount > 0 && (
                <Badge variant="outline">{totalCount} total</Badge>
              )}
            </span>
          </CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="space-y-3">
              {Array.from({ length: 3 }).map((_, i) => (
                <div key={i} className="border rounded-lg p-4 space-y-2 animate-pulse">
                  <div className="h-4 bg-muted rounded w-3/4" />
                  <div className="h-4 bg-muted rounded w-1/2" />
                  <div className="h-8 bg-muted rounded w-1/4" />
                </div>
              ))}
            </div>
          ) : publications.length === 0 ? (
            <div className="text-center py-12">
              <FileText className="w-12 h-12 text-muted-foreground mx-auto mb-4" />
              <h3 className="text-lg font-medium mb-2">No hay publicaciones</h3>
              <p className="text-muted-foreground mb-4">
                {statusFilter === 'all' 
                  ? 'Aún no has creado ninguna publicación.'
                  : `No tienes publicaciones con el estado "${statusOptions.find(o => o.value === statusFilter)?.label}".`
                }
              </p>
              {showCreateButton && statusFilter === 'all' && (
                <Button>
                  <Plus className="w-4 h-4 mr-2" />
                  Crear Primera Publicación
                </Button>
              )}
            </div>
          ) : (
            <div className="space-y-3">
              {publications.map((publication) => (
                <div
                  key={publication.id}
                  className="border rounded-lg p-4 hover:bg-accent/50 transition-colors cursor-pointer"
                  onClick={() => handlePublicationSelect(publication)}
                >
                  <div className="flex items-start justify-between mb-3">
                    <div className="space-y-1 flex-1">
                      <div className="flex items-center gap-2">
                        <Badge 
                          variant={
                            publication.status === PublicationStatus.Published ? 'default' :
                            publication.status === PublicationStatus.Rejected ? 'destructive' :
                            'outline'
                          }
                        >
                          {statusOptions.find(s => s.value === publication.status)?.label}
                        </Badge>
                        <span className="text-xs text-muted-foreground">
                          {new Date(publication.createdAtUtc).toLocaleDateString('es-ES')}
                        </span>
                      </div>
                      <p className="text-sm text-muted-foreground">
                        Manga ID: {publication.mangaId.slice(0, 8)}...
                      </p>
                    </div>
                    
                    <Button variant="ghost" size="sm">
                      <Search className="w-4 h-4" />
                    </Button>
                  </div>

                  {publication.moderatorComments && (
                    <div className="bg-muted p-2 rounded text-sm mb-3">
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
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Paginación */}
      {totalCount > 10 && (
        <div className="flex justify-center gap-2">
          <Button
            variant="outline"
            disabled={currentPageLocal === 1 || loading}
            onClick={() => setCurrentPageLocal(currentPageLocal - 1)}
          >
            Anterior
          </Button>
          <span className="flex items-center px-3 text-sm">
            Página {currentPageLocal} de {Math.ceil(totalCount / 10)}
          </span>
          <Button
            variant="outline"
            disabled={currentPageLocal >= Math.ceil(totalCount / 10) || loading}
            onClick={() => setCurrentPageLocal(currentPageLocal + 1)}
          >
            Siguiente
          </Button>
        </div>
      )}
    </div>
  )
}