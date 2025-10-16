'use client'

import React, { useEffect, useState } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { LoadingSpinner } from '@/components/ui/loading-spinner'
import { 
  Shield, 
  AlertTriangle, 
  Users, 
  Flag,
  Eye,
  CheckCircle,
  XCircle,
  Clock,
  FileText,
  MessageSquare,
  TrendingUp
} from 'lucide-react'
import Link from 'next/link'
import { useApiData } from '@/hooks'

interface ModeratorStats {
  pendingReports: number
  pendingContent: number
  totalReportsHandled: number
  totalContentModerated: number
  recentReports: Array<{
    id: string
    type: 'content' | 'user' | 'comment'
    reason: string
    reportedAt: string
    status: 'pending' | 'resolved' | 'dismissed'
    priority: 'low' | 'medium' | 'high' | 'critical'
    reportedBy: string
    targetTitle?: string
  }>
  contentQueue: Array<{
    id: string
    title: string
    uploader: string
    uploadedAt: string
    status: 'pending' | 'approved' | 'rejected'
    flags: string[]
    size: number
  }>
  userReports: Array<{
    id: string
    username: string
    reportCount: number
    lastReportAt: string
    status: 'active' | 'warned' | 'suspended'
    riskLevel: 'low' | 'medium' | 'high'
  }>
  monthlyStats: {
    reportsHandled: number
    contentApproved: number
    contentRejected: number
    usersWarned: number
  }
}

