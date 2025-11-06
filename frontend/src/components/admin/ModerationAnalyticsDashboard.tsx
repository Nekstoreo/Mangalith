'use client'

import React, { useEffect, useState } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { LoadingSpinner } from '@/components/ui/loading-spinner'
import { Select } from '@/components/ui/select'
import { AnalyticsNavigation } from './AnalyticsNavigation'
import { 
  analyticsService, 
  ModerationAnalytics, 
  AnalyticsDateRange,
  SystemAlert 
} from '@/services/analytics/analytics.service'
import { 
  BarChart3, 
  TrendingUp, 
  TrendingDown, 
  AlertTriangle, 
  Clock, 
  CheckCircle, 
  XCircle,
  Download,
  RefreshCw,
  Users,
  FileText,
  Flag
} from 'lucide-react'

interface DateRangeOption {
  label: string
  value: AnalyticsDateRange
}

export const ModerationAnalyticsDashboard: React.FC = () => {
  const [analytics, setAnalytics] = useState<ModerationAnalytics | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selectedRange, setSelectedRange] = useState<AnalyticsDateRange>({})
  const [exporting, setExporting] = useState(false)

  const dateRangeOptions: DateRangeOption[] = [
    { label: 'Últimos 7 días', value: { fromDate: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0] } },
    { label: 'Últimos 30 días', value: { fromDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0] } },
    { label: 'Últimos 90 días', value: { fromDate: new Date(Date.now() - 90 * 24 * 60 * 60 * 1000).toISOString().split('T')[0] } },
    { label: 'Todo el tiempo', value: {} }
  ]

  useEffect(() => {
    loadAnalytics()
  }, [selectedRange])

  const loadAnalytics = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await analyticsService.getModerationAnalytics(selectedRange)
      setAnalytics(data)
    } catch (err) {
      console.error('Error loading analytics:', err)
      setError('Error al cargar las analíticas')
    } finally {
      setLoading(false)
    }
  }

  const handleExport = async () => {
    if (!selectedRange.fromDate) return
    
    try {
      setExporting(true)
      const fromDate = selectedRange.fromDate
      const toDate = selectedRange.toDate || new Date().toISOString().split('T')[0]
      
      const blob = await analyticsService.exportAnalyticsReport(fromDate, toDate, 'csv')
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `analytics-report-${fromDate}-to-${toDate}.csv`
      document.body.appendChild(a)
      a.click()
      window.URL.revokeObjectURL(url)
      document.body.removeChild(a)
    } catch (err) {
      console.error('Error exporting report:', err)
    } finally {
      setExporting(false)
    }
  }

  const handleCheckAlerts = async () => {
    try {
      await analyticsService.checkSystemAlerts()
      await loadAnalytics() // Reload to get new alerts
    } catch (err) {
      console.error('Error checking alerts:', err)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <LoadingSpinner size="lg" />
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600 mb-4">{error}</p>
        <Button onClick={loadAnalytics}>Reintentar</Button>
      </div>
    )
  }

  if (!analytics) return null

  const { metrics, moderatorPerformances, contentTrends, systemAlerts } = analytics

  return (
    <div className="space-y-6">
      <AnalyticsNavigation />
      
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold">Analíticas de Moderación</h1>
          <p className="text-muted-foreground">
            Métricas y estadísticas del sistema de moderación
          </p>
        </div>
        
        <div className="flex gap-2">
          <select 
            className="px-3 py-2 border rounded-md"
            onChange={(e) => {
              const option = dateRangeOptions[parseInt(e.target.value)]
              setSelectedRange(option.value)
            }}
          >
            {dateRangeOptions.map((option, index) => (
              <option key={index} value={index}>
                {option.label}
              </option>
            ))}
          </select>
          
          <Button 
            variant="outline" 
            onClick={loadAnalytics}
            disabled={loading}
          >
            <RefreshCw className="h-4 w-4 mr-2" />
            Actualizar
          </Button>
          
          <Button 
            variant="outline" 
            onClick={handleExport}
            disabled={exporting || !selectedRange.fromDate}
          >
            <Download className="h-4 w-4 mr-2" />
            Exportar
          </Button>
        </div>
      </div>

      {/* Key Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">En Revisión</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{metrics.publicationsInReview}</div>
            <p className="text-xs text-muted-foreground">
              {analyticsService.formatHours(metrics.averageReviewTimeHours)} promedio
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Tasa de Aprobación</CardTitle>
            <CheckCircle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {analyticsService.formatPercentage(metrics.approvalRate)}
            </div>
            <p className="text-xs text-muted-foreground">
              {metrics.publicationsApproved} aprobadas
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Reportes Pendientes</CardTitle>
            <Flag className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{metrics.pendingReports}</div>
            <p className="text-xs text-muted-foreground">
              {metrics.resolvedReports} resueltos
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Publicaciones</CardTitle>
            <FileText className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{metrics.totalPublications}</div>
            <p className="text-xs text-muted-foreground">
              {metrics.publicationsRejected} rechazadas
            </p>
          </CardContent>
        </Card>
      </div>

      {/* System Alerts */}
      {systemAlerts.length > 0 && (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-yellow-600" />
              Alertas del Sistema
            </CardTitle>
            <Button variant="outline" size="sm" onClick={handleCheckAlerts}>
              Verificar Alertas
            </Button>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {systemAlerts.slice(0, 5).map((alert) => (
                <AlertCard key={alert.id} alert={alert} />
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Content Trends */}
      <Card>
        <CardHeader>
          <CardTitle>Tendencias de Contenido</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {contentTrends.map((trend, index) => (
              <div key={index} className="flex items-center justify-between p-3 border rounded-lg">
                <div>
                  <p className="font-medium">{trend.category}</p>
                  <p className="text-sm text-muted-foreground">{trend.description}</p>
                </div>
                <div className="text-right">
                  <div className="flex items-center gap-2">
                    <span className="text-2xl">{analyticsService.getTrendIcon(trend.direction)}</span>
                    <div>
                      <p className="font-bold">{trend.count}</p>
                      <p className={`text-xs ${
                        trend.direction === 'Up' ? 'text-green-600' : 
                        trend.direction === 'Down' ? 'text-red-600' : 'text-gray-600'
                      }`}>
                        {trend.percentageChange > 0 ? '+' : ''}{trend.percentageChange.toFixed(1)}%
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Moderator Performance */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Users className="h-5 w-5" />
            Rendimiento de Moderadores
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            {moderatorPerformances.slice(0, 10).map((moderator) => (
              <div key={moderator.moderatorId} className="flex items-center justify-between p-3 border rounded-lg">
                <div>
                  <p className="font-medium">{moderator.moderatorName}</p>
                  <p className="text-sm text-muted-foreground">
                    {moderator.actionsCompleted} acciones • {moderator.reportsReviewed} reportes
                  </p>
                </div>
                <div className="text-right">
                  <p className="font-medium">
                    {analyticsService.formatPercentage(moderator.approvalRate)}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {analyticsService.formatHours(moderator.averageReviewTimeHours)} promedio
                  </p>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Status Distribution */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle>Distribución por Estado</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {Object.entries(metrics.statusDistribution).map(([status, count]) => (
                <div key={status} className="flex justify-between items-center">
                  <span className="text-sm">{getStatusLabel(status)}</span>
                  <div className="flex items-center gap-2">
                    <div className="w-24 bg-gray-200 rounded-full h-2">
                      <div 
                        className="bg-blue-600 h-2 rounded-full" 
                        style={{ width: `${(count / metrics.totalPublications) * 100}%` }}
                      />
                    </div>
                    <span className="text-sm font-medium w-8">{count}</span>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Reportes por Categoría</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {Object.entries(metrics.reportCategoryDistribution).map(([category, count]) => (
                <div key={category} className="flex justify-between items-center">
                  <span className="text-sm">{getReportCategoryLabel(category)}</span>
                  <div className="flex items-center gap-2">
                    <div className="w-24 bg-gray-200 rounded-full h-2">
                      <div 
                        className="bg-red-600 h-2 rounded-full" 
                        style={{ width: `${(count / (metrics.pendingReports + metrics.resolvedReports)) * 100}%` }}
                      />
                    </div>
                    <span className="text-sm font-medium w-8">{count}</span>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

const AlertCard: React.FC<{ alert: SystemAlert }> = ({ alert }) => {
  return (
    <div className="flex items-center justify-between p-3 border rounded-lg">
      <div>
        <p className="font-medium">{alert.title}</p>
        <p className="text-sm text-muted-foreground">{alert.description}</p>
        <p className="text-xs text-muted-foreground">
          {new Date(alert.createdAt).toLocaleString()}
        </p>
      </div>
      <Badge className={analyticsService.getSeverityColor(alert.severity)}>
        {alert.severity}
      </Badge>
    </div>
  )
}

function getStatusLabel(status: string): string {
  const statusMap: Record<string, string> = {
    '0': 'Borrador',
    '1': 'En Revisión',
    '2': 'Necesita Revisión',
    '3': 'Publicado',
    '4': 'Rechazado',
    '5': 'Archivado',
    '6': 'Bajo Revisión'
  }
  return statusMap[status] || status
}

function getReportCategoryLabel(category: string): string {
  const categoryMap: Record<string, string> = {
    '0': 'Derechos de Autor',
    '1': 'Contenido Inapropiado',
    '2': 'Spam',
    '3': 'Acoso',
    '4': 'Violencia',
    '5': 'Contenido Adulto',
    '6': 'Otro'
  }
  return categoryMap[category] || category
}