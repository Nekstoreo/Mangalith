'use client'

import { useState } from 'react'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Alert } from '@/components/ui/alert'
import { Publication } from '@/services/publication/client'

interface SubmitPublicationDialogProps {
  open: boolean
  publication: Publication | null
  loading?: boolean
  onConfirm?: () => Promise<void>
  onOpenChange: (open: boolean) => void
}

export function SubmitPublicationDialog({
  open,
  publication,
  loading = false,
  onConfirm,
  onOpenChange,
}: SubmitPublicationDialogProps) {
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleConfirm = async () => {
    if (!onConfirm) return
    setIsSubmitting(true)
    try {
      await onConfirm()
      onOpenChange(false)
    } finally {
      setIsSubmitting(false)
    }
  }

  if (!open || !publication) return null

  return (
    <Card className="fixed inset-4 md:inset-auto md:left-1/2 md:top-1/2 md:transform md:-translate-x-1/2 md:-translate-y-1/2 md:w-96 z-50 p-6 space-y-4">
      <div>
        <h2 className="text-lg font-semibold">Enviar para Revisión</h2>
        <p className="text-sm text-muted-foreground mt-1">
          Una vez enviada, un moderador revisará tu publicación. Este proceso puede tomar varios días.
        </p>
      </div>

      <div className="bg-muted p-3 rounded text-sm space-y-1">
        <p>
          <span className="text-muted-foreground">ID de Manga:</span> {publication.mangaId}
        </p>
        <p>
          <span className="text-muted-foreground">Creado:</span>{' '}
          {new Date(publication.createdAtUtc).toLocaleDateString('es-ES')}
        </p>
      </div>

      <Alert className="space-y-2">
        <h4 className="font-medium text-sm">Punto de Control:</h4>
        <ul className="text-sm space-y-1 list-disc list-inside text-muted-foreground">
          <li>Verifica que todas las imágenes se hayan subido correctamente</li>
          <li>Comprueba que los metadatos sean completos</li>
          <li>Asegúrate de que el contenido cumple con las normas comunitarias</li>
        </ul>
      </Alert>

      <div className="flex gap-2 justify-end pt-4">
        <Button
          variant="outline"
          onClick={() => onOpenChange(false)}
          disabled={isSubmitting}
        >
          Cancelar
        </Button>
        <Button onClick={handleConfirm} disabled={isSubmitting || loading}>
          {isSubmitting ? 'Enviando...' : 'Enviar para Revisión'}
        </Button>
      </div>

      {/* Overlay backdrop */}
      <div
        className="fixed inset-0 bg-black/50 z-40"
        onClick={() => onOpenChange(false)}
      />
    </Card>
  )
}
