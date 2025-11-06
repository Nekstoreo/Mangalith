'use client'

import { useState } from 'react'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Progress } from '@/components/ui/progress'
import { Alert } from '@/components/ui/alert'
import { 
  CheckCircle, 
  XCircle, 
  AlertTriangle, 
  Shield,
  Users,
  Clock,
  Zap,
  Info,
  X
} from 'lucide-react'

export type BulkActionType = 'approve' | 'reject' | 'request-revision' | 'archive'

export interface BulkActionData {
  actionType: BulkActionType
  comments: string
  reason?: string
  contentRating?: 'General' | 'Teen' | 'Mature' | 'Adult'
  isNsfw?: boolean
}

export interface BulkActionResult {
  successCount: number
  failureCount: number
  errors?: string[]
  processedIds?: string[]
}

interface BulkModerationActionsProps {
  selectedIds: string[]
  onAction: (data: BulkActionData) => Promise<BulkActionResult>
  onCancel: () => void
  isProcessing?: boolean
}

export function BulkModerationActions({
  selectedIds,
  onAction,
  onCancel,
  isProcessing = false,
}: BulkModerationActionsProps) {
  const [actionType, setActionType] = useState<BulkActionType | ''>('')
  const [comments, setComments] = useState('')
  const [reason, setReason] = useState('')
  const [contentRating, setContentRating] = useState<'General' | 'Teen' | 'Mature' | 'Adult'>('General')
  const [isNsfw, setIsNsfw] = useState(false)
  const [result, setResult] = useState<BulkActionResult | null>(null)
  const [showAdvanced, setShowAdvanced] = useState(false)

  const handleSubmit = async () => {
    if (!actionType || !comments.trim()) return

    const actionData: BulkActionData = {
      actionType: actionType as BulkActionType,
      comments: comments.trim(),
      reason: actionType === 'reject' ? reason : undefined,
      contentRating: actionType === 'approve' ? contentRating : undefined,
      isNsfw: actionType === 'approve' ? isNsfw : undefined,
    }

    try {
      const result = await onAction(actionData)
      setResult(result)
    } catch (error) {
      console.error('Bulk action failed:', error)
    }
  }

  const canSubmit = actionType && comments.trim() && 
    (actionType !== 'reject' || reason.trim()) &&
    !isProcessing

  const actionConfig = {
    approve: {
      label: 'Aprobar Publicaciones',
      color: 'bg-green-600 hover:bg-green-700',
      icon: CheckCircle,
      description: 'Aprobar todas las publicaciones seleccionadas',
      bgColor: 'bg-green-50 border-green-200',
      textColor: 'text-green-800'
    },
    reject: {
      label: 'Rechazar Publicaciones',
      color: 'bg-red-600 hover:bg-red-700',
      icon: XCircle,
      description: 'Rechazar todas las publicaciones seleccionadas',
      bgColor: 'bg-red-50 border-red-200',
      textColor: 'text-red-800'
    },
    'request-revision': {
      label: 'Solicitar Revisiones',
      color: 'bg-yellow-600 hover:bg-yellow-700',
      icon: AlertTriangle,
      description: 'Solicitar cambios en todas las publicaciones seleccionadas',
      bgColor: 'bg-yellow-50 border-yellow-200',
      textColor: 'text-yellow-800'
    },
    archive: {
      label: 'Archivar Publicaciones',
      color: 'bg-gray-600 hover:bg-gray-700',
      icon: Shield,
      description: 'Archivar todas las publicaciones seleccionadas',
      bgColor: 'bg-gray-50 border-gray-200',
      textColor: 'text-gray-800'
    }
  }

  const rejectionReasons = [
    { value: 'inappropriate-content', label: 'Contenido inapropiado' },
    { value: 'copyright-violation', label: 'Violación de derechos de autor' },
    { value: 'poor-quality', label: 'Calidad insuficiente' },
    { value: 'incomplete', label: 'Contenido incompleto' },
    { value: 'spam', label: 'Spam o contenido irrelevante' },
    { value: 'policy-violation', label: 'Violación de políticas' },
    { value: 'other', label: 'Otra razón' },
  ]

  if (result) {
    return (
      <Card className="p-6">
        <div className="text-center space-y-4">
          <div className="mx-auto w-16 h-16 bg-green-100 rounded-full flex items-center justify-center">
            <CheckCircle className="w-8 h-8 text-green-600" />
          </div>
          <div>
            <h3 className="text-lg font-semibold">Acción Completada</h3>
            <p className="text-muted-foreground">
              Se procesaron {selectedIds.length} publicaciones
            </p>
          </div>
          
          <div className="grid grid-cols-2 gap-4 max-w-md mx-auto">
            <div className="text-center p-3 bg-green-50 rounded-lg">
              <p className="text-2xl font-bold text-green-600">{result.successCount}</p>
              <p className="text-sm text-green-700">Exitosas</p>
            </div>
            <div className="text-center p-3 bg-red-50 rounded-lg">
              <p className="text-2xl font-bold text-red-600">{result.failureCount}</p>
              <p className="text-sm text-red-700">Fallidas</p>
            </div>
          </div>

          {result.errors && result.errors.length > 0 && (
            <Alert className="text-left">
              <AlertTriangle className="w-4 h-4" />
              <div>
                <p className="font-medium">Errores encontrados:</p>
                <ul className="text-sm mt-2 space-y-1">
                  {result.errors.map((error, index) => (
                    <li key={index} className="text-muted-foreground">• {error}</li>
                  ))}
                </ul>
              </div>
            </Alert>
          )}

          <Button onClick={onCancel} className="w-full">
            Continuar
          </Button>
        </div>
      </Card>
    )
  }

  return (
    <Card className="p-6">
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
              <Users className="w-5 h-5 text-blue-600" />
            </div>
            <div>
              <h3 className="text-lg font-semibold">Acción Masiva</h3>
              <p className="text-sm text-muted-foreground">
                {selectedIds.length} publicaciones seleccionadas
              </p>
            </div>
          </div>
          <Button variant="ghost" onClick={onCancel} disabled={isProcessing}>
            <X className="w-4 h-4" />
          </Button>
        </div>

        {/* Selected Items Preview */}
        <div className="p-3 bg-muted rounded-lg">
          <div className="flex items-center gap-2 mb-2">
            <Info className="w-4 h-4 text-muted-foreground" />
            <span className="text-sm font-medium">Publicaciones Seleccionadas</span>
          </div>
          <div className="flex flex-wrap gap-1">
            {selectedIds.slice(0, 10).map((id) => (
              <Badge key={id} variant="outline" className="text-xs font-mono">
                {id.substring(0, 8)}
              </Badge>
            ))}
            {selectedIds.length > 10 && (
              <Badge variant="outline" className="text-xs">
                +{selectedIds.length - 10} más
              </Badge>
            )}
          </div>
        </div>

        {/* Action Selection */}
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-2">
              Seleccionar Acción <span className="text-red-500">*</span>
            </label>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              {Object.entries(actionConfig).map(([action, config]) => {
                const Icon = config.icon
                return (
                  <button
                    key={action}
                    onClick={() => setActionType(action as BulkActionType)}
                    disabled={isProcessing}
                    className={`p-4 rounded-lg border-2 transition-all text-left ${
                      actionType === action
                        ? 'border-primary bg-primary/5'
                        : 'border-gray-200 hover:border-gray-300'
                    } ${isProcessing ? 'opacity-50 cursor-not-allowed' : ''}`}
                  >
                    <div className="flex items-center gap-3">
                      <Icon className="w-5 h-5" />
                      <div>
                        <p className="font-medium text-sm">{config.label}</p>
                        <p className="text-xs text-muted-foreground">{config.description}</p>
                      </div>
                    </div>
                  </button>
                )
              })}
            </div>
          </div>

          {/* Action-specific Configuration */}
          {actionType === 'approve' && (
            <div className={`p-4 rounded-lg border ${actionConfig.approve.bgColor}`}>
              <h4 className={`font-medium mb-3 ${actionConfig.approve.textColor}`}>
                Configuración de Aprobación
              </h4>
              <div className="space-y-3">
                <div>
                  <label className="block text-sm font-medium mb-2">
                    Clasificación de Contenido por Defecto
                  </label>
                  <Select value={contentRating} onValueChange={(value: any) => setContentRating(value)}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="General">General (Todas las edades)</SelectItem>
                      <SelectItem value="Teen">Adolescente (13+)</SelectItem>
                      <SelectItem value="Mature">Maduro (17+)</SelectItem>
                      <SelectItem value="Adult">Adulto (18+)</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    id="bulk-nsfw"
                    checked={isNsfw}
                    onChange={(e) => setIsNsfw(e.target.checked)}
                    className="w-4 h-4"
                  />
                  <label htmlFor="bulk-nsfw" className="text-sm font-medium">
                    Marcar todas como NSFW
                  </label>
                </div>
              </div>
            </div>
          )}

          {actionType === 'reject' && (
            <div className={`p-4 rounded-lg border ${actionConfig.reject.bgColor}`}>
              <h4 className={`font-medium mb-3 ${actionConfig.reject.textColor}`}>
                Razón del Rechazo
              </h4>
              <Select value={reason} onValueChange={setReason}>
                <SelectTrigger>
                  <SelectValue placeholder="Selecciona una razón" />
                </SelectTrigger>
                <SelectContent>
                  {rejectionReasons.map((reasonOption) => (
                    <SelectItem key={reasonOption.value} value={reasonOption.value}>
                      {reasonOption.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          )}

          {/* Comments */}
          <div className="space-y-2">
            <label className="block text-sm font-medium">
              Comentarios para los Creadores <span className="text-red-500">*</span>
            </label>
            <Textarea
              placeholder="Proporciona retroalimentación que será enviada a todos los creadores..."
              value={comments}
              onChange={(e) => setComments(e.target.value)}
              className="min-h-24"
              disabled={isProcessing}
            />
            <p className="text-xs text-muted-foreground">
              Este mensaje será enviado a todos los creadores de las publicaciones seleccionadas
            </p>
          </div>

          {/* Advanced Options */}
          <div className="border-t pt-4">
            <button
              onClick={() => setShowAdvanced(!showAdvanced)}
              className="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground"
            >
              <Zap className="w-4 h-4" />
              Opciones Avanzadas
              <span className="text-xs">({showAdvanced ? 'ocultar' : 'mostrar'})</span>
            </button>
            
            {showAdvanced && (
              <div className="mt-3 p-3 bg-muted rounded-lg">
                <Alert>
                  <Clock className="w-4 h-4" />
                  <div>
                    <p className="font-medium">Procesamiento por Lotes</p>
                    <p className="text-sm text-muted-foreground">
                      Las acciones masivas se procesan en lotes para evitar sobrecargar el sistema.
                      El proceso puede tomar varios minutos dependiendo del número de publicaciones.
                    </p>
                  </div>
                </Alert>
              </div>
            )}
          </div>

          {/* Submit Button */}
          <div className="flex gap-3 pt-4 border-t">
            <Button
              variant="outline"
              onClick={onCancel}
              disabled={isProcessing}
              className="flex-1"
            >
              Cancelar
            </Button>
            <Button
              onClick={handleSubmit}
              disabled={!canSubmit}
              className={`flex-1 ${actionType ? actionConfig[actionType as BulkActionType].color : ''} text-white`}
            >
              {isProcessing ? (
                <div className="flex items-center gap-2">
                  <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                  Procesando...
                </div>
              ) : (
                `Aplicar a ${selectedIds.length} Publicaciones`
              )}
            </Button>
          </div>
        </div>
      </div>
    </Card>
  )
}

interface BulkActionProgressProps {
  current: number
  total: number
  currentItem?: string
}

export function BulkActionProgress({ current, total, currentItem }: BulkActionProgressProps) {
  const percentage = Math.round((current / total) * 100)

  return (
    <Card className="p-6">
      <div className="space-y-4">
        <div className="text-center">
          <h3 className="text-lg font-semibold">Procesando Acción Masiva</h3>
          <p className="text-muted-foreground">
            {current} de {total} publicaciones procesadas
          </p>
        </div>

        <div className="space-y-2">
          <div className="flex justify-between text-sm">
            <span>Progreso</span>
            <span>{percentage}%</span>
          </div>
          <Progress value={percentage} className="h-2" />
        </div>

        {currentItem && (
          <div className="text-center">
            <p className="text-sm text-muted-foreground">
              Procesando: <code className="bg-muted px-2 py-1 rounded text-xs">{currentItem}</code>
            </p>
          </div>
        )}

        <div className="flex items-center justify-center gap-2 text-sm text-muted-foreground">
          <div className="w-4 h-4 border-2 border-primary border-t-transparent rounded-full animate-spin" />
          Por favor, no cierres esta ventana...
        </div>
      </div>
    </Card>
  )
}