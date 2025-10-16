import { apiClient } from '@/services/api/client'
import { 
  AuditLog,
  AuditLogFilter,
  AuditLogStatistics,
  AuditActivitySummary,
  AuditExportRequest,
  AuditExportResponse,
  AuditDashboardData,
  PaginatedResponse,
  ApiResponse
} from '@/lib/types'

export class AuditService {
  // Get paginated audit logs
  async getAuditLogs(filter?: AuditLogFilter): Promise<PaginatedResponse<AuditLog>> {
    try {
      const params = new URLSearchParams()
      
      if (filter) {
        Object.entries(filter).forEach(([key, value]) => {
          if (value !== undefined && value !== null) {
            params.append(key, value.toString())
          }
        })
      }

      const response = await apiClient.get<AuditLog[]>(`/admin/audit?${params.toString()}`)
      return response.data as PaginatedResponse<AuditLog>
    } catch (error) {
      console.error('Get audit logs error:', error)
      throw error
    }
  }

  // Get audit log statistics
  async getStatistics(fromDate?: string, toDate?: string): Promise<AuditLogStatistics> {
    try {
      const params = new URLSearchParams()
      if (fromDate) params.append('fromDate', fromDate)
      if (toDate) params.append('toDate', toDate)

      const response = await apiClient.get<AuditLogStatistics>(`/admin/audit/statistics?${params.toString()}`)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get audit statistics')
      }
    } catch (error) {
      console.error('Get audit statistics error:', error)
      throw error
    }
  }

  // Get audit activity summary
  async getActivitySummary(): Promise<AuditActivitySummary> {
    try {
      const response = await apiClient.get<AuditActivitySummary>('/admin/audit/summary')
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get activity summary')
      }
    } catch (error) {
      console.error('Get activity summary error:', error)
      throw error
    }
  }

  // Export audit logs
  async exportAuditLogs(request: AuditExportRequest): Promise<AuditExportResponse> {
    try {
      const response = await apiClient.post<AuditExportResponse>('/admin/audit/export', request)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to export audit logs')
      }
    } catch (error) {
      console.error('Export audit logs error:', error)
      throw error
    }
  }

  // Download exported audit logs
  async downloadExport(fileName: string): Promise<Blob> {
    try {
      const response = await fetch(`/api/admin/audit/export/download/${fileName}`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        }
      })

      if (!response.ok) {
        throw new Error('Failed to download export file')
      }

      return await response.blob()
    } catch (error) {
      console.error('Download export error:', error)
      throw error
    }
  }

  // Get dashboard data
  async getDashboardData(): Promise<AuditDashboardData> {
    try {
      const response = await apiClient.get<AuditDashboardData>('/admin/audit/dashboard')
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get dashboard data')
      }
    } catch (error) {
      console.error('Get dashboard data error:', error)
      throw error
    }
  }

  // Search audit logs by action or resource
  async searchAuditLogs(
    query: string, 
    filter?: Partial<AuditLogFilter>
  ): Promise<AuditLog[]> {
    try {
      const params = new URLSearchParams()
      params.append('q', query)
      
      if (filter) {
        Object.entries(filter).forEach(([key, value]) => {
          if (value !== undefined && value !== null) {
            params.append(key, value.toString())
          }
        })
      }

      const response = await apiClient.get<AuditLog[]>(`/admin/audit/search?${params.toString()}`)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to search audit logs')
      }
    } catch (error) {
      console.error('Search audit logs error:', error)
      throw error
    }
  }

  // Get audit logs for specific user
  async getUserAuditLogs(
    userId: string, 
    filter?: Partial<AuditLogFilter>
  ): Promise<PaginatedResponse<AuditLog>> {
    try {
      const params = new URLSearchParams()
      params.append('userId', userId)
      
      if (filter) {
        Object.entries(filter).forEach(([key, value]) => {
          if (value !== undefined && value !== null) {
            params.append(key, value.toString())
          }
        })
      }

      const response = await apiClient.get<AuditLog[]>(`/admin/audit/user?${params.toString()}`)
      return response.data as PaginatedResponse<AuditLog>
    } catch (error) {
      console.error('Get user audit logs error:', error)
      throw error
    }
  }

  // Get audit logs for specific resource
  async getResourceAuditLogs(
    resource: string,
    resourceId?: string,
    filter?: Partial<AuditLogFilter>
  ): Promise<PaginatedResponse<AuditLog>> {
    try {
      const params = new URLSearchParams()
      params.append('resource', resource)
      if (resourceId) params.append('resourceId', resourceId)
      
      if (filter) {
        Object.entries(filter).forEach(([key, value]) => {
          if (value !== undefined && value !== null) {
            params.append(key, value.toString())
          }
        })
      }

      const response = await apiClient.get<AuditLog[]>(`/admin/audit/resource?${params.toString()}`)
      return response.data as PaginatedResponse<AuditLog>
    } catch (error) {
      console.error('Get resource audit logs error:', error)
      throw error
    }
  }
}

// Export singleton instance
export const auditService = new AuditService()