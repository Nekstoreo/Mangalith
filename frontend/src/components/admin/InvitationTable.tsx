'use client'

import React, { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { InvitationSummary, InvitationStatus, UserRole } from '@/lib/types'
import { InvitationStatusBadge } from './InvitationStatusBadge'
import { invitationService } from '@/services/admin/invitations.service'
import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight, ArrowUpDown, ArrowUp, ArrowDown, Mail, Trash2, X, Copy, Check } from 'lucide-react'

interface InvitationTableProps {
  invitations: InvitationSummary[]
  pagination: {
    currentPage: number
    totalPages: number
    totalItems: number
    pageSize: number
  }
  onPageChange: (page: number) => void
  onInvitationAction: () => void
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
  onSort: (sortBy: string, sortDirection: 'asc' | 'desc') => void
}

export const InvitationTable: React.FC<InvitationTableProps> = ({
  invitations,
  pagination,
  onPageChange,
  onInvitationAction,
  sortBy,
  sortDirection,
  onSort
}) => {
  const [actionLoading, setActionLoading] = useState<string | null>(null)
  const [copiedToken, setCopiedToken] = useState<string | null>(null)

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

  const getRoleColor = (role: UserRole): string => {
    switch (role) {
      case UserRole.Reader:
        return 'bg-blue-100 text-blue-800'
      case UserRole.Uploader:
        return 'bg-green-100 text-green-800'
      case UserRole.Moderator:
        return 'bg-yellow-100 text-yellow-800'
      case UserRole.Administrator:
        return 'bg-red-100 text-red-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const formatTimeUntilExpiration = (hoursUntilExpiration?: number): string => {
    if (!hoursUntilExpiration || hoursUntilExpiration <= 0) return 'Expirada'
    
    if (hoursUntilExpiration < 24) {
      return `${Math.round(hoursUntilExpiration)}h`
    } else {
      const days = Math.round(hoursUntilExpiration / 24)
      return `${days}d`
    }
  }

  const handleResendInvitation = async (invitationId: string) => {
    try {
      setActionLoading(invitationId)
      await invitationService.resendInvitation({ invitationId })
      onInvitationAction()
      alert('Invitación reenviada exitosamente')
    } catch (error) {
      console.error('Error resending invitation:', error)
      alert('Error al reenviar la invitación')
    } finally {
      setActionLoading(null)
    }
  }

  const handleCancelInvitation = async (invitationId: string) => {
    const confirmed = confirm('¿Estás seguro de que quieres cancelar esta invitación?')
    if (!confirmed) return

    try {
      setActionLoading(invitationId)
      await invitationService.cancelInvitation({ invitationId })
      onInvitationAction()
      alert('Invitación cancelada exitosamente')
    } catch (error) {
      console.error('Error canceling invitation:', error)
      alert('Error al cancelar la invitación')
    } finally {
      setActionLoading(null)
    }
  }

  const handleDeleteInvitation = async (invitationId: string) => {
    const confirmed = confirm('¿Estás seguro de que quieres eliminar esta invitación? Esta acción no se puede deshacer.')
    if (!confirmed) return

    try {
      setActionLoading(invitationId)
      await invitationService.deleteInvitation(invitationId)
      onInvitationAction()
      alert('Invitación eliminada exitosamente')
    } catch (error) {
      console.error('Error deleting invitation:', error)
      alert('Error al eliminar la invitación')
    } finally {
      setActionLoading(null)
    }
  }

  const copyInvitationLink = async (invitation: InvitationSummary) => {
    // This would typically be the actual invitation URL
    const invitationUrl = `${window.location.origin}/invitations/accept?token=${invitation.id}`
    
    try {
      await navigator.clipboard.writeText(invitationUrl)
      setCopiedToken(invitation.id)
      setTimeout(() => setCopiedToken(null), 2000)
    } catch (error) {
      console.error('Error copying to clipboard:', error)
      alert('Error al copiar el enlace')
    }
  }

  const canResend = (invitation: InvitationSummary): boolean => {
    return invitation.status === InvitationStatus.Pending && !invitation.isExpired
  }

  const canCancel = (invitation: InvitationSummary): boolean => {
    return invitation.status === InvitationStatus.Pending
  }

  const canDelete = (invitation: InvitationSummary): boolean => {
    return invitation.status !== InvitationStatus.Accepted
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
                  onClick={() => handleSort('email')}
                  className="h-auto p-0 font-semibold"
                >
                  Email
                  {getSortIcon('email')}
                </Button>
              </th>
              <th className="text-left p-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleSort('targetRole')}
                  className="h-auto p-0 font-semibold"
                >
                  Rol Objetivo
                  {getSortIcon('targetRole')}
                </Button>
              </th>
              <th className="text-left p-2">Estado</th>
              <th className="text-left p-2">Invitado por</th>
              <th className="text-left p-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleSort('createdAt')}
                  className="h-auto p-0 font-semibold"
                >
                  Creada
                  {getSortIcon('createdAt')}
                </Button>
              </th>
              <th className="text-left p-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleSort('expiresAt')}
                  className="h-auto p-0 font-semibold"
                >
                  Expira
                  {getSortIcon('expiresAt')}
                </Button>
              </th>
              <th className="text-left p-2">Acciones</th>
            </tr>
          </thead>
          <tbody>
            {invitations.map((invitation) => (
              <tr key={invitation.id} className="border-b hover:bg-gray-50">
                <td className="p-2">
                  <div className="font-medium">{invitation.email}</div>
                </td>
                <td className="p-2">
                  <Badge className={getRoleColor(invitation.targetRole)}>
                    {getRoleName(invitation.targetRole)}
                  </Badge>
                </td>
                <td className="p-2">
                  <InvitationStatusBadge 
                    status={invitation.status}
                    isExpired={invitation.isExpired}
                  />
                </td>
                <td className="p-2">
                  <div>
                    <div className="font-medium text-sm">{invitation.invitedBy.fullName}</div>
                    <div className="text-xs text-muted-foreground">{invitation.invitedBy.email}</div>
                  </div>
                </td>
                <td className="p-2 text-sm">
                  {new Date(invitation.createdAt).toLocaleDateString()}
                </td>
                <td className="p-2 text-sm">
                  <div>
                    <div>{new Date(invitation.expiresAt).toLocaleDateString()}</div>
                    {invitation.status === InvitationStatus.Pending && (
                      <div className={`text-xs ${invitation.isExpired ? 'text-red-600' : 'text-muted-foreground'}`}>
                        {formatTimeUntilExpiration(invitation.hoursUntilExpiration)}
                      </div>
                    )}
                  </div>
                </td>
                <td className="p-2">
                  <div className="flex gap-1">
                    {invitation.status === InvitationStatus.Pending && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => copyInvitationLink(invitation)}
                        title="Copiar enlace de invitación"
                      >
                        {copiedToken === invitation.id ? (
                          <Check className="h-3 w-3" />
                        ) : (
                          <Copy className="h-3 w-3" />
                        )}
                      </Button>
                    )}
                    
                    {canResend(invitation) && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleResendInvitation(invitation.id)}
                        disabled={actionLoading === invitation.id}
                        title="Reenviar invitación"
                      >
                        <Mail className="h-3 w-3" />
                      </Button>
                    )}
                    
                    {canCancel(invitation) && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleCancelInvitation(invitation.id)}
                        disabled={actionLoading === invitation.id}
                        title="Cancelar invitación"
                      >
                        <X className="h-3 w-3" />
                      </Button>
                    )}
                    
                    {canDelete(invitation) && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleDeleteInvitation(invitation.id)}
                        disabled={actionLoading === invitation.id}
                        title="Eliminar invitación"
                        className="text-red-600 hover:text-red-700"
                      >
                        <Trash2 className="h-3 w-3" />
                      </Button>
                    )}
                  </div>
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
            {pagination.totalItems} invitaciones
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

      {invitations.length === 0 && (
        <div className="text-center py-12">
          <p className="text-muted-foreground">No se encontraron invitaciones</p>
        </div>
      )}
    </div>
  )
}