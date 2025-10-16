import { useMemo } from 'react'
import { useAuthStore } from '@/stores/auth'
import { PermissionString, UserRole, RoleHierarchy } from '@/lib/types'

/**
 * Hook for checking if the current user has a specific permission
 */
export const usePermission = (permission: PermissionString) => {
  const { user, hasPermission } = useAuthStore()
  
  return useMemo(() => {
    if (!user) return false
    return hasPermission(permission)
  }, [user, permission, hasPermission])
}

/**
 * Hook for checking if the current user has any of the specified permissions
 */
export const useAnyPermission = (permissions: PermissionString[]) => {
  const { user, hasAnyPermission } = useAuthStore()
  
  return useMemo(() => {
    if (!user || permissions.length === 0) return false
    return hasAnyPermission(permissions)
  }, [user, permissions, hasAnyPermission])
}

/**
 * Hook for checking if the current user has all of the specified permissions
 */
export const useAllPermissions = (permissions: PermissionString[]) => {
  const { user, hasAllPermissions } = useAuthStore()
  
  return useMemo(() => {
    if (!user || permissions.length === 0) return false
    return hasAllPermissions(permissions)
  }, [user, permissions, hasAllPermissions])
}

/**
 * Hook for checking if the current user has a specific role or higher
 */
export const useRole = (minimumRole: UserRole) => {
  const { user, hasRole } = useAuthStore()
  
  return useMemo(() => {
    if (!user) return false
    return hasRole(minimumRole)
  }, [user, minimumRole, hasRole])
}

/**
 * Hook for checking if the current user has exactly the specified role
 */
export const useExactRole = (role: UserRole) => {
  const { user } = useAuthStore()
  
  return useMemo(() => {
    if (!user) return false
    return user.role === role
  }, [user, role])
}

/**
 * Hook for checking if the current user has any of the specified roles
 */
export const useAnyRole = (roles: UserRole[]) => {
  const { user } = useAuthStore()
  
  return useMemo(() => {
    if (!user || roles.length === 0) return false
    return roles.some(role => user.role >= role)
  }, [user, roles])
}

/**
 * Hook that returns permission checking functions
 */
export const usePermissions = () => {
  const { user, hasPermission, hasAnyPermission, hasAllPermissions, hasRole } = useAuthStore()
  
  return useMemo(() => ({
    user,
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
    hasRole,
    isAuthenticated: !!user,
    isAdmin: user?.role === UserRole.Administrator,
    isModerator: user ? user.role >= UserRole.Moderator : false,
    isUploader: user ? user.role >= UserRole.Uploader : false,
    isReader: user ? user.role >= UserRole.Reader : false,
    
    // Utility functions
    canManageUsers: hasPermission('user.manage'),
    canViewAudit: hasPermission('system.audit'),
    canInviteUsers: hasPermission('user.invite'),
    canUploadFiles: hasPermission('file.upload'),
    canCreateManga: hasPermission('manga.create'),
    canModerateManga: hasPermission('manga.moderate'),
    canConfigureSystem: hasPermission('system.configure'),
    
    // Role checking utilities
    isAtLeastRole: (role: UserRole) => user ? user.role >= role : false,
    hasInheritedRole: (role: UserRole) => {
      if (!user) return false
      return RoleHierarchy[user.role].includes(role)
    }
  }), [user, hasPermission, hasAnyPermission, hasAllPermissions, hasRole])
}

/**
 * Hook for resource-specific permission checking
 */
export const useResourcePermission = (
  permission: PermissionString,
  resourceOwnerId?: string
) => {
  const { user, hasPermission } = useAuthStore()
  
  return useMemo(() => {
    if (!user) return false
    
    // If user has the general permission, they can access the resource
    if (hasPermission(permission)) return true
    
    // If resource has an owner and user is the owner, check for owner-specific permissions
    if (resourceOwnerId && user.id === resourceOwnerId) {
      // Map general permissions to owner-specific ones
      const ownerPermissionMap: Record<string, string> = {
        'manga.update': 'manga.update',
        'manga.delete': 'manga.delete',
        'chapter.update': 'chapter.update',
        'chapter.delete': 'chapter.delete',
        'file.delete': 'file.delete'
      }
      
      const ownerPermission = ownerPermissionMap[permission]
      if (ownerPermission && hasPermission(ownerPermission as PermissionString)) {
        return true
      }
    }
    
    return false
  }, [user, permission, resourceOwnerId, hasPermission])
}

/**
 * Hook for conditional permission checking with loading state
 */
export const usePermissionWithLoading = (permission: PermissionString) => {
  const { user, isLoading } = useAuthStore()
  const hasPermission = usePermission(permission)
  
  return useMemo(() => ({
    hasPermission,
    isLoading,
    isReady: !isLoading && !!user
  }), [hasPermission, isLoading, user])
}