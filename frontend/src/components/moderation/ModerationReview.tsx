'use client'

import { useState } from 'react'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Alert } from '@/components/ui/alert'
import { Progress } from '@/components/ui/progress'
import { 
  CheckCircle, 
  XCircle, 
  AlertTriangle, 
  Eye, 
  FileText, 
  User, 
  Calendar,
  Clock,
  MessageSquare,
  Flag,
  ArrowLeft,
  Shield
} from 'lucide-react'

export interface PublicationDetails {
  id: string
  mangaId: string
  mangaTitle: string
  creatorId: string
  creatorUsername: string
  submittedAtUtc: string
  status: string
  contentRating?: 'General' | 'Teen' | 'Mature' | 'Adult'
  isNsfw: boolean
  description?: string
  tags: string[]
  chapterCount: number
  pageCount: number
  fileSize: string
  reports: ContentReport[]
  moderationHistory: ModerationHistoryItem[]
}

export interface ContentReport {
  id: string
  category: string
  description: string
  reportedByUsername: string
  createdAtUtc: string
  status: string
}

export interface ModerationHistoryItem {
  id: string
  actionType: string
  moderatorUsername: string
  comments: string
  createdAtUtc: string
}

export type ModerationAction = 'approve' | 'reject' | 'request-revision' | 'archive'

interface ModerationReviewProps {
  publication: PublicationDetails
  onAction: (action: ModerationAction, data: ModerationActionData) => Promise<void>
  onBack: () => void
  isProcessing?: boolean
}

export interface ModerationActionData {
  comments: string
  reason?: string
  contentRating?: 'General' | 'Teen' | 'Mature' | 'Adult'
  isNsfw?: boolean
  tags?: string[]
}

