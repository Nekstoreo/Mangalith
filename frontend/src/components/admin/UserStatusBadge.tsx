'use client'

import React from 'react'
import { Badge } from '@/components/ui/badge'

interface UserStatusBadgeProps {
  isActive: boolean
}

export const UserStatusBadge: React.FC<UserStatusBadgeProps> = ({ isActive }) => {
  return (
    <Badge 
      variant={isActive ? 'default' : 'secondary'}
      className={isActive 
        ? 'bg-green-100 text-green-800 hover:bg-green-100' 
        : 'bg-red-100 text-red-800 hover:bg-red-100'
      }
    >
      {isActive ? 'Activo' : 'Inactivo'}
    </Badge>
  )
}