'use client'

import React from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { LoadingSpinner } from '@/components/ui/loading-spinner'
import { AuditLogStatistics, AuditSeverity } from '@/lib/types'
import { RefreshCw, TrendingUp, Users, AlertTriangle, CheckCircle, XCircle, Activity } from 'lucide-react'

interface AuditStatisticsProps {
  statistics: AuditLogStatistics | null
  loading: boolean
  onRefresh: () => void
}

export const AuditStatistics: React.FC<AuditStatisticsProps> = ({
  statistics,
  loading,
  onRefresh
}) => {
  const getSeverityIcon = (severity: AuditSeverity) => {
    switch (severity) {
      case AuditSeverity.Info:
        return <Activity className="h-4 w-4 text-blue-600" />
      case AuditSeverity.Warning:
        return <AlertTriangle className="h-4 w-4 text-yellow-600" />
      case AuditSeverity.Error:
        return <XCircle className="h-4 w-4 text-red-600" />
      case AuditSeverity.Critical:
        return <AlertTriangle className="h-4 w-4 text-red-800" />
      default:
        return <Activity className="h-4 w-4 text-gray-600" />
    }
  }

  const getSeverityColor = (severity: AuditSeverity) => {
    switch (severity) {
      case AuditSeverity.Info:
        return 'bg-blue-100 text-blue-800'
      case AuditSeverity.Warning:
        return 'bg-yellow-100 text-yellow-800'
      case AuditSeverity.Error:
        return 'bg-red-100 text-red-800'
      case AuditSeverity.Critical:
        return 'bg-red-200 text-red-900'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const formatActionName = (action: string) => {
    return action.split('.').map(part => 
      part.charAt(0).toUpperCase() + part.slice(1)
    ).join(' ')
  }

  if (loading) {
    return (
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-center py-8">
            <LoadingSpinner size="lg" />
          </div>
        </CardContent>
      </Card>
    )
  }

  if (!statistics) {
    return (
      <Card>
        <CardContent className="pt-6">
          <div className="text-center py-8">
            <p className="text-muted-foreground mb-4">No se pudieron cargar las estadísticas</p>
            <Button onClick={onRefresh}>Reintentar</Button>
          </div>
        </CardContent>
      </Card>
    )
  }

  const successRate = statistics.totalEvents > 0 
    ? Math.round((statistics.successfulEvents / statistics.totalEvents) * 100)
    : 0

  return (
    <div className="space-y-6">
      {/* Overview Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total de Eventos</CardTitle>
            <Activity className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{statistics.totalEvents.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              Eventos registrados
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Tasa de Éxito</CardTitle>
            <CheckCircle className="h-4 w-4 text-green-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">{successRate}%</div>
            <p className="text-xs text-muted-foreground">
              {statistics.successfulEvents.toLocaleString()} exitosos
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Eventos Fallidos</CardTitle>
            <XCircle className="h-4 w-4 text-red-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-red-600">{statistics.failedEvents.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              {statistics.totalEvents > 0 
                ? Math.round((statistics.failedEvents / statistics.totalEvents) * 100)
                : 0}% del total
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Usuarios Únicos</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{statistics.uniqueUsers}</div>
            <p className="text-xs text-muted-foreground">
              Usuarios activos
            </p>
          </CardContent>
        </Card>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Top Actions */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>Acciones Más Frecuentes</CardTitle>
            <Button variant="outline" size="sm" onClick={onRefresh}>
              <RefreshCw className="h-4 w-4" />
            </Button>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {Object.entries(statistics.topActions)
                .sort(([,a], [,b]) => b - a)
                .slice(0, 10)
                .map(([action, count]) => (
                  <div key={action} className="flex items-center justify-between">
                    <span className="text-sm font-medium">
                      {formatActionName(action)}
                    </span>
                    <div className="flex items-center gap-2">
                      <Badge variant="outline">{count}</Badge>
                      <div className="w-20 bg-gray-200 rounded-full h-2">
                        <div 
                          className="bg-blue-600 h-2 rounded-full" 
                          style={{ 
                            width: `${Math.min((count / Math.max(...Object.values(statistics.topActions))) * 100, 100)}%` 
                          }}
                        />
                      </div>
                    </div>
                  </div>
                ))}
            </div>
          </CardContent>
        </Card>

        {/* Events by Severity */}
        <Card>
          <CardHeader>
            <CardTitle>Eventos por Severidad</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {Object.entries(statistics.eventsBySeverity).map(([severity, count]) => (
                <div key={severity} className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    {getSeverityIcon(severity as AuditSeverity)}
                    <Badge className={getSeverityColor(severity as AuditSeverity)}>
                      {severity}
                    </Badge>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-medium">{count}</span>
                    <div className="w-20 bg-gray-200 rounded-full h-2">
                      <div 
                        className="bg-blue-600 h-2 rounded-full" 
                        style={{ 
                          width: `${statistics.totalEvents > 0 ? (count / statistics.totalEvents) * 100 : 0}%` 
                        }}
                      />
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Events by Day */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TrendingUp className="h-5 w-5" />
            Actividad por Día
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            {Object.entries(statistics.eventsByDay)
              .sort(([a], [b]) => new Date(b).getTime() - new Date(a).getTime())
              .slice(0, 14)
              .map(([date, count]) => (
                <div key={date} className="flex items-center justify-between">
                  <span className="text-sm">
                    {new Date(date).toLocaleDateString()}
                  </span>
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-medium">{count}</span>
                    <div className="w-32 bg-gray-200 rounded-full h-2">
                      <div 
                        className="bg-blue-600 h-2 rounded-full" 
                        style={{ 
                          width: `${Math.max(...Object.values(statistics.eventsByDay)) > 0 
                            ? (count / Math.max(...Object.values(statistics.eventsByDay))) * 100 
                            : 0}%` 
                        }}
                      />
                    </div>
                  </div>
                </div>
              ))}
          </div>
        </CardContent>
      </Card>

      {/* Generation Info */}
      <div className="text-center text-sm text-muted-foreground">
        Estadísticas generadas el {new Date(statistics.generatedAt).toLocaleString()}
      </div>
    </div>
  )
}