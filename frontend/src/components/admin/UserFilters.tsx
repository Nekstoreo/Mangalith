'use client'

import React, { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Card, CardContent } from '@/components/ui/card'
import { UserFilter, UserRole } from '@/lib/types'
import { Filter, X } from 'lucide-react'

interface UserFiltersProps {
  filters: UserFilter
  onFiltersChange: (filters: Partial<UserFilter>) => void
}

export const UserFilters: React.FC<UserFiltersProps> = ({
  filters,
  onFiltersChange
}) => {
  const [showFilters, setShowFilters] = useState(false)

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

  const clearFilters = () => {
    onFiltersChange({
      role: undefined,
      isActive: undefined,
      hasUploads: undefined,
      createdAfter: undefined,
      createdBefore: undefined,
      lastLoginAfter: undefined,
      lastLoginBefore: undefined
    })
  }

  const hasActiveFilters = !!(
    filters.role !== undefined ||
    filters.isActive !== undefined ||
    filters.hasUploads !== undefined ||
    filters.createdAfter ||
    filters.createdBefore ||
    filters.lastLoginAfter ||
    filters.lastLoginBefore
  )

  return (
    <div className="relative">
      <Button
        variant="outline"
        onClick={() => setShowFilters(!showFilters)}
        className={hasActiveFilters ? 'border-blue-500 bg-blue-50' : ''}
      >
        <Filter className="h-4 w-4 mr-2" />
        Filtros
        {hasActiveFilters && (
          <span className="ml-2 bg-blue-500 text-white rounded-full px-2 py-0.5 text-xs">
            {Object.values(filters).filter(v => v !== undefined && v !== '').length}
          </span>
        )}
      </Button>

      {showFilters && (
        <Card className="absolute top-full right-0 mt-2 w-80 z-50 shadow-lg">
          <CardContent className="p-4 space-y-4">
            <div className="flex items-center justify-between">
              <h3 className="font-semibold">Filtros</h3>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setShowFilters(false)}
              >
                <X className="h-4 w-4" />
              </Button>
            </div>

            {/* Role Filter */}
            <div>
              <label className="text-sm font-medium mb-2 block">Rol</label>
              <Select
                value={filters.role?.toString() || ''}
                onValueChange={(value) => 
                  onFiltersChange({ 
                    role: value ? parseInt(value) as UserRole : undefined 
                  })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos los roles" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Todos los roles</SelectItem>
                  <SelectItem value={UserRole.Reader.toString()}>
                    {getRoleName(UserRole.Reader)}
                  </SelectItem>
                  <SelectItem value={UserRole.Uploader.toString()}>
                    {getRoleName(UserRole.Uploader)}
                  </SelectItem>
                  <SelectItem value={UserRole.Moderator.toString()}>
                    {getRoleName(UserRole.Moderator)}
                  </SelectItem>
                  <SelectItem value={UserRole.Administrator.toString()}>
                    {getRoleName(UserRole.Administrator)}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Status Filter */}
            <div>
              <label className="text-sm font-medium mb-2 block">Estado</label>
              <Select
                value={filters.isActive?.toString() || ''}
                onValueChange={(value) => 
                  onFiltersChange({ 
                    isActive: value === '' ? undefined : value === 'true' 
                  })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos los estados" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Todos los estados</SelectItem>
                  <SelectItem value="true">Activos</SelectItem>
                  <SelectItem value="false">Inactivos</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Has Uploads Filter */}
            <div>
              <label className="text-sm font-medium mb-2 block">Subidas</label>
              <Select
                value={filters.hasUploads?.toString() || ''}
                onValueChange={(value) => 
                  onFiltersChange({ 
                    hasUploads: value === '' ? undefined : value === 'true' 
                  })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Todos</SelectItem>
                  <SelectItem value="true">Con subidas</SelectItem>
                  <SelectItem value="false">Sin subidas</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Date Filters */}
            <div>
              <label className="text-sm font-medium mb-2 block">Fecha de registro</label>
              <div className="grid grid-cols-2 gap-2">
                <input
                  type="date"
                  value={filters.createdAfter || ''}
                  onChange={(e) => onFiltersChange({ createdAfter: e.target.value || undefined })}
                  className="px-3 py-2 border rounded-md text-sm"
                  placeholder="Desde"
                />
                <input
                  type="date"
                  value={filters.createdBefore || ''}
                  onChange={(e) => onFiltersChange({ createdBefore: e.target.value || undefined })}
                  className="px-3 py-2 border rounded-md text-sm"
                  placeholder="Hasta"
                />
              </div>
            </div>

            <div>
              <label className="text-sm font-medium mb-2 block">Último acceso</label>
              <div className="grid grid-cols-2 gap-2">
                <input
                  type="date"
                  value={filters.lastLoginAfter || ''}
                  onChange={(e) => onFiltersChange({ lastLoginAfter: e.target.value || undefined })}
                  className="px-3 py-2 border rounded-md text-sm"
                  placeholder="Desde"
                />
                <input
                  type="date"
                  value={filters.lastLoginBefore || ''}
                  onChange={(e) => onFiltersChange({ lastLoginBefore: e.target.value || undefined })}
                  className="px-3 py-2 border rounded-md text-sm"
                  placeholder="Hasta"
                />
              </div>
            </div>

            {/* Actions */}
            <div className="flex gap-2 pt-2">
              <Button
                variant="outline"
                size="sm"
                onClick={clearFilters}
                disabled={!hasActiveFilters}
                className="flex-1"
              >
                Limpiar
              </Button>
              <Button
                size="sm"
                onClick={() => setShowFilters(false)}
                className="flex-1"
              >
                Aplicar
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}