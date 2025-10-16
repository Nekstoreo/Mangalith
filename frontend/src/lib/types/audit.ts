import { UserRole } from './auth'

/**
 * Audit log entry
 */
export interface AuditLog {
  id: string
  userId: string
  user: AuditUserInfo
  action: string
  resource: string
  resourceId?: string
  details?: string
  ipAddress: string
  userAgent?: string
  success: boolean
  timestamp: string
  severity: AuditSeverity
}

/**
 * User information in audit logs
 */
export interface AuditUserInfo {
  id: string
  email: string
  fullName: string
  role: UserRole
}

/**
 * Audit log severity levels
 */
export enum AuditSeverity {
  Info = 'Info',
  Warning = 'Warning',
  Error = 'Error',
  Critical = 'Critical'
}

/**
 * Audit log filter for querying
 */
export interface AuditLogFilter {
  userId?: string
  action?: string
  resource?: string
  success?: boolean
  severity?: AuditSeverity
  fromDate?: string
  toDate?: string
  ipAddress?: string
  page?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
}

/**
 * Audit log statistics
 */
export interface AuditLogStatistics {
  totalEvents: number
  successfulEvents: number
  failedEvents: number
  uniqueUsers: number
  topActions: Record<string, number>
  eventsByDay: Record<string, number>
  eventsBySeverity: Record<AuditSeverity, number>
  generatedAt: string
}

/**
 * Audit activity summary
 */
export interface AuditActivitySummary {
  last24Hours: AuditPeriodSummary
  last7Days: AuditPeriodSummary
  last30Days: AuditPeriodSummary
  securityAlerts: SecurityAlert[]
  generatedAt: string
}

/**
 * Audit summary for a specific period
 */
export interface AuditPeriodSummary {
  totalEvents: number
  successfulEvents: number
  failedEvents: number
  uniqueUsers: number
  topActions: Record<string, number>
}

/**
 * Security alert based on audit logs
 */
export interface SecurityAlert {
  id: string
  type: SecurityAlertType
  severity: AuditSeverity
  message: string
  user?: AuditUserInfo
  ipAddress?: string
  eventCount: number
  firstEvent: string
  lastEvent: string
}

/**
 * Types of security alerts
 */
export enum SecurityAlertType {
  MultipleFailedAttempts = 'MultipleFailedAttempts',
  SuspiciousIpAccess = 'SuspiciousIpAccess',
  UnusualRoleChanges = 'UnusualRoleChanges',
  OffHoursActivity = 'OffHoursActivity',
  MassDataDeletion = 'MassDataDeletion',
  SensitiveResourceAccess = 'SensitiveResourceAccess'
}

/**
 * Audit export request
 */
export interface AuditExportRequest {
  format: 'CSV' | 'JSON' | 'Excel'
  includeDetails: boolean
  includeUserInfo: boolean
  filter?: AuditLogFilter
}

/**
 * Audit export response
 */
export interface AuditExportResponse {
  fileName: string
  contentType: string
  fileSize: number
  recordCount: number
  generatedAt: string
  appliedFilters?: string
}

/**
 * Audit dashboard data
 */
export interface AuditDashboardData {
  summary: AuditActivitySummary
  recentLogs: AuditLog[]
  statistics: AuditLogStatistics
  alerts: SecurityAlert[]
}