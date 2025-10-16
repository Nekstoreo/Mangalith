'use client'

import React from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Progress } from '@/components/ui/progress'
import { Badge } from '@/components/ui/badge'
import { 
  HardDrive, 
  Upload, 
  Shield, 
  Settings, 
  Crown,
  TrendingUp,
  AlertTriangle,
  Users,
  FileText,
  Zap
} from 'lucide-react'
import { UserRole } from '@/lib/types'
import { useAuthStore } from '@/stores/auth'
import { useApiData } from '@/hooks'
import Link from 'next/link'

interface QuotaInfo {
  storageUsed: number
  storageLimit: number
  uploadsThisMonth: number
  uploadLimit: number
  apiCallsToday: number
  apiCallLimit: number
}

interface RoleFeatures {
  available: string[]
  upcoming: string[]
  restricted: string[]
}

export const RoleSpecificSettings: React.FC = () => {
  const { user } = useAuthStore()
  const { data: quotaInfo } = useApiData<QuotaInfo>('/profile/quota')
  const { data: roleFeatures } = useApiData<RoleFeatures>(`/profile/role-features/${user?.role}`)

  if (!user) return null

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  const getRoleName = (role: UserRole) => {
    switch (role) {
      case UserRole.Reader:
        return 'Lector'
      case UserRole.Uploader:
        return 'Uploader'
      case UserRole.Moderator:
        return 'Moderador'
      case UserRole.Administrator:
        return 'Administrador'
      default:
        return 'Usuario'
    }
  }

  const getRoleIcon = (role: UserRole) => {
    switch (role) {
      case UserRole.Reader:
        return <FileText className="h-5 w-5" />
      case UserRole.Uploader:
        return <Upload className="h-5 w-5" />
      case UserRole.Moderator:
        return <Shield className="h-5 w-5" />
      case UserRole.Administrator:
        return <Crown className="h-5 w-5" />
      default:
        return <Users className="h-5 w-5" />
    }
  }

  const getRoleColor = (role: UserRole) => {
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

  const storagePercentage = quotaInfo ? (quotaInfo.storageUsed / quotaInfo.storageLimit) * 100 : 0
  const uploadPercentage = quotaInfo ? (quotaInfo.uploadsThisMonth / quotaInfo.uploadLimit) * 100 : 0
  const apiPercentage = quotaInfo ? (quotaInfo.apiCallsToday / quotaInfo.apiCallLimit) * 100 : 0

  return (
    <div className="space-y-6">
      {/* Role Information */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            {getRoleIcon(user.role)}
            Información del Rol
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-between mb-4">
            <div>
              <h3 className="text-lg font-semibold">Rol Actual</h3>
              <p className="text-muted-foreground">Tu nivel de acceso en la plataforma</p>
            </div>
            <Badge className={getRoleColor(user.role)}>
              {getRoleName(user.role)}
            </Badge>
          </div>

          {user.role === UserRole.Reader && (
            <div className="bg-blue-50 p-4 rounded-lg">
              <h4 className="font-medium text-blue-900 mb-2">¿Quieres contribuir más?</h4>
              <p className="text-sm text-blue-700 mb-3">
                Solicita convertirte en Uploader para subir tu propio contenido y ayudar a crecer la comunidad.
              </p>
              <Button size="sm" variant="outline">
                Solicitar Upgrade
              </Button>
            </div>
          )}

          {user.role === UserRole.Uploader && (
            <div className="bg-green-50 p-4 rounded-lg">
              <h4 className="font-medium text-green-900 mb-2">¡Gracias por contribuir!</h4>
              <p className="text-sm text-green-700 mb-3">
                Como Uploader, puedes subir contenido y gestionar tu biblioteca personal.
              </p>
              <Link href="/upload">
                <Button size="sm">
                  <Upload className="mr-2 h-4 w-4" />
                  Subir Contenido
                </Button>
              </Link>
            </div>
          )}

          {user.role === UserRole.Moderator && (
            <div className="bg-yellow-50 p-4 rounded-lg">
              <h4 className="font-medium text-yellow-900 mb-2">Panel de Moderación</h4>
              <p className="text-sm text-yellow-700 mb-3">
                Tienes acceso a herramientas de moderación para mantener la calidad de la plataforma.
              </p>
              <Link href="/moderation">
                <Button size="sm">
                  <Shield className="mr-2 h-4 w-4" />
                  Ir a Moderación
                </Button>
              </Link>
            </div>
          )}

          {user.role === UserRole.Administrator && (
            <div className="bg-red-50 p-4 rounded-lg">
              <h4 className="font-medium text-red-900 mb-2">Panel de Administración</h4>
              <p className="text-sm text-red-700 mb-3">
                Acceso completo a todas las funciones administrativas de la plataforma.
              </p>
              <Link href="/admin">
                <Button size="sm">
                  <Crown className="mr-2 h-4 w-4" />
                  Panel de Admin
                </Button>
              </Link>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Quotas and Limits */}
      {quotaInfo && (user.role === UserRole.Uploader || user.role === UserRole.Moderator || user.role === UserRole.Administrator) && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <HardDrive className="h-5 w-5" />
              Cuotas y Límites
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-6">
            {/* Storage Quota */}
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span>Almacenamiento</span>
                <span>{formatFileSize(quotaInfo.storageUsed)} / {formatFileSize(quotaInfo.storageLimit)}</span>
              </div>
              <Progress value={storagePercentage} className="h-2" />
              <div className="flex justify-between text-xs text-muted-foreground">
                <span>{storagePercentage.toFixed(1)}% utilizado</span>
                {storagePercentage > 80 && (
                  <span className="text-orange-600 flex items-center gap-1">
                    <AlertTriangle className="h-3 w-3" />
                    Cerca del límite
                  </span>
                )}
              </div>
            </div>

            {/* Upload Quota */}
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span>Subidas este mes</span>
                <span>{quotaInfo.uploadsThisMonth} / {quotaInfo.uploadLimit}</span>
              </div>
              <Progress value={uploadPercentage} className="h-2" />
              <div className="flex justify-between text-xs text-muted-foreground">
                <span>{uploadPercentage.toFixed(1)}% del límite mensual</span>
                {uploadPercentage > 80 && (
                  <span className="text-orange-600">Cerca del límite</span>
                )}
              </div>
            </div>

            {/* API Calls */}
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span>Llamadas API hoy</span>
                <span>{quotaInfo.apiCallsToday} / {quotaInfo.apiCallLimit}</span>
              </div>
              <Progress value={apiPercentage} className="h-2" />
              <div className="flex justify-between text-xs text-muted-foreground">
                <span>{apiPercentage.toFixed(1)}% del límite diario</span>
                {apiPercentage > 90 && (
                  <span className="text-red-600">Límite casi alcanzado</span>
                )}
              </div>
            </div>

            {(storagePercentage > 80 || uploadPercentage > 80) && (
              <div className="bg-orange-50 p-4 rounded-lg">
                <h4 className="font-medium text-orange-900 mb-2">¿Necesitas más espacio?</h4>
                <p className="text-sm text-orange-700 mb-3">
                  Contacta con un administrador para solicitar un aumento de cuota.
                </p>
                <Button size="sm" variant="outline">
                  Solicitar Aumento
                </Button>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Role Features */}
      {roleFeatures && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Zap className="h-5 w-5" />
              Características del Rol
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {/* Available Features */}
            <div>
              <h4 className="font-medium text-green-900 mb-2">✓ Características Disponibles</h4>
              <div className="grid gap-2 md:grid-cols-2">
                {roleFeatures.available.map((feature, index) => (
                  <div key={index} className="flex items-center gap-2 text-sm text-green-700">
                    <div className="h-2 w-2 bg-green-500 rounded-full" />
                    {feature}
                  </div>
                ))}
              </div>
            </div>

            {/* Upcoming Features */}
            {roleFeatures.upcoming.length > 0 && (
              <div>
                <h4 className="font-medium text-blue-900 mb-2">🚀 Próximamente</h4>
                <div className="grid gap-2 md:grid-cols-2">
                  {roleFeatures.upcoming.map((feature, index) => (
                    <div key={index} className="flex items-center gap-2 text-sm text-blue-700">
                      <div className="h-2 w-2 bg-blue-500 rounded-full" />
                      {feature}
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Restricted Features */}
            {roleFeatures.restricted.length > 0 && (
              <div>
                <h4 className="font-medium text-gray-600 mb-2">🔒 Requiere Mayor Rol</h4>
                <div className="grid gap-2 md:grid-cols-2">
                  {roleFeatures.restricted.map((feature, index) => (
                    <div key={index} className="flex items-center gap-2 text-sm text-gray-500">
                      <div className="h-2 w-2 bg-gray-400 rounded-full" />
                      {feature}
                    </div>
                  ))}
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Role-Specific Settings */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Settings className="h-5 w-5" />
            Configuración Específica
          </CardTitle>
        </CardHeader>
        <CardContent>
          {user.role === UserRole.Reader && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium">Notificaciones de Nuevos Capítulos</h4>
                  <p className="text-sm text-muted-foreground">Recibe alertas cuando se publiquen nuevos capítulos</p>
                </div>
                <Button variant="outline" size="sm">Configurar</Button>
              </div>
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium">Recomendaciones Personalizadas</h4>
                  <p className="text-sm text-muted-foreground">Mejora las sugerencias basadas en tu historial</p>
                </div>
                <Button variant="outline" size="sm">Configurar</Button>
              </div>
            </div>
          )}

          {user.role === UserRole.Uploader && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium">Auto-procesamiento</h4>
                  <p className="text-sm text-muted-foreground">Procesar automáticamente archivos subidos</p>
                </div>
                <Button variant="outline" size="sm">Configurar</Button>
              </div>
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium">Notificaciones de Procesamiento</h4>
                  <p className="text-sm text-muted-foreground">Alertas sobre el estado de tus subidas</p>
                </div>
                <Button variant="outline" size="sm">Configurar</Button>
              </div>
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium">Metadatos Automáticos</h4>
                  <p className="text-sm text-muted-foreground">Extraer metadatos automáticamente de archivos</p>
                </div>
                <Button variant="outline" size="sm">Configurar</Button>
              </div>
            </div>
          )}

          {user.role === UserRole.Moderator && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium">Alertas de Moderación</h4>
                  <p className="text-sm text-muted-foreground">Notificaciones de reportes y contenido pendiente</p>
                </div>
                <Button variant="outline" size="sm">Configurar</Button>
              </div>
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium">Auto-aprobación</h4>
                  <p className="text-sm text-muted-foreground">Criterios para aprobación automática de contenido</p>
                </div>
                <Button variant="outline" size="sm">Configurar</Button>
              </div>
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium">Filtros de Contenido</h4>
                  <p className="text-sm text-muted-foreground">Personalizar filtros para la cola de moderación</p>
                </div>
                <Button variant="outline" size="sm">Configurar</Button>
              </div>
            </div>
          )}

          {user.role === UserRole.Administrator && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium">Alertas del Sistema</h4>
                  <p className="text-sm text-muted-foreground">Notificaciones críticas del sistema</p>
                </div>
                <Button variant="outline" size="sm">Configurar</Button>
              </div>
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium">Respaldos Automáticos</h4>
                  <p className="text-sm text-muted-foreground">Programar respaldos de la base de datos</p>
                </div>
                <Button variant="outline" size="sm">Configurar</Button>
              </div>
              <div className="flex items-center justify-between">
                <div>
                  <h4 className="font-medium">Monitoreo de Rendimiento</h4>
                  <p className="text-sm text-muted-foreground">Alertas de rendimiento y uso de recursos</p>
                </div>
                <Button variant="outline" size="sm">Configurar</Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}