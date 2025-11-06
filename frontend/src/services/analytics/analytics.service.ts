import { apiClient } from '../api/client'

export interface ModerationMetrics {
  totalPublications: number
  publicationsInReview: number
  publicationsApproved: number
  publicationsRejected: number
  publicationsArchived: number
  pendingReports: number
  resolvedReports: number
  averageReviewTimeHours: number
  approvalRate: number
  statusDistribution: Record<string, number>
  reportCategoryDistribution: Record<string, number>
}

export interface PublicationMetrics {
  totalSubmissions: number
  submissionsToday: number
  submissionsThisWeek: number
  submissionsThisMonth: number
  averageProcessingTimeHours: number
  contentRatingDistribution: Record<string, number>
  topCreators: Record<string, number>
  trends: PublicationTrend[]
}

export interface PublicationTrend {
  date: string
  submissions: number
  approvals: number
  rejections: number
  averageReviewTime: number
}

export interface ModeratorPerformance {
  moderatorId: string
  moderatorName: string
  actionsCompleted: number
  approvalsCount: number
  rejectionsCount: number
  reportsReviewed: number
  averageReviewTimeHours: number
  approvalRate: number
  lastActiveAt: string
  actionsLast7Days: number
  actionsLast30Days: number
}

export interface ContentTrend {
  category: string
  description: string
  count: number
  percentageChange: number
  direction: 'Up' | 'Down' | 'Stable'
  periodStart: string
  periodEnd: string
}

export interface SystemAlert {
  id: string
  type: 'QueueBacklog' | 'UnusualActivity' | 'PerformanceIssue' | 'PolicyViolation' | 'SystemHealth'
  severity: 'Low' | 'Medium' | 'High' | 'Critical'
  title: string
  description: string
  metadata: Record<string, any>
  createdAt: string
  isResolved: boolean
}

export interface ModerationAnalytics {
  metrics: ModerationMetrics
  moderatorPerformances: ModeratorPerformance[]
  contentTrends: ContentTrend[]
  systemAlerts: SystemAlert[]
  generatedAt: string
}

export interface AnalyticsDateRange {
  fromDate?: string
  toDate?: string
}

class AnalyticsService {
  async getModerationAnalytics(dateRange?: AnalyticsDateRange): Promise<ModerationAnalytics> {
    const params = new URLSearchParams()
    if (dateRange?.fromDate) params.append('fromDate', dateRange.fromDate)
    if (dateRange?.toDate) params.append('toDate', dateRange.toDate)

    const response = await apiClient.get(`/analytics/moderation?${params}`)
    return response.data
  }

  async getPublicationMetrics(dateRange?: AnalyticsDateRange): Promise<PublicationMetrics> {
    const params = new URLSearchParams()
    if (dateRange?.fromDate) params.append('fromDate', dateRange.fromDate)
    if (dateRange?.toDate) params.append('toDate', dateRange.toDate)

    const response = await apiClient.get(`/analytics/publications?${params}`)
    return response.data
  }

  async getModeratorPerformance(dateRange?: AnalyticsDateRange): Promise<ModeratorPerformance[]> {
    const params = new URLSearchParams()
    if (dateRange?.fromDate) params.append('fromDate', dateRange.fromDate)
    if (dateRange?.toDate) params.append('toDate', dateRange.toDate)

    const response = await apiClient.get(`/analytics/moderators/performance?${params}`)
    return response.data
  }

  async getContentTrends(days: number = 30): Promise<ContentTrend[]> {
    const response = await apiClient.get(`/analytics/trends?days=${days}`)
    return response.data
  }

  async getSystemAlerts(includeResolved: boolean = false): Promise<SystemAlert[]> {
    const response = await apiClient.get(`/analytics/alerts?includeResolved=${includeResolved}`)
    return response.data
  }

  async exportAnalyticsReport(fromDate: string, toDate: string, format: string = 'csv'): Promise<Blob> {
    const response = await apiClient.get(`/analytics/export`, {
      params: { fromDate, toDate, format },
      responseType: 'blob'
    })
    return response.data
  }

  async checkSystemAlerts(): Promise<void> {
    await apiClient.post('/analytics/alerts/check')
  }

  // Utility methods for formatting
  formatPercentage(value: number): string {
    return `${value.toFixed(1)}%`
  }

  formatHours(hours: number): string {
    if (hours < 1) {
      return `${Math.round(hours * 60)} min`
    }
    if (hours < 24) {
      return `${hours.toFixed(1)} hrs`
    }
    return `${(hours / 24).toFixed(1)} días`
  }

  getSeverityColor(severity: SystemAlert['severity']): string {
    switch (severity) {
      case 'Low':
        return 'text-blue-600 bg-blue-100'
      case 'Medium':
        return 'text-yellow-600 bg-yellow-100'
      case 'High':
        return 'text-orange-600 bg-orange-100'
      case 'Critical':
        return 'text-red-600 bg-red-100'
      default:
        return 'text-gray-600 bg-gray-100'
    }
  }

  getTrendIcon(direction: ContentTrend['direction']): string {
    switch (direction) {
      case 'Up':
        return '↗️'
      case 'Down':
        return '↘️'
      case 'Stable':
        return '➡️'
      default:
        return '➡️'
    }
  }
}

export const analyticsService = new AnalyticsService()