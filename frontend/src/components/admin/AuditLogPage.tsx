'use client'

import React, { useState, useEffect } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { LoadingSpinner } from '@/components/ui/loading-spinner'
import { AuditLogTable } from './AuditLogTable'
import { AuditFilters } from './AuditFilters'
import { AuditStatistics } from './AuditStatistics'
import { auditService } from '@/services/admin/audit.service'
import { AuditLog, AuditLogFilter, AuditLogStatistics, PaginatedResponse } from '@/lib/types'
import { Search, Download, RefreshCw, BarChart3 } from 'lucide-react'

export const AuditLogPage: React.FC = () => {
  const [auditLogs, setAuditLogs] = useState<AuditLog[]>([])
  const [statistics, setStatistics] = useState<AuditLogStatistics | null>(null)
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [showStatistics, setShowStatistics] = useState(false)
  const [filters, setFilters] = useState<AuditLogFilter>({
    page: 1,
    pageSize: 50,
    sortBy: 'timestamp',
    sortDirection: 'desc'
  })
  const [pagination, setPagination] = useState({
    currentPage: 1,
    totalPages: 1,
    totalItems: 0,
    pageSize: 50
  })
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    loadAuditLogs()
  }, [filters])

  useEffect(() => {
    const delayedSearch = setTimeout(() => {
      if (searchQuery !== filters.action && searchQuery !== filters.resource) {
        setFilters(prev => ({
          ...prev,
          action: searchQuery.includes('.') ? searchQuery : undefined,
          resource: !searchQuery.includes('.') && searchQuery ? searchQuery : undefined,
          page: 1
        }))
      }
    }, 500)

    return () => clearTimeout(delayedSearch)
  }, [searchQuery])

  const loadAuditLogs = async () => {
    try {
      setLoading(true)
      setError(null)
      
      const response = await auditService.getAuditLogs(filters)
      
      if ('data' in response) {
        setAuditLogs(response.data)
        setPagination({
          currentPage: response.currentPage,
          totalPages: response.totalPages,
          totalItems: response.totalItems,
          pageSize: response.pageSize
        })
      } else {
        setAuditLogs(response as AuditLog[])
      }
    } catch (err) {
      console.error('Error loading audit logs:', err)
      setError('Error al cargar los logs de auditoría')
    } finally {
      setLoading(false)
    }
  }

  const loadStatistics = async () => {
    try {
      const stats = await auditService.getStatistics(
        filters.fromDate,
        filters.toDate
      )
      setStatistics(stats)
    } catch (err) {
      console.error('Error loading statistics:', err)
    }
  }

  const handleFilterChange = (newFilters: Partial<AuditLogFilter>) => {
    setFilters(prev => ({
      ...prev,
      ...newFilters,
      page: 1
    }))
  }

  const handlePageChange = (page: number) => {
    setFilters(prev => ({ ...prev, page }))
  }

  const handleExport = async () => {
    try {
      const exportResponse = await auditService.exportAuditLogs({
        format: 'CSV',
        includeDetails: true,
        includeUserInfo: true,
        filter: filters
      })

      // Download the file
      const blob = await auditService.downloadExport(exportResponse.fileName)
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = exportResponse.fileName
      document.body.appendChild(a)
      a.click()
      window.URL.revokeObjectURL(url)
      document.body.removeChild(a)
    } catch (err) {
      console.error('Error exporting audit logs:', err)
      alert('Error al exportar los logs de auditoría')
    }
  }

  const toggleStatistics = () => {
    setShowStatistics(!showStatistics)
    if (!showStatistics && !statistics) {
      loadStatistics()
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h2 className="text-2xl font-bold">Logs de Auditoría</h2>
          <p className="text-muted-foreground">
            Revisa todas las acciones realizadas en el sistema
          </p>
        </div>
        <div className="flex gap-2">
          <Button 
            variant="outline" 
            onClick={toggleStatistics}
            className={showStatistics ? 'bg-blue-50 border-blue-200' : ''}
          >
            <BarChart3 className="h-4 w-4 mr-2" />
            {showStatistics ? 'Ocultar' : 'Mostrar'} Estadísticas
          </Button>
          <Button variant="outline" onClick={handleExport}>
            <Download className="h-4 w-4 mr-2" />
            Exportar
          </Button>
        </div>
      </div>

      {/* Statistics */}
      {showStatistics && (
        <AuditStatistics 
          statistics={statistics}
          loading={!statistics}
          onRefresh={loadStatistics}
        />
      )}

      {/* Search and Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col lg:flex-row gap-4">
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Buscar por acción o recurso..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-10"
                />
              </div>
            </div>
            <div className="flex gap-2">
              <AuditFilters 
                filters={filters}
                onFiltersChange={handleFilterChange}
              />
              <Button 
                variant="outline" 
                size="icon"
                onClick={loadAuditLogs}
                disabled={loading}
              >
                <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Audit Logs Table */}
      <Card>
        <CardHeader>
          <CardTitle>
            Eventos de Auditoría ({pagination.totalItems})
          </CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="flex items-center justify-center py-12">
              <LoadingSpinner size="lg" />
            </div>
          ) : error ? (
            <div className="text-center py-12">
              <p className="text-red-600 mb-4">{error}</p>
              <Button onClick={loadAuditLogs}>Reintentar</Button>
            </div>
          ) : (
            <AuditLogTable
              auditLogs={auditLogs}
              pagination={pagination}
              onPageChange={handlePageChange}
              sortBy={filters.sortBy}
              sortDirection={filters.sortDirection}
              onSort={(sortBy, sortDirection) => 
                setFilters(prev => ({ ...prev, sortBy, sortDirection }))
              }
            />
          )}
        </CardContent>
      </Card>
    </div>
  )
}