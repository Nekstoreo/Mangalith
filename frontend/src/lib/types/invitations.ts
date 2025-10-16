import { UserRole } from './auth'

/**
 * Invitation status
 */
export enum InvitationStatus {
  Pending = 'Pending',
  Accepted = 'Accepted',
  Expired = 'Expired',
  Cancelled = 'Cancelled'
}

/**
 * User information for invitations
 */
export interface InvitationUserInfo {
  id: string
  email: string
  fullName: string
  role: UserRole
}

/**
 * Detailed invitation response
 */
export interface InvitationDetail {
  id: string
  email: string
  targetRole: UserRole
  token: string
  invitedBy: InvitationUserInfo
  createdAt: string
  expiresAt: string
  acceptedAt?: string
  acceptedBy?: InvitationUserInfo
  status: InvitationStatus
  isExpired: boolean
  isAccepted: boolean
  timeUntilExpiration?: number // in milliseconds
  message?: string
  emailSendAttempts: number
  lastEmailSentAt?: string
}

/**
 * Summary invitation response for listings
 */
export interface InvitationSummary {
  id: string
  email: string
  targetRole: UserRole
  invitedBy: InvitationUserInfo
  createdAt: string
  expiresAt: string
  status: InvitationStatus
  isExpired: boolean
  isAccepted: boolean
  hoursUntilExpiration?: number
}

/**
 * Create invitation request
 */
export interface CreateInvitationRequest {
  email: string
  targetRole: UserRole
  message?: string
  expirationHours?: number
}

/**
 * Accept invitation request
 */
export interface AcceptInvitationRequest {
  token: string
}

/**
 * Accept invitation response
 */
export interface AcceptInvitationResponse {
  success: boolean
  message: string
  newRole?: UserRole
  user?: InvitationUserInfo
  acceptedAt?: string
}

/**
 * Bulk invitation operation response
 */
export interface BulkInvitationOperationResponse {
  successCount: number
  failureCount: number
  errors: BulkInvitationError[]
  message: string
}

/**
 * Bulk invitation error
 */
export interface BulkInvitationError {
  invitationId: string
  email: string
  errorMessage: string
}

/**
 * Invitation statistics
 */
export interface InvitationStatistics {
  totalInvitations: number
  pendingInvitations: number
  acceptedInvitations: number
  expiredInvitations: number
  invitationsByStatus: Record<InvitationStatus, number>
  invitationsByMonth: Record<string, number>
  averageAcceptanceTimeHours: number
  expiringInNext7Days: number
  acceptanceRate: number
  generatedAt: string
}

/**
 * Validate invitation response
 */
export interface ValidateInvitationResponse {
  isValid: boolean
  invitation?: InvitationDetail
  errorMessage?: string
  errorCode?: string
}

/**
 * Invitation filter for querying
 */
export interface InvitationFilter {
  status?: InvitationStatus
  targetRole?: UserRole
  invitedByUserId?: string
  email?: string
  fromDate?: string
  toDate?: string
  isExpired?: boolean
  page?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
}

/**
 * Bulk invitation request
 */
export interface BulkInvitationRequest {
  invitations: CreateInvitationRequest[]
  sendEmails?: boolean
}

/**
 * Resend invitation request
 */
export interface ResendInvitationRequest {
  invitationId: string
  newExpirationHours?: number
}

/**
 * Cancel invitation request
 */
export interface CancelInvitationRequest {
  invitationId: string
  reason?: string
}