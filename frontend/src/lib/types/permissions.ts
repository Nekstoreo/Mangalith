import { UserRole } from './auth'

/**
 * Permission definition structure
 */
export interface Permission {
  id: number
  resource: string
  action: string
  name: string // computed: resource.action
  description: string
  createdAt: string
}

/**
 * Role-permission mapping
 */
export interface RolePermission {
  role: UserRole
  permissionId: number
  permission: Permission
  grantedAt: string
}

/**
 * Permission constants matching backend definitions
 */
export const Permissions = {
  Manga: {
    Create: 'manga.create',
    Read: 'manga.read',
    Update: 'manga.update',
    Delete: 'manga.delete',
    Publish: 'manga.publish',
    Moderate: 'manga.moderate',
    ManageAll: 'manga.manage_all'
  },
  Chapter: {
    Create: 'chapter.create',
    Read: 'chapter.read',
    Update: 'chapter.update',
    Delete: 'chapter.delete',
    Publish: 'chapter.publish',
    Moderate: 'chapter.moderate',
    ManageAll: 'chapter.manage_all'
  },
  File: {
    Upload: 'file.upload',
    Download: 'file.download',
    Delete: 'file.delete',
    Process: 'file.process',
    ManageAll: 'file.manage_all'
  },
  User: {
    Read: 'user.read',
    Update: 'user.update',
    Delete: 'user.delete',
    Manage: 'user.manage',
    Invite: 'user.invite',
    ViewProfile: 'user.view_profile',
    UpdateProfile: 'user.update_profile'
  },
  System: {
    Configure: 'system.configure',
    Audit: 'system.audit',
    Backup: 'system.backup',
    Monitor: 'system.monitor',
    Maintenance: 'system.maintenance'
  },
  Comment: {
    Create: 'comment.create',
    Read: 'comment.read',
    Update: 'comment.update',
    Delete: 'comment.delete',
    Moderate: 'comment.moderate'
  }
} as const

/**
 * Type for all possible permission strings
 */
export type PermissionString = 
  | typeof Permissions.Manga[keyof typeof Permissions.Manga]
  | typeof Permissions.Chapter[keyof typeof Permissions.Chapter]
  | typeof Permissions.File[keyof typeof Permissions.File]
  | typeof Permissions.User[keyof typeof Permissions.User]
  | typeof Permissions.System[keyof typeof Permissions.System]
  | typeof Permissions.Comment[keyof typeof Permissions.Comment]

/**
 * Role hierarchy mapping for permission inheritance
 */
export const RoleHierarchy: Record<UserRole, UserRole[]> = {
  [UserRole.Reader]: [UserRole.Reader],
  [UserRole.Uploader]: [UserRole.Reader, UserRole.Uploader],
  [UserRole.Moderator]: [UserRole.Reader, UserRole.Uploader, UserRole.Moderator],
  [UserRole.Administrator]: [UserRole.Reader, UserRole.Uploader, UserRole.Moderator, UserRole.Administrator]
}

/**
 * Permission check result
 */
export interface PermissionCheckResult {
  hasPermission: boolean
  reason?: string
  requiredRole?: UserRole
}

/**
 * Permission context for components
 */
export interface PermissionContext {
  user: {
    id: string
    role: UserRole
    permissions: string[]
  } | null
  hasPermission: (permission: PermissionString) => boolean
  hasRole: (role: UserRole) => boolean
  hasAnyPermission: (permissions: PermissionString[]) => boolean
  hasAllPermissions: (permissions: PermissionString[]) => boolean
}

/**
 * Resource-specific permission check
 */
export interface ResourcePermissionCheck {
  permission: PermissionString
  resourceId?: string
  resourceType?: string
  ownerId?: string
}

/**
 * Permission validation result for forms and actions
 */
export interface PermissionValidation {
  isValid: boolean
  errors: string[]
  warnings: string[]
}