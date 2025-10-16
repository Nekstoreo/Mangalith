import { UserRole } from './auth'

/**
 * Detailed user response for admin management
 */
export interface UserDetail {
  id: string
  email: string
  fullName: string
  username?: string
  role: UserRole
  isActive: boolean
  createdAt: string
  updatedAt: string
  lastLoginAt?: string
  activityStats: UserActivityStats
  roleHistory: UserRoleChangeHistory[]
}

/**
 * Summary user response for listings
 */
export interface UserSummary {
  id: string
  email: string
  fullName: string
  username?: string
  role: UserRole
  isActive: boolean
  createdAt: string
  lastLoginAt?: string
  totalUploads: number
}

/**
 * User activity statistics
 */
export interface UserActivityStats {
  totalUploads: number
  totalMangaCreated: number
  totalChaptersCreated: number
  storageUsed: number
  recentAuditActions: number
  lastActivityAt?: string
}

/**
 * User role change history
 */
export interface UserRoleChangeHistory {
  previousRole: UserRole
  newRole: UserRole
  changedByUser: string
  changedAt: string
  reason?: string
}

/**
 * Update user request
 */
export interface UpdateUserRequest {
  fullName?: string
  username?: string
  role?: UserRole
  isActive?: boolean
  reason?: string
}

/**
 * Bulk user operation response
 */
export interface BulkUserOperationResponse {
  successCount: number
  failureCount: number
  errors: BulkOperationError[]
  message: string
}

/**
 * Bulk operation error
 */
export interface BulkOperationError {
  userId: string
  userEmail: string
  errorMessage: string
}

/**
 * User system statistics
 */
export interface UserSystemStatistics {
  totalUsers: number
  activeUsers: number
  inactiveUsers: number
  usersByRole: Record<UserRole, number>
  newUsersLast30Days: number
  activeUsersLast30Days: number
  totalStorageUsed: number
  generatedAt: string
}

/**
 * User filter for querying
 */
export interface UserFilter {
  role?: UserRole
  isActive?: boolean
  email?: string
  fullName?: string
  createdAfter?: string
  createdBefore?: string
  lastLoginAfter?: string
  lastLoginBefore?: string
  hasUploads?: boolean
  page?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
}

/**
 * Bulk user operation request
 */
export interface BulkUserOperationRequest {
  userIds: string[]
  operation: BulkUserOperation
  data?: Record<string, any>
  reason?: string
}

/**
 * Types of bulk user operations
 */
export enum BulkUserOperation {
  Activate = 'activate',
  Deactivate = 'deactivate',
  ChangeRole = 'changeRole',
  Delete = 'delete'
}

/**
 * User quota information
 */
export interface UserQuota {
  userId: string
  storageLimit: number
  storageUsed: number
  uploadLimit: number
  uploadsUsed: number
  apiCallLimit: number
  apiCallsUsed: number
  resetDate: string
}

/**
 * Update user quota request
 */
export interface UpdateUserQuotaRequest {
  storageLimit?: number
  uploadLimit?: number
  apiCallLimit?: number
  reason?: string
}

/**
 * User dashboard data for admin
 */
export interface UserManagementDashboard {
  statistics: UserSystemStatistics
  recentUsers: UserSummary[]
  activeUsers: UserSummary[]
  roleDistribution: Record<UserRole, number>
  storageUsage: {
    total: number
    byRole: Record<UserRole, number>
  }
}