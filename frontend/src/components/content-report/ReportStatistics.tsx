'use client'

import { useState, useEffect } from 'react'
import { Card } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { 
  ContentReportCategory, 
  ContentReportStatus 
} from '@/services/content-report/client'
import { 
  BarChart3, 
  TrendingUp, 
  Clock, 
  CheckCircle, 
  XCircle,
  AlertTriangle,
  Users,
  FileText,
  Calendar,
  Activity
} from 'lucide-react'

interface ReportStatisticsProps {
  className?: string
}

interface StatisticsData {
  totalReports: number
  pendingReports: number
  resolvedReports: number
  dismissedReports: number
  averageResolutionTime: number
  reportsByCategory: Record<ContentReportCategory, number>
  reportsByStatus: Record<ContentReportStatus, number>
  reportsThisWeek: number
  reportsThisMonth: number
  topReporters: Array<{ userId: string; count: number }>
  resolutionRate: number
}

const categoryLabels: Record<ContentReportCategory, string> = {
  [ContentReportCategory.Copyright]: 'Derechos de Autor',
  [ContentReportCategory.InappropriateContent]: 'Contenido Inapropiado',
  [ContentReportCategory.Spam]: 'Spam',
  [ContentReportCategory.Harassment]: 'Acoso',
  [ContentReportCategory.Violence]: 'Violencia',
  [ContentReportCategory.AdultContent]: 'Contenido Adulto',
  [ContentReportCategory.Other]: 'Otro',
}

const statusLabels: Record<ContentReportStatus, string> = {
  [ContentReportStatus.Pending]: 'Pendientes',
  [ContentReportStatus.Reviewed]: 'En Revisión',
  [ContentReportStatus.Resolved]: 'Resueltos',
  [ContentReportStatus.Dismissed]: 'Desestimados',
}

export function ReportStatistics({ className = '' }: ReportStatisticsProps) {
  const [timeRange, setTimeRange] = useState<'week' | 'month' | 'quarter' | 'year'>('month')
  const [loading, setLoading] = useState(true)
  const [statistics, setStatistics] = useState<StatisticsData | null>(null)

  useEffect(() => {
    loadStatistics()
  }, [timeRange])

  const loadStatistics = async () => {
    setLoading(true)
    try {
      // Mock data - replace with actual API call
      const mockData: StatisticsData = {
        totalReports: 156,
        pendingReports: 23,
        resolvedReports: 98,
        dismissedReports: 35,
        averageResolutionTime: 2.3,
        reportsByCategory: {
          [ContentReportCategory.Copyright]: 32,
          [ContentReportCategory.InappropriateContent]: 25,
          [ContentReportCategory.Spam]: 45,
          [ContentReportCategory.Harassment]: 28,
          [ContentReportCategory.Violence]: 15,
          [ContentReportCategory.AdultContent]: 18,
          [ContentReportCategory.Other]: 8,
        },
        reportsByStatus: {
          [ContentReportStatus.Pending]: 23,
          [ContentReportStatus.Reviewed]: 0,
          [ContentReportStatus.Resolved]: 98,
          [ContentReportStatus.Dismissed]: 35,
        },
        reportsThisWeek: 12,
        reportsThisMonth: 47,
        topReporters: [
          { userId: 'user1', count: 8 },
          { userId: 'user2', count: 6 },
          { userId: 'user3', count: 5 },
        ],
        resolutionRate: 85.2
      }
      setStatistics(mockData)
    } catch (error) {
      console.error('Error loading statistics:', error)
    } finally {
      setLoading(false)
    }
  }

  if (loading || !statistics) {
    return (
      <div className={`space-y-6 ${className}`}>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Card key={i} className="p-4 animate-pulse">
              <div className="space-y-2">
                <div className="h-4 bg-muted rounded w-3/4" />
                <div className="h-8 bg-muted rounded w-1/2" />
              </div>
            </Card>
          ))}
        </div>
      </div>
    )
  }

  return (
    <div className={`space-y-6 ${className}`}>
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Estadísticas de Reportes</h1>
          <p className="text-muted-foreground">
            Análisis y métricas del sistema de reportes de contenido
          </p>
        </div>
        <Select value={timeRange} onValueChange={(value: any) => setTimeRange(value)}>
          <SelectTrigger className="w-48">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="week">Última semana</SelectItem>
            <SelectItem value="month">Último mes</SelectItem>
            <SelectItem value="quarter">Último trimestre</SelectItem>
            <SelectItem value="year">Último año</SelectItem>
          </SelectContent>
        </Select>
      </div>
      {/* Key Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Total Reportes</p>
              <p className="text-2xl font-bold">{statistics.totalReports}</p>
            </div>
            <FileText className="w-8 h-8 text-blue-500" />
          </div>
          <div className="mt-2 flex items-center text-sm">
            <TrendingUp className="w-4 h-4 text-green-500 mr-1" />
            <span className="text-green-600">+{statistics.reportsThisMonth} este mes</span>
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Pendientes</p>
              <p className="text-2xl font-bold text-yellow-600">{statistics.pendingReports}</p>
            </div>
            <Clock className="w-8 h-8 text-yellow-500" />
          </div>
          <div className="mt-2 flex items-center text-sm">
            <span className="text-muted-foreground">
              {((statistics.pendingReports / statistics.totalReports) * 100).toFixed(1)}% del total
            </span>
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Tasa de Resolución</p>
              <p className="text-2xl font-bold text-green-600">{statistics.resolutionRate}%</p>
            </div>
            <CheckCircle className="w-8 h-8 text-green-500" />
          </div>
          <div className="mt-2 flex items-center text-sm">
            <span className="text-muted-foreground">
              {statistics.resolvedReports} resueltos
            </span>
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-muted-foreground">Tiempo Promedio</p>
              <p className="text-2xl font-bold">{statistics.averageResolutionTime}d</p>
            </div>
            <Activity className="w-8 h-8 text-purple-500" />
          </div>
          <div className="mt-2 flex items-center text-sm">
            <span className="text-muted-foreground">días de resolución</span>
          </div>
        </Card>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Reports by Category */}
        <Card className="p-6">
          <div className="flex items-center gap-2 mb-4">
            <BarChart3 className="w-5 h-5" />
            <h3 className="text-lg font-semibold">Reportes por Categoría</h3>
          </div>
          <div className="space-y-3">
            {Object.entries(statistics.reportsByCategory)
              .sort(([,a], [,b]) => b - a)
              .map(([category, count]) => {
                const percentage = (count / statistics.totalReports) * 100
                return (
                  <div key={category} className="space-y-1">
                    <div className="flex items-center justify-between text-sm">
                      <span>{categoryLabels[parseInt(category) as ContentReportCategory]}</span>
                      <span className="font-medium">{count} ({percentage.toFixed(1)}%)</span>
                    </div>
                    <div className="w-full bg-muted rounded-full h-2">
                      <div 
                        className="bg-blue-500 h-2 rounded-full transition-all"
                        style={{ width: `${percentage}%` }}
                      />
                    </div>
                  </div>
                )
              })}
          </div>
        </Card>

        {/* Reports by Status */}
        <Card className="p-6">
          <div className="flex items-center gap-2 mb-4">
            <Activity className="w-5 h-5" />
            <h3 className="text-lg font-semibold">Estado de Reportes</h3>
          </div>
          <div className="space-y-4">
            {Object.entries(statistics.reportsByStatus).map(([status, count]) => {
              const statusKey = parseInt(status) as ContentReportStatus
              const percentage = (count / statistics.totalReports) * 100
              const colors = {
                [ContentReportStatus.Pending]: 'bg-yellow-500',
                [ContentReportStatus.Reviewed]: 'bg-blue-500',
                [ContentReportStatus.Resolved]: 'bg-green-500',
                [ContentReportStatus.Dismissed]: 'bg-red-500',
              }
              
              return (
                <div key={status} className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div className={`w-3 h-3 rounded-full ${colors[statusKey]}`} />
                    <span className="text-sm">{statusLabels[statusKey]}</span>
                  </div>
                  <div className="text-right">
                    <div className="text-sm font-medium">{count}</div>
                    <div className="text-xs text-muted-foreground">{percentage.toFixed(1)}%</div>
                  </div>
                </div>
              )
            })}
          </div>
        </Card>
      </div>

      {/* Additional Insights */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <Card className="p-4">
          <div className="flex items-center gap-2 mb-3">
            <Calendar className="w-4 h-4 text-muted-foreground" />
            <h4 className="font-medium">Actividad Reciente</h4>
          </div>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between">
              <span>Esta semana:</span>
              <span className="font-medium">{statistics.reportsThisWeek} reportes</span>
            </div>
            <div className="flex justify-between">
              <span>Este mes:</span>
              <span className="font-medium">{statistics.reportsThisMonth} reportes</span>
            </div>
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center gap-2 mb-3">
            <Users className="w-4 h-4 text-muted-foreground" />
            <h4 className="font-medium">Usuarios Más Activos</h4>
          </div>
          <div className="space-y-2 text-sm">
            {statistics.topReporters.slice(0, 3).map((reporter, index) => (
              <div key={reporter.userId} className="flex justify-between">
                <span>Usuario #{index + 1}:</span>
                <span className="font-medium">{reporter.count} reportes</span>
              </div>
            ))}
          </div>
        </Card>

        <Card className="p-4">
          <div className="flex items-center gap-2 mb-3">
            <AlertTriangle className="w-4 h-4 text-muted-foreground" />
            <h4 className="font-medium">Alertas</h4>
          </div>
          <div className="space-y-2 text-sm">
            {statistics.pendingReports > 20 && (
              <div className="flex items-center gap-2 text-yellow-600">
                <div className="w-2 h-2 bg-yellow-500 rounded-full" />
                <span>Cola de reportes alta</span>
              </div>
            )}
            {statistics.averageResolutionTime > 3 && (
              <div className="flex items-center gap-2 text-orange-600">
                <div className="w-2 h-2 bg-orange-500 rounded-full" />
                <span>Tiempo de resolución elevado</span>
              </div>
            )}
            {statistics.resolutionRate < 80 && (
              <div className="flex items-center gap-2 text-red-600">
                <div className="w-2 h-2 bg-red-500 rounded-full" />
                <span>Tasa de resolución baja</span>
              </div>
            )}
            {statistics.pendingReports <= 20 && statistics.averageResolutionTime <= 3 && statistics.resolutionRate >= 80 && (
              <div className="flex items-center gap-2 text-green-600">
                <CheckCircle className="w-4 h-4" />
                <span>Sistema funcionando bien</span>
              </div>
            )}
          </div>
        </Card>
      </div>
    </div>
  )
}