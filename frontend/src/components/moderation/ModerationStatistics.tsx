'use client'

import { useState, useEffect } from 'react'
import { Card } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Progress } from '@/components/ui/progress'
import { 
  BarChart3, 
  TrendingUp, 
  TrendingDown, 
  Clock, 
  CheckCircle, 
  XCircle, 
  AlertTriangle,
  Users,
  FileText,
  Calendar,
  Target,
  Award,
  Activity,
  RefreshCw
} from 'lucide-react'

export interface ModerationStats {
  overview: {
    totalPending: number
    totalProcessed: number
    averageReviewTime: number // in hours
    approvalRate: number // percentage
    rejectionRate: number // percentage
    revisionRate: number // percentage
  }
  timeRange: {
    period: string
    processedCount: number
    averageTimeToReview: number
    peakHours: number[]
  }
  moderatorPerformance: ModeratorStats[]
  contentTrends: {
    categories: CategoryStats[]
    reportTrends: ReportTrend[]
    qualityMetrics: QualityMetric[]
  }
  alerts: SystemAlert[]
}

export interface ModeratorStats {
  moderatorId: string
  moderatorUsername: string
  actionsCount: number
  averageReviewTime: number
  approvalRate: number
  rejectionRate: number
  revisionRate: number
  lastActive: string
  efficiency: number // actions per hour
}

export interface CategoryStats {
  category: string
  count: number
  approvalRate: number
  averageReviewTime: number
}

export interface ReportTrend {
  date: string
  reportCount: number
  resolvedCount: number
  category: string
}

export interface QualityMetric {
  metric: string
  value: number
  trend: 'up' | 'down' | 'stable'
  description: string
}

export interface SystemAlert {
  id: string
  type: 'warning' | 'error' | 'info'
  message: string
  createdAt: string
  isResolved: boolean
}

interface ModerationStatisticsProps {
  stats: ModerationStats
  onRefresh: () => void
  isLoading?: boolean
}

