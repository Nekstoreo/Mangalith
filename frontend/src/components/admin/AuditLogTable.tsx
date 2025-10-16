'use client'

import React from 'react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { AuditLog, AuditSeverity } from '@/lib/types'
import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight, ArrowUpDown, ArrowUp, ArrowDown, CheckCircle, XCircle, AlertTriangle, AlertCircle } from 'lucide-react'

interface AuditLogTableProps {
  auditLogs: AuditLog[]
  pagination: {
    currentPage: number
    totalPages: number
    totalItems: number
    pageSize: number
  }
  onPageChange: (page: number) => void
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
  onSort: (sortBy: string, sortDirection: 'asc' | 'desc') => void
}

export const AuditLogTable: React.FC<AuditLogTableProps> = ({
  auditLogs,
  pagination,
  onPageChange,
  sortBy,
  sortDirection,
  onSort
}) => {
  const handleSort = (field: string) => {
    if (sortBy === field) {
      onSort(field, sortDirection === 'asc' ? 'desc' : 'asc')
    } else {
      onSort(field, 'desc')
    }
  }

  const getSortIcon = (field: string) => {
    if (sortBy !== field) return <ArrowUpDown className="h-4 w-4" />
    return sortDirection === 'asc' ? <ArrowUp className="h-4 w-4" /> : <ArrowDown className="h-4 w-4" />
  }

  const getSuccessIcon = (success: boolean) => {
    return success ? (
      <CheckCircle className="h-4 w-4 text-green-600" />
    ) : (
      <XCircle className="h-4 w-4 text-red-600" />
    )
  }

  const getSeverityBadge = (severity: AuditSeverity) => {
    const config = {
      [AuditSeverity.Info]: {
        color: 'bg-blue-100 text-blue-800',
        icon: <AlertCircle className="h-3 w-3" />
      },
      [AuditSeverity.Warning]: {
        color: 'bg-yellow-100 text-yellow-800',
        icon: <AlertTriangle className="h-3 w-3" />
      },
      [AuditSeverity.Error]: {
        color: 'bg-red-100 text-red-800',
        icon: <XCircle className="h-3 w-3" />
      },
      [AuditSeverity.Critical]: {
        color: 'bg-red-200 text-red-900',
        icon: <AlertTriangle className="h-3 w-3" />
      }
    }

    const { color, icon } = config[severity]

    return (
      <Badge className={`${color} flex items-center gap-1`}>
        {icon}
        {severity}
      </Badge>
    )
  }

  const formatAction = (action: string) => {
    return action.split('.').map(part => 
      part.charAt(0).toUpperCase() + part.slice(1)
    ).join(' ')
  }

  const formatResource = (resource: string, resourceId?: string) => {
    const formattedResource = resource.charAt(0).toUpperCase() + resource.slice(1)
    return resourceId ? `${formattedResource} (${resourceId.slice(0, 8)}...)` : formattedResource
  }

  const truncateDetails = (details: string | null, maxLength: number = 100) => {
    if (!details) return '-'
    return details.length > maxLength ? `${details.slice(0, maxLength)}...` : details
  }

  return (
    <div className="space-y-4">
      {/* Table */}
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="border-b">
              <th className="text-left p-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleSort('timestamp')}
                  className="h-auto p-0 font-semibold"
                >
                  Fecha/Hora
                  {getSortIcon('timestamp')}
                </Button>
              </th>
              <th className="text-left p-2">Usuario</th>
              <th className="text-left p-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleSort('action')}
                  className="h-auto p-0 font-semibold"
                >
                  Acción
                  {getSortIcon('action')}
                </Button>
              </th>
              <th className="text-left p-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleSort('resource')}
                  className="h-auto p-0 font-semibold"
                >
                  Recurso
                  {getSortIcon('resource')}
                </Button>
              </th>
              <th className="text-left p-2">Detalles</th>
              <th className="text-left p-2">Estado</th>
              <th className="text-left p-2">Severidad</th>
              <th className="text-left p-2">IP</th>
            </tr>
          </thead>
          <tbody>
            {auditLogs.map((log) => (
              <tr key={log.id} className="border-b hover:bg-gray-50">
                <td className="p-2 text-sm">
                  <div>
                    <div>{new Date(log.timestamp).toLocaleDateString()}</div>
                    <div className="text-xs text-muted-foreground">
                      {new Date(log.timestamp).toLocaleTimeString()}
                    </div>
                  </div>
                </td>
                <td className="p-2">
                  <div>
                    <div className="font-medium text-sm">{log.user.fullName}</div>
                    <div className="text-xs text-muted-foreground">{log.user.email}</div>
                    <Badge variant="outline" className="text-xs mt-1">
                      {log.user.role === 0 ? 'Lector' : 
                       log.user.role === 1 ? 'Subidor' :
                       log.user.role === 2 ? 'Moderador' : 'Administrador'}
                    </Badge>
                  </div>
                </td>
                <td className="p-2">
                  <span className="font-mono text-sm bg-gray-100 px-2 py-1 rounded">
                    {formatAction(log.action)}
                  </span>
                </td>
                <td className="p-2 text-sm">
                  {formatResource(log.resource, log.resourceId)}
                </td>
                <td className="p-2 text-sm max-w-xs">
                  <div className="truncate" title={log.details || undefined}>
                    {truncateDetails(log.details)}
                  </div>
                </td>
                <td className="p-2">
                  <div className="flex items-center gap-2">
                    {getSuccessIcon(log.success)}
                    <span className="text-sm">
                      {log.success ? 'Éxito' : 'Error'}
                    </span>
                  </div>
                </td>
                <td className="p-2">
                  {getSeverityBadge(log.severity)}
                </td>
                <td className="p-2 text-sm font-mono">
                  {log.ipAddress}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {pagination.totalPages > 1 && (
        <div className="flex items-center justify-between">
          <div className="text-sm text-muted-foreground">
            Mostrando {((pagination.currentPage - 1) * pagination.pageSize) + 1} a{' '}
            {Math.min(pagination.currentPage * pagination.pageSize, pagination.totalItems)} de{' '}
            {pagination.totalItems} eventos
          </div>
          
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => onPageChange(1)}
              disabled={pagination.currentPage === 1}
            >
              <ChevronsLeft className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => onPageChange(pagination.currentPage - 1)}
              disabled={pagination.currentPage === 1}
            >
              <ChevronLeft className="h-4 w-4" />
            </Button>
            
            <span className="text-sm">
              Página {pagination.currentPage} de {pagination.totalPages}
            </span>
            
            <Button
              variant="outline"
              size="sm"
              onClick={() => onPageChange(pagination.currentPage + 1)}
              disabled={pagination.currentPage === pagination.totalPages}
            >
              <ChevronRight className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => onPageChange(pagination.totalPages)}
              disabled={pagination.currentPage === pagination.totalPages}
            >
              <ChevronsRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}

      {auditLogs.length === 0 && (
        <div className="text-center py-12">
          <p className="text-muted-foreground">No se encontraron eventos de auditoría</p>
        </div>
      )}
    </div>
  )
}