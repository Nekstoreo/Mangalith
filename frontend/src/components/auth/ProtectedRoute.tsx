'use client'

import React from 'react'
import { useRouter } from 'next/navigation'
import { useAuthStore } from '@/stores/auth'
import { usePermission, useRole } from '@/hooks'
import { UserRole, PermissionString } from '@/lib/types'
import { LoadingSpinner } from '@/components/ui'

interface ProtectedRouteProps {
  children: React.ReactNode
  permission?: PermissionString
  role?: UserRole
  permissions?: PermissionString[]
  requireAll?: boolean
  fallback?: React.ReactNode
  redirectTo?: string
  showLoading?: boolean
}

/**
 * Component that protects routes based on permissions or roles
 */
export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  permission,
  role,
  permissions = [],
  requireAll = false,
  fallback,
  redirectTo = '/auth/login',
  showLoading = true
}) => {
  const router = useRouter()
  const { user, isLoading, isAuthenticated } = useAuthStore()
  
  const hasPermission = usePermission(permission!)
  const hasRole = useRole(role!)
  
  // Check if user is still loading
  if (isLoading && showLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <LoadingSpinner size="lg" />
      </div>
    )
  }
  
  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    router.push(redirectTo)
    return null
  }
  
  // Check single permission
  if (permission && !hasPermission) {
    return fallback || <ForbiddenPage />
  }
  
  // Check role requirement
  if (role && !hasRole) {
    return fallback || <ForbiddenPage />
  }
  
  // Check multiple permissions
  if (permissions.length > 0) {
    const userPermissions = user?.permissions || []
    
    if (requireAll) {
      // User must have ALL specified permissions
      const hasAllPermissions = permissions.every(perm => userPermissions.includes(perm))
      if (!hasAllPermissions) {
        return fallback || <ForbiddenPage />
      }
    } else {
      // User must have ANY of the specified permissions
      const hasAnyPermission = permissions.some(perm => userPermissions.includes(perm))
      if (!hasAnyPermission) {
        return fallback || <ForbiddenPage />
      }
    }
  }
  
  return <>{children}</>
}

/**
 * Default forbidden page component
 */
const ForbiddenPage: React.FC = () => {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gray-50">
      <div className="text-center">
        <div className="text-6xl font-bold text-gray-400 mb-4">403</div>
        <h1 className="text-2xl font-semibold text-gray-900 mb-2">
          Acceso Denegado
        </h1>
        <p className="text-gray-600 mb-6">
          No tienes permisos para acceder a esta página.
        </p>
        <button
          onClick={() => window.history.back()}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
        >
          Volver
        </button>
      </div>
    </div>
  )
}