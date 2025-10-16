'use client'

import React, { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { userManagementService } from '@/services/admin/user-management.service'
import { BulkUserOperation, UserRole } from '@/lib/types'
import { UserRoleSelect } from './UserRoleSelect'
import { AlertTriangle, Users, X } from 'lucide-react'

interface UserBulkActionsProps {
  selectedUserIds: string[]
  onActionComplete: () => void
}

export const UserBulkActions: React.FC<UserBulkActionsProps> = ({
  selectedUserIds,
  onActionComplete
}) => {
  const [operation, setOperation] = useState<BulkUserOperation | ''>('')
  const [targetRole, setTargetRole] = useState<UserRole>(UserRole.Reader)
  const [reason, setReason] = useState('')
  const [loading, setLoading] = useState(false)
  const [showConfirmation, setShowConfirmation] = useState(false)

  const getOperationName = (op: BulkUserOperation): string => {
    switch (op) {
      case BulkUserOperation.Activate:
        return 'Activar usuarios'
      case BulkUserOperation.Deactivate:
        return 'Desactivar usuarios'
      case BulkUserOperation.ChangeRole:
        return 'Cambiar rol'
      case BulkUserOperation.Delete:
        return 'Eliminar usuarios'
      default:
        return 'Operación desconocida'
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

  const handleExecute = async () => {
    if (!operation) return

    try {
      setLoading(true)
      
      const data: Record<string, any> = {}
      if (operation === BulkUserOperation.ChangeRole) {
        data.role = targetRole
      }

      const result = await userManagementService.bulkOperation({
        userIds: selectedUserIds,
        operation,
        data,
        reason: reason || undefined
      })

      console.log('Bulk operation result:', result)
      
      // Show success message
      alert(`Operación completada: ${result.successCount} usuarios procesados exitosamente`)
      
      if (result.failureCount > 0) {
        console.warn('Some operations failed:', result.errors)
      }

      onActionComplete()
      setShowConfirmation(false)
      setOperation('')
      setReason('')
    } catch (error) {
      console.error('Bulk operation error:', error)
      alert('Error al ejecutar la operación masiva')
    } finally {
      setLoading(false)
    }
  }

  const getConfirmationMessage = (): string => {
    const count = selectedUserIds.length
    switch (operation) {
      case BulkUserOperation.Activate:
        return `¿Estás seguro de que quieres activar ${count} usuario(s)?`
      case BulkUserOperation.Deactivate:
        return `¿Estás seguro de que quieres desactivar ${count} usuario(s)?`
      case BulkUserOperation.ChangeRole:
        return `¿Estás seguro de que quieres cambiar el rol de ${count} usuario(s) a ${getRoleName(targetRole)}?`
      case BulkUserOperation.Delete:
        return `¿Estás seguro de que quieres ELIMINAR ${count} usuario(s)? Esta acción no se puede deshacer.`
      default:
        return `¿Estás seguro de que quieres ejecutar esta operación en ${count} usuario(s)?`
    }
  }

  const isDestructive = operation === BulkUserOperation.Delete

  return (
    <Card className="border-blue-200 bg-blue-50">
      <CardContent className="pt-4">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-2">
            <Users className="h-5 w-5 text-blue-600" />
            <span className="font-medium">
              {selectedUserIds.length} usuario(s) seleccionado(s)
            </span>
          </div>
          <Button
            variant="ghost"
            size="sm"
            onClick={onActionComplete}
          >
            <X className="h-4 w-4" />
          </Button>
        </div>

        {!showConfirmation ? (
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div>
              <Label htmlFor="operation">Operación</Label>
              <Select
                value={operation}
                onValueChange={(value) => setOperation(value as BulkUserOperation)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Seleccionar operación" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value={BulkUserOperation.Activate}>
                    Activar usuarios
                  </SelectItem>
                  <SelectItem value={BulkUserOperation.Deactivate}>
                    Desactivar usuarios
                  </SelectItem>
                  <SelectItem value={BulkUserOperation.ChangeRole}>
                    Cambiar rol
                  </SelectItem>
                  <SelectItem value={BulkUserOperation.Delete}>
                    Eliminar usuarios
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>

            {operation === BulkUserOperation.ChangeRole && (
              <div>
                <Label htmlFor="role">Nuevo rol</Label>
                <UserRoleSelect
                  value={targetRole}
                  onChange={setTargetRole}
                />
              </div>
            )}

            <div className={operation === BulkUserOperation.ChangeRole ? 'md:col-span-1' : 'md:col-span-2'}>
              <Label htmlFor="reason">Razón (opcional)</Label>
              <Input
                id="reason"
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                placeholder="Motivo de la operación..."
              />
            </div>

            <div className="flex items-end">
              <Button
                onClick={() => setShowConfirmation(true)}
                disabled={!operation}
                variant={isDestructive ? 'destructive' : 'default'}
                className="w-full"
              >
                {operation ? getOperationName(operation) : 'Seleccionar operación'}
              </Button>
            </div>
          </div>
        ) : (
          <div className="space-y-4">
            <div className={`p-4 rounded-lg ${isDestructive ? 'bg-red-50 border border-red-200' : 'bg-yellow-50 border border-yellow-200'}`}>
              <div className="flex items-center gap-2 mb-2">
                <AlertTriangle className={`h-5 w-5 ${isDestructive ? 'text-red-600' : 'text-yellow-600'}`} />
                <span className="font-medium">Confirmar operación</span>
              </div>
              <p className="text-sm">{getConfirmationMessage()}</p>
              {reason && (
                <p className="text-sm mt-2">
                  <strong>Razón:</strong> {reason}
                </p>
              )}
            </div>

            <div className="flex gap-2 justify-end">
              <Button
                variant="outline"
                onClick={() => setShowConfirmation(false)}
                disabled={loading}
              >
                Cancelar
              </Button>
              <Button
                onClick={handleExecute}
                disabled={loading}
                variant={isDestructive ? 'destructive' : 'default'}
              >
                {loading ? 'Ejecutando...' : 'Confirmar'}
              </Button>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  )
}