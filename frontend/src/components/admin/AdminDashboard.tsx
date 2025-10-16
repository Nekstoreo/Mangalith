'use client'

import React, { useEffect, useState } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { LoadingSpinner } from '@/components/ui/loading-spinner'
import { userManagementService } from '@/services/admin/user-management.service'
import { auditService } from '@/services/admin/audit.service'
import { invitationService } from '@/services/admin/invitations.service'
import { UserManagementDashboard, AuditActivitySummary, InvitationStatistics, UserRole } from '@/lib/types'
import { Users, UserPlus, FileText, AlertTriangle, TrendingUp, Database } from 'lucide-react'
import { useRouter } from 'next/navigation'

export const AdminDashboard: React.FC = () => {
  const [dashboardData, setDashboardData] = useState<UserManagementDashboard | null>(null)
  const [auditSummary, setAuditSummary] = useState<AuditActivitySummary | null>(null)
  const [invitationStats, setInvitationStats] = useState<InvitationStatistics | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const router = useRouter()

  useEffect(() => {
    loadDashboardData()
  }, [])

  const loadDashboardData = async () => {
    try {
      setLoading(true)
      setError(null)

      const [dashboard, audit, invitations] = await Promise.all([
        userManagementService.getDashboardData(),
        auditService.getActivitySummary(),
        invitationService.getStatistics()
      ])

      setDashboardData(dashboard)
      setAuditSummary(audit)
      setInvitationStats(invitations)
    } catch (err) {
      console.error('Error loading dashboard data:', err)
      setError('Error al cargar los datos del dashboard')
    } finally {
      setLoading(false)
    }
  }

  const getRoleName = (role: UserRole): string => {
    switch (role) {
      case UserRole.Reader:
        return 'Lectores'
      case UserRole.Uploader:
        return 'Subidores'
      case UserRole.Moderator:
        return 'Moderadores'
      case UserRole.Administrator:
        return 'Administradores'
      default:
        return 'Desconocido'
    }
  }

  const getRoleColor = (role: UserRole): string => {
    switch (role) {
      case UserRole.Reader:
        return 'bg-blue-100 text-blue-800'
      case UserRole.Uploader:
        return 'bg-green-100 text-green-800'
      case UserRole.Moderator:
        return 'bg-yellow-100 text-yellow-800'
      case UserRole.Administrator:
        return 'bg-red-100 text-red-800'
      default:
        return 'bg-gray-100 text-gray-800'
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
        <Button onClick={loadDashboardData}>Reintentar</Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Usuarios</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{dashboardData?.statistics.totalUsers || 0}</div>
            <p className="text-xs text-muted-foreground">
              {dashboardData?.statistics.activeUsers || 0} activos
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Nuevos Usuarios</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{dashboardData?.statistics.newUsersLast30Days || 0}</div>
            <p className="text-xs text-muted-foreground">Últimos 30 días</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Invitaciones Pendientes</CardTitle>
            <UserPlus className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{invitationStats?.pendingInvitations || 0}</div>
            <p className="text-xs text-muted-foreground">
              {invitationStats?.expiringInNext7Days || 0} expiran pronto
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Eventos de Auditoría</CardTitle>
            <FileText className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{auditSummary?.last24Hours.totalEvents || 0}</div>
            <p className="text-xs text-muted-foreground">Últimas 24 horas</p>
          </CardContent>
        </Card>
      </div>

      {/* Role Distribution */}
      <Card>
        <CardHeader>
          <CardTitle>Distribución por Roles</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {dashboardData?.roleDistribution && Object.entries(dashboardData.roleDistribution).map(([role, count]) => (
              <div key={role} className="text-center">
                <Badge className={getRoleColor(parseInt(role) as UserRole)}>
                  {getRoleName(parseInt(role) as UserRole)}
                </Badge>
                <div className="text-2xl font-bold mt-2">{count}</div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Security Alerts */}
      {auditSummary?.securityAlerts && auditSummary.securityAlerts.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-yellow-600" />
              Alertas de Seguridad
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {auditSummary.securityAlerts.slice(0, 5).map((alert) => (
                <div key={alert.id} className="flex items-center justify-between p-3 border rounded-lg">
                  <div>
                    <p className="font-medium">{alert.message}</p>
                    <p className="text-sm text-muted-foreground">
                      {alert.eventCount} eventos • {new Date(alert.lastEvent).toLocaleString()}
                    </p>
                  </div>
                  <Badge variant={alert.severity === 'Critical' ? 'destructive' : 'secondary'}>
                    {alert.severity}
                  </Badge>
                </div>
              ))}
            </div>
            <Button 
              variant="outline" 
              className="w-full mt-4"
              onClick={() => router.push('/admin/audit')}
            >
              Ver Todos los Logs de Auditoría
            </Button>
          </CardContent>
        </Card>
      )}

      {/* Recent Users */}
      <Card>
        <CardHeader>
          <CardTitle>Usuarios Recientes</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            {dashboardData?.recentUsers?.slice(0, 5).map((user) => (
              <div key={user.id} className="flex items-center justify-between p-3 border rounded-lg">
                <div>
                  <p className="font-medium">{user.fullName}</p>
                  <p className="text-sm text-muted-foreground">{user.email}</p>
                </div>
                <div className="text-right">
                  <Badge className={getRoleColor(user.role)}>
                    {getRoleName(user.role)}
                  </Badge>
                  <p className="text-xs text-muted-foreground mt-1">
                    {new Date(user.createdAt).toLocaleDateString()}
                  </p>
                </div>
              </div>
            ))}
          </div>
          <Button 
            variant="outline" 
            className="w-full mt-4"
            onClick={() => router.push('/admin/users')}
          >
            Ver Todos los Usuarios
          </Button>
        </CardContent>
      </Card>

      {/* Storage Usage */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Database className="h-5 w-5" />
            Uso de Almacenamiento
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div>
              <div className="flex justify-between text-sm mb-1">
                <span>Total Usado</span>
                <span>{Math.round((dashboardData?.statistics.totalStorageUsed || 0) / 1024 / 1024)} MB</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div 
                  className="bg-blue-600 h-2 rounded-full" 
                  style={{ width: '45%' }}
                ></div>
              </div>
            </div>
            
            {dashboardData?.storageUsage.byRole && Object.entries(dashboardData.storageUsage.byRole).map(([role, usage]) => (
              <div key={role} className="flex justify-between items-center">
                <Badge className={getRoleColor(parseInt(role) as UserRole)}>
                  {getRoleName(parseInt(role) as UserRole)}
                </Badge>
                <span className="text-sm">{Math.round(usage / 1024 / 1024)} MB</span>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Acciones Rápidas</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Button 
              onClick={() => router.push('/admin/users')}
              className="flex items-center gap-2"
            >
              <Users className="h-4 w-4" />
              Gestionar Usuarios
            </Button>
            <Button 
              onClick={() => router.push('/admin/invitations')}
              variant="outline"
              className="flex items-center gap-2"
            >
              <UserPlus className="h-4 w-4" />
              Crear Invitación
            </Button>
            <Button 
              onClick={() => router.push('/admin/audit')}
              variant="outline"
              className="flex items-center gap-2"
            >
              <FileText className="h-4 w-4" />
              Ver Auditoría
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}