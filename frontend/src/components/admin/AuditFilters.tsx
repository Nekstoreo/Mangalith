'use client'

import React, { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Card, CardContent } from '@/components/ui/card'
import { AuditLogFilter, AuditSeverity } from '@/lib/types'
import { Filter, X } from 'lucide-react'

interface AuditFiltersProps {
  filters: AuditLogFilter
  onFiltersChange: (filters: Partial<AuditLogFilter>) => void
}

export const AuditFilters: React.FC<AuditFiltersProps> = ({
  filters,
  onFiltersChange
}) => {
  const [showFilters, setShowFilters] = useState(false)

  const clearFilters = () => {
    onFiltersChange({
      userId: undefined,
      action: undefined,
      resource: undefined,
      success: undefined,
      severity: undefined,
      fromDate: undefined,
      toDate: undefined,
      ipAddress: undefined
    })
  }

  const hasActiveFilters = !!(
    filters.userId ||
    filters.action ||
    filters.resource ||
    filters.success !== undefined ||
    filters.severity ||
    filters.fromDate ||
    filters.toDate ||
    filters.ipAddress
  )

  const commonActions = [
    'user.login',
    'user.logout',
    'user.create',
    'user.update',
    'user.delete',
    'manga.create',
    'manga.update',
    'manga.delete',
    'file.upload',
    'file.delete',
    'system.configure'
  ]

  const commonResources = [
    'user',
    'manga',
    'chapter',
    'file',
    'system',
    'invitation'
  ]

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
        <Card className="absolute top-full right-0 mt-2 w-96 z-50 shadow-lg">
          <CardContent className="p-4 space-y-4">
            <div className="flex items-center justify-between">
              <h3 className="font-semibold">Filtros de Auditoría</h3>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setShowFilters(false)}
              >
                <X className="h-4 w-4" />
              </Button>
            </div>

            {/* Action Filter */}
            <div>
              <label className="text-sm font-medium mb-2 block">Acción</label>
              <Select
                value={filters.action || ''}
                onValueChange={(value) => 
                  onFiltersChange({ action: value || undefined })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todas las acciones" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Todas las acciones</SelectItem>
                  {commonActions.map(action => (
                    <SelectItem key={action} value={action}>
                      {action.split('.').map(part => 
                        part.charAt(0).toUpperCase() + part.slice(1)
                      ).join(' ')}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Resource Filter */}
            <div>
              <label className="text-sm font-medium mb-2 block">Recurso</label>
              <Select
                value={filters.resource || ''}
                onValueChange={(value) => 
                  onFiltersChange({ resource: value || undefined })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos los recursos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Todos los recursos</SelectItem>
                  {commonResources.map(resource => (
                    <SelectItem key={resource} value={resource}>
                      {resource.charAt(0).toUpperCase() + resource.slice(1)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Success Filter */}
            <div>
              <label className="text-sm font-medium mb-2 block">Estado</label>
              <Select
                value={filters.success?.toString() || ''}
                onValueChange={(value) => 
                  onFiltersChange({ 
                    success: value === '' ? undefined : value === 'true' 
                  })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos los estados" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Todos los estados</SelectItem>
                  <SelectItem value="true">Exitosos</SelectItem>
                  <SelectItem value="false">Con errores</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Severity Filter */}
            <div>
              <label className="text-sm font-medium mb-2 block">Severidad</label>
              <Select
                value={filters.severity || ''}
                onValueChange={(value) => 
                  onFiltersChange({ 
                    severity: value ? value as AuditSeverity : undefined 
                  })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todas las severidades" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Todas las severidades</SelectItem>
                  <SelectItem value={AuditSeverity.Info}>Info</SelectItem>
                  <SelectItem value={AuditSeverity.Warning}>Warning</SelectItem>
                  <SelectItem value={AuditSeverity.Error}>Error</SelectItem>
                  <SelectItem value={AuditSeverity.Critical}>Critical</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Date Range */}
            <div>
              <label className="text-sm font-medium mb-2 block">Rango de fechas</label>
              <div className="grid grid-cols-2 gap-2">
                <input
                  type="date"
                  value={filters.fromDate || ''}
                  onChange={(e) => onFiltersChange({ fromDate: e.target.value || undefined })}
                  className="px-3 py-2 border rounded-md text-sm"
                  placeholder="Desde"
                />
                <input
                  type="date"
                  value={filters.toDate || ''}
                  onChange={(e) => onFiltersChange({ toDate: e.target.value || undefined })}
                  className="px-3 py-2 border rounded-md text-sm"
                  placeholder="Hasta"
                />
              </div>
            </div>

            {/* IP Address Filter */}
            <div>
              <label className="text-sm font-medium mb-2 block">Dirección IP</label>
              <input
                type="text"
                value={filters.ipAddress || ''}
                onChange={(e) => onFiltersChange({ ipAddress: e.target.value || undefined })}
                className="w-full px-3 py-2 border rounded-md text-sm"
                placeholder="192.168.1.1"
              />
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