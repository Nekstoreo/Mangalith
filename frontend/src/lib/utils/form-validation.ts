import { z } from 'zod'
import { 
  User, 
  UserRole, 
  PermissionString,
  PermissionValidation,
  ResourcePermissionCheck
} from '@/lib/types'
import { validatePermissions, hasPermission, hasRole } from './permission-utils'

/**
 * Create a permission-based validation schema
 */
export const createPermissionValidation = (
  requiredPermissions: PermissionString[],
  resourceChecks?: ResourcePermissionCheck[]
) => {
  return z.object({}).refine(
    () => true, // We'll validate permissions separately
    {
      message: 'Permisos insuficientes'
    }
  )
}

/**
 * Validate form submission based on user permissions
 */
export const validateFormPermissions = (
  user: User | null,
  requiredPermissions: PermissionString[],
  resourceChecks?: ResourcePermissionCheck[]
): PermissionValidation => {
  return validatePermissions(user, requiredPermissions, resourceChecks)
}

/**
 * Role-based form field validation
 */
export const createRoleBasedValidation = (minimumRole: UserRole) => {
  return {
    validate: (user: User | null) => {
      if (!user) return 'Usuario no autenticado'
      if (!hasRole(user, minimumRole)) {
        return `Rol mínimo requerido: ${UserRole[minimumRole]}`
      }
      return true
    }
  }
}

/**
 * Permission-based form field validation
 */
export const createPermissionBasedValidation = (permission: PermissionString) => {
  return {
    validate: (user: User | null) => {
      if (!user) return 'Usuario no autenticado'
      if (!hasPermission(user, permission)) {
        return `Permiso requerido: ${permission}`
      }
      return true
    }
  }
}

/**
 * Validation schema for user management forms
 */
export const userManagementValidation = z.object({
  fullName: z.string().min(1, 'Nombre completo es requerido'),
  username: z.string().min(3, 'Nombre de usuario debe tener al menos 3 caracteres').optional(),
  role: z.nativeEnum(UserRole),
  isActive: z.boolean(),
  reason: z.string().optional()
}).refine((data, ctx) => {
  // Additional permission-based validation would be added here
  return true
})

/**
 * Validation schema for invitation forms
 */
export const invitationValidation = z.object({
  email: z.string().email('Email inválido'),
  targetRole: z.nativeEnum(UserRole),
  message: z.string().optional(),
  expirationHours: z.number().min(1).max(8760).optional() // Max 1 year
})

/**
 * Validation schema for bulk operations
 */
export const bulkOperationValidation = z.object({
  userIds: z.array(z.string().uuid()).min(1, 'Selecciona al menos un usuario'),
  operation: z.enum(['activate', 'deactivate', 'changeRole', 'delete']),
  data: z.record(z.any()).optional(),
  reason: z.string().optional()
})

/**
 * Validation schema for role assignment
 */
export const roleAssignmentValidation = z.object({
  userId: z.string().uuid(),
  newRole: z.nativeEnum(UserRole),
  reason: z.string().min(1, 'Razón es requerida para cambios de rol')
})

/**
 * Dynamic validation based on user permissions
 */
export const createDynamicValidation = (
  user: User | null,
  baseSchema: z.ZodSchema,
  permissionRules: Array<{
    permission: PermissionString
    field: string
    message?: string
  }>
) => {
  return baseSchema.refine((data) => {
    for (const rule of permissionRules) {
      if (!hasPermission(user, rule.permission)) {
        return false
      }
    }
    return true
  }, {
    message: 'Permisos insuficientes para completar esta acción'
  })
}

/**
 * Validate file upload permissions
 */
export const validateFileUpload = (
  user: User | null,
  fileSize: number,
  fileType: string
): PermissionValidation => {
  const errors: string[] = []
  const warnings: string[] = []
  
  if (!user) {
    errors.push('Usuario no autenticado')
    return { isValid: false, errors, warnings }
  }
  
  if (!hasPermission(user, 'file.upload')) {
    errors.push('Sin permisos para subir archivos')
  }
  
  // Check file size limits based on role
  const fileSizeLimits: Record<UserRole, number> = {
    [UserRole.Reader]: 0, // No upload permission
    [UserRole.Uploader]: 100 * 1024 * 1024, // 100MB
    [UserRole.Moderator]: 500 * 1024 * 1024, // 500MB
    [UserRole.Administrator]: 1024 * 1024 * 1024 // 1GB
  }
  
  const maxSize = fileSizeLimits[user.role] || 0
  if (fileSize > maxSize) {
    errors.push(`Archivo demasiado grande. Máximo permitido: ${Math.round(maxSize / 1024 / 1024)}MB`)
  }
  
  // Check file type restrictions
  const allowedTypes = ['application/zip', 'application/x-cbr', 'application/x-cbz']
  if (!allowedTypes.includes(fileType)) {
    errors.push('Tipo de archivo no permitido. Solo se permiten archivos ZIP, CBR y CBZ')
  }
  
  return {
    isValid: errors.length === 0,
    errors,
    warnings
  }
}

/**
 * Validate quota limits
 */
export const validateQuotaLimits = (
  user: User | null,
  currentUsage: number,
  additionalUsage: number
): PermissionValidation => {
  const errors: string[] = []
  const warnings: string[] = []
  
  if (!user) {
    errors.push('Usuario no autenticado')
    return { isValid: false, errors, warnings }
  }
  
  // Storage quotas by role (in bytes)
  const storageQuotas: Record<UserRole, number> = {
    [UserRole.Reader]: 0,
    [UserRole.Uploader]: 1024 * 1024 * 1024, // 1GB
    [UserRole.Moderator]: 5 * 1024 * 1024 * 1024, // 5GB
    [UserRole.Administrator]: -1 // Unlimited
  }
  
  const quota = storageQuotas[user.role]
  if (quota > 0) {
    const totalUsage = currentUsage + additionalUsage
    
    if (totalUsage > quota) {
      errors.push(`Cuota de almacenamiento excedida. Límite: ${Math.round(quota / 1024 / 1024)}MB`)
    } else if (totalUsage > quota * 0.8) {
      warnings.push(`Acercándose al límite de cuota (${Math.round((totalUsage / quota) * 100)}%)`)
    }
  }
  
  return {
    isValid: errors.length === 0,
    errors,
    warnings
  }
}

/**
 * Validate resource ownership
 */
export const validateResourceOwnership = (
  user: User | null,
  resourceOwnerId: string,
  permission: PermissionString
): PermissionValidation => {
  const errors: string[] = []
  const warnings: string[] = []
  
  if (!user) {
    errors.push('Usuario no autenticado')
    return { isValid: false, errors, warnings }
  }
  
  // Check if user owns the resource or has admin permissions
  const isOwner = user.id === resourceOwnerId
  const hasAdminPermission = hasPermission(user, permission)
  
  if (!isOwner && !hasAdminPermission) {
    errors.push('No tienes permisos para acceder a este recurso')
  }
  
  return {
    isValid: errors.length === 0,
    errors,
    warnings
  }
}