import { 
  UserRole, 
  PermissionString, 
  User, 
  RoleHierarchy,
  PermissionValidation,
  ResourcePermissionCheck,
  PermissionCheckResult
} from '@/lib/types'

/**
 * Check if a user has a specific permission
 */
export const hasPermission = (user: User | null, permission: PermissionString): boolean => {
  if (!user || !user.permissions) return false
  return user.permissions.includes(permission)
}

/**
 * Check if a user has a specific role or higher
 */
export const hasRole = (user: User | null, role: UserRole): boolean => {
  if (!user) return false
  return user.role >= role
}

/**
 * Check if a user has any of the specified permissions
 */
export const hasAnyPermission = (user: User | null, permissions: PermissionString[]): boolean => {
  if (!user || !user.permissions || permissions.length === 0) return false
  return permissions.some(permission => user.permissions.includes(permission))
}

/**
 * Check if a user has all of the specified permissions
 */
export const hasAllPermissions = (user: User | null, permissions: PermissionString[]): boolean => {
  if (!user || !user.permissions || permissions.length === 0) return false
  return permissions.every(permission => user.permissions.includes(permission))
}

/**
 * Check if a user can access a resource based on ownership and permissions
 */
export const canAccessResource = (
  user: User | null,
  permission: PermissionString,
  resourceOwnerId?: string
): boolean => {
  if (!user) return false
  
  // Check if user has the general permission
  if (hasPermission(user, permission)) return true
  
  // Check if user owns the resource and has owner-specific permissions
  if (resourceOwnerId && user.id === resourceOwnerId) {
    const ownerPermissions: Record<string, PermissionString> = {
      'manga.update': 'manga.update',
      'manga.delete': 'manga.delete',
      'chapter.update': 'chapter.update',
      'chapter.delete': 'chapter.delete',
      'file.delete': 'file.delete'
    }
    
    const ownerPermission = ownerPermissions[permission]
    if (ownerPermission && hasPermission(user, ownerPermission)) {
      return true
    }
  }
  
  return false
}

/**
 * Get the minimum role required for a permission
 */
export const getMinimumRoleForPermission = (permission: PermissionString): UserRole | null => {
  // This would ideally come from the backend, but we can provide defaults
  const permissionRoleMap: Record<string, UserRole> = {
    // Reader permissions
    'manga.read': UserRole.Reader,
    'chapter.read': UserRole.Reader,
    'file.download': UserRole.Reader,
    'user.view_profile': UserRole.Reader,
    'user.update_profile': UserRole.Reader,
    'comment.create': UserRole.Reader,
    'comment.read': UserRole.Reader,
    'comment.update': UserRole.Reader,
    
    // Uploader permissions
    'manga.create': UserRole.Uploader,
    'manga.update': UserRole.Uploader,
    'manga.delete': UserRole.Uploader,
    'manga.publish': UserRole.Uploader,
    'chapter.create': UserRole.Uploader,
    'chapter.update': UserRole.Uploader,
    'chapter.delete': UserRole.Uploader,
    'chapter.publish': UserRole.Uploader,
    'file.upload': UserRole.Uploader,
    'file.delete': UserRole.Uploader,
    'file.process': UserRole.Uploader,
    
    // Moderator permissions
    'manga.moderate': UserRole.Moderator,
    'chapter.moderate': UserRole.Moderator,
    'comment.delete': UserRole.Moderator,
    'comment.moderate': UserRole.Moderator,
    'user.read': UserRole.Moderator,
    'user.update': UserRole.Moderator,
    'user.invite': UserRole.Moderator,
    
    // Administrator permissions
    'manga.manage_all': UserRole.Administrator,
    'chapter.manage_all': UserRole.Administrator,
    'file.manage_all': UserRole.Administrator,
    'user.delete': UserRole.Administrator,
    'user.manage': UserRole.Administrator,
    'system.configure': UserRole.Administrator,
    'system.audit': UserRole.Administrator,
    'system.backup': UserRole.Administrator,
    'system.monitor': UserRole.Administrator,
    'system.maintenance': UserRole.Administrator
  }
  
  return permissionRoleMap[permission] || null
}

/**
 * Validate multiple permissions for a user
 */
