'use client'

import React from 'react'
import { usePermissions } from '@/hooks'
import { UserRole, PermissionString } from '@/lib/types'

interface ConditionalRenderProps {
  children: React.ReactNode
  condition?: boolean
  permission?: PermissionString
  role?: UserRole
  authenticated?: boolean
  fallback?: React.ReactNode
}

/**
 * Generic conditional rendering component
 */
export const ConditionalRender: React.FC<ConditionalRenderProps> = ({
  children,
  condition,
  permission,
  role,
  authenticated,
  fallback = null
}) => {
  const { hasPermission, hasRole, isAuthenticated } = usePermissions()
  
  let shouldRender = true
  
  if (condition !== undefined) {
    shouldRender = condition
  }
  
  if (authenticated !== undefined) {
    shouldRender = shouldRender && (authenticated ? isAuthenticated : !isAuthenticated)
  }
  
  if (permission) {
    shouldRender = shouldRender && hasPermission(permission)
  }
  
  if (role !== undefined) {
    shouldRender = shouldRender && hasRole(role)
  }
  
  return shouldRender ? <>{children}</> : <>{fallback}</>
}

/**
 * Component that renders children only for authenticated users
 */
export const AuthenticatedOnly: React.FC<{
  children: React.ReactNode
  fallback?: React.ReactNode
}> = ({ children, fallback }) => (
  <ConditionalRender authenticated={true} fallback={fallback}>
    {children}
  </ConditionalRender>
)

/**
 * Component that renders children only for unauthenticated users
 */
export const UnauthenticatedOnly: React.FC<{
  children: React.ReactNode
  fallback?: React.ReactNode
}> = ({ children, fallback }) => (
  <ConditionalRender authenticated={false} fallback={fallback}>
    {children}
  </ConditionalRender>
)

/**
 * Component that renders different content based on authentication status
 */
export const AuthSwitch: React.FC<{
  authenticated?: React.ReactNode
  unauthenticated?: React.ReactNode
}> = ({ authenticated, unauthenticated }) => {
  const { isAuthenticated } = usePermissions()
  
  return <>{isAuthenticated ? authenticated : unauthenticated}</>
}

/**
 * Component for showing loading states with permission checks
 */
export const PermissionAwareLoader: React.FC<{
  children: React.ReactNode
  permission?: PermissionString
  role?: UserRole
  loading?: boolean
  loadingComponent?: React.ReactNode
  errorComponent?: React.ReactNode
}> = ({
  children,
  permission,
  role,
  loading = false,
  loadingComponent,
  errorComponent
}) => {
  const { hasPermission, hasRole, isAuthenticated } = usePermissions()
  
  if (loading) {
    return <>{loadingComponent || <div>Cargando...</div>}</>
  }
  
  if (!isAuthenticated) {
    return <>{errorComponent || <div>No autenticado</div>}</>
  }
  
  if (permission && !hasPermission(permission)) {
    return <>{errorComponent || <div>Sin permisos</div>}</>
  }
  
  if (role !== undefined && !hasRole(role)) {
    return <>{errorComponent || <div>Rol insuficiente</div>}</>
  }
  
  return <>{children}</>
}

/**
 * Higher-order component for permission-based rendering
 */
export const withPermission = <P extends object>(
  Component: React.ComponentType<P>,
  permission: PermissionString,
  fallback?: React.ReactNode
) => {
  return (props: P) => (
    <PermissionGuard permission={permission} fallback={fallback}>
      <Component {...props} />
    </PermissionGuard>
  )
}

/**
 * Higher-order component for role-based rendering
 */
export const withRole = <P extends object>(
  Component: React.ComponentType<P>,
  role: UserRole,
  fallback?: React.ReactNode
) => {
  return (props: P) => (
    <RoleGuard role={role} fallback={fallback}>
      <Component {...props} />
    </RoleGuard>
  )
}

// Import the guards for the HOCs
import { PermissionGuard } from './PermissionGuard'
import { RoleGuard } from './RoleGuard'