export function ModerationStatistics({
  stats,
  onRefresh,
  isLoading = false,
}: ModerationStatisticsProps) {
  const [timeRange, setTimeRange] = useState('7d')
  const [selectedModerator, setSelectedModerator] = useState<string>('all')

  const formatTime = (hours: number) => {
    if (hours < 1) return `${Math.round(hours * 60)}m`
    if (hours < 24) return `${Math.round(hours)}h`
    return `${Math.round(hours / 24)}d`
  }

  const formatPercentage = (value: number) => `${Math.round(value)}%`

  const getEfficiencyColor = (efficiency: number) => {
    if (efficiency >= 5) return 'text-green-600'
    if (efficiency >= 3) return 'text-yellow-600'
    return 'text-red-600'
  }

  const getTrendIcon = (trend: 'up' | 'down' | 'stable') => {
    switch (trend) {
      case 'up': return <TrendingUp className="w-4 h-4 text-green-600" />
      case 'down': return <TrendingDown className="w-4 h-4 text-red-600" />
      default: return <Activity className="w-4 h-4 text-gray-600" />
    }
  }

  const getAlertIcon = (type: 'warning' | 'error' | 'info') => {
    switch (type) {
      case 'error': return <XCircle className="w-4 h-4 text-red-600" />
      case 'warning': return <AlertTriangle className="w-4 h-4 text-yellow-600" />
      default: return <CheckCircle className="w-4 h-4 text-blue-600" />
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Card key={i} className="p-6 animate-pulse">
              <div className="h-4 bg-muted rounded w-3/4 mb-2" />
              <div className="h-8 bg-muted rounded w-1/2" />
            </Card>
          ))}
        </div>
        <Card className="p-6 animate-pulse">
          <div className="h-64 bg-muted rounded" />
        </Card>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold">Estadísticas de Moderación</h2>
          <p className="text-muted-foreground">
            Métricas de rendimiento y análisis del sistema de moderación
          </p>
        </div>
        <div className="flex items-center gap-3">
          <Select value={timeRange} onValueChange={setTimeRange}>
            <SelectTrigger className="w-32">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="24h">Últimas 24h</SelectItem>
              <SelectItem value="7d">Últimos 7 días</SelectItem>
              <SelectItem value="30d">Últimos 30 días</SelectItem>
              <SelectItem value="90d">Últimos 90 días</SelectItem>
            </SelectContent>
          </Select>
          <Button onClick={onRefresh} variant="outline" size="sm">
            <RefreshCw className="w-4 h-4 mr-2" />
            Actualizar
          </Button>
        </div>
      </div>

      {/* System Alerts */}
      {stats.alerts.filter(alert => !alert.isResolved).length > 0 && (
        <div className="space-y-2">
          {stats.alerts
            .filter(alert => !alert.isResolved)
            .slice(0, 3)
            .map((alert) => (
              <div
                key={alert.id}
                className={`p-3 rounded-lg border flex items-center gap-3 ${
                  alert.type === 'error' 
                    ? 'bg-red-50 border-red-200' 
                    : alert.type === 'warning'
                    ? 'bg-yellow-50 border-yellow-200'
                    : 'bg-blue-50 border-blue-200'
                }`}
              >
                {getAlertIcon(alert.type)}
                <div className="flex-1">
                  <p className="text-sm font-medium">{alert.message}</p>
                  <p className="text-xs text-muted-foreground">
                    {new Date(alert.createdAt).toLocaleString('es-ES')}
                  </p>
                </div>
              </div>
            ))}
        </div>
      )}

      {/* Overview Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card className="p-6">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
              <Clock className="w-5 h-5 text-blue-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Pendientes</p>
              <p className="text-2xl font-bold">{stats.overview.totalPending}</p>
            </div>
          </div>
        </Card>

        <Card className="p-6">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-green-100 rounded-full flex items-center justify-center">
              <CheckCircle className="w-5 h-5 text-green-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Procesadas</p>
              <p className="text-2xl font-bold">{stats.overview.totalProcessed}</p>
            </div>
          </div>
        </Card>

        <Card className="p-6">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-purple-100 rounded-full flex items-center justify-center">
              <Target className="w-5 h-5 text-purple-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Tiempo Promedio</p>
              <p className="text-2xl font-bold">{formatTime(stats.overview.averageReviewTime)}</p>
            </div>
          </div>
        </Card>

        <Card className="p-6">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-orange-100 rounded-full flex items-center justify-center">
              <Award className="w-5 h-5 text-orange-600" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Tasa de Aprobación</p>
              <p className="text-2xl font-bold">{formatPercentage(stats.overview.approvalRate)}</p>
            </div>
          </div>
        </Card>
      </div>

      {/* Detailed Statistics */}
      <Tabs defaultValue="performance" className="space-y-4">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="performance">
            <Users className="w-4 h-4 mr-2" />
            Rendimiento
          </TabsTrigger>
          <TabsTrigger value="content">
            <FileText className="w-4 h-4 mr-2" />
            Contenido
          </TabsTrigger>
          <TabsTrigger value="trends">
            <BarChart3 className="w-4 h-4 mr-2" />
            Tendencias
          </TabsTrigger>
          <TabsTrigger value="quality">
            <Target className="w-4 h-4 mr-2" />
            Calidad
          </TabsTrigger>
        </TabsList>

        <TabsContent value="performance" className="space-y-4">
          <Card className="p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold">Rendimiento de Moderadores</h3>
              <Select value={selectedModerator} onValueChange={setSelectedModerator}>
                <SelectTrigger className="w-48">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos los moderadores</SelectItem>
                  {stats.moderatorPerformance.map((mod) => (
                    <SelectItem key={mod.moderatorId} value={mod.moderatorId}>
                      {mod.moderatorUsername}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-4">
              {stats.moderatorPerformance
                .filter(mod => selectedModerator === 'all' || mod.moderatorId === selectedModerator)
                .map((moderator) => (
                  <div key={moderator.moderatorId} className="p-4 border rounded-lg">
                    <div className="flex items-center justify-between mb-3">
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 bg-primary/10 rounded-full flex items-center justify-center">
                          <Users className="w-4 h-4 text-primary" />
                        </div>
                        <div>
                          <p className="font-medium">{moderator.moderatorUsername}</p>
                          <p className="text-sm text-muted-foreground">
                            {moderator.actionsCount} acciones • Última actividad: {' '}
                            {new Date(moderator.lastActive).toLocaleDateString('es-ES')}
                          </p>
                        </div>
                      </div>
                      <div className="text-right">
                        <p className={`text-sm font-medium ${getEfficiencyColor(moderator.efficiency)}`}>
                          {moderator.efficiency.toFixed(1)} acc/h
                        </p>
                        <p className="text-xs text-muted-foreground">Eficiencia</p>
                      </div>
                    </div>

                    <div className="grid grid-cols-3 gap-4">
                      <div className="text-center">
                        <div className="text-lg font-semibold text-green-600">
                          {formatPercentage(moderator.approvalRate)}
                        </div>
                        <div className="text-xs text-muted-foreground">Aprobaciones</div>
                        <Progress value={moderator.approvalRate} className="h-1 mt-1" />
                      </div>
                      <div className="text-center">
                        <div className="text-lg font-semibold text-red-600">
                          {formatPercentage(moderator.rejectionRate)}
                        </div>
                        <div className="text-xs text-muted-foreground">Rechazos</div>
                        <Progress value={moderator.rejectionRate} className="h-1 mt-1" />
                      </div>
                      <div className="text-center">
                        <div className="text-lg font-semibold text-yellow-600">
                          {formatPercentage(moderator.revisionRate)}
                        </div>
                        <div className="text-xs text-muted-foreground">Revisiones</div>
                        <Progress value={moderator.revisionRate} className="h-1 mt-1" />
                      </div>
                    </div>
                  </div>
                ))}
            </div>
          </Card>
        </TabsContent>

        <TabsContent value="content" className="space-y-4">
          <Card className="p-6">
            <h3 className="text-lg font-semibold mb-4">Estadísticas por Categoría</h3>
            <div className="space-y-4">
              {stats.contentTrends.categories.map((category, index) => (
                <div key={index} className="flex items-center justify-between p-3 border rounded-lg">
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                      <Badge variant="outline">{category.category}</Badge>
                      <span className="text-sm text-muted-foreground">
                        {category.count} publicaciones
                      </span>
                    </div>
                    <div className="text-xs text-muted-foreground">
                      Tiempo promedio: {formatTime(category.averageReviewTime)}
                    </div>
                  </div>
                  <div className="text-right">
                    <div className="text-lg font-semibold">
                      {formatPercentage(category.approvalRate)}
                    </div>
                    <div className="text-xs text-muted-foreground">Aprobación</div>
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </TabsContent>

        <TabsContent value="trends" className="space-y-4">
          <Card className="p-6">
            <h3 className="text-lg font-semibold mb-4">Tendencias de Reportes</h3>
            <div className="space-y-3">
              {stats.contentTrends.reportTrends.slice(0, 7).map((trend, index) => (
                <div key={index} className="flex items-center justify-between p-3 border rounded-lg">
                  <div className="flex items-center gap-3">
                    <Calendar className="w-4 h-4 text-muted-foreground" />
                    <div>
                      <p className="font-medium">
                        {new Date(trend.date).toLocaleDateString('es-ES')}
                      </p>
                      <p className="text-sm text-muted-foreground">{trend.category}</p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="font-medium">{trend.reportCount} reportes</p>
                    <p className="text-sm text-green-600">{trend.resolvedCount} resueltos</p>
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </TabsContent>

        <TabsContent value="quality" className="space-y-4">
          <Card className="p-6">
            <h3 className="text-lg font-semibold mb-4">Métricas de Calidad</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {stats.contentTrends.qualityMetrics.map((metric, index) => (
                <div key={index} className="p-4 border rounded-lg">
                  <div className="flex items-center justify-between mb-2">
                    <div className="flex items-center gap-2">
                      {getTrendIcon(metric.trend)}
                      <span className="font-medium">{metric.metric}</span>
                    </div>
                    <span className="text-2xl font-bold">{metric.value}</span>
                  </div>
                  <p className="text-sm text-muted-foreground">{metric.description}</p>
                </div>
              ))}
            </div>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}

interface ModerationStatsSummaryProps {
  stats: ModerationStats
  className?: string
}

export function ModerationStatsSummary({ stats, className }: ModerationStatsSummaryProps) {
  return (
    <div className={`grid grid-cols-2 md:grid-cols-4 gap-4 ${className}`}>
      <div className="text-center p-3 bg-blue-50 rounded-lg">
        <div className="text-2xl font-bold text-blue-600">{stats.overview.totalPending}</div>
        <div className="text-sm text-blue-700">Pendientes</div>
      </div>
      <div className="text-center p-3 bg-green-50 rounded-lg">
        <div className="text-2xl font-bold text-green-600">
          {Math.round(stats.overview.approvalRate)}%
        </div>
        <div className="text-sm text-green-700">Aprobación</div>
      </div>
      <div className="text-center p-3 bg-purple-50 rounded-lg">
        <div className="text-2xl font-bold text-purple-600">
          {Math.round(stats.overview.averageReviewTime)}h
        </div>
        <div className="text-sm text-purple-700">Tiempo Promedio</div>
      </div>
      <div className="text-center p-3 bg-orange-50 rounded-lg">
        <div className="text-2xl font-bold text-orange-600">
          {stats.moderatorPerformance.length}
        </div>
        <div className="text-sm text-orange-700">Moderadores</div>
      </div>
    </div>
  )
}