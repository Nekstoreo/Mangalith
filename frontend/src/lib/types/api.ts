/**
 * Standard API response wrapper
 */
export interface ApiResponse<T = any> {
  success: boolean
  data: T
  message?: string
  errors?: string[]
}

/**
 * Paginated API response
 */
export interface PaginatedResponse<T = any> {
  success: boolean
  data: T[]
  pagination: PaginationInfo
  message?: string
}

/**
 * Pagination information
 */
export interface PaginationInfo {
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

/**
 * API error response
 */
export interface ApiError {
  code: string
  message: string
  details?: Record<string, any>
  timestamp: string
}

/**
 * Validation error details
 */
export interface ValidationError {
  field: string
  message: string
  code: string
}

/**
 * API client configuration
 */
export interface ApiClientConfig {
  baseURL: string
  timeout: number
  headers?: Record<string, string>
}

/**
 * Request options for API calls
 */
export interface RequestOptions {
  headers?: Record<string, string>
  timeout?: number
  retries?: number
  cache?: boolean
}

/**
 * File upload progress
 */
export interface UploadProgress {
  loaded: number
  total: number
  percentage: number
}

/**
 * API endpoints configuration
 */
export const ApiEndpoints = {
  // Authentication
  Auth: {
    Login: '/auth/login',
    Register: '/auth/register',
    Profile: '/auth/profile',
    ChangePassword: '/auth/change-password',
    RefreshToken: '/auth/refresh'
  },
  
  // User Management
  Users: {
    List: '/admin/users',
    Detail: (id: string) => `/admin/users/${id}`,
    Update: (id: string) => `/admin/users/${id}`,
    Delete: (id: string) => `/admin/users/${id}`,
    BulkOperation: '/admin/users/bulk',
    Statistics: '/admin/users/statistics',
    Quota: (id: string) => `/admin/users/${id}/quota`
  },
  
  // Invitations
  Invitations: {
    List: '/admin/invitations',
    Create: '/admin/invitations',
    Detail: (id: string) => `/admin/invitations/${id}`,
    Accept: '/invitations/accept',
    Validate: (token: string) => `/invitations/validate/${token}`,
    Resend: (id: string) => `/admin/invitations/${id}/resend`,
    Cancel: (id: string) => `/admin/invitations/${id}/cancel`,
    Statistics: '/admin/invitations/statistics'
  },
  
  // Audit Logs
  Audit: {
    List: '/admin/audit',
    Statistics: '/admin/audit/statistics',
    Export: '/admin/audit/export',
    Summary: '/admin/audit/summary'
  },
  
  // Permissions
  Permissions: {
    List: '/permissions',
    UserPermissions: (userId: string) => `/permissions/user/${userId}`,
    RolePermissions: (role: string) => `/permissions/role/${role}`,
    Check: '/permissions/check'
  },
  
  // Files
  Files: {
    Upload: '/files/upload',
    List: '/files',
    Detail: (id: string) => `/files/${id}`,
    Delete: (id: string) => `/files/${id}`,
    Download: (id: string) => `/files/${id}/download`
  }
} as const

/**
 * HTTP methods
 */
export enum HttpMethod {
  GET = 'GET',
  POST = 'POST',
  PUT = 'PUT',
  PATCH = 'PATCH',
  DELETE = 'DELETE'
}

/**
 * API client interface
 */
export interface ApiClient {
  get<T>(url: string, options?: RequestOptions): Promise<ApiResponse<T>>
  post<T>(url: string, data?: any, options?: RequestOptions): Promise<ApiResponse<T>>
  put<T>(url: string, data?: any, options?: RequestOptions): Promise<ApiResponse<T>>
  patch<T>(url: string, data?: any, options?: RequestOptions): Promise<ApiResponse<T>>
  delete<T>(url: string, options?: RequestOptions): Promise<ApiResponse<T>>
  upload<T>(url: string, file: File, onProgress?: (progress: UploadProgress) => void): Promise<ApiResponse<T>>
}