'use client'

import React, { useEffect, useState } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { LoadingSpinner } from '@/components/ui/loading-spinner'
import { AnalyticsNavigation } from './AnalyticsNavigation'
import { 
  analyticsService, 
  PublicationMetrics, 
  AnalyticsDateRange 
} from '@/services/analytics/analytics.service'
import { 
  FileText, 
  TrendingUp, 
  Calendar, 
  Clock, 
  Star,
  Users,
  BarChart3
} from 'lucide-react'

export const PublicationMetricsDashboard: React.FC = () => {
  const [metrics, setMetrics] = useState<PublicationMetrics | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selectedRange, setSelectedRange] = useState<AnalyticsDateRange>({
    fromDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  })

  const dateRangeOptions = [
    { label: 'Últimos 7 días', days: 7 },
    { label: 'Últimos 30 días', days: 30 },
    { label: 'Últimos 90 días', days: 90 },
    { label: 'Todo el tiempo', days: null }
  ]

  useEffect(() => {
    loadMetrics()
  }, [selectedRange])

  const loadMetrics = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await analyticsService.getPublicationMetrics(selectedRange)
      setMetrics(data)
    } catch (err) {
      console.error('Error loading publication metrics:', err)
      setError('Error al cargar las métricas de publicaciones')
    } finally {
      setLoading(false)
    }
  }

  const handleDateRangeChange = (days: number | null) => {
    if (days === null) {
      setSelectedRange({})
    } else {
      setSelectedRange({
        fromDate: new Date(Date.now() - days * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
      })
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
        <Button onClick={loadMetrics}>Reintentar</Button>
      </div>
    )
  }

  if (!metrics) return null

  return (
    <div className="space-y-6">
      <AnalyticsNavigation />
      
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold">Métricas de Publicaciones</h1>
          <p className="text-muted-foreground">
            Análisis del pipeline de contenido y tendencias de publicación
          </p>
        </div>
        
        <select 
          className="px-3 py-2 border rounded-md"
          onChange={(e) => {
            const option = dateRangeOptions[parseInt(e.target.value)]
            handleDateRangeChange(option.days)
          }}
        >
          {dateRangeOptions.map((option, index) => (
            <option key={index} value={index}>
              {option.label}
            </option>
          ))}
        </select>
      </div>

      {/* Key Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Envíos</CardTitle>
            <FileText className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{metrics.totalSubmissions}</div>
            <p className="text-xs text-muted-foreground">
              Todas las publicaciones
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Hoy</CardTitle>
            <Calendar className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{metrics.submissionsToday}</div>
            <p className="text-xs text-muted-foreground">
              Envíos de hoy
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Esta Semana</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{metrics.submissionsThisWeek}</div>
            <p className="text-xs text-muted-foreground">
              Últimos 7 días
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Tiempo Promedio</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {analyticsService.formatHours(metrics.averageProcessingTimeHours)}
            </div>
            <p className="text-xs text-muted-foreground">
              Procesamiento
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Content Rating Distribution */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Star className="h-5 w-5" />
            Distribución por Clasificación
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {Object.entries(metrics.contentRatingDistribution).map(([rating, count]) => (
              <div key={rating} className="text-center p-4 border rounded-lg">
                <div className="text-2xl font-bold">{count}</div>
                <p className="text-sm text-muted-foreground">
                  {getContentRatingLabel(rating)}
                </p>
                <div className="w-full bg-gray-200 rounded-full h-2 mt-2">
                  <div 
                    className="bg-blue-600 h-2 rounded-full" 
                    style={{ 
                      width: `${(count / metrics.totalSubmissions) * 100}%` 
                    }}
                  />
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Top Creators */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Users className="h-5 w-5" />
            Creadores Más Activos
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            {Object.entries(metrics.topCreators)
              .sort(([,a], [,b]) => b - a)
              .slice(0, 10)
              .map(([creator, count], index) => (
              <div key={creator} className="flex items-center justify-between p-3 border rounded-lg">
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                    <span className="text-sm font-bold text-blue-600">#{index + 1}</span>
                  </div>
                  <div>
                    <p className="font-medium">{creator}</p>
                    <p className="text-sm text-muted-foreground">
                      {count} publicaciones
                    </p>
                  </div>
                </div>
                <div className="text-right">
                  <div className="w-24 bg-gray-200 rounded-full h-2">
                    <div 
                      className="bg-green-600 h-2 rounded-full" 
                      style={{ 
                        width: `${(count / Math.max(...Object.values(metrics.topCreators))) * 100}%` 
                      }}
                    />
                  </div>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Publication Trends */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <BarChart3 className="h-5 w-5" />
            Tendencias de Publicación
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {metrics.trends.length > 0 ? (
              <div className="grid grid-cols-1 gap-3">
                {metrics.trends.slice(-14).map((trend, index) => (
                  <div key={index} className="flex items-center justify-between p-3 border rounded-lg">
                    <div>
                      <p className="font-medium">
                        {new Date(trend.date).toLocaleDateString()}
                      </p>
                      <p className="text-sm text-muted-foreground">
                        {trend.submissions} envíos • {trend.approvals} aprobaciones
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="font-medium">
                        {analyticsService.formatHours(trend.averageReviewTime)}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        tiempo promedio
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-8 text-muted-foreground">
                No hay datos de tendencias disponibles
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Monthly Summary */}
      <Card>
        <CardHeader>
          <CardTitle>Resumen del Mes</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="text-center">
              <div className="text-3xl font-bold text-blue-600">
                {metrics.submissionsThisMonth}
              </div>
              <p className="text-sm text-muted-foreground">Envíos este mes</p>
            </div>
            
            <div className="text-center">
              <div className="text-3xl font-bold text-green-600">
                {Math.round((metrics.submissionsThisMonth / 30) * 10) / 10}
              </div>
              <p className="text-sm text-muted-foreground">Promedio diario</p>
            </div>
            
            <div className="text-center">
              <div className="text-3xl font-bold text-purple-600">
                {Object.keys(metrics.topCreators).length}
              </div>
              <p className="text-sm text-muted-foreground">Creadores activos</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

function getContentRatingLabel(rating: string): string {
  const ratingMap: Record<string, string> = {
    '0': 'General',
    '1': 'Adolescente',
    '2': 'Maduro',
    '3': 'Adulto'
  }
  return ratingMap[rating] || rating
}