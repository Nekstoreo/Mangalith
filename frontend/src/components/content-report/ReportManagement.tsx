'use client'

import { useState, useEffect } from 'react'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Textarea } from '@/components/ui/textarea'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import { 
  ContentReport, 
  ContentReportCategory, 
  ContentReportStatus,
  PagedContentReports 
} from '@/services/content-report/client'
import { useContentReport } from '@/hooks/useContentReport'
import { 
  Clock, 
  CheckCircle, 
  XCircle, 
  AlertTriangle, 
  Search, 
  Filter,
  Eye,
  MessageSquare,
  Calendar,
  User,
  FileText
} from 'lucide-react'

interface ReportManagementProps {
  className?: string
}

const categoryLabels: Record<ContentReportCategory, string> = {
  [ContentReportCategory.Copyright]: 'Derechos de Autor',
  [ContentReportCategory.InappropriateContent]: 'Contenido Inapropiado',
  [ContentReportCategory.Spam]: 'Spam',
  [ContentReportCategory.Harassment]: 'Acoso',
  [ContentReportCategory.Violence]: 'Violencia',
  [ContentReportCategory.AdultContent]: 'Contenido Adulto',
  [ContentReportCategory.Other]: 'Otro',
}

const statusConfig: Record<ContentReportStatus, {
  label: string
  icon: React.ReactNode
  variant: 'default' | 'secondary' | 'destructive' | 'outline'
  color: string
}> = {
  [ContentReportStatus.Pending]: {
    label: 'Pendiente',
    icon: <Clock className="w-3 h-3" />,
    variant: 'outline',
    color: 'text-yellow-600'
  },
  [ContentReportStatus.Reviewed]: {
    label: 'En Revisión',
    icon: <Eye className="w-3 h-3" />,
    variant: 'secondary',
    color: 'text-blue-600'
  },
  [ContentReportStatus.Resolved]: {
    label: 'Resuelto',
    icon: <CheckCircle className="w-3 h-3" />,
    variant: 'default',
    color: 'text-green-600'
  },
  [ContentReportStatus.Dismissed]: {
    label: 'Desestimado',
    icon: <XCircle className="w-3 h-3" />,
    variant: 'destructive',
    color: 'text-red-600'
  }
}

const priorityOrder = {
  [ContentReportCategory.Copyright]: 3,
  [ContentReportCategory.Harassment]: 3,
  [ContentReportCategory.Violence]: 3,
  [ContentReportCategory.AdultContent]: 3,
  [ContentReportCategory.InappropriateContent]: 2,
  [ContentReportCategory.Spam]: 1,
  [ContentReportCategory.Other]: 1
}

interface ReportFilters {
  status?: ContentReportStatus
  category?: ContentReportCategory
  search?: string
  sortBy: 'date' | 'priority' | 'category'
  sortOrder: 'asc' | 'desc'
}

export function ReportManagement({ className = '' }: ReportManagementProps) {
  const {
    reports,
    loading,
    error,
    totalCount,
    currentPage,
    getPendingReports,
    reviewReport,
    clearError
  } = useContentReport()

  const [selectedReport, setSelectedReport] = useState<ContentReport | null>(null)
  const [reviewNotes, setReviewNotes] = useState('')
  const [isReviewing, setIsReviewing] = useState(false)
  const [filters, setFilters] = useState<ReportFilters>({
    sortBy: 'priority',
    sortOrder: 'desc'
  })
  const [activeTab, setActiveTab] = useState('pending')

  useEffect(() => {
    loadReports()
  }, [activeTab, filters])

  const loadReports = async () => {
    try {
      await getPendingReports(1, 20)
    } catch (err) {
      console.error('Error loading reports:', err)
    }
  }

  const handleReviewReport = async (reportId: string, status: ContentReportStatus) => {
    if (!reviewNotes.trim() && status !== ContentReportStatus.Dismissed) {
      return
    }

    setIsReviewing(true)
    try {
      await reviewReport(reportId, status === ContentReportStatus.Resolved, reviewNotes)
      setSelectedReport(null)
      setReviewNotes('')
      await loadReports()
    } catch (err) {
      console.error('Error reviewing report:', err)
    } finally {
      setIsReviewing(false)
    }
  }

  const getReportPriority = (report: ContentReport): number => {
    return priorityOrder[report.category] || 1
  }

  const sortedReports = [...reports].sort((a, b) => {
    let comparison = 0
    
    switch (filters.sortBy) {
      case 'priority':
        comparison = getReportPriority(b) - getReportPriority(a)
        break
      case 'date':
        comparison = new Date(b.createdAtUtc).getTime() - new Date(a.createdAtUtc).getTime()
        break
      case 'category':
        comparison = categoryLabels[a.category].localeCompare(categoryLabels[b.category])
        break
    }
    
    return filters.sortOrder === 'desc' ? comparison : -comparison
  })

  const filteredReports = sortedReports.filter(report => {
    if (filters.status && report.status !== filters.status) return false
    if (filters.category && report.category !== filters.category) return false
    if (filters.search) {
      const searchLower = filters.search.toLowerCase()
      return (
        report.description.toLowerCase().includes(searchLower) ||
        categoryLabels[report.category].toLowerCase().includes(searchLower) ||
        report.id.toLowerCase().includes(searchLower)
      )
    }
    return true
  })

  const getPriorityBadge = (category: ContentReportCategory) => {
    const priority = priorityOrder[category]
    const colors = {
      3: 'bg-red-100 text-red-800 border-red-200',
      2: 'bg-orange-100 text-orange-800 border-orange-200',
      1: 'bg-yellow-100 text-yellow-800 border-yellow-200'
    }
    const labels = { 3: 'Alta', 2: 'Media', 1: 'Baja' }
    
    return (
      <Badge variant="outline" className={colors[priority as keyof typeof colors]}>
        {labels[priority as keyof typeof labels]}
      </Badge>
    )
  }

  return (
    <div className={`space-y-6 ${className}`}>
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Gestión de Reportes</h1>
          <p className="text-muted-foreground">
            Revisa y gestiona los reportes de contenido de la comunidad
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant="outline" className="bg-yellow-50">
            {totalCount} reportes totales
          </Badge>
        </div>
      </div>

      {/* Filters */}
      <Card className="p-4">
        <div className="flex flex-wrap gap-4 items-center">
          <div className="flex items-center gap-2">
            <Search className="w-4 h-4 text-muted-foreground" />
            <Input
              placeholder="Buscar reportes..."
              value={filters.search || ''}
              onChange={(e) => setFilters(prev => ({ ...prev, search: e.target.value }))}
              className="w-64"
            />
          </div>
          
          <Select
            value={filters.category?.toString() || 'all'}
            onValueChange={(value) => setFilters(prev => ({ 
              ...prev, 
              category: value === 'all' ? undefined : parseInt(value) as ContentReportCategory 
            }))}
          >
            <SelectTrigger className="w-48">
              <SelectValue placeholder="Todas las categorías" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Todas las categorías</SelectItem>
              {Object.entries(categoryLabels).map(([key, label]) => (
                <SelectItem key={key} value={key}>{label}</SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select
            value={filters.sortBy}
            onValueChange={(value) => setFilters(prev => ({ 
              ...prev, 
              sortBy: value as 'date' | 'priority' | 'category'
            }))}
          >
            <SelectTrigger className="w-40">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="priority">Por prioridad</SelectItem>
              <SelectItem value="date">Por fecha</SelectItem>
              <SelectItem value="category">Por categoría</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </Card>

      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="pending">Pendientes</TabsTrigger>
          <TabsTrigger value="reviewed">En Revisión</TabsTrigger>
          <TabsTrigger value="resolved">Resueltos</TabsTrigger>
          <TabsTrigger value="all">Todos</TabsTrigger>
        </TabsList>

        <TabsContent value={activeTab} className="space-y-4">
          {loading ? (
            <div className="space-y-3">
              {Array.from({ length: 3 }).map((_, i) => (
                <Card key={i} className="p-4 animate-pulse">
                  <div className="space-y-2">
                    <div className="h-4 bg-muted rounded w-3/4" />
                    <div className="h-4 bg-muted rounded w-1/2" />
                  </div>
                </Card>
              ))}
            </div>
          ) : filteredReports.length === 0 ? (
            <Card className="p-8 text-center">
              <AlertTriangle className="w-12 h-12 text-muted-foreground mx-auto mb-4" />
              <h3 className="text-lg font-medium mb-2">No hay reportes</h3>
              <p className="text-muted-foreground">
                No se encontraron reportes que coincidan con los filtros seleccionados.
              </p>
            </Card>
          ) : (
            <div className="grid gap-4">
              {filteredReports.map((report) => (
                <Card key={report.id} className="p-4">
                  <div className="flex items-start justify-between">
                    <div className="space-y-3 flex-1">
                      <div className="flex items-center gap-2 flex-wrap">
                        <Badge variant="outline">
                          {categoryLabels[report.category]}
                        </Badge>
                        {getPriorityBadge(report.category)}
                        <Badge variant={statusConfig[report.status].variant}>
                          <span className="flex items-center gap-1">
                            {statusConfig[report.status].icon}
                            {statusConfig[report.status].label}
                          </span>
                        </Badge>
                      </div>

                      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
                        <div className="flex items-center gap-2">
                          <FileText className="w-4 h-4 text-muted-foreground" />
                          <span className="text-muted-foreground">ID:</span>
                          <span className="font-mono">{report.id.substring(0, 8)}...</span>
                        </div>
                        <div className="flex items-center gap-2">
                          <User className="w-4 h-4 text-muted-foreground" />
                          <span className="text-muted-foreground">Reportado por:</span>
                          <span className="font-mono">{report.reporterId.substring(0, 8)}...</span>
                        </div>
                        <div className="flex items-center gap-2">
                          <Calendar className="w-4 h-4 text-muted-foreground" />
                          <span className="text-muted-foreground">Fecha:</span>
                          <span>{new Date(report.createdAtUtc).toLocaleDateString('es-ES')}</span>
                        </div>
                      </div>

                      {report.description && (
                        <div className="p-3 bg-muted rounded">
                          <p className="text-sm font-medium mb-1">Descripción:</p>
                          <p className="text-sm text-muted-foreground">{report.description}</p>
                        </div>
                      )}

                      {report.resolutionNotes && (
                        <div className="p-3 bg-green-50 rounded border border-green-200">
                          <p className="text-sm font-medium mb-1 text-green-900">Resolución:</p>
                          <p className="text-sm text-green-800">{report.resolutionNotes}</p>
                        </div>
                      )}
                    </div>

                    {report.status === ContentReportStatus.Pending && (
                      <Button
                        onClick={() => setSelectedReport(report)}
                        size="sm"
                        className="ml-4"
                      >
                        <Eye className="w-4 h-4 mr-2" />
                        Revisar
                      </Button>
                    )}
                  </div>
                </Card>
              ))}
            </div>
          )}
        </TabsContent>
      </Tabs>

      {/* Review Modal */}
      {selectedReport && (
        <>
          <div className="fixed inset-0 bg-black/50 z-40" onClick={() => setSelectedReport(null)} />
          <Card className="fixed left-1/2 top-1/2 transform -translate-x-1/2 -translate-y-1/2 w-full max-w-2xl max-h-[90vh] overflow-y-auto z-50 p-6 space-y-4">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-semibold">Revisar Reporte</h3>
              <Button variant="ghost" size="sm" onClick={() => setSelectedReport(null)}>
                ✕
              </Button>
            </div>

            <div className="space-y-4">
              <div className="flex items-center gap-2">
                <Badge variant="outline">{categoryLabels[selectedReport.category]}</Badge>
                {getPriorityBadge(selectedReport.category)}
              </div>

              <div className="p-4 bg-muted rounded">
                <p className="font-medium mb-2">Descripción del reporte:</p>
                <p className="text-sm">{selectedReport.description}</p>
              </div>

              <div className="space-y-2">
                <label className="block text-sm font-medium">
                  Notas de resolución *
                </label>
                <Textarea
                  placeholder="Explica la decisión tomada y las acciones realizadas..."
                  value={reviewNotes}
                  onChange={(e) => setReviewNotes(e.target.value)}
                  className="min-h-24"
                />
              </div>

              <div className="flex gap-2 justify-end pt-4 border-t">
                <Button
                  variant="outline"
                  onClick={() => setSelectedReport(null)}
                  disabled={isReviewing}
                >
                  Cancelar
                </Button>
                <Button
                  variant="destructive"
                  onClick={() => handleReviewReport(selectedReport.id, ContentReportStatus.Dismissed)}
                  disabled={isReviewing}
                >
                  Desestimar
                </Button>
                <Button
                  onClick={() => handleReviewReport(selectedReport.id, ContentReportStatus.Resolved)}
                  disabled={isReviewing || !reviewNotes.trim()}
                >
                  {isReviewing ? 'Procesando...' : 'Resolver'}
                </Button>
              </div>
            </div>
          </Card>
        </>
      )}
    </div>
  )
}