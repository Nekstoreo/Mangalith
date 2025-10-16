'use client'

import React from 'react'
import { useRole, useExactRole, useAnyRole } from '@/hooks'
import { UserRole } from '@/lib/types'

interface RoleGuardProps {
  children: React.ReactNode
  role?: UserRole
  exactRole?: UserRole
  roles?: UserRole[]
  fallback?: React.ReactNode
  invert?: boolean
}

/**
 * Component that conditionally renders children based on user role
 */
export const RoleGuard: React.FC<RoleGuardProps> = ({
  children,
  role,
  exactRole,
  roles,
  fallback = null,
  invert = false
}) => {
  const hasRole = useRole(role!)
  const hasExactRole = useExactRole(exactRole!)
  const hasAnyRole = useAnyRole(roles || [])
  
  let shouldRender = false
  
  if (role !== undefined) {
    shouldRender = hasRole
  } else if (exactRole !== undefined) {
    shouldRender = hasExactRole
  } else if (roles && roles.length > 0) {
    shouldRender = hasAnyRole
  }
  
  // Invert the logic if requested
  if (invert) {
    shouldRender = !shouldRender
  }
  
  return shouldRender ? <>{children}</> : <>{fallback}</>
}

/**
 * Specific role guard components for common use cases
 */
export const AdminOnly: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback
}) => (
  <RoleGuard exactRole={UserRole.Administrator} fallback={fallback}>
    {children}
  </RoleGuard>
)

export const ModeratorOnly: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback
}) => (
  <RoleGuard role={UserRole.Moderator} fallback={fallback}>
    {children}
  </RoleGuard>
)

export const UploaderOnly: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback
}) => (
  <RoleGuard role={UserRole.Uploader} fallback={fallback}>
    {children}
  </RoleGuard>
)

export const ReaderOnly: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback
}) => (
  <RoleGuard exactRole={UserRole.Reader} fallback={fallback}>
    {children}
  </RoleGuard>
)

/**
 * Component that shows different content based on user role
 */
export const RoleSwitch: React.FC<{
  admin?: React.ReactNode
  moderator?: React.ReactNode
  uploader?: React.ReactNode
  reader?: React.ReactNode
  fallback?: React.ReactNode
}> = ({ admin, moderator, uploader, reader, fallback }) => {
  const hasAdminRole = useExactRole(UserRole.Administrator)
  const hasModeratorRole = useExactRole(UserRole.Moderator)
  const hasUploaderRole = useExactRole(UserRole.Uploader)
  const hasReaderRole = useExactRole(UserRole.Reader)
  
  if (hasAdminRole && admin) return <>{admin}</>
  if (hasModeratorRole && moderator) return <>{moderator}</>
  if (hasUploaderRole && uploader) return <>{uploader}</>
  if (hasReaderRole && reader) return <>{reader}</>
  
  return <>{fallback}</>
}