export function ModerationReview({
  publication,
  onAction,
  onBack,
  isProcessing = false,
}: ModerationReviewProps) {
  const [selectedAction, setSelectedAction] = useState<ModerationAction | null>(null)
  const [comments, setComments] = useState('')
  const [reason, setReason] = useState('')
  const [contentRating, setContentRating] = useState<'General' | 'Teen' | 'Mature' | 'Adult'>(
    publication.contentRating || 'General'
  )
  const [isNsfw, setIsNsfw] = useState(publication.isNsfw)
  const [activeTab, setActiveTab] = useState('overview')

  const handleSubmitAction = async () => {
    if (!selectedAction || !comments.trim()) return

    const actionData: ModerationActionData = {
      comments: comments.trim(),
      reason: selectedAction === 'reject' ? reason : undefined,
      contentRating: selectedAction === 'approve' ? contentRating : undefined,
      isNsfw: selectedAction === 'approve' ? isNsfw : undefined,
    }

    await onAction(selectedAction, actionData)
  }

  const canSubmit = selectedAction && comments.trim() && 
    (selectedAction !== 'reject' || reason.trim())

  const actionConfig = {
    approve: {
      label: 'Aprobar Publicación',
      color: 'bg-green-600 hover:bg-green-700',
      icon: CheckCircle,
      description: 'La publicación cumple con los estándares y será publicada'
    },
    reject: {
      label: 'Rechazar Publicación',
      color: 'bg-red-600 hover:bg-red-700',
      icon: XCircle,
      description: 'La publicación no cumple con los estándares y será rechazada'
    },
    'request-revision': {
      label: 'Solicitar Revisión',
      color: 'bg-yellow-600 hover:bg-yellow-700',
      icon: AlertTriangle,
      description: 'La publicación necesita cambios antes de ser aprobada'
    },
    archive: {
      label: 'Archivar',
      color: 'bg-gray-600 hover:bg-gray-700',
      icon: Shield,
      description: 'Archivar la publicación por violaciones graves'
    }
  }

  const getStatusBadge = (status: string) => {
    const statusConfig = {
      'InReview': { color: 'bg-blue-100 text-blue-800', label: 'En Revisión' },
      'UnderReview': { color: 'bg-orange-100 text-orange-800', label: 'Bajo Revisión' },
      'NeedsRevision': { color: 'bg-yellow-100 text-yellow-800', label: 'Necesita Revisión' },
    }
    const config = statusConfig[status as keyof typeof statusConfig] || 
      { color: 'bg-gray-100 text-gray-800', label: status }
    
    return <Badge className={config.color}>{config.label}</Badge>
  }

  const getRatingBadge = (rating: string) => {
    const ratingConfig = {
      'General': { color: 'bg-green-100 text-green-800' },
      'Teen': { color: 'bg-blue-100 text-blue-800' },
      'Mature': { color: 'bg-orange-100 text-orange-800' },
      'Adult': { color: 'bg-red-100 text-red-800' },
    }
    const config = ratingConfig[rating as keyof typeof ratingConfig] || 
      { color: 'bg-gray-100 text-gray-800' }
    
    return <Badge className={config.color}>{rating}</Badge>
  }

  const timeAgo = (date: string) => {
    const now = new Date()
    const past = new Date(date)
    const diffInHours = Math.floor((now.getTime() - past.getTime()) / (1000 * 60 * 60))
    
    if (diffInHours < 1) return 'Hace menos de 1 hora'
    if (diffInHours < 24) return `Hace ${diffInHours} horas`
    const diffInDays = Math.floor(diffInHours / 24)
    return `Hace ${diffInDays} días`
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" onClick={onBack} disabled={isProcessing}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Volver a la Cola
          </Button>
          <div>
            <h1 className="text-2xl font-bold">Revisión de Publicación</h1>
            <p className="text-muted-foreground">
              ID: <code className="bg-muted px-2 py-1 rounded text-sm">{publication.id}</code>
            </p>
          </div>
        </div>
        {getStatusBadge(publication.status)}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Content */}
        <div className="lg:col-span-2 space-y-6">
          <Tabs value={activeTab} onValueChange={setActiveTab}>
            <TabsList className="grid w-full grid-cols-4">
              <TabsTrigger value="overview">
                <Eye className="w-4 h-4 mr-2" />
                Resumen
              </TabsTrigger>
              <TabsTrigger value="content">
                <FileText className="w-4 h-4 mr-2" />
                Contenido
              </TabsTrigger>
              <TabsTrigger value="reports">
                <Flag className="w-4 h-4 mr-2" />
                Reportes ({publication.reports.length})
              </TabsTrigger>
              <TabsTrigger value="history">
                <Clock className="w-4 h-4 mr-2" />
                Historial
              </TabsTrigger>
            </TabsList>

            <TabsContent value="overview" className="space-y-4">
              <Card className="p-6">
                <h3 className="text-lg font-semibold mb-4">Información General</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-3">
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Título del Manga</label>
                      <p className="font-medium">{publication.mangaTitle}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Creador</label>
                      <div className="flex items-center gap-2">
                        <User className="w-4 h-4" />
                        <span>{publication.creatorUsername}</span>
                        <code className="text-xs bg-muted px-1 rounded">
                          {publication.creatorId.substring(0, 8)}
                        </code>
                      </div>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Fecha de Envío</label>
                      <div className="flex items-center gap-2">
                        <Calendar className="w-4 h-4" />
                        <span>{new Date(publication.submittedAtUtc).toLocaleString('es-ES')}</span>
                        <span className="text-xs text-muted-foreground">({timeAgo(publication.submittedAtUtc)})</span>
                      </div>
                    </div>
                  </div>
                  <div className="space-y-3">
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Clasificación Actual</label>
                      <div className="flex items-center gap-2">
                        {publication.contentRating && getRatingBadge(publication.contentRating)}
                        {publication.isNsfw && <Badge className="bg-red-100 text-red-800">NSFW</Badge>}
                      </div>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Estadísticas</label>
                      <div className="space-y-1 text-sm">
                        <p>{publication.chapterCount} capítulos</p>
                        <p>{publication.pageCount} páginas totales</p>
                        <p>Tamaño: {publication.fileSize}</p>
                      </div>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Etiquetas</label>
                      <div className="flex flex-wrap gap-1 mt-1">
                        {publication.tags.map((tag, index) => (
                          <Badge key={index} variant="outline" className="text-xs">
                            {tag}
                          </Badge>
                        ))}
                      </div>
                    </div>
                  </div>
                </div>
                {publication.description && (
                  <div className="mt-4">
                    <label className="text-sm font-medium text-muted-foreground">Descripción</label>
                    <p className="mt-1 text-sm bg-muted p-3 rounded">{publication.description}</p>
                  </div>
                )}
              </Card>
            </TabsContent>

            <TabsContent value="content" className="space-y-4">
              <Card className="p-6">
                <h3 className="text-lg font-semibold mb-4">Vista Previa del Contenido</h3>
                <div className="bg-muted rounded-lg p-8 text-center">
                  <FileText className="w-16 h-16 mx-auto text-muted-foreground mb-4" />
                  <p className="text-muted-foreground">
                    Vista previa del contenido no implementada
                  </p>
                  <p className="text-sm text-muted-foreground mt-2">
                    Aquí se mostraría una vista previa de las páginas del manga
                  </p>
                </div>
              </Card>
            </TabsContent>

            <TabsContent value="reports" className="space-y-4">
              {publication.reports.length === 0 ? (
                <Card className="p-6 text-center">
                  <Flag className="w-12 h-12 mx-auto text-muted-foreground mb-4" />
                  <p className="text-muted-foreground">No hay reportes para esta publicación</p>
                </Card>
              ) : (
                <div className="space-y-3">
                  {publication.reports.map((report) => (
                    <Card key={report.id} className="p-4">
                      <div className="flex justify-between items-start mb-2">
                        <div className="flex items-center gap-2">
                          <Badge variant="outline">{report.category}</Badge>
                          <span className="text-sm text-muted-foreground">
                            por {report.reportedByUsername}
                          </span>
                        </div>
                        <span className="text-xs text-muted-foreground">
                          {timeAgo(report.createdAtUtc)}
                        </span>
                      </div>
                      <p className="text-sm">{report.description}</p>
                    </Card>
                  ))}
                </div>
              )}
            </TabsContent>

            <TabsContent value="history" className="space-y-4">
              {publication.moderationHistory.length === 0 ? (
                <Card className="p-6 text-center">
                  <Clock className="w-12 h-12 mx-auto text-muted-foreground mb-4" />
                  <p className="text-muted-foreground">No hay historial de moderación</p>
                </Card>
              ) : (
                <div className="space-y-3">
                  {publication.moderationHistory.map((action) => (
                    <Card key={action.id} className="p-4">
                      <div className="flex justify-between items-start mb-2">
                        <div className="flex items-center gap-2">
                          <Badge variant="outline">{action.actionType}</Badge>
                          <span className="text-sm text-muted-foreground">
                            por {action.moderatorUsername}
                          </span>
                        </div>
                        <span className="text-xs text-muted-foreground">
                          {timeAgo(action.createdAtUtc)}
                        </span>
                      </div>
                      <p className="text-sm">{action.comments}</p>
                    </Card>
                  ))}
                </div>
              )}
            </TabsContent>
          </Tabs>
        </div>

        {/* Action Panel */}
        <div className="space-y-6">
          <Card className="p-6">
            <h3 className="text-lg font-semibold mb-4">Acciones de Moderación</h3>
            
            {publication.reports.length > 0 && (
              <Alert className="mb-4 bg-orange-50 border-orange-200">
                <AlertTriangle className="w-4 h-4" />
                <div>
                  <p className="font-medium">Atención: Contenido Reportado</p>
                  <p className="text-sm text-muted-foreground">
                    Esta publicación tiene {publication.reports.length} reporte(s) comunitario(s)
                  </p>
                </div>
              </Alert>
            )}

            <div className="space-y-3 mb-6">
              {Object.entries(actionConfig).map(([action, config]) => {
                const Icon = config.icon
                return (
                  <button
                    key={action}
                    onClick={() => setSelectedAction(action as ModerationAction)}
                    className={`w-full p-3 rounded-lg border-2 transition-all text-left ${
                      selectedAction === action
                        ? 'border-primary bg-primary/5'
                        : 'border-gray-200 hover:border-gray-300'
                    }`}
                  >
                    <div className="flex items-center gap-3">
                      <Icon className="w-5 h-5" />
                      <div>
                        <p className="font-medium">{config.label}</p>
                        <p className="text-xs text-muted-foreground">{config.description}</p>
                      </div>
                    </div>
                  </button>
                )
              })}
            </div>

            {selectedAction === 'approve' && (
              <div className="space-y-4 mb-6 p-4 bg-green-50 rounded-lg border border-green-200">
                <h4 className="font-medium text-green-800">Configuración de Aprobación</h4>
                <div className="space-y-3">
                  <div>
                    <label className="block text-sm font-medium mb-2">Clasificación de Contenido</label>
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
                      id="nsfw"
                      checked={isNsfw}
                      onChange={(e) => setIsNsfw(e.target.checked)}
                      className="w-4 h-4"
                    />
                    <label htmlFor="nsfw" className="text-sm font-medium">
                      Marcar como NSFW (No Seguro Para el Trabajo)
                    </label>
                  </div>
                </div>
              </div>
            )}

            {selectedAction === 'reject' && (
              <div className="space-y-4 mb-6 p-4 bg-red-50 rounded-lg border border-red-200">
                <h4 className="font-medium text-red-800">Razón del Rechazo</h4>
                <Select value={reason} onValueChange={setReason}>
                  <SelectTrigger>
                    <SelectValue placeholder="Selecciona una razón" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="inappropriate-content">Contenido inapropiado</SelectItem>
                    <SelectItem value="copyright-violation">Violación de derechos de autor</SelectItem>
                    <SelectItem value="poor-quality">Calidad insuficiente</SelectItem>
                    <SelectItem value="incomplete">Contenido incompleto</SelectItem>
                    <SelectItem value="spam">Spam o contenido irrelevante</SelectItem>
                    <SelectItem value="policy-violation">Violación de políticas</SelectItem>
                    <SelectItem value="other">Otra razón</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            )}

            <div className="space-y-2">
              <label className="block text-sm font-medium">
                Comentarios {selectedAction && <span className="text-red-500">*</span>}
              </label>
              <Textarea
                placeholder="Proporciona retroalimentación detallada para el creador..."
                value={comments}
                onChange={(e) => setComments(e.target.value)}
                className="min-h-24"
              />
              <p className="text-xs text-muted-foreground">
                Los comentarios serán enviados al creador de la publicación
              </p>
            </div>

            {selectedAction && (
              <Button
                onClick={handleSubmitAction}
                disabled={!canSubmit || isProcessing}
                className={`w-full mt-4 ${actionConfig[selectedAction].color} text-white`}
              >
                {isProcessing ? (
                  <div className="flex items-center gap-2">
                    <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                    Procesando...
                  </div>
                ) : (
                  actionConfig[selectedAction].label
                )}
              </Button>
            )}
          </Card>
        </div>
      </div>
    </div>
  )
}