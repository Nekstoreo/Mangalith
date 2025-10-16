'use client'

import React from 'react'
import { usePermission, useAnyPermission, useAllPermissions, useResourcePermission } from '@/hooks'
import { PermissionString } from '@/lib/types'

interface PermissionGuardProps {
  children: React.ReactNode
  permission?: PermissionString
  permissions?: PermissionString[]
  requireAll?: boolean
  resourceOwnerId?: string
  fallback?: React.ReactNode
  invert?: boolean
}

/**
 * Component that conditionally renders children based on user permissions
 */
export const PermissionGuard: React.FC<PermissionGuardProps> = ({
  children,
  permission,
  permissions,
  requireAll = false,
  resourceOwnerId,
  fallback = null,
  invert = false
}) => {
  const hasPermission = usePermission(permission!)
  const hasAnyPermission = useAnyPermission(permissions || [])
  const hasAllPermissions = useAllPermissions(permissions || [])
  const hasResourcePermission = useResourcePermission(permission!, resourceOwnerId)
  
  let shouldRender = false
  
  if (permission && resourceOwnerId) {
    shouldRender = hasResourcePermission
  } else if (permission) {
    shouldRender = hasPermission
  } else if (permissions && permissions.length > 0) {
    shouldRender = requireAll ? hasAllPermissions : hasAnyPermission
  }
  
  // Invert the logic if requested
  if (invert) {
    shouldRender = !shouldRender
  }
  
  return shouldRender ? <>{children}</> : <>{fallback}</>
}

/**
 * Specific permission guard components for common use cases
 */
export const CanManageUsers: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback
}) => (
  <PermissionGuard permission="user.manage" fallback={fallback}>
    {children}
  </PermissionGuard>
)

export const CanViewAudit: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback
}) => (
  <PermissionGuard permission="system.audit" fallback={fallback}>
    {children}
  </PermissionGuard>
)

export const CanInviteUsers: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback
}) => (
  <PermissionGuard permission="user.invite" fallback={fallback}>
    {children}
  </PermissionGuard>
)

export const CanUploadFiles: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback
}) => (
  <PermissionGuard permission="file.upload" fallback={fallback}>
    {children}
  </PermissionGuard>
)

export const CanCreateManga: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback
}) => (
  <PermissionGuard permission="manga.create" fallback={fallback}>
    {children}
  </PermissionGuard>
)

export const CanModerateManga: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback
}) => (
  <PermissionGuard permission="manga.moderate" fallback={fallback}>
    {children}
  </PermissionGuard>
)

export const CanConfigureSystem: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback
}) => (
  <PermissionGuard permission="system.configure" fallback={fallback}>
    {children}
  </PermissionGuard>
)

/**
 * Component that shows different content based on permissions
 */
export const PermissionSwitch: React.FC<{
  cases: Array<{
    permission?: PermissionString
    permissions?: PermissionString[]
    requireAll?: boolean
    component: React.ReactNode
  }>
  fallback?: React.ReactNode
}> = ({ cases, fallback }) => {
  for (const { permission, permissions, requireAll, component } of cases) {
    const hasPermission = usePermission(permission!)
    const hasAnyPermission = useAnyPermission(permissions || [])
    const hasAllPermissions = useAllPermissions(permissions || [])
    
    let shouldRender = false
    
    if (permission) {
      shouldRender = hasPermission
    } else if (permissions && permissions.length > 0) {
      shouldRender = requireAll ? hasAllPermissions : hasAnyPermission
    }
    
    if (shouldRender) {
      return <>{component}</>
    }
  }
  
  return <>{fallback}</>
}

/**
 * Component for resource-specific permission checking
 */
export const ResourcePermissionGuard: React.FC<{
  children: React.ReactNode
  permission: PermissionString
  resourceOwnerId: string
  fallback?: React.ReactNode
}> = ({ children, permission, resourceOwnerId, fallback }) => {
  const hasResourcePermission = useResourcePermission(permission, resourceOwnerId)
  
  return hasResourcePermission ? <>{children}</> : <>{fallback}</>
}