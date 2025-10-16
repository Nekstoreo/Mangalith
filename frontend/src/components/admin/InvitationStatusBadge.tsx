'use client'

import React from 'react'
import { Badge } from '@/components/ui/badge'
import { InvitationStatus } from '@/lib/types'
import { Clock, CheckCircle, XCircle, Ban } from 'lucide-react'

interface InvitationStatusBadgeProps {
  status: InvitationStatus
  isExpired?: boolean
}

export const InvitationStatusBadge: React.FC<InvitationStatusBadgeProps> = ({ 
  status, 
  isExpired = false 
}) => {
  const getStatusConfig = () => {
    if (status === InvitationStatus.Pending && isExpired) {
      return {
        color: 'bg-red-100 text-red-800 hover:bg-red-100',
        icon: <XCircle className="h-3 w-3" />,
        text: 'Expirada'
      }
    }

    switch (status) {
      case InvitationStatus.Pending:
        return {
          color: 'bg-yellow-100 text-yellow-800 hover:bg-yellow-100',
          icon: <Clock className="h-3 w-3" />,
          text: 'Pendiente'
        }
      case InvitationStatus.Accepted:
        return {
          color: 'bg-green-100 text-green-800 hover:bg-green-100',
          icon: <CheckCircle className="h-3 w-3" />,
          text: 'Aceptada'
        }
      case InvitationStatus.Expired:
        return {
          color: 'bg-red-100 text-red-800 hover:bg-red-100',
          icon: <XCircle className="h-3 w-3" />,
          text: 'Expirada'
        }
      case InvitationStatus.Cancelled:
        return {
          color: 'bg-gray-100 text-gray-800 hover:bg-gray-100',
          icon: <Ban className="h-3 w-3" />,
          text: 'Cancelada'
        }
      default:
        return {
          color: 'bg-gray-100 text-gray-800 hover:bg-gray-100',
          icon: null,
          text: 'Desconocido'
        }
    }
  }

  const { color, icon, text } = getStatusConfig()

  return (
    <Badge className={`${color} flex items-center gap-1`}>
      {icon}
      {text}
    </Badge>
  )
}