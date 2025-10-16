'use client'

import React from 'react'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { UserRole } from '@/lib/types'

interface UserRoleSelectProps {
  value: UserRole
  onChange: (role: UserRole) => void
  disabled?: boolean
}

export const UserRoleSelect: React.FC<UserRoleSelectProps> = ({
  value,
  onChange,
  disabled = false
}) => {
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

  return (
    <Select
      value={value.toString()}
      onValueChange={(value) => onChange(parseInt(value) as UserRole)}
      disabled={disabled}
    >
      <SelectTrigger>
        <SelectValue>{getRoleName(value)}</SelectValue>
      </SelectTrigger>
      <SelectContent>
        <SelectItem value={UserRole.Reader.toString()}>
          {getRoleName(UserRole.Reader)}
        </SelectItem>
        <SelectItem value={UserRole.Uploader.toString()}>
          {getRoleName(UserRole.Uploader)}
        </SelectItem>
        <SelectItem value={UserRole.Moderator.toString()}>
          {getRoleName(UserRole.Moderator)}
        </SelectItem>
        <SelectItem value={UserRole.Administrator.toString()}>
          {getRoleName(UserRole.Administrator)}
        </SelectItem>
      </SelectContent>
    </Select>
  )
}