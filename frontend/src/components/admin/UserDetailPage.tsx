'use client'

import React, { useState, useEffect } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { LoadingSpinner } from '@/components/ui/loading-spinner'
import { UserStatusBadge } from './UserStatusBadge'
import { UserRoleSelect } from './UserRoleSelect'
import { userManagementService } from '@/services/admin/user-management.service'
import { UserDetail, UserRole, UpdateUserRequest } from '@/lib/types'
import { ArrowLeft, Save, Trash2, User, Activity, Settings, History } from 'lucide-react'
import { useRouter } from 'next/navigation'

interface UserDetailPageProps {
  userId: string
}

export const UserDetailPage: React.FC<UserDetailPageProps> = ({ userId }) => {
  const [user, setUser] = useState<UserDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [editMode, setEditMode] = useState(false)
  const [formData, setFormData] = useState({
    fullName: '',
    username: '',
    role: UserRole.Reader,
    isActive: true,
    reason: ''
  })
  const router = useRouter()

  useEffect(() => {
    loadUser()
  }, [userId])

  useEffect(() => {
    if (user) {
      setFormData({
        fullName: user.fullName,
        username: user.username || '',
        role: user.role,
        isActive: user.isActive,
        reason: ''
      })
    }
  }, [user])

  const loadUser = async () => {
    try {
      setLoading(true)
      setError(null)
      const userData = await userManagementService.getUserDetail(userId)
      setUser(userData)
    } catch (err) {
      console.error('Error loading user:', err)
      setError('Error al cargar los datos del usuario')
    } finally {
      setLoading(false)
    }
  }

  const handleSave = async () => {
    if (!user) return

    try {
      setSaving(true)
      
      const updates: UpdateUserRequest = {}
      
      if (formData.fullName !== user.fullName) {
        updates.fullName = formData.fullName
      }
      if (formData.username !== (user.username || '')) {
        updates.username = formData.username || undefined
      }
      if (formData.role !== user.role) {
        updates.role = formData.role
      }
      if (formData.isActive !== user.isActive) {
        updates.isActive = formData.isActive
      }
      if (formData.reason) {
        updates.reason = formData.reason
      }

      if (Object.keys(updates).length === 0) {
        setEditMode(false)
        return
      }

      const updatedUser = await userManagementService.updateUser(userId, updates)
      setUser(updatedUser)
      setEditMode(false)
      setFormData(prev => ({ ...prev, reason: '' }))
    } catch (err) {
      console.error('Error updating user:', err)
      alert('Error al actualizar el usuario')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async () => {
    if (!user) return

    const confirmed = confirm(
      `¿Estás seguro de que quieres eliminar al usuario ${user.fullName}? Esta acción no se puede deshacer.`
    )

    if (!confirmed) return

    try {
      await userManagementService.deleteUser(userId, formData.reason || undefined)
      router.push('/admin/users')
    } catch (err) {
      console.error('Error deleting user:', err)
      alert('Error al eliminar el usuario')
    }
  }

  const getRoleName = (role: UserRole): string => {
    switch (role) {
      case UserRole.Reader:
        return 'Lector'
      case UserRole.Uploader:
        return 'Subidor'
      case UserRole.Moderator:
        return 'Moderador'
      case UserRole.Administrator:
        return 'Administrador'
      default:
        return 'Desconocido'
    }
  }

  const formatBytes = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <LoadingSpinner size="lg" />
      </div>
    )
  }

  if (error || !user) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600 mb-4">{error || 'Usuario no encontrado'}</p>
        <Button onClick={() => router.push('/admin/users')}>
          Volver a usuarios
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button
            variant="outline"
            size="sm"
            onClick={() => router.push('/admin/users')}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Volver
          </Button>
          <div>
            <h2 className="text-2xl font-bold">{user.fullName}</h2>
            <p className="text-muted-foreground">{user.email}</p>
          </div>
        </div>
        
        <div className="flex gap-2">
          {editMode ? (
            <>
              <Button
                variant="outline"
                onClick={() => {
                  setEditMode(false)
                  setFormData({
                    fullName: user.fullName,
                    username: user.username || '',
                    role: user.role,
                    isActive: user.isActive,
                    reason: ''
                  })
                }}
                disabled={saving}
              >
                Cancelar
              </Button>
              <Button
                onClick={handleSave}
                disabled={saving}
              >
                <Save className="h-4 w-4 mr-2" />
                {saving ? 'Guardando...' : 'Guardar'}
              </Button>
            </>
          ) : (
            <Button onClick={() => setEditMode(true)}>
              Editar Usuario
            </Button>
          )}
        </div>
      </div>

      <Tabs defaultValue="profile" className="w-full">
        <TabsList>
          <TabsTrigger value="profile" className="flex items-center gap-2">
            <User className="h-4 w-4" />
            Perfil
          </TabsTrigger>
          <TabsTrigger value="activity" className="flex items-center gap-2">
            <Activity className="h-4 w-4" />
            Actividad
          </TabsTrigger>
          <TabsTrigger value="settings" className="flex items-center gap-2">
            <Settings className="h-4 w-4" />
            Configuración
          </TabsTrigger>
          <TabsTrigger value="history" className="flex items-center gap-2">
            <History className="h-4 w-4" />
            Historial
          </TabsTrigger>
        </TabsList>

        <TabsContent value="profile" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Información del Usuario</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="fullName">Nombre completo</Label>
                  <Input
                    id="fullName"
                    value={formData.fullName}
                    onChange={(e) => setFormData(prev => ({ ...prev, fullName: e.target.value }))}
                    disabled={!editMode}
                  />
                </div>
                
                <div>
                  <Label htmlFor="username">Nombre de usuario</Label>
                  <Input
                    id="username"
                    value={formData.username}
                    onChange={(e) => setFormData(prev => ({ ...prev, username: e.target.value }))}
                    disabled={!editMode}
                    placeholder="Opcional"
                  />
                </div>
                
                <div>
                  <Label htmlFor="email">Email</Label>
                  <Input
                    id="email"
                    value={user.email}
                    disabled
                  />
                </div>
                
                <div>
                  <Label htmlFor="role">Rol</Label>
                  {editMode ? (
                    <UserRoleSelect
                      value={formData.role}
                      onChange={(role) => setFormData(prev => ({ ...prev, role }))}
                    />
                  ) : (
                    <div className="mt-2">
                      <Badge>{getRoleName(user.role)}</Badge>
                    </div>
                  )}
                </div>
                
                <div>
                  <Label>Estado</Label>
                  {editMode ? (
                    <div className="mt-2">
                      <label className="flex items-center gap-2">
                        <input
                          type="checkbox"
                          checked={formData.isActive}
                          onChange={(e) => setFormData(prev => ({ ...prev, isActive: e.target.checked }))}
                        />
                        Usuario activo
                      </label>
                    </div>
                  ) : (
                    <div className="mt-2">
                      <UserStatusBadge isActive={user.isActive} />
                    </div>
                  )}
                </div>
                
                <div>
                  <Label>Fecha de registro</Label>
                  <div className="mt-2 text-sm text-muted-foreground">
                    {new Date(user.createdAt).toLocaleString()}
                  </div>
                </div>
                
                <div>
                  <Label>Último acceso</Label>
                  <div className="mt-2 text-sm text-muted-foreground">
                    {user.lastLoginAt 
                      ? new Date(user.lastLoginAt).toLocaleString()
                      : 'Nunca'
                    }
                  </div>
                </div>
                
                <div>
                  <Label>Última actualización</Label>
                  <div className="mt-2 text-sm text-muted-foreground">
                    {new Date(user.updatedAt).toLocaleString()}
                  </div>
                </div>
              </div>

              {editMode && (
                <div>
                  <Label htmlFor="reason">Razón del cambio</Label>
                  <Input
                    id="reason"
                    value={formData.reason}
                    onChange={(e) => setFormData(prev => ({ ...prev, reason: e.target.value }))}
                    placeholder="Motivo de los cambios..."
                  />
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="activity" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Estadísticas de Actividad</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="text-center">
                  <div className="text-2xl font-bold">{user.activityStats.totalUploads}</div>
                  <div className="text-sm text-muted-foreground">Subidas totales</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold">{user.activityStats.totalMangaCreated}</div>
                  <div className="text-sm text-muted-foreground">Manga creados</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold">{user.activityStats.totalChaptersCreated}</div>
                  <div className="text-sm text-muted-foreground">Capítulos creados</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold">{formatBytes(user.activityStats.storageUsed)}</div>
                  <div className="text-sm text-muted-foreground">Almacenamiento usado</div>
                </div>
              </div>
              
              {user.activityStats.lastActivityAt && (
                <div className="mt-4 pt-4 border-t">
                  <p className="text-sm text-muted-foreground">
                    Última actividad: {new Date(user.activityStats.lastActivityAt).toLocaleString()}
                  </p>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="settings" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Configuración de Cuenta</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="p-4 border border-red-200 rounded-lg bg-red-50">
                <h3 className="font-semibold text-red-800 mb-2">Zona de Peligro</h3>
                <p className="text-sm text-red-700 mb-4">
                  Estas acciones son permanentes y no se pueden deshacer.
                </p>
                
                {editMode && (
                  <div className="mb-4">
                    <Label htmlFor="deleteReason">Razón para eliminar</Label>
                    <Input
                      id="deleteReason"
                      value={formData.reason}
                      onChange={(e) => setFormData(prev => ({ ...prev, reason: e.target.value }))}
                      placeholder="Motivo de la eliminación..."
                    />
                  </div>
                )}
                
                <Button
                  variant="destructive"
                  onClick={handleDelete}
                  className="flex items-center gap-2"
                >
                  <Trash2 className="h-4 w-4" />
                  Eliminar Usuario
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="history" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Historial de Cambios de Rol</CardTitle>
            </CardHeader>
            <CardContent>
              {user.roleHistory.length > 0 ? (
                <div className="space-y-3">
                  {user.roleHistory.map((change, index) => (
                    <div key={index} className="flex items-center justify-between p-3 border rounded-lg">
                      <div>
                        <p className="font-medium">
                          {getRoleName(change.previousRole)} → {getRoleName(change.newRole)}
                        </p>
                        <p className="text-sm text-muted-foreground">
                          Por {change.changedByUser} • {new Date(change.changedAt).toLocaleString()}
                        </p>
                        {change.reason && (
                          <p className="text-sm text-muted-foreground mt-1">
                            Razón: {change.reason}
                          </p>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-muted-foreground text-center py-8">
                  No hay cambios de rol registrados
                </p>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}