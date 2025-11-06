'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Publication, PublicationStatus } from '@/services/publication/client'
import { usePublication } from '@/hooks/usePublication'
import { 
  Upload, 
  Edit, 
  Archive, 
  Eye, 
  RefreshCw, 
  Trash2, 
  AlertTriangle,
  CheckCircle,
  Clock
} from 'lucide-react'

interface PublicationActionsProps {
  publication: Publication
  onActionComplete?: (publication: Publication, action: string) => void
  showAllActions?: boolean
  className?: string
}

export function PublicationActions({ 
  publication, 
  onActionComplete,
  showAllActions = false,
  className 
}: PublicationActionsProps) {
  const { 
    submitForReview, 
    archivePublication, 
    loading 
  } = usePublication()
  
  const [actionLoading, setActionLoading] = useState<string | null>(null)
  const [showConfirmDialog, setShowConfirmDialog] = useState<string | null>(null)

  const handleAction = async (action: string, publicationId: string) => {
    setActionLoading(action)
    try {
      let result: Publication | undefined

      switch (action) {
        case 'submit':
          result = await submitForReview(publicationId)
          break
        case 'archive':
          result = await archivePublication(publicationId)
          break
        default:
          console.warn('Unknown action:', action)
          return
      }

      if (result) {
        onActionComplete?.(result, action)
      }
    } catch (error) {
      console.error(`Error performing ${action}:`, error)
    } finally {
      setActionLoading(null)
      setShowConfirmDialog(null)
    }
  }

  const getAvailableActions = () => {
    const actions: Array<{
      id: string
      label: string
      icon: React.ComponentType<{ className?: string }>
      variant: 'default' | 'outline' | 'destructive' | 'secondary'
      description: string
      requiresConfirmation?: boolean
      disabled?: boolean
    }> = []

    switch (publication.status) {
      case PublicationStatus.Draft:
        actions.push(
          {
            id: 'edit',
            label: 'Editar',
            icon: Edit,
            variant: 'outline',
            description: 'Modificar el contenido y metadatos',
          },
          {
            id: 'submit',
            label: 'Enviar para Revisión',
            icon: Upload,
            variant: 'default',
            description: 'Enviar a moderación para aprobación',
          }
        )
        break

      case PublicationStatus.InReview:
        actions.push({
          id: 'view',
          label: 'Ver Estado',
          icon: Eye,
          variant: 'outline',
          description: 'Revisar el progreso de la moderación',
        })
        break

      case PublicationStatus.NeedsRevision:
        actions.push(
          {
            id: 'edit',
            label: 'Realizar Cambios',
            icon: Edit,
            variant: 'default',
            description: 'Corregir según comentarios del moderador',
          },
          {
            id: 'submit',
            label: 'Reenviar',
            icon: RefreshCw,
            variant: 'default',
            description: 'Enviar nuevamente después de correcciones',
          }
        )
        break

      case PublicationStatus.Published:
        actions.push(
          {
            id: 'view',
            label: 'Ver Publicación',
            icon: Eye,
            variant: 'outline',
            description: 'Ver cómo aparece públicamente',
          }
        )
        if (showAllActions) {
          actions.push({
            id: 'archive',
            label: 'Archivar',
            icon: Archive,
            variant: 'destructive',
            description: 'Remover de la vista pública',
            requiresConfirmation: true,
          })
        }
        break

      case PublicationStatus.Rejected:
        actions.push(
          {
            id: 'view',
            label: 'Ver Comentarios',
            icon: Eye,
            variant: 'outline',
            description: 'Revisar motivos del rechazo',
          },
          {
            id: 'edit',
            label: 'Crear Nueva Versión',
            icon: RefreshCw,
            variant: 'default',
            description: 'Crear una nueva publicación corregida',
          }
        )
        break

      case PublicationStatus.Archived:
        if (showAllActions) {
          actions.push({
            id: 'restore',
            label: 'Restaurar',
            icon: RefreshCw,
            variant: 'outline',
            description: 'Volver a hacer disponible',
            disabled: true, // No implementado aún
          })
        }
        break

      case PublicationStatus.UnderReview:
        actions.push({
          id: 'view',
          label: 'Ver Estado',
          icon: AlertTriangle,
          variant: 'outline',
          description: 'Revisar el estado de la investigación',
        })
        break
    }

    return actions
  }

  const availableActions = getAvailableActions()

  if (availableActions.length === 0) {
    return null
  }

  return (
    <>
      <Card className={className}>
        <CardHeader className="pb-3">
          <CardTitle className="text-base">Acciones Disponibles</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
            {availableActions.map((action) => {
              const Icon = action.icon
              const isLoading = actionLoading === action.id
              
              return (
                <Button
                  key={action.id}
                  variant={action.variant}
                  disabled={action.disabled || isLoading || loading}
                  onClick={() => {
                    if (action.requiresConfirmation) {
                      setShowConfirmDialog(action.id)
                    } else if (action.id === 'submit' || action.id === 'archive') {
                      handleAction(action.id, publication.id)
                    } else {
                      // Para otras acciones como edit, view, etc.
                      onActionComplete?.(publication, action.id)
                    }
                  }}
                  className="justify-start h-auto p-3 flex-col items-start gap-1"
                >
                  <div className="flex items-center gap-2 w-full">
                    {isLoading ? (
                      <div className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
                    ) : (
                      <Icon className="w-4 h-4" />
                    )}
                    <span className="font-medium">{action.label}</span>
                  </div>
                  <span className="text-xs opacity-80 text-left">
                    {action.description}
                  </span>
                </Button>
              )
            })}
          </div>

          {publication.status === PublicationStatus.NeedsRevision && publication.moderatorComments && (
            <Alert>
              <AlertTriangle className="w-4 h-4" />
              <AlertDescription>
                <strong>Comentarios del moderador:</strong>
                <div className="mt-1 text-sm">{publication.moderatorComments}</div>
              </AlertDescription>
            </Alert>
          )}

          {publication.status === PublicationStatus.Rejected && publication.rejectionReason && (
            <Alert variant="destructive">
              <AlertTriangle className="w-4 h-4" />
              <AlertDescription>
                <strong>Motivo del rechazo:</strong>
                <div className="mt-1 text-sm">{publication.rejectionReason}</div>
              </AlertDescription>
            </Alert>
          )}
        </CardContent>
      </Card>

      {/* Confirmation Dialog */}
      {showConfirmDialog && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="fixed inset-0 bg-black/50" onClick={() => setShowConfirmDialog(null)} />
          <Card className="relative w-full max-w-md">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <AlertTriangle className="w-5 h-5 text-orange-500" />
                Confirmar Acción
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="text-sm">
                {showConfirmDialog === 'archive' && 
                  '¿Estás seguro de que quieres archivar esta publicación? Esto la removerá de la vista pública.'}
              </p>
              
              <div className="flex gap-2 justify-end">
                <Button
                  variant="outline"
                  onClick={() => setShowConfirmDialog(null)}
                >
                  Cancelar
                </Button>
                <Button
                  variant="destructive"
                  onClick={() => handleAction(showConfirmDialog, publication.id)}
                  disabled={actionLoading === showConfirmDialog}
                >
                  {actionLoading === showConfirmDialog ? 'Procesando...' : 'Confirmar'}
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      )}
    </>
  )
}