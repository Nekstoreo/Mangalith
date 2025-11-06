'use client'

import { useEffect, useState } from 'react'
import { useModeration } from '@/hooks/useModeration'
import { ModerationQueueList, QueueFilters } from './ModerationQueue'
import { BulkModerationActions, BulkActionData, BulkActionResult } from './BulkModerationActions'
import { ModerationReview, PublicationDetails, ModerationAction, ModerationActionData } from './ModerationReview'
import { ModerationStatistics, ModerationStats } from './ModerationStatistics'
import { ModerationWorkflowGuide } from './ModerationWorkflowGuide'
import { ModerationQueueItem } from '@/services/moderation/client'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Alert } from '@/components/ui/alert'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Badge } from '@/components/ui/badge'
import { 
  Queue, 
  BarChart3, 
  HelpCircle, 
  Settings, 
  Bell,
  RefreshCw,
  Filter,
  Users,
  Clock,
  CheckCircle,
  AlertTriangle
} from 'lucide-react'

interface ModerationDashboardProps {
  moderatorId: string
  isAdmin?: boolean
}

type ViewMode = 'queue' | 'review' | 'bulk-actions' | 'statistics' | 'guide'

export function ModerationDashboard({
  moderatorId,
  isAdmin = false,
}: ModerationDashboardProps) {
  const {
    queue,
    loading,
    error,
    totalCount,
    getModerationQueue,
    bulkAction,
    clearError,
  } = useModeration()

  const [selectedItems, setSelectedItems] = useState<Set<string>>(new Set())
  const [currentPage, setCurrentPage] = useState(1)
  const [viewMode, setViewMode] = useState<ViewMode>('queue')
  const [selectedPublication, setSelectedPublication] = useState<PublicationDetails | null>(null)
  const [queueFilters, setQueueFilters] = useState<QueueFilters>({
    priority: 'all',
    hasReports: null,
    searchTerm: '',
    sortField: 'submittedAt',
    sortDirection: 'desc',
  })
  const [showGuide, setShowGuide] = useState(false)
  const [notifications, setNotifications] = useState<string[]>([])
  const [stats, setStats] = useState<ModerationStats | null>(null)

  useEffect(() => {
    loadQueue()
    if (isAdmin) {
      loadStatistics()
    }
  }, [currentPage, queueFilters])

  const loadQueue = async () => {
    try {
      await getModerationQueue(currentPage, 10)
    } catch (err) {
      console.error('Error loading moderation queue:', err)
    }
  }

  const loadStatistics = async () => {
    // Mock statistics data - replace with actual API call
    const mockStats: ModerationStats = {
      overview: {
        totalPending: totalCount,
        totalProcessed: 156,
        averageReviewTime: 2.5,
        approvalRate: 78,
        rejectionRate: 15,
        revisionRate: 7,
      },
      timeRange: {
        period: '7d',
        processedCount: 45,
        averageTimeToReview: 2.1,
        peakHours: [14, 15, 16, 20, 21],
      },
      moderatorPerformance: [
        {
          moderatorId: moderatorId,
          moderatorUsername: 'Moderador Actual',
          actionsCount: 23,
          averageReviewTime: 1.8,
          approvalRate: 82,
          rejectionRate: 12,
          revisionRate: 6,
          lastActive: new Date().toISOString(),
          efficiency: 4.2,
        },
      ],
      contentTrends: {
        categories: [
          { category: 'Manga', count: 45, approvalRate: 85, averageReviewTime: 2.1 },
          { category: 'Doujinshi', count: 23, approvalRate: 72, averageReviewTime: 3.2 },
          { category: 'Webtoon', count: 12, approvalRate: 90, averageReviewTime: 1.5 },
        ],
        reportTrends: [],
        qualityMetrics: [
          { metric: 'Tiempo Promedio de Revisión', value: 2.5, trend: 'down', description: 'Horas por publicación' },
          { metric: 'Tasa de Aprobación', value: 78, trend: 'up', description: 'Porcentaje de aprobaciones' },
          { metric: 'Satisfacción del Creador', value: 4.2, trend: 'stable', description: 'Puntuación sobre 5' },
        ],
      },
      alerts: [
        {
          id: '1',
          type: 'warning',
          message: 'Cola de moderación por encima del límite recomendado',
          createdAt: new Date().toISOString(),
          isResolved: false,
        },
      ],
    }
    setStats(mockStats)
  }

  const handleSelectItem = (item: ModerationQueueItem) => {
    const newSelected = new Set(selectedItems)
    if (newSelected.has(item.id)) {
      newSelected.delete(item.id)
    } else {
      newSelected.add(item.id)
    }
    setSelectedItems(newSelected)
  }

  const handleSelectAll = (selected: boolean) => {
    if (selected) {
      setSelectedItems(new Set(queue.map((item) => item.id)))
    } else {
      setSelectedItems(new Set())
    }
  }

  const handleBulkAction = async (data: BulkActionData): Promise<BulkActionResult> => {
    try {
      const result = await bulkAction(
        Array.from(selectedItems),
        data.actionType,
        data.comments,
        data.reason
      )

      // Add notification
      setNotifications(prev => [
        ...prev,
        `Acción masiva completada: ${result.successCount} exitosas, ${result.failureCount} fallidas`
      ])

      // Clear selection and reload
      setSelectedItems(new Set())
      await loadQueue()
      
      return result
    } catch (err) {
      console.error('Error performing bulk action:', err)
      throw err
    }
  }

  const handleReviewPublication = async (item: ModerationQueueItem) => {
    // Mock publication details - replace with actual API call
    const mockPublication: PublicationDetails = {
      id: item.id,
      mangaId: item.mangaId,
      mangaTitle: 'Título del Manga de Ejemplo',
      creatorId: item.creatorId,
      creatorUsername: 'CreadorEjemplo',
      submittedAtUtc: item.submittedAtUtc,
      status: 'InReview',
      contentRating: 'Teen',
      isNsfw: false,
      description: 'Descripción del manga...',
      tags: ['Acción', 'Aventura', 'Shonen'],
      chapterCount: 5,
      pageCount: 120,
      fileSize: '45.2 MB',
      reports: [],
      moderationHistory: [],
    }
    
    setSelectedPublication(mockPublication)
    setViewMode('review')
  }

  const handleModerationAction = async (action: ModerationAction, data: ModerationActionData) => {
    try {
      // Mock API call - replace with actual implementation
      console.log('Moderation action:', action, data)
      
      // Add notification
      setNotifications(prev => [
        ...prev,
        `Publicación ${action === 'approve' ? 'aprobada' : action === 'reject' ? 'rechazada' : 'actualizada'} exitosamente`
      ])

      // Return to queue
      setViewMode('queue')
      setSelectedPublication(null)
      await loadQueue()
    } catch (err) {
      console.error('Error performing moderation action:', err)
      throw err
    }
  }

  const clearNotification = (index: number) => {
    setNotifications(prev => prev.filter((_, i) => i !== index))
  }

  const getQueueSummary = () => {
    const highPriority = queue.filter(item => item.priority === 'high').length
    const withReports = queue.filter(item => (item.reportCount || 0) > 0).length
    
    return { highPriority, withReports }
  }

  const { highPriority, withReports } = getQueueSummary()

  // Show individual publication review
  if (viewMode === 'review' && selectedPublication) {
    return (
      <ModerationReview
        publication={selectedPublication}
        onAction={handleModerationAction}
        onBack={() => {
          setViewMode('queue')
          setSelectedPublication(null)
        }}
        isProcessing={loading}
      />
    )
  }

  // Show bulk actions interface
  if (viewMode === 'bulk-actions' && selectedItems.size > 0) {
    return (
      <BulkModerationActions
        selectedIds={Array.from(selectedItems)}
        onAction={handleBulkAction}
        onCancel={() => {
          setViewMode('queue')
          setSelectedItems(new Set())
        }}
        isProcessing={loading}
      />
    )
  }

  return (
    <div className="space-y-6">
      {/* Notifications */}
      {notifications.length > 0 && (
        <div className="space-y-2">
          {notifications.slice(0, 3).map((notification, index) => (
            <Alert key={index} className="bg-green-50 border-green-200">
              <CheckCircle className="w-4 h-4" />
              <div className="flex justify-between items-center">
                <p className="text-sm text-green-700">{notification}</p>
                <button
                  onClick={() => clearNotification(index)}
                  className="text-green-700 hover:text-green-900 text-sm font-medium"
                >
                  ×
                </button>
              </div>
            </Alert>
          ))}
        </div>
      )}

      {/* Error Display */}
      {error && (
        <Alert className="bg-red-50 border-red-200">
          <AlertTriangle className="w-4 h-4" />
          <div className="flex justify-between items-center">
            <p className="text-sm text-red-700">{error}</p>
            <button
              onClick={clearError}
              className="text-red-700 hover:text-red-900 text-sm font-medium"
            >
              Cerrar
            </button>
          </div>
        </Alert>
      )}

      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Panel de Moderación</h1>
          <p className="text-muted-foreground">
            Gestiona la cola de moderación y revisa el contenido pendiente
          </p>
        </div>
        <div className="flex items-center gap-3">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowGuide(!showGuide)}
          >
            <HelpCircle className="w-4 h-4 mr-2" />
            Guía
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={loadQueue}
            disabled={loading}
          >
            <RefreshCw className={`w-4 h-4 mr-2 ${loading ? 'animate-spin' : ''}`} />
            Actualizar
          </Button>
        </div>
      </div>

      {/* Quick Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="p-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
              <Queue className="w-5 h-5 text-blue-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Total en Cola</p>
              <p className="text-2xl font-bold">{totalCount}</p>
            </div>
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-red-100 rounded-full flex items-center justify-center">
              <AlertTriangle className="w-5 h-5 text-red-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Alta Prioridad</p>
              <p className="text-2xl font-bold">{highPriority}</p>
            </div>
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-orange-100 rounded-full flex items-center justify-center">
              <Bell className="w-5 h-5 text-orange-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Con Reportes</p>
              <p className="text-2xl font-bold">{withReports}</p>
            </div>
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-green-100 rounded-full flex items-center justify-center">
              <Users className="w-5 h-5 text-green-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Seleccionados</p>
              <p className="text-2xl font-bold">{selectedItems.size}</p>
            </div>
          </div>
        </Card>
      </div>

      {/* Workflow Guide */}
      {showGuide && (
        <ModerationWorkflowGuide
          onClose={() => setShowGuide(false)}
          compact={false}
        />
      )}

      {/* Main Content */}
      <Tabs value={viewMode} onValueChange={(value) => setViewMode(value as ViewMode)}>
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="queue">
            <Queue className="w-4 h-4 mr-2" />
            Cola de Moderación
            {totalCount > 0 && (
              <Badge className="ml-2 bg-blue-100 text-blue-800">{totalCount}</Badge>
            )}
          </TabsTrigger>
          {isAdmin && (
            <TabsTrigger value="statistics">
              <BarChart3 className="w-4 h-4 mr-2" />
              Estadísticas
            </TabsTrigger>
          )}
          <TabsTrigger value="guide">
            <HelpCircle className="w-4 h-4 mr-2" />
            Guía de Trabajo
          </TabsTrigger>
        </TabsList>

        <TabsContent value="queue" className="space-y-4">
          {/* Bulk Actions Bar */}
          {selectedItems.size > 0 && (
            <Card className="p-4 bg-blue-50 border-blue-200">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <Users className="w-5 h-5 text-blue-600" />
                  <span className="font-medium">
                    {selectedItems.size} elemento(s) seleccionado(s)
                  </span>
                </div>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setSelectedItems(new Set())}
                  >
                    Limpiar Selección
                  </Button>
                  <Button
                    size="sm"
                    onClick={() => setViewMode('bulk-actions')}
                  >
                    Acciones Masivas
                  </Button>
                </div>
              </div>
            </Card>
          )}

          {/* Queue List */}
          <ModerationQueueList
            items={queue}
            selectedItems={selectedItems}
            onSelect={handleSelectItem}
            onSelectAll={handleSelectAll}
            onReview={handleReviewPublication}
            isLoading={loading}
            filters={queueFilters}
            onFiltersChange={setQueueFilters}
            totalCount={totalCount}
          />

          {/* Pagination */}
          {totalCount > 0 && (
            <div className="flex justify-center gap-2">
              <Button
                variant="outline"
                disabled={currentPage === 1 || loading}
                onClick={() => setCurrentPage(currentPage - 1)}
              >
                Anterior
              </Button>
              <span className="flex items-center px-3 text-sm">
                Página {currentPage} de {Math.ceil(totalCount / 10)}
              </span>
              <Button
                variant="outline"
                disabled={currentPage >= Math.ceil(totalCount / 10) || loading}
                onClick={() => setCurrentPage(currentPage + 1)}
              >
                Siguiente
              </Button>
            </div>
          )}
        </TabsContent>

        {isAdmin && (
          <TabsContent value="statistics">
            {stats ? (
              <ModerationStatistics
                stats={stats}
                onRefresh={loadStatistics}
                isLoading={loading}
              />
            ) : (
              <Card className="p-6 text-center">
                <div className="w-16 h-16 bg-muted rounded-full flex items-center justify-center mx-auto mb-4">
                  <BarChart3 className="w-8 h-8 text-muted-foreground" />
                </div>
                <p className="text-muted-foreground">Cargando estadísticas...</p>
              </Card>
            )}
          </TabsContent>
        )}

        <TabsContent value="guide">
          <ModerationWorkflowGuide compact={false} />
        </TabsContent>
      </Tabs>
    </div>
  )
}
