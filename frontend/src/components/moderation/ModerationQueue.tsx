'use client'

import { useState } from 'react'
import { ModerationQueueItem } from '@/services/moderation/client'
import { Badge } from '@/components/ui/badge'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import { Search, Filter, SortAsc, SortDesc, Clock, AlertTriangle, User, Calendar } from 'lucide-react'

export type SortField = 'submittedAt' | 'priority' | 'reportCount' | 'creatorId'
export type SortDirection = 'asc' | 'desc'
export type FilterPriority = 'all' | 'high' | 'normal' | 'low'

export interface QueueFilters {
  priority: FilterPriority
  hasReports: boolean | null
  searchTerm: string
  sortField: SortField
  sortDirection: SortDirection
}

interface ModerationQueueFiltersProps {
  filters: QueueFilters
  onFiltersChange: (filters: QueueFilters) => void
  totalCount: number
  filteredCount: number
}

export function ModerationQueueFilters({
  filters,
  onFiltersChange,
  totalCount,
  filteredCount,
}: ModerationQueueFiltersProps) {
  const updateFilter = (key: keyof QueueFilters, value: any) => {
    onFiltersChange({ ...filters, [key]: value })
  }

  const clearFilters = () => {
    onFiltersChange({
      priority: 'all',
      hasReports: null,
      searchTerm: '',
      sortField: 'submittedAt',
      sortDirection: 'desc',
    })
  }

  const hasActiveFilters = 
    filters.priority !== 'all' || 
    filters.hasReports !== null || 
    filters.searchTerm !== '' ||
    filters.sortField !== 'submittedAt' ||
    filters.sortDirection !== 'desc'

  return (
    <Card className="p-4 space-y-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Filter className="w-4 h-4 text-muted-foreground" />
          <h3 className="font-medium">Filtros y Ordenamiento</h3>
          {hasActiveFilters && (
            <Button variant="ghost" size="sm" onClick={clearFilters}>
              Limpiar
            </Button>
          )}
        </div>
        <div className="text-sm text-muted-foreground">
          {filteredCount} de {totalCount} elementos
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Search */}
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <Input
            placeholder="Buscar por ID..."
            value={filters.searchTerm}
            onChange={(e) => updateFilter('searchTerm', e.target.value)}
            className="pl-9"
          />
        </div>

        {/* Priority Filter */}
        <Select value={filters.priority} onValueChange={(value) => updateFilter('priority', value)}>
          <SelectTrigger>
            <SelectValue placeholder="Prioridad" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Todas las prioridades</SelectItem>
            <SelectItem value="high">Alta prioridad</SelectItem>
            <SelectItem value="normal">Prioridad normal</SelectItem>
            <SelectItem value="low">Baja prioridad</SelectItem>
          </SelectContent>
        </Select>

        {/* Reports Filter */}
        <Select 
          value={filters.hasReports === null ? 'all' : filters.hasReports ? 'with-reports' : 'no-reports'} 
          onValueChange={(value) => 
            updateFilter('hasReports', value === 'all' ? null : value === 'with-reports')
          }
        >
          <SelectTrigger>
            <SelectValue placeholder="Reportes" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Todos</SelectItem>
            <SelectItem value="with-reports">Con reportes</SelectItem>
            <SelectItem value="no-reports">Sin reportes</SelectItem>
          </SelectContent>
        </Select>

        {/* Sort */}
        <div className="flex gap-2">
          <Select value={filters.sortField} onValueChange={(value) => updateFilter('sortField', value)}>
            <SelectTrigger className="flex-1">
              <SelectValue placeholder="Ordenar por" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="submittedAt">Fecha de envío</SelectItem>
              <SelectItem value="priority">Prioridad</SelectItem>
              <SelectItem value="reportCount">Número de reportes</SelectItem>
              <SelectItem value="creatorId">Creador</SelectItem>
            </SelectContent>
          </Select>
          <Button
            variant="outline"
            size="sm"
            onClick={() => updateFilter('sortDirection', filters.sortDirection === 'asc' ? 'desc' : 'asc')}
          >
            {filters.sortDirection === 'asc' ? <SortAsc className="w-4 h-4" /> : <SortDesc className="w-4 h-4" />}
          </Button>
        </div>
      </div>
    </Card>
  )
}

interface ModerationQueueCardProps {
  item: ModerationQueueItem
  isSelected?: boolean
  onSelect?: (item: ModerationQueueItem) => void
  onReview?: (item: ModerationQueueItem) => void
}

export function ModerationQueueCard({
  item,
  isSelected,
  onSelect,
  onReview,
}: ModerationQueueCardProps) {
  const priorityConfig = {
    high: { color: 'bg-red-100 text-red-800 border-red-200', label: 'Alta', icon: AlertTriangle },
    normal: { color: 'bg-yellow-100 text-yellow-800 border-yellow-200', label: 'Normal', icon: Clock },
    low: { color: 'bg-green-100 text-green-800 border-green-200', label: 'Baja', icon: Clock },
  }

  const priority = priorityConfig[item.priority]
  const PriorityIcon = priority.icon

  const timeAgo = (date: string) => {
    const now = new Date()
    const submitted = new Date(date)
    const diffInHours = Math.floor((now.getTime() - submitted.getTime()) / (1000 * 60 * 60))
    
    if (diffInHours < 1) return 'Hace menos de 1 hora'
    if (diffInHours < 24) return `Hace ${diffInHours} horas`
    const diffInDays = Math.floor(diffInHours / 24)
    return `Hace ${diffInDays} días`
  }

  return (
    <Card className={`p-4 cursor-pointer transition-all duration-200 ${
      isSelected 
        ? 'ring-2 ring-primary bg-primary/5' 
        : 'hover:bg-accent hover:shadow-md'
    }`}>
      <div className="flex items-start justify-between gap-4">
        <div className="flex-1 space-y-3">
          <div className="flex items-center gap-3">
            <input
              type="checkbox"
              checked={isSelected}
              onChange={() => onSelect?.(item)}
              className="w-4 h-4 rounded border-gray-300 text-primary focus:ring-primary"
            />
            <div className="flex items-center gap-2">
              <code className="text-xs bg-muted px-2 py-1 rounded font-mono">
                {item.id.substring(0, 8)}
              </code>
              <Badge className={`${priority.color} flex items-center gap-1`}>
                <PriorityIcon className="w-3 h-3" />
                {priority.label}
              </Badge>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-2 text-sm">
            <div className="flex items-center gap-2">
              <User className="w-4 h-4 text-muted-foreground" />
              <span className="text-muted-foreground">Manga:</span>
              <code className="text-xs bg-muted px-1 rounded">
                {item.mangaId.substring(0, 8)}
              </code>
            </div>
            <div className="flex items-center gap-2">
              <User className="w-4 h-4 text-muted-foreground" />
              <span className="text-muted-foreground">Creador:</span>
              <code className="text-xs bg-muted px-1 rounded">
                {item.creatorId.substring(0, 8)}
              </code>
            </div>
            <div className="flex items-center gap-2">
              <Calendar className="w-4 h-4 text-muted-foreground" />
              <span className="text-muted-foreground">Enviado:</span>
              <span className="text-xs">{timeAgo(item.submittedAtUtc)}</span>
            </div>
            {item.reportCount && item.reportCount > 0 && (
              <div className="flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 text-orange-500" />
                <span className="text-orange-600 font-medium text-xs">
                  {item.reportCount} reporte{item.reportCount > 1 ? 's' : ''} comunitario{item.reportCount > 1 ? 's' : ''}
                </span>
              </div>
            )}
          </div>
        </div>

        <Button onClick={() => onReview?.(item)} size="sm">
          Revisar
        </Button>
      </div>
    </Card>
  )
}

interface ModerationQueueListProps {
  items: ModerationQueueItem[]
  selectedItems?: Set<string>
  onSelect?: (item: ModerationQueueItem) => void
  onSelectAll?: (selected: boolean) => void
  onReview?: (item: ModerationQueueItem) => void
  isLoading?: boolean
  filters?: QueueFilters
  onFiltersChange?: (filters: QueueFilters) => void
  totalCount?: number
}

export function ModerationQueueList({
  items,
  selectedItems = new Set(),
  onSelect,
  onSelectAll,
  onReview,
  isLoading,
  filters,
  onFiltersChange,
  totalCount = 0,
}: ModerationQueueListProps) {
  const [localFilters, setLocalFilters] = useState<QueueFilters>({
    priority: 'all',
    hasReports: null,
    searchTerm: '',
    sortField: 'submittedAt',
    sortDirection: 'desc',
  })

  const activeFilters = filters || localFilters
  const handleFiltersChange = onFiltersChange || setLocalFilters

  // Apply filters and sorting
  const filteredItems = items
    .filter((item) => {
      if (activeFilters.priority !== 'all' && item.priority !== activeFilters.priority) {
        return false
      }
      if (activeFilters.hasReports !== null) {
        const hasReports = (item.reportCount || 0) > 0
        if (activeFilters.hasReports !== hasReports) {
          return false
        }
      }
      if (activeFilters.searchTerm) {
        const searchLower = activeFilters.searchTerm.toLowerCase()
        return (
          item.id.toLowerCase().includes(searchLower) ||
          item.mangaId.toLowerCase().includes(searchLower) ||
          item.creatorId.toLowerCase().includes(searchLower)
        )
      }
      return true
    })
    .sort((a, b) => {
      const { sortField, sortDirection } = activeFilters
      let comparison = 0

      switch (sortField) {
        case 'submittedAt':
          comparison = new Date(a.submittedAtUtc).getTime() - new Date(b.submittedAtUtc).getTime()
          break
        case 'priority':
          const priorityOrder = { high: 3, normal: 2, low: 1 }
          comparison = priorityOrder[a.priority] - priorityOrder[b.priority]
          break
        case 'reportCount':
          comparison = (a.reportCount || 0) - (b.reportCount || 0)
          break
        case 'creatorId':
          comparison = a.creatorId.localeCompare(b.creatorId)
          break
      }

      return sortDirection === 'desc' ? -comparison : comparison
    })

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="h-32 bg-muted rounded animate-pulse" />
        <div className="space-y-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <Card key={i} className="p-4 space-y-2 animate-pulse">
              <div className="h-4 bg-muted rounded w-3/4" />
              <div className="h-4 bg-muted rounded w-1/2" />
            </Card>
          ))}
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <ModerationQueueFilters
        filters={activeFilters}
        onFiltersChange={handleFiltersChange}
        totalCount={totalCount}
        filteredCount={filteredItems.length}
      />

      {filteredItems.length === 0 ? (
        <div className="text-center py-12">
          <div className="mx-auto w-16 h-16 bg-muted rounded-full flex items-center justify-center mb-4">
            <Search className="w-8 h-8 text-muted-foreground" />
          </div>
          <p className="text-lg font-medium">
            {items.length === 0 ? '✓ Cola vacía' : 'No se encontraron resultados'}
          </p>
          <p className="text-sm text-muted-foreground mt-1">
            {items.length === 0 
              ? 'No hay publicaciones pendientes de revisar' 
              : 'Intenta ajustar los filtros de búsqueda'
            }
          </p>
        </div>
      ) : (
        <div className="space-y-3">
          <div className="flex items-center gap-2 pb-2 border-b">
            <input
              type="checkbox"
              ref={(el) => {
                if (el) el.indeterminate = selectedItems.size > 0 && selectedItems.size < filteredItems.length
              }}
              checked={selectedItems.size === filteredItems.length && filteredItems.length > 0}
              onChange={(e) => onSelectAll?.(e.target.checked)}
              className="w-4 h-4 rounded border-gray-300 text-primary focus:ring-primary"
            />
            <span className="text-sm text-muted-foreground">
              {selectedItems.size > 0 
                ? `${selectedItems.size} seleccionados de ${filteredItems.length}` 
                : `Seleccionar todos (${filteredItems.length})`
              }
            </span>
          </div>

          {filteredItems.map((item) => (
            <ModerationQueueCard
              key={item.id}
              item={item}
              isSelected={selectedItems.has(item.id)}
              onSelect={onSelect}
              onReview={onReview}
            />
          ))}
        </div>
      )}
    </div>
  )
}
