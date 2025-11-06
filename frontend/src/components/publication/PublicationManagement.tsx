'use client'

import { useEffect, useState } from 'react'
import { usePublication } from '@/hooks/usePublication'
import { PublicationList } from './PublicationCard'
import { SubmitPublicationDialog } from './SubmitPublicationDialog'
import { Publication, PublicationStatus } from '@/services/publication/client'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Alert } from '@/components/ui/alert'

interface PublicationManagementProps {
  mangaId?: string
  isAdmin?: boolean
}

export function PublicationManagement({
  mangaId,
  isAdmin = false,
}: PublicationManagementProps) {
  const {
    publications,
    loading,
    error,
    totalCount,
    currentPage,
    getMyPublications,
    getPublicationsByStatus,
    submitForReview,
    clearError,
  } = usePublication()

  const [selectedPublication, setSelectedPublication] = useState<Publication | null>(null)
  const [showSubmitDialog, setShowSubmitDialog] = useState(false)
  const [statusFilter, setStatusFilter] = useState<PublicationStatus | 'all'>('all')
  const [currentPageLocal, setCurrentPageLocal] = useState(1)

  useEffect(() => {
    loadPublications()
  }, [statusFilter, currentPageLocal])

  const loadPublications = async () => {
    try {
      if (statusFilter === 'all') {
        await getMyPublications(currentPageLocal, 10)
      } else {
        await getPublicationsByStatus(statusFilter, currentPageLocal, 10)
      }
    } catch (err) {
      console.error('Error loading publications:', err)
    }
  }

  const handleSubmit = async (publication: Publication) => {
    setSelectedPublication(publication)
    setShowSubmitDialog(true)
  }

  const handleConfirmSubmit = async () => {
    if (!selectedPublication) return
    try {
      await submitForReview(selectedPublication.id)
      await loadPublications()
      setShowSubmitDialog(false)
      setSelectedPublication(null)
    } catch (err) {
      console.error('Error submitting publication:', err)
    }
  }

  const statusOptions = [
    { value: 'all', label: 'Todas las Publicaciones' },
    { value: PublicationStatus.Draft, label: 'Borradores' },
    { value: PublicationStatus.InReview, label: 'En Revisión' },
    { value: PublicationStatus.Published, label: 'Publicadas' },
    { value: PublicationStatus.Rejected, label: 'Rechazadas' },
    { value: PublicationStatus.NeedsRevision, label: 'Requieren Revisión' },
    { value: PublicationStatus.Archived, label: 'Archivadas' },
  ]

  return (
    <div className="space-y-6">
      {error && (
        <Alert className="bg-red-50 border-red-200">
          <div className="flex justify-between items-center">
            <p className="text-sm text-red-700">{error}</p>
            <button
              onClick={clearError}
              className="text-red-700 hover:text-red-900 text-sm font-medium"
            >
              Cerrar
            </button>
          </div>
        </Alert>
      )}

      <Card className="p-4 space-y-4">
        <div>
          <h2 className="text-lg font-semibold mb-2">Gestión de Publicaciones</h2>
          <p className="text-sm text-muted-foreground">
            Aquí puedes ver y gestionar todas tus publicaciones de manga
          </p>
        </div>

        <div className="flex flex-wrap gap-2">
          {statusOptions.map((option) => (
            <button
              key={option.value}
              onClick={() => {
                setStatusFilter(option.value as PublicationStatus | 'all')
                setCurrentPageLocal(1)
              }}
              className={`px-3 py-1.5 rounded text-sm transition-colors ${
                statusFilter === option.value
                  ? 'bg-primary text-primary-foreground'
                  : 'border hover:bg-accent'
              }`}
            >
              {option.label}
            </button>
          ))}
        </div>
      </Card>

      <PublicationList
        publications={publications}
        isLoading={loading}
        onSubmit={handleSubmit}
        onView={(pub) => {
          // TODO: Navigate to publication detail
          console.log('View publication:', pub)
        }}
        onEdit={(pub) => {
          // TODO: Navigate to publication editor
          console.log('Edit publication:', pub)
        }}
      />

      {/* Pagination */}
      {totalCount > 0 && (
        <div className="flex justify-center gap-2">
          <Button
            variant="outline"
            disabled={currentPageLocal === 1}
            onClick={() => setCurrentPageLocal(currentPageLocal - 1)}
          >
            Anterior
          </Button>
          <span className="flex items-center px-3 text-sm">
            Página {currentPageLocal} de {Math.ceil(totalCount / 10)}
          </span>
          <Button
            variant="outline"
            disabled={currentPageLocal >= Math.ceil(totalCount / 10)}
            onClick={() => setCurrentPageLocal(currentPageLocal + 1)}
          >
            Siguiente
          </Button>
        </div>
      )}

      <SubmitPublicationDialog
        open={showSubmitDialog}
        publication={selectedPublication}
        onConfirm={handleConfirmSubmit}
        onOpenChange={setShowSubmitDialog}
      />
    </div>
  )
}
