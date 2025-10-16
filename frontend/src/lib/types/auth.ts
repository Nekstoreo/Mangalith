/**
 * User roles in the system with hierarchical structure
 */
export enum UserRole {
  Reader = 0,
  Uploader = 1,
  Moderator = 2,
  Administrator = 3
}

/**
 * Enhanced user interface with role and permission information
 */
export interface User {
  id: string
  username: string
  email: string
  fullName: string
  role: UserRole
  permissions: string[]
  avatar?: string
  bio?: string
  isActive: boolean
  createdAt: string
  updatedAt: string
  lastLoginAt?: string
  stats?: UserStats
}

/**
 * User statistics for dashboard display
 */
export interface UserStats {
  totalManga: number
  totalReadChapters: number
  favoriteSeries: number
  readingStreak: number
  totalUploads?: number
  storageUsed?: number
}

/**
 * Authentication response from the API
 */
export interface AuthResponse {
  user: User
  token: string
  expiresAt: string
}

/**
 * Login request payload
 */
export interface LoginRequest {
  email: string
  password: string
}

/**
 * Registration request payload
 */
export interface RegisterRequest {
  username: string
  email: string
  password: string
  fullName: string
}

/**
 * Profile update request payload
 */
export interface ProfileUpdateRequest {
  username?: string
  fullName?: string
  bio?: string
}

/**
 * Password change request payload
 */
export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

/**
 * JWT token payload structure
 */
export interface JwtPayload {
  sub: string // User ID
  email: string
  role: UserRole
  permissions: string[]
  exp: number
  iat: number
}