'use client'

import { useState } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Publication, PublicationStatus } from '@/services/publication/client'
import { usePublication } from '@/hooks/usePublication'
import { CheckCircle, AlertTriangle, Upload, FileText, Eye } from 'lucide-react'

interface PublicationSubmissionProps {
  publication: Publication
  onSubmissionComplete?: (publication: Publication) => void
  className?: string
}

export function PublicationSubmission({ 
  publication, 
  onSubmissionComplete,
  className 
}: PublicationSubmissionProps) {
  const { submitForReview, loading } = usePublication()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [showChecklist, setShowChecklist] = useState(false)

  const canSubmit = publication.status === PublicationStatus.Draft || 
                   publication.status === PublicationStatus.NeedsRevision

  const handleSubmit = async () => {
    if (!canSubmit) return

    setIsSubmitting(true)
    try {
      const updatedPublication = await submitForReview(publication.id)
      onSubmissionComplete?.(updatedPublication)
    } catch (error) {
      console.error('Error submitting publication:', error)
    } finally {
      setIsSubmitting(false)
    }
  }

  const checklistItems = [
    {
      id: 'content',
      label: 'Contenido completo',
      description: 'Todas las páginas del manga han sido subidas correctamente',
      checked: true, // Asumimos que si existe la publicación, el contenido está subido
    },
    {
      id: 'metadata',
      label: 'Metadatos completos',
      description: 'Título, descripción y categorías están configurados',
      checked: true, // Asumimos que los metadatos básicos están completos
    },
    {
      id: 'guidelines',
      label: 'Cumple las normas',
      description: 'El contenido respeta las normas de la comunidad',
      checked: false, // El usuario debe confirmar esto
    },
    {
      id: 'quality',
      label: 'Calidad adecuada',
      description: 'Las imágenes tienen buena resolución y legibilidad',
      checked: false, // El usuario debe confirmar esto
    },
  ]

  const allChecked = checklistItems.every(item => item.checked)

  if (!canSubmit) {
    return (
      <Card className={className}>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="w-5 h-5" />
            Envío para Revisión
          </CardTitle>
        </CardHeader>
        <CardContent>
          <Alert>
            <AlertTriangle className="w-4 h-4" />
            <AlertDescription>
              Esta publicación no puede ser enviada para revisión en su estado actual.
              {publication.status === PublicationStatus.InReview && ' Ya está siendo revisada.'}
              {publication.status === PublicationStatus.Published && ' Ya está publicada.'}
              {publication.status === PublicationStatus.Rejected && ' Ha sido rechazada. Puedes crear una nueva versión.'}
              {publication.status === PublicationStatus.Archived && ' Está archivada.'}
            </AlertDescription>
          </Alert>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Upload className="w-5 h-5" />
          Enviar para Revisión
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-3">
          <p className="text-sm text-muted-foreground">
            Una vez enviada, tu publicación será revisada por nuestro equipo de moderación. 
            Este proceso puede tomar entre 1-3 días hábiles.
          </p>

          {publication.status === PublicationStatus.NeedsRevision && (
            <Alert>
              <AlertTriangle className="w-4 h-4" />
              <AlertDescription>
                Esta publicación fue devuelta para revisión. Asegúrate de haber realizado 
                todos los cambios solicitados antes de reenviarla.
              </AlertDescription>
            </Alert>
          )}

          <div className="bg-muted p-3 rounded-md space-y-2">
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium">Lista de Verificación</span>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setShowChecklist(!showChecklist)}
              >
                {showChecklist ? 'Ocultar' : 'Mostrar'}
              </Button>
            </div>

            {showChecklist && (
              <div className="space-y-2 mt-3">
                {checklistItems.map((item) => (
                  <div key={item.id} className="flex items-start gap-2">
                    <div className={`w-4 h-4 rounded-sm border-2 flex items-center justify-center mt-0.5 ${
                      item.checked 
                        ? 'bg-green-500 border-green-500' 
                        : 'border-gray-300'
                    }`}>
                      {item.checked && <CheckCircle className="w-3 h-3 text-white" />}
                    </div>
                    <div className="flex-1 space-y-1">
                      <div className="text-sm font-medium">{item.label}</div>
                      <div className="text-xs text-muted-foreground">{item.description}</div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          <Alert>
            <Eye className="w-4 h-4" />
            <AlertDescription>
              <strong>Proceso de Revisión:</strong>
              <ul className="mt-2 space-y-1 text-sm list-disc list-inside">
                <li>Verificación de contenido apropiado</li>
                <li>Revisión de calidad de imagen</li>
                <li>Comprobación de metadatos</li>
                <li>Validación de derechos de autor</li>
              </ul>
            </AlertDescription>
          </Alert>
        </div>

        <div className="flex flex-col sm:flex-row gap-2 pt-4">
          <Button
            onClick={handleSubmit}
            disabled={isSubmitting || loading}
            className="flex-1"
          >
            {isSubmitting ? (
              <>
                <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
                Enviando...
              </>
            ) : (
              <>
                <Upload className="w-4 h-4 mr-2" />
                Enviar para Revisión
              </>
            )}
          </Button>
          
          <Button
            variant="outline"
            onClick={() => setShowChecklist(!showChecklist)}
            className="sm:w-auto"
          >
            <Eye className="w-4 h-4 mr-2" />
            {showChecklist ? 'Ocultar' : 'Ver'} Lista
          </Button>
        </div>

        <div className="text-xs text-muted-foreground space-y-1">
          <p>• Recibirás una notificación cuando la revisión esté completa</p>
          <p>• Puedes ver el estado en cualquier momento desde tu panel</p>
          <p>• Los comentarios del moderador aparecerán aquí si se requieren cambios</p>
        </div>
      </CardContent>
    </Card>
  )
}