'use client'

import { useState } from 'react'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { Badge } from '@/components/ui/badge'
import { Alert } from '@/components/ui/alert'
import { ContentReportCategory } from '@/services/content-report/client'
import { AlertTriangle, Flag, Shield, Copyright, MessageSquare, Zap, HelpCircle } from 'lucide-react'

interface ContentReportFormProps {
  publicationId: string
  onSubmit?: (category: ContentReportCategory, description: string) => Promise<void>
  onCancel?: () => void
  isSubmitting?: boolean
  className?: string
}

const categoryConfig: Record<ContentReportCategory, {
  label: string
  description: string
  icon: React.ReactNode
  severity: 'low' | 'medium' | 'high'
  examples: string[]
}> = {
  [ContentReportCategory.Copyright]: {
    label: 'Violación de Derechos de Autor',
    description: 'Contenido que infringe derechos de autor o propiedad intelectual',
    icon: <Copyright className="w-4 h-4" />,
    severity: 'high',
    examples: [
      'Manga oficial sin autorización',
      'Contenido con marca de agua de otros sitios',
      'Uso no autorizado de material protegido'
    ]
  },
  [ContentReportCategory.InappropriateContent]: {
    label: 'Contenido Inapropiado',
    description: 'Contenido no apto para la audiencia general o mal categorizado',
    icon: <AlertTriangle className="w-4 h-4" />,
    severity: 'medium',
    examples: [
      'Contenido adulto sin etiqueta NSFW',
      'Material no apto para menores',
      'Contenido mal categorizado'
    ]
  },
  [ContentReportCategory.Spam]: {
    label: 'Spam',
    description: 'Contenido duplicado, promocional no deseado o irrelevante',
    icon: <Zap className="w-4 h-4" />,
    severity: 'low',
    examples: [
      'Contenido duplicado repetitivo',
      'Promoción excesiva de otros sitios',
      'Publicaciones irrelevantes al tema'
    ]
  },
  [ContentReportCategory.Harassment]: {
    label: 'Acoso o Comportamiento Abusivo',
    description: 'Contenido que promueve acoso, intimidación o comportamiento tóxico',
    icon: <Shield className="w-4 h-4" />,
    severity: 'high',
    examples: [
      'Ataques personales en comentarios',
      'Contenido discriminatorio',
      'Amenazas o intimidación'
    ]
  },
  [ContentReportCategory.Violence]: {
    label: 'Violencia Excesiva',
    description: 'Contenido con violencia gráfica extrema no apropiada',
    icon: <AlertTriangle className="w-4 h-4" />,
    severity: 'high',
    examples: [
      'Violencia gráfica extrema',
      'Contenido que glorifica la violencia',
      'Imágenes perturbadoras sin advertencia'
    ]
  },
  [ContentReportCategory.AdultContent]: {
    label: 'Contenido Adulto No Etiquetado',
    description: 'Material sexual explícito sin las etiquetas apropiadas',
    icon: <AlertTriangle className="w-4 h-4" />,
    severity: 'high',
    examples: [
      'Contenido sexual explícito sin NSFW',
      'Desnudez no etiquetada apropiadamente',
      'Material adulto accesible a menores'
    ]
  },
  [ContentReportCategory.Other]: {
    label: 'Otro Motivo',
    description: 'Otra razón no cubierta por las categorías anteriores',
    icon: <HelpCircle className="w-4 h-4" />,
    severity: 'low',
    examples: [
      'Problemas técnicos con el contenido',
      'Violación de reglas específicas de la comunidad',
      'Otros problemas no listados'
    ]
  }
}

const severityColors = {
  low: 'bg-yellow-100 text-yellow-800 border-yellow-200',
  medium: 'bg-orange-100 text-orange-800 border-orange-200',
  high: 'bg-red-100 text-red-800 border-red-200'
}

export function ContentReportForm({
  publicationId,
  onSubmit,
  onCancel,
  isSubmitting = false,
  className = ''
}: ContentReportFormProps) {
  const [selectedCategory, setSelectedCategory] = useState<ContentReportCategory | null>(null)
  const [description, setDescription] = useState('')
  const [showGuidelines, setShowGuidelines] = useState(true)

  const handleSubmit = async () => {
    if (!selectedCategory) return
    
    try {
      await onSubmit?.(selectedCategory, description)
    } catch (error) {
      console.error('Error submitting report:', error)
    }
  }

  const selectedConfig = selectedCategory !== null ? categoryConfig[selectedCategory] : null

  return (
    <Card className={`p-6 space-y-6 ${className}`}>
      <div className="space-y-2">
        <div className="flex items-center gap-2">
          <Flag className="w-5 h-5 text-red-500" />
          <h2 className="text-xl font-semibold">Reportar Contenido</h2>
        </div>
        <p className="text-sm text-muted-foreground">
          Ayúdanos a mantener una comunidad segura y de calidad reportando contenido que viole nuestras normas.
        </p>
      </div>

      {showGuidelines && (
        <Alert className="border-blue-200 bg-blue-50">
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <h3 className="font-medium text-blue-900">Pautas para Reportar</h3>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setShowGuidelines(false)}
                className="text-blue-700 hover:text-blue-900"
              >
                Ocultar
              </Button>
            </div>
            <ul className="text-sm text-blue-800 space-y-1">
              <li>• Solo reporta contenido que realmente viole las normas de la comunidad</li>
              <li>• Proporciona detalles específicos para ayudar en la revisión</li>
              <li>• Los reportes falsos o malintencionados pueden resultar en sanciones</li>
              <li>• Recibirás una notificación cuando se resuelva tu reporte</li>
            </ul>
          </div>
        </Alert>
      )}

      <div className="space-y-3">
        <label className="block text-sm font-medium">
          Selecciona la categoría del problema *
        </label>
        <div className="grid gap-3">
          {Object.entries(categoryConfig).map(([key, config]) => {
            const categoryKey = parseInt(key) as ContentReportCategory
            const isSelected = selectedCategory === categoryKey
            
            return (
              <button
                key={key}
                onClick={() => setSelectedCategory(categoryKey)}
                className={`w-full p-4 rounded-lg border text-left transition-all ${
                  isSelected
                    ? 'bg-blue-50 border-blue-300 ring-2 ring-blue-200'
                    : 'hover:bg-accent border-border'
                }`}
              >
                <div className="flex items-start gap-3">
                  <div className="mt-0.5 text-muted-foreground">
                    {config.icon}
                  </div>
                  <div className="flex-1 space-y-2">
                    <div className="flex items-center gap-2">
                      <span className="font-medium text-sm">{config.label}</span>
                      <Badge 
                        variant="outline" 
                        className={`text-xs ${severityColors[config.severity]}`}
                      >
                        {config.severity === 'high' ? 'Alta' : 
                         config.severity === 'medium' ? 'Media' : 'Baja'} prioridad
                      </Badge>
                    </div>
                    <p className="text-xs text-muted-foreground">
                      {config.description}
                    </p>
                  </div>
                </div>
              </button>
            )
          })}
        </div>
      </div>

      {selectedConfig && (
        <div className="p-4 bg-muted rounded-lg space-y-3">
          <h4 className="font-medium text-sm">Ejemplos de esta categoría:</h4>
          <ul className="text-sm text-muted-foreground space-y-1">
            {selectedConfig.examples.map((example, index) => (
              <li key={index}>• {example}</li>
            ))}
          </ul>
        </div>
      )}

      <div className="space-y-2">
        <label className="block text-sm font-medium">
          Descripción detallada {selectedCategory !== null && '*'}
        </label>
        <Textarea
          placeholder={
            selectedCategory !== null
              ? "Describe específicamente el problema que encontraste. Incluye detalles que ayuden a nuestro equipo a entender y revisar el reporte..."
              : "Selecciona una categoría primero..."
          }
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          className="min-h-24"
          disabled={selectedCategory === null}
        />
        <p className="text-xs text-muted-foreground">
          {selectedCategory !== null && selectedConfig?.severity === 'high' && (
            <span className="text-red-600 font-medium">
              Reportes de alta prioridad requieren descripción detallada.
            </span>
          )}
        </p>
      </div>

      <div className="flex gap-3 justify-end pt-4 border-t">
        <Button
          variant="outline"
          onClick={onCancel}
          disabled={isSubmitting}
        >
          Cancelar
        </Button>
        <Button
          onClick={handleSubmit}
          disabled={
            selectedCategory === null || 
            isSubmitting ||
            (selectedConfig?.severity === 'high' && description.trim().length < 10)
          }
        >
          {isSubmitting ? 'Enviando...' : 'Enviar Reporte'}
        </Button>
      </div>

      <div className="text-xs text-muted-foreground pt-2 border-t">
        <p>
          Al enviar este reporte, confirmas que la información proporcionada es precisa y que 
          has leído nuestras <span className="text-blue-600 cursor-pointer hover:underline">normas de la comunidad</span>.
        </p>
      </div>
    </Card>
  )
}