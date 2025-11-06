'use client'

import { useState } from 'react'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { ContentReportCategory } from '@/services/content-report/client'

interface ReportContentDialogProps {
  publicationId: string
  open: boolean
  onOpenChange: (open: boolean) => void
  onSubmit?: (category: ContentReportCategory, description: string) => Promise<void>
  isSubmitting?: boolean
}

const categoryLabels: Record<ContentReportCategory, { label: string; description: string }> = {
  [ContentReportCategory.Copyright]: {
    label: 'Violación de Derechos de Autor',
    description: 'Contenido con derechos de autor no permitido',
  },
  [ContentReportCategory.InappropriateContent]: {
    label: 'Contenido Inapropiado',
    description: 'Contenido no apto para audiencia general',
  },
  [ContentReportCategory.Spam]: {
    label: 'Spam',
    description: 'Contenido duplicado o promocional',
  },
  [ContentReportCategory.Harassment]: {
    label: 'Acoso',
    description: 'Contenido acosador u ofensivo',
  },
  [ContentReportCategory.Violence]: {
    label: 'Violencia Excesiva',
    description: 'Contenido con violencia gráfica extrema',
  },
  [ContentReportCategory.AdultContent]: {
    label: 'Contenido Adulto',
    description: 'Material sexual explícito no etiquetado',
  },
  [ContentReportCategory.Other]: {
    label: 'Otro',
    description: 'Otra razón (especificar en detalles)',
  },
}

export function ReportContentDialog({
  publicationId,
  open,
  onOpenChange,
  onSubmit,
  isSubmitting = false,
}: ReportContentDialogProps) {
  const [selectedCategory, setSelectedCategory] = useState<ContentReportCategory | null>(null)
  const [description, setDescription] = useState('')
  const [submitted, setSubmitted] = useState(false)

  const handleSubmit = async () => {
    if (!selectedCategory) return
    try {
      await onSubmit?.(selectedCategory, description)
      setSubmitted(true)
      setTimeout(() => {
        onOpenChange(false)
        setSelectedCategory(null)
        setDescription('')
        setSubmitted(false)
      }, 1500)
    } catch (err) {
      console.error('Error submitting report:', err)
    }
  }

  if (!open) return null

  if (submitted) {
    return (
      <Card className="fixed inset-4 md:inset-auto md:left-1/2 md:top-1/2 md:transform md:-translate-x-1/2 md:-translate-y-1/2 md:w-96 z-50 p-6 text-center space-y-4">
        <div className="text-4xl">✓</div>
        <h2 className="text-lg font-semibold">Reporte Enviado</h2>
        <p className="text-sm text-muted-foreground">
          Gracias por tu reporte. Nuestro equipo lo revisará pronto.
        </p>
      </Card>
    )
  }

  return (
    <>
      <Card className="fixed inset-4 md:inset-auto md:left-1/2 md:top-1/2 md:transform md:-translate-x-1/2 md:-translate-y-1/2 md:w-96 z-50 p-6 space-y-4 max-h-[90vh] overflow-y-auto">
        <div>
          <h2 className="text-lg font-semibold">Reportar Contenido</h2>
          <p className="text-sm text-muted-foreground mt-1">
            Ayúdanos a mantener la comunidad segura reportando contenido inapropiado
          </p>
        </div>

        <div className="space-y-2">
          <label className="block text-sm font-medium">Categoría</label>
          <div className="space-y-2">
            {Object.entries(categoryLabels).map(([key, { label, description }]) => (
              <button
                key={key}
                onClick={() => setSelectedCategory(parseInt(key) as ContentReportCategory)}
                className={`w-full p-3 rounded border text-left transition-colors ${
                  selectedCategory === parseInt(key)
                    ? 'bg-blue-50 border-blue-300'
                    : 'hover:bg-accent'
                }`}
              >
                <div className="font-medium text-sm">{label}</div>
                <div className="text-xs text-muted-foreground">{description}</div>
              </button>
            ))}
          </div>
        </div>

        <div className="space-y-2">
          <label className="block text-sm font-medium">Detalles (opcional)</label>
          <Textarea
            placeholder="Proporciona más información sobre tu reporte..."
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            className="min-h-24"
          />
        </div>

        <div className="flex gap-2 justify-end pt-4">
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={isSubmitting}
          >
            Cancelar
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={selectedCategory === null || isSubmitting}
          >
            {isSubmitting ? 'Enviando...' : 'Enviar Reporte'}
          </Button>
        </div>
      </Card>

      {/* Overlay backdrop */}
      <div
        className="fixed inset-0 bg-black/50 z-40"
        onClick={() => onOpenChange(false)}
      />
    </>
  )
}
