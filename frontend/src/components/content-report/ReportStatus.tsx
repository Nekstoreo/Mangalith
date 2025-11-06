'use client'

import { useState, useEffect } from 'react'
import { Card } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Progress } from '@/components/ui/progress'
import { 
  ContentReport, 
  ContentReportCategory, 
  ContentReportStatus 
} from '@/services/content-report/client'
import { useContentReport } from '@/hooks/useContentReport'
import { 
  Clock, 
  CheckCircle, 
  XCircle, 
  Eye, 
  AlertCircle,
  MessageSquare,
  Calendar,
  FileText,
  RefreshCw
} from 'lucide-react'

interface ReportStatusProps {
  userId?: string
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
  description: string
  icon: React.ReactNode
  variant: 'default' | 'secondary' | 'destructive' | 'outline'
  color: string
  progress: number
}> = {
  [ContentReportStatus.Pending]: {
    label: 'Pendiente',
    description: 'Tu reporte está en cola esperando revisión',
    icon: <Clock className="w-4 h-4" />,
    variant: 'outline',
    color: 'text-yellow-600',
    progress: 25
  },
  [ContentReportStatus.Reviewed]: {
    label: 'En Revisión',
    description: 'Un moderador está revisando tu reporte',
    icon: <Eye className="w-4 h-4" />,
    variant: 'secondary',
    color: 'text-blue-600',
    progress: 75
  },
  [ContentReportStatus.Resolved]: {
    label: 'Resuelto',
    description: 'Tu reporte ha sido procesado y resuelto',
    icon: <CheckCircle className="w-4 h-4" />,
    variant: 'default',
    color: 'text-green-600',
    progress: 100
  },
  [ContentReportStatus.Dismissed]: {
    label: 'Desestimado',
    description: 'El reporte fue revisado pero no se encontró violación',
    icon: <XCircle className="w-4 h-4" />,
    variant: 'destructive',
    color: 'text-red-600',
    progress: 100
  }
}

interface ReportStatusCardProps {
  report: ContentReport
  onRefresh?: () => void
}

function ReportStatusCard({ report, onRefresh }: ReportStatusCardProps) {
  const config = statusConfig[report.status]
  const daysSinceReport = Math.floor(
    (new Date().getTime() - new Date(report.createdAtUtc).getTime()) / (1000 * 60 * 60 * 24)
  )

  const getEstimatedTime = () => {
    if (report.status === ContentReportStatus.Resolved || report.status === ContentReportStatus.Dismissed) {
      return null
    }
    
    const baseDays = report.category === ContentReportCategory.Copyright || 
                    report.category === ContentReportCategory.Harassment ||
                    report.category === ContentReportCategory.Violence ||
                    report.category === ContentReportCategory.AdultContent ? 1 : 3
    const remainingDays = Math.max(0, baseDays - daysSinceReport)
    
    if (remainingDays === 0) {
      return 'Procesándose pronto'
    }
    return `~${remainingDays} día${remainingDays > 1 ? 's' : ''} restante${remainingDays > 1 ? 's' : ''}`
  }

  return (
    <Card className="p-4 space-y-4">
      <div className="flex items-start justify-between">
        <div className="space-y-2 flex-1">
          <div className="flex items-center gap-2 flex-wrap">
            <Badge variant="outline">
              {categoryLabels[report.category]}
            </Badge>
            <Badge variant={config.variant}>
              <span className="flex items-center gap-1">
                {config.icon}
                {config.label}
              </span>
            </Badge>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-3 text-sm">
            <div className="flex items-center gap-2">
              <FileText className="w-4 h-4 text-muted-foreground" />
              <span className="text-muted-foreground">ID:</span>
              <span className="font-mono">{report.id.substring(0, 8)}...</span>
            </div>
            <div className="flex items-center gap-2">
              <Calendar className="w-4 h-4 text-muted-foreground" />
              <span className="text-muted-foreground">Reportado:</span>
              <span>{new Date(report.createdAtUtc).toLocaleDateString('es-ES')}</span>
            </div>
          </div>
        </div>

        {(report.status === ContentReportStatus.Pending || report.status === ContentReportStatus.Reviewed) && (
          <Button variant="ghost" size="sm" onClick={onRefresh}>
            <RefreshCw className="w-4 h-4" />
          </Button>
        )}
      </div>

      {/* Progress Bar */}
      <div className="space-y-2">
        <div className="flex items-center justify-between text-sm">
          <span className="text-muted-foreground">Progreso</span>
          <span className={config.color}>{config.progress}%</span>
        </div>
        <Progress value={config.progress} className="h-2" />
      </div>

      {/* Status Description */}
      <div className="p-3 bg-muted rounded">
        <div className="flex items-start gap-2">
          <div className={`mt-0.5 ${config.color}`}>
            {config.icon}
          </div>
          <div className="space-y-1">
            <p className="text-sm font-medium">{config.description}</p>
            {getEstimatedTime() && (
              <p className="text-xs text-muted-foreground">
                Tiempo estimado: {getEstimatedTime()}
              </p>
            )}
          </div>
        </div>
      </div>

      {/* Report Description */}
      {report.description && (
        <div className="p-3 border rounded">
          <p className="text-sm font-medium mb-1">Tu reporte:</p>
          <p className="text-sm text-muted-foreground">{report.description}</p>
        </div>
      )}

      {/* Resolution Notes */}
      {report.resolutionNotes && (
        <div className={`p-3 rounded border ${
          report.status === ContentReportStatus.Resolved 
            ? 'bg-green-50 border-green-200' 
            : 'bg-red-50 border-red-200'
        }`}>
          <div className="flex items-start gap-2">
            <MessageSquare className={`w-4 h-4 mt-0.5 ${
              report.status === ContentReportStatus.Resolved ? 'text-green-600' : 'text-red-600'
            }`} />
            <div>
              <p className={`text-sm font-medium mb-1 ${
                report.status === ContentReportStatus.Resolved ? 'text-green-900' : 'text-red-900'
              }`}>
                Respuesta del moderador:
              </p>
              <p className={`text-sm ${
                report.status === ContentReportStatus.Resolved ? 'text-green-800' : 'text-red-800'
              }`}>
                {report.resolutionNotes}
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Review Date */}
      {report.reviewedAtUtc && (
        <div className="text-xs text-muted-foreground flex items-center gap-1">
          <Calendar className="w-3 h-3" />
          Revisado el {new Date(report.reviewedAtUtc).toLocaleString('es-ES')}
        </div>
      )}
    </Card>
  )
}

export function ReportStatus({ className = '' }: Omit<ReportStatusProps, 'userId'>) {
  const {
    reports,
    loading,
    error,
    totalCount,
    getUserReports,
    clearError
  } = useContentReport()

  const [refreshing, setRefreshing] = useState(false)

  useEffect(() => {
    loadUserReports()
  }, [])

  const loadUserReports = async () => {
    try {
      await getUserReports(1, 50)
    } catch (err) {
      console.error('Error loading user reports:', err)
    }
  }

  const handleRefresh = async () => {
    setRefreshing(true)
    try {
      await loadUserReports()
    } finally {
      setRefreshing(false)
    }
  }

  const pendingReports = reports.filter(r => r.status === ContentReportStatus.Pending)
  const reviewingReports = reports.filter(r => r.status === ContentReportStatus.Reviewed)
  const completedReports = reports.filter(r => 
    r.status === ContentReportStatus.Resolved || r.status === ContentReportStatus.Dismissed
  )

  if (loading && reports.length === 0) {
    return (
      <div className={`space-y-4 ${className}`}>
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-semibold">Estado de tus Reportes</h2>
        </div>
        <div className="space-y-3">
          {Array.from({ length: 2 }).map((_, i) => (
            <Card key={i} className="p-4 animate-pulse">
              <div className="space-y-2">
                <div className="h-4 bg-muted rounded w-3/4" />
                <div className="h-4 bg-muted rounded w-1/2" />
                <div className="h-2 bg-muted rounded w-full" />
              </div>
            </Card>
          ))}
        </div>
      </div>
    )
  }

  return (
    <div className={`space-y-6 ${className}`}>
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold">Estado de tus Reportes</h2>
          <p className="text-sm text-muted-foreground">
            Sigue el progreso de los reportes que has enviado
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant="outline">
            {totalCount} reporte{totalCount !== 1 ? 's' : ''} total{totalCount !== 1 ? 'es' : ''}
          </Badge>
          <Button 
            variant="outline" 
            size="sm" 
            onClick={handleRefresh}
            disabled={refreshing}
          >
            <RefreshCw className={`w-4 h-4 ${refreshing ? 'animate-spin' : ''}`} />
          </Button>
        </div>
      </div>

      {error && (
        <Card className="p-4 border-red-200 bg-red-50">
          <div className="flex items-center gap-2">
            <AlertCircle className="w-4 h-4 text-red-600" />
            <p className="text-sm text-red-800">{error}</p>
            <Button variant="ghost" size="sm" onClick={clearError}>
              ✕
            </Button>
          </div>
        </Card>
      )}

      {reports.length === 0 ? (
        <Card className="p-8 text-center">
          <MessageSquare className="w-12 h-12 text-muted-foreground mx-auto mb-4" />
          <h3 className="text-lg font-medium mb-2">No has enviado reportes</h3>
          <p className="text-muted-foreground">
            Cuando reportes contenido, podrás ver el estado de tus reportes aquí.
          </p>
        </Card>
      ) : (
        <div className="space-y-6">
          {/* Active Reports */}
          {(pendingReports.length > 0 || reviewingReports.length > 0) && (
            <div className="space-y-3">
              <h3 className="text-lg font-medium">Reportes Activos</h3>
              {[...pendingReports, ...reviewingReports].map((report) => (
                <ReportStatusCard 
                  key={report.id} 
                  report={report} 
                  onRefresh={handleRefresh}
                />
              ))}
            </div>
          )}

          {/* Completed Reports */}
          {completedReports.length > 0 && (
            <div className="space-y-3">
              <h3 className="text-lg font-medium">Reportes Completados</h3>
              {completedReports.map((report) => (
                <ReportStatusCard key={report.id} report={report} />
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  )
}