export const ModeratorDashboard: React.FC = () => {
  const { data: stats, isLoading, error } = useApiData<ModeratorStats>('/dashboard/moderator')

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'critical':
        return 'bg-red-100 text-red-800 border-red-200'
      case 'high':
        return 'bg-orange-100 text-orange-800 border-orange-200'
      case 'medium':
        return 'bg-yellow-100 text-yellow-800 border-yellow-200'
      case 'low':
        return 'bg-blue-100 text-blue-800 border-blue-200'
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200'
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'resolved':
      case 'approved':
        return 'bg-green-100 text-green-800'
      case 'pending':
        return 'bg-yellow-100 text-yellow-800'
      case 'dismissed':
      case 'rejected':
        return 'bg-red-100 text-red-800'
      case 'warned':
        return 'bg-orange-100 text-orange-800'
      case 'suspended':
        return 'bg-red-100 text-red-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const getRiskLevelColor = (level: string) => {
    switch (level) {
      case 'high':
        return 'bg-red-100 text-red-800'
      case 'medium':
        return 'bg-yellow-100 text-yellow-800'
      case 'low':
        return 'bg-green-100 text-green-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const getReportIcon = (type: string) => {
    switch (type) {
      case 'content':
        return <FileText className="h-4 w-4" />
      case 'user':
        return <Users className="h-4 w-4" />
      case 'comment':
        return <MessageSquare className="h-4 w-4" />
      default:
        return <Flag className="h-4 w-4" />
    }
  }

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <LoadingSpinner size="lg" />
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600 mb-4">Error al cargar las estadísticas del moderador</p>
        <Button onClick={() => window.location.reload()}>Reintentar</Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Panel de Moderación</h1>
          <p className="text-muted-foreground">Gestiona reportes y modera contenido</p>
        </div>
        <div className="flex items-center space-x-2">
          <Link href="/moderation/reports">
            <Button variant="outline">
              <Flag className="mr-2 h-4 w-4" />
              Ver Reportes
            </Button>
          </Link>
          <Link href="/moderation/content">
            <Button>
              <Shield className="mr-2 h-4 w-4" />
              Cola de Moderación
            </Button>
          </Link>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Reportes Pendientes</CardTitle>
            <AlertTriangle className="h-4 w-4 text-orange-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-orange-600">{stats?.pendingReports || 0}</div>
            <p className="text-xs text-muted-foreground">
              Requieren atención inmediata
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Contenido Pendiente</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.pendingContent || 0}</div>
            <p className="text-xs text-muted-foreground">
              En cola de moderación
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Reportes Resueltos</CardTitle>
            <CheckCircle className="h-4 w-4 text-green-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.totalReportsHandled || 0}</div>
            <p className="text-xs text-muted-foreground">
              +{stats?.monthlyStats.reportsHandled || 0} este mes
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Contenido Moderado</CardTitle>
            <Shield className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.totalContentModerated || 0}</div>
            <p className="text-xs text-muted-foreground">
              Total procesado
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Monthly Performance */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TrendingUp className="h-5 w-5" />
            Rendimiento del Mes
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="text-center">
              <div className="text-2xl font-bold text-green-600">{stats?.monthlyStats.reportsHandled || 0}</div>
              <p className="text-sm text-muted-foreground">Reportes Resueltos</p>
            </div>
            <div className="text-center">
              <div className="text-2xl font-bold text-blue-600">{stats?.monthlyStats.contentApproved || 0}</div>
              <p className="text-sm text-muted-foreground">Contenido Aprobado</p>
            </div>
            <div className="text-center">
              <div className="text-2xl font-bold text-red-600">{stats?.monthlyStats.contentRejected || 0}</div>
              <p className="text-sm text-muted-foreground">Contenido Rechazado</p>
            </div>
            <div className="text-center">
              <div className="text-2xl font-bold text-orange-600">{stats?.monthlyStats.usersWarned || 0}</div>
              <p className="text-sm text-muted-foreground">Usuarios Advertidos</p>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Recent Reports */}
        <Card>
          <CardHeader>
            <CardTitle>Reportes Recientes</CardTitle>
          </CardHeader>
          <CardContent>
            {stats?.recentReports && stats.recentReports.length > 0 ? (
              <div className="space-y-4">
                {stats.recentReports.slice(0, 5).map((report) => (
                  <div key={report.id} className="p-3 border rounded-lg">
                    <div className="flex items-start justify-between mb-2">
                      <div className="flex items-center gap-2">
                        {getReportIcon(report.type)}
                        <span className="font-medium">{report.reason}</span>
                      </div>
                      <Badge className={getPriorityColor(report.priority)}>
                        {report.priority === 'critical' ? 'Crítico' :
                         report.priority === 'high' ? 'Alto' :
                         report.priority === 'medium' ? 'Medio' : 'Bajo'}
                      </Badge>
                    </div>
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm text-muted-foreground">
                          Por: {report.reportedBy}
                        </p>
                        {report.targetTitle && (
                          <p className="text-sm font-medium truncate max-w-[200px]">
                            {report.targetTitle}
                          </p>
                        )}
                      </div>
                      <div className="text-right">
                        <Badge className={getStatusColor(report.status)}>
                          {report.status === 'pending' ? 'Pendiente' :
                           report.status === 'resolved' ? 'Resuelto' : 'Desestimado'}
                        </Badge>
                        <p className="text-xs text-muted-foreground mt-1">
                          {new Date(report.reportedAt).toLocaleDateString()}
                        </p>
                      </div>
                    </div>
                  </div>
                ))}
                <Link href="/moderation/reports">
                  <Button variant="ghost" className="w-full">
                    Ver Todos los Reportes
                  </Button>
                </Link>
              </div>
            ) : (
              <div className="text-center py-6">
                <Flag className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                <p className="text-muted-foreground">
                  No hay reportes recientes
                </p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Content Queue */}
        <Card>
          <CardHeader>
            <CardTitle>Cola de Contenido</CardTitle>
          </CardHeader>
          <CardContent>
            {stats?.contentQueue && stats.contentQueue.length > 0 ? (
              <div className="space-y-4">
                {stats.contentQueue.slice(0, 5).map((content) => (
                  <div key={content.id} className="p-3 border rounded-lg">
                    <div className="flex items-start justify-between mb-2">
                      <div>
                        <p className="font-medium truncate max-w-[200px]">{content.title}</p>
                        <p className="text-sm text-muted-foreground">
                          Por: {content.uploader} • {formatFileSize(content.size)}
                        </p>
                      </div>
                      <Badge className={getStatusColor(content.status)}>
                        {content.status === 'pending' ? 'Pendiente' :
                         content.status === 'approved' ? 'Aprobado' : 'Rechazado'}
                      </Badge>
                    </div>
                    {content.flags.length > 0 && (
                      <div className="flex flex-wrap gap-1 mb-2">
                        {content.flags.map((flag, index) => (
                          <Badge key={index} variant="outline" className="text-xs">
                            {flag}
                          </Badge>
                        ))}
                      </div>
                    )}
                    <p className="text-xs text-muted-foreground">
                      Subido: {new Date(content.uploadedAt).toLocaleDateString()}
                    </p>
                  </div>
                ))}
                <Link href="/moderation/content">
                  <Button variant="ghost" className="w-full">
                    Ver Cola Completa
                  </Button>
                </Link>
              </div>
            ) : (
              <div className="text-center py-6">
                <CheckCircle className="h-12 w-12 text-green-600 mx-auto mb-4" />
                <p className="text-muted-foreground">
                  No hay contenido pendiente de moderación
                </p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* User Reports */}
      <Card>
        <CardHeader>
          <CardTitle>Usuarios con Reportes</CardTitle>
        </CardHeader>
        <CardContent>
          {stats?.userReports && stats.userReports.length > 0 ? (
            <div className="space-y-4">
              {stats.userReports.slice(0, 5).map((user) => (
                <div key={user.id} className="flex items-center justify-between p-3 border rounded-lg">
                  <div className="flex items-center space-x-3">
                    <Users className="h-5 w-5 text-muted-foreground" />
                    <div>
                      <p className="font-medium">{user.username}</p>
                      <p className="text-sm text-muted-foreground">
                        {user.reportCount} reportes • Último: {new Date(user.lastReportAt).toLocaleDateString()}
                      </p>
                    </div>
                  </div>
                  <div className="text-right space-y-1">
                    <Badge className={getRiskLevelColor(user.riskLevel)}>
                      Riesgo {user.riskLevel === 'high' ? 'Alto' : 
                              user.riskLevel === 'medium' ? 'Medio' : 'Bajo'}
                    </Badge>
                    <Badge className={getStatusColor(user.status)}>
                      {user.status === 'active' ? 'Activo' :
                       user.status === 'warned' ? 'Advertido' : 'Suspendido'}
                    </Badge>
                  </div>
                </div>
              ))}
              <Link href="/moderation/users">
                <Button variant="ghost" className="w-full">
                  Ver Todos los Usuarios
                </Button>
              </Link>
            </div>
          ) : (
            <div className="text-center py-6">
              <Users className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">
                No hay usuarios con reportes activos
              </p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Acciones Rápidas</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-4">
            <Link href="/moderation/reports?priority=critical">
              <Button variant="outline" className="w-full h-20 flex-col border-red-200 hover:bg-red-50">
                <AlertTriangle className="h-6 w-6 mb-2 text-red-600" />
                Reportes Críticos
              </Button>
            </Link>
            <Link href="/moderation/content">
              <Button variant="outline" className="w-full h-20 flex-col">
                <Shield className="h-6 w-6 mb-2" />
                Moderar Contenido
              </Button>
            </Link>
            <Link href="/moderation/users">
              <Button variant="outline" className="w-full h-20 flex-col">
                <Users className="h-6 w-6 mb-2" />
                Gestionar Usuarios
              </Button>
            </Link>
            <Link href="/moderation/statistics">
              <Button variant="outline" className="w-full h-20 flex-col">
                <TrendingUp className="h-6 w-6 mb-2" />
                Ver Estadísticas
              </Button>
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}