export const validatePermissions = (
  user: User | null,
  requiredPermissions: PermissionString[],
  resourceChecks?: ResourcePermissionCheck[]
): PermissionValidation => {
  const errors: string[] = []
  const warnings: string[] = []
  
  if (!user) {
    errors.push('Usuario no autenticado')
    return { isValid: false, errors, warnings }
  }
  
  // Check required permissions
  const missingPermissions = requiredPermissions.filter(
    permission => !hasPermission(user, permission)
  )
  
  if (missingPermissions.length > 0) {
    errors.push(`Permisos faltantes: ${missingPermissions.join(', ')}`)
  }
  
  // Check resource-specific permissions
  if (resourceChecks) {
    for (const check of resourceChecks) {
      if (!canAccessResource(user, check.permission, check.ownerId)) {
        errors.push(`Sin acceso al recurso ${check.resourceType || 'desconocido'}: ${check.permission}`)
      }
    }
  }
  
  // Add warnings for role-based access
  const insufficientRolePermissions = requiredPermissions.filter(permission => {
    const minRole = getMinimumRoleForPermission(permission)
    return minRole && user.role < minRole
  })
  
  if (insufficientRolePermissions.length > 0) {
    warnings.push(`Rol insuficiente para: ${insufficientRolePermissions.join(', ')}`)
  }
  
  return {
    isValid: errors.length === 0,
    errors,
    warnings
  }
}

/**
 * Get user role display name
 */
export const getRoleDisplayName = (role: UserRole): string => {
  const roleNames: Record<UserRole, string> = {
    [UserRole.Reader]: 'Lector',
    [UserRole.Uploader]: 'Subidor',
    [UserRole.Moderator]: 'Moderador',
    [UserRole.Administrator]: 'Administrador'
  }
  
  return roleNames[role] || 'Desconocido'
}

/**
 * Get role color for UI display
 */
export const getRoleColor = (role: UserRole): string => {
  const roleColors: Record<UserRole, string> = {
    [UserRole.Reader]: 'text-blue-600 bg-blue-100',
    [UserRole.Uploader]: 'text-green-600 bg-green-100',
    [UserRole.Moderator]: 'text-yellow-600 bg-yellow-100',
    [UserRole.Administrator]: 'text-red-600 bg-red-100'
  }
  
  return roleColors[role] || 'text-gray-600 bg-gray-100'
}

/**
 * Check if a role can be assigned by another role
 */
export const canAssignRole = (assignerRole: UserRole, targetRole: UserRole): boolean => {
  // Only administrators can assign administrator role
  if (targetRole === UserRole.Administrator) {
    return assignerRole === UserRole.Administrator
  }
  
  // Moderators and administrators can assign moderator and below
  if (targetRole === UserRole.Moderator) {
    return assignerRole >= UserRole.Moderator
  }
  
  // Moderators and administrators can assign uploader and reader roles
  return assignerRole >= UserRole.Moderator
}

/**
 * Get permissions that a role inherits from lower roles
 */
export const getInheritedPermissions = (role: UserRole): UserRole[] => {
  return RoleHierarchy[role] || [role]
}

/**
 * Format permission name for display
 */
export const formatPermissionName = (permission: PermissionString): string => {
  const [resource, action] = permission.split('.')
  
  const resourceNames: Record<string, string> = {
    manga: 'Manga',
    chapter: 'Capítulo',
    file: 'Archivo',
    user: 'Usuario',
    system: 'Sistema',
    comment: 'Comentario'
  }
  
  const actionNames: Record<string, string> = {
    create: 'Crear',
    read: 'Leer',
    update: 'Actualizar',
    delete: 'Eliminar',
    publish: 'Publicar',
    moderate: 'Moderar',
    manage: 'Gestionar',
    manage_all: 'Gestionar Todo',
    upload: 'Subir',
    download: 'Descargar',
    process: 'Procesar',
    invite: 'Invitar',
    view_profile: 'Ver Perfil',
    update_profile: 'Actualizar Perfil',
    configure: 'Configurar',
    audit: 'Auditar',
    backup: 'Respaldar',
    monitor: 'Monitorear',
    maintenance: 'Mantenimiento'
  }
  
  const resourceName = resourceNames[resource] || resource
  const actionName = actionNames[action] || action
  
  return `${resourceName}: ${actionName}`
}

/**
 * Check if user can perform bulk operations
 */
export const canPerformBulkOperation = (
  user: User | null,
  operation: string,
  targetRole?: UserRole
): boolean => {
  if (!user) return false
  
  // Only administrators can perform bulk operations on administrators
  if (targetRole === UserRole.Administrator && user.role !== UserRole.Administrator) {
    return false
  }
  
  // Check specific bulk operation permissions
  const bulkPermissions: Record<string, PermissionString> = {
    'activate': 'user.manage',
    'deactivate': 'user.manage',
    'changeRole': 'user.manage',
    'delete': 'user.delete'
  }
  
  const requiredPermission = bulkPermissions[operation]
  return requiredPermission ? hasPermission(user, requiredPermission) : false
}

/**
 * Get available roles that a user can assign
 */
export const getAssignableRoles = (assignerRole: UserRole): UserRole[] => {
  const allRoles = [UserRole.Reader, UserRole.Uploader, UserRole.Moderator, UserRole.Administrator]
  
  return allRoles.filter(role => canAssignRole(assignerRole, role))
}