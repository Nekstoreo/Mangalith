'use client'

import React, { useEffect, useState } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { LoadingSpinner } from '@/components/ui/loading-spinner'
import { AnalyticsNavigation } from './AnalyticsNavigation'
import { 
  analyticsService, 
  ModeratorPerformance, 
  AnalyticsDateRange 
} from '@/services/analytics/analytics.service'
import { 
  Users, 
  Clock, 
  CheckCircle, 
  XCircle, 
  Flag,
  TrendingUp,
  Award,
  Activity
} from 'lucide-react'

export const ModeratorPerformanceDashboard: React.FC = () => {
  const [moderators, setModerators] = useState<ModeratorPerformance[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selectedRange, setSelectedRange] = useState<AnalyticsDateRange>({
    fromDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  })
  const [sortBy, setSortBy] = useState<keyof ModeratorPerformance>('actionsCompleted')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc')

  const dateRangeOptions = [
    { label: 'Últimos 7 días', days: 7 },
    { label: 'Últimos 30 días', days: 30 },
    { label: 'Últimos 90 días', days: 90 }
  ]

  useEffect(() => {
    loadModerators()
  }, [selectedRange])

  const loadModerators = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await analyticsService.getModeratorPerformance(selectedRange)
      setModerators(data)
    } catch (err) {
      console.error('Error loading moderator performance:', err)
      setError('Error al cargar el rendimiento de moderadores')
    } finally {
      setLoading(false)
    }
  }

  const handleDateRangeChange = (days: number) => {
    setSelectedRange({
      fromDate: new Date(Date.now() - days * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
    })
  }

  const handleSort = (field: keyof ModeratorPerformance) => {
    if (sortBy === field) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')
    } else {
      setSortBy(field)
      setSortOrder('desc')
    }
  }

  const sortedModerators = [...moderators].sort((a, b) => {
    const aValue = a[sortBy]
    const bValue = b[sortBy]
    
    if (typeof aValue === 'number' && typeof bValue === 'number') {
      return sortOrder === 'asc' ? aValue - bValue : bValue - aValue
    }
    
    if (typeof aValue === 'string' && typeof bValue === 'string') {
      return sortOrder === 'asc' ? aValue.localeCompare(bValue) : bValue.localeCompare(aValue)
    }
    
    return 0
  })

  const getPerformanceBadge = (moderator: ModeratorPerformance) => {
    const score = calculatePerformanceScore(moderator)
    
    if (score >= 90) return { label: 'Excelente', color: 'bg-green-100 text-green-800' }
    if (score >= 75) return { label: 'Bueno', color: 'bg-blue-100 text-blue-800' }
    if (score >= 60) return { label: 'Regular', color: 'bg-yellow-100 text-yellow-800' }
    return { label: 'Necesita Mejora', color: 'bg-red-100 text-red-800' }
  }

  const calculatePerformanceScore = (moderator: ModeratorPerformance): number => {
    // Algoritmo simple de puntuación basado en múltiples factores
    let score = 0
    
    // Actividad (30%)
    const activityScore = Math.min((moderator.actionsCompleted / 100) * 30, 30)
    score += activityScore
    
    // Tasa de aprobación (25%) - óptimo alrededor del 70-80%
    const approvalScore = moderator.approvalRate >= 70 && moderator.approvalRate <= 85 ? 25 : 
                         Math.max(0, 25 - Math.abs(moderator.approvalRate - 77.5) * 0.5)
    score += approvalScore
    
    // Tiempo de revisión (25%) - menos es mejor
    const timeScore = moderator.averageReviewTimeHours <= 2 ? 25 : 
                     Math.max(0, 25 - (moderator.averageReviewTimeHours - 2) * 2)
    score += timeScore
    
    // Consistencia (20%) - basado en actividad reciente
    const consistencyScore = moderator.actionsLast7Days > 0 ? 20 : 0
    score += consistencyScore
    
    return Math.round(score)
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
        <Button onClick={loadModerators}>Reintentar</Button>
      </div>
    )
  }

  const totalActions = moderators.reduce((sum, mod) => sum + mod.actionsCompleted, 0)
  const avgApprovalRate = moderators.length > 0 ? 
    moderators.reduce((sum, mod) => sum + mod.approvalRate, 0) / moderators.length : 0
  const avgReviewTime = moderators.length > 0 ? 
    moderators.reduce((sum, mod) => sum + mod.averageReviewTimeHours, 0) / moderators.length : 0

  return (
    <div className="space-y-6">
      <AnalyticsNavigation />
      
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold">Rendimiento de Moderadores</h1>
          <p className="text-muted-foreground">
            Análisis detallado del desempeño del equipo de moderación
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

      {/* Summary Stats */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Moderadores Activos</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{moderators.length}</div>
            <p className="text-xs text-muted-foreground">
              {moderators.filter(m => m.actionsLast7Days > 0).length} activos esta semana
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Acciones</CardTitle>
            <Activity className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{totalActions}</div>
            <p className="text-xs text-muted-foreground">
              {Math.round(totalActions / moderators.length || 0)} promedio por moderador
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Tasa Aprobación Promedio</CardTitle>
            <CheckCircle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {analyticsService.formatPercentage(avgApprovalRate)}
            </div>
            <p className="text-xs text-muted-foreground">
              Promedio del equipo
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
              {analyticsService.formatHours(avgReviewTime)}
            </div>
            <p className="text-xs text-muted-foreground">
              Revisión promedio
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Top Performers */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Award className="h-5 w-5" />
            Mejores Desempeños
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {sortedModerators
              .slice(0, 3)
              .map((moderator, index) => {
                const badge = getPerformanceBadge(moderator)
                const score = calculatePerformanceScore(moderator)
                
                return (
                  <div key={moderator.moderatorId} className="p-4 border rounded-lg bg-gradient-to-br from-blue-50 to-indigo-50">
                    <div className="flex items-center justify-between mb-2">
                      <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                        <span className="text-sm font-bold text-blue-600">#{index + 1}</span>
                      </div>
                      <Badge className={badge.color}>
                        {badge.label}
                      </Badge>
                    </div>
                    <h3 className="font-semibold">{moderator.moderatorName}</h3>
                    <p className="text-sm text-muted-foreground mb-3">
                      Puntuación: {score}/100
                    </p>
                    <div className="space-y-1 text-xs">
                      <div className="flex justify-between">
                        <span>Acciones:</span>
                        <span className="font-medium">{moderator.actionsCompleted}</span>
                      </div>
                      <div className="flex justify-between">
                        <span>Aprobación:</span>
                        <span className="font-medium">{analyticsService.formatPercentage(moderator.approvalRate)}</span>
                      </div>
                      <div className="flex justify-between">
                        <span>Tiempo:</span>
                        <span className="font-medium">{analyticsService.formatHours(moderator.averageReviewTimeHours)}</span>
                      </div>
                    </div>
                  </div>
                )
              })}
          </div>
        </CardContent>
      </Card>

      {/* Detailed Performance Table */}
      <Card>
        <CardHeader>
          <CardTitle>Rendimiento Detallado</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b">
                  <th className="text-left p-2">
                    <Button 
                      variant="ghost" 
                      size="sm"
                      onClick={() => handleSort('moderatorName')}
                    >
                      Moderador
                    </Button>
                  </th>
                  <th className="text-left p-2">
                    <Button 
                      variant="ghost" 
                      size="sm"
                      onClick={() => handleSort('actionsCompleted')}
                    >
                      Acciones
                    </Button>
                  </th>
                  <th className="text-left p-2">
                    <Button 
                      variant="ghost" 
                      size="sm"
                      onClick={() => handleSort('approvalRate')}
                    >
                      Aprobación
                    </Button>
                  </th>
                  <th className="text-left p-2">
                    <Button 
                      variant="ghost" 
                      size="sm"
                      onClick={() => handleSort('averageReviewTimeHours')}
                    >
                      Tiempo Promedio
                    </Button>
                  </th>
                  <th className="text-left p-2">
                    <Button 
                      variant="ghost" 
                      size="sm"
                      onClick={() => handleSort('reportsReviewed')}
                    >
                      Reportes
                    </Button>
                  </th>
                  <th className="text-left p-2">Actividad</th>
                  <th className="text-left p-2">Rendimiento</th>
                </tr>
              </thead>
              <tbody>
                {sortedModerators.map((moderator) => {
                  const badge = getPerformanceBadge(moderator)
                  const score = calculatePerformanceScore(moderator)
                  
                  return (
                    <tr key={moderator.moderatorId} className="border-b hover:bg-gray-50">
                      <td className="p-2">
                        <div>
                          <p className="font-medium">{moderator.moderatorName}</p>
                          <p className="text-xs text-muted-foreground">
                            Último activo: {new Date(moderator.lastActiveAt).toLocaleDateString()}
                          </p>
                        </div>
                      </td>
                      <td className="p-2">
                        <div>
                          <p className="font-medium">{moderator.actionsCompleted}</p>
                          <p className="text-xs text-muted-foreground">
                            {moderator.approvalsCount} aprobaciones, {moderator.rejectionsCount} rechazos
                          </p>
                        </div>
                      </td>
                      <td className="p-2">
                        <div>
                          <p className="font-medium">{analyticsService.formatPercentage(moderator.approvalRate)}</p>
                          <div className="w-16 bg-gray-200 rounded-full h-1 mt-1">
                            <div 
                              className="bg-blue-600 h-1 rounded-full" 
                              style={{ width: `${moderator.approvalRate}%` }}
                            />
                          </div>
                        </div>
                      </td>
                      <td className="p-2">
                        <p className="font-medium">{analyticsService.formatHours(moderator.averageReviewTimeHours)}</p>
                      </td>
                      <td className="p-2">
                        <p className="font-medium">{moderator.reportsReviewed}</p>
                      </td>
                      <td className="p-2">
                        <div>
                          <p className="text-sm">7d: {moderator.actionsLast7Days}</p>
                          <p className="text-sm">30d: {moderator.actionsLast30Days}</p>
                        </div>
                      </td>
                      <td className="p-2">
                        <div>
                          <Badge className={badge.color}>
                            {badge.label}
                          </Badge>
                          <p className="text-xs text-muted-foreground mt-1">
                            {score}/100
                          </p>
                        </div>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}