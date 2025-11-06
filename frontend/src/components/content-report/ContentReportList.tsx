'use client'

import { ContentReport, ContentReportCategory, ContentReportStatus } from '@/services/content-report/client'
import { Badge } from '@/components/ui/badge'
import { Card } from '@/components/ui/card'

const categoryLabels: Record<ContentReportCategory, string> = {
  [ContentReportCategory.Copyright]: 'Derechos de Autor',
  [ContentReportCategory.InappropriateContent]: 'Contenido Inapropiado',
  [ContentReportCategory.Spam]: 'Spam',
  [ContentReportCategory.Harassment]: 'Acoso',
  [ContentReportCategory.Violence]: 'Violencia',
  [ContentReportCategory.AdultContent]: 'Contenido Adulto',
  [ContentReportCategory.Other]: 'Otro',
}

const statusLabels: Record<ContentReportStatus, { label: string; variant: 'default' | 'secondary' | 'destructive' | 'outline' }> = {
  [ContentReportStatus.Pending]: { label: 'Pendiente', variant: 'outline' },
  [ContentReportStatus.Reviewed]: { label: 'Revisado', variant: 'secondary' },
  [ContentReportStatus.Resolved]: { label: 'Resuelto', variant: 'default' },
  [ContentReportStatus.Dismissed]: { label: 'Desestimado', variant: 'destructive' },
}

interface ContentReportCardProps {
  report: ContentReport
  onReview?: (report: ContentReport) => void
}

export function ContentReportCard({ report, onReview }: ContentReportCardProps) {
  const statusConfig = statusLabels[report.status]

  return (
    <Card className="p-4 space-y-3">
      <div className="flex justify-between items-start">
        <div className="space-y-2 flex-1">
          <div className="flex items-center gap-2">
            <Badge variant="outline">{categoryLabels[report.category]}</Badge>
            <Badge variant={statusConfig.variant}>{statusConfig.label}</Badge>
          </div>
          <p className="text-sm">
            <span className="text-muted-foreground">ID:</span>{' '}
            <span className="font-mono">{report.id.substring(0, 8)}...</span>
          </p>
          <p className="text-sm">
            <span className="text-muted-foreground">Publicación:</span>{' '}
            <span className="font-mono">{report.publicationId.substring(0, 8)}...</span>
          </p>
          <p className="text-sm">
            <span className="text-muted-foreground">Reportado por:</span>{' '}
            <span className="font-mono">{report.reporterId.substring(0, 8)}...</span>
          </p>
          <p className="text-sm">
            <span className="text-muted-foreground">Fecha:</span>{' '}
            {new Date(report.createdAtUtc).toLocaleString('es-ES')}
          </p>
        </div>

        {report.status === ContentReportStatus.Pending && onReview && (
          <button
            onClick={() => onReview(report)}
            className="px-4 py-2 rounded bg-blue-500 text-white hover:bg-blue-600 text-sm"
          >
            Revisar
          </button>
        )}
      </div>

      {report.description && (
        <div className="p-3 bg-muted rounded">
          <p className="text-sm font-medium mb-1">Descripción:</p>
          <p className="text-sm text-muted-foreground">{report.description}</p>
        </div>
      )}

      {report.resolutionNotes && (
        <div className="p-3 bg-green-50 rounded border border-green-200">
          <p className="text-sm font-medium mb-1 text-green-900">Notas de Resolución:</p>
          <p className="text-sm text-green-800">{report.resolutionNotes}</p>
        </div>
      )}
    </Card>
  )
}

interface ContentReportListProps {
  reports: ContentReport[]
  onReview?: (report: ContentReport) => void
  isLoading?: boolean
}

export function ContentReportList({
  reports,
  onReview,
  isLoading,
}: ContentReportListProps) {
  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <Card key={i} className="p-4 space-y-2 animate-pulse">
            <div className="h-4 bg-muted rounded w-3/4" />
            <div className="h-4 bg-muted rounded w-1/2" />
          </Card>
        ))}
      </div>
    )
  }

  if (reports.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-lg font-medium">No hay reportes</p>
        <p className="text-sm text-muted-foreground mt-1">
          No hay reportes de contenido pendientes
        </p>
      </div>
    )
  }

  return (
    <div className="space-y-3">
      {reports.map((report) => (
        <ContentReportCard key={report.id} report={report} onReview={onReview} />
      ))}
    </div>
  )
}
