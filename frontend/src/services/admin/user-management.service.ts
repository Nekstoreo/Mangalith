import { apiClient } from '@/services/api/client'
import { 
  UserDetail,
  UserSummary,
  UserFilter,
  UpdateUserRequest,
  BulkUserOperationRequest,
  BulkUserOperationResponse,
  UserSystemStatistics,
  UserQuota,
  UpdateUserQuotaRequest,
  UserManagementDashboard,
  PaginatedResponse,
  ApiResponse
} from '@/lib/types'

export class UserManagementService {
  // Get paginated list of users
  async getUsers(filter?: UserFilter): Promise<PaginatedResponse<UserSummary>> {
    try {
      const params = new URLSearchParams()
      
      if (filter) {
        Object.entries(filter).forEach(([key, value]) => {
          if (value !== undefined && value !== null) {
            params.append(key, value.toString())
          }
        })
      }

      const response = await apiClient.get<UserSummary[]>(`/admin/users?${params.toString()}`)
      return response.data as PaginatedResponse<UserSummary>
    } catch (error) {
      console.error('Get users error:', error)
      throw error
    }
  }

  // Get detailed user information
  async getUserDetail(userId: string): Promise<UserDetail> {
    try {
      const response = await apiClient.get<UserDetail>(`/admin/users/${userId}`)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get user details')
      }
    } catch (error) {
      console.error('Get user detail error:', error)
      throw error
    }
  }

  // Update user information
  async updateUser(userId: string, updates: UpdateUserRequest): Promise<UserDetail> {
    try {
      const response = await apiClient.put<UserDetail>(`/admin/users/${userId}`, updates)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to update user')
      }
    } catch (error) {
      console.error('Update user error:', error)
      throw error
    }
  }

  // Delete user
  async deleteUser(userId: string, reason?: string): Promise<void> {
    try {
      const response = await apiClient.delete(`/admin/users/${userId}`, {
        headers: reason ? { 'X-Delete-Reason': reason } : undefined
      })
      
      if (!response.data.success) {
        throw new Error(response.data.message || 'Failed to delete user')
      }
    } catch (error) {
      console.error('Delete user error:', error)
      throw error
    }
  }

  // Perform bulk operations on users
  async bulkOperation(request: BulkUserOperationRequest): Promise<BulkUserOperationResponse> {
    try {
      const response = await apiClient.post<BulkUserOperationResponse>('/admin/users/bulk', request)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to perform bulk operation')
      }
    } catch (error) {
      console.error('Bulk operation error:', error)
      throw error
    }
  }

  // Get user system statistics
  async getSystemStatistics(): Promise<UserSystemStatistics> {
    try {
      const response = await apiClient.get<UserSystemStatistics>('/admin/users/statistics')
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get system statistics')
      }
    } catch (error) {
      console.error('Get system statistics error:', error)
      throw error
    }
  }

  // Get user quota information
  async getUserQuota(userId: string): Promise<UserQuota> {
    try {
      const response = await apiClient.get<UserQuota>(`/admin/users/${userId}/quota`)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get user quota')
      }
    } catch (error) {
      console.error('Get user quota error:', error)
      throw error
    }
  }

  // Update user quota
  async updateUserQuota(userId: string, updates: UpdateUserQuotaRequest): Promise<UserQuota> {
    try {
      const response = await apiClient.put<UserQuota>(`/admin/users/${userId}/quota`, updates)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to update user quota')
      }
    } catch (error) {
      console.error('Update user quota error:', error)
      throw error
    }
  }

  // Get dashboard data
  async getDashboardData(): Promise<UserManagementDashboard> {
    try {
      const response = await apiClient.get<UserManagementDashboard>('/admin/users/dashboard')
      
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

  // Search users by email or name
  async searchUsers(query: string, limit: number = 10): Promise<UserSummary[]> {
    try {
      const response = await apiClient.get<UserSummary[]>(`/admin/users/search?q=${encodeURIComponent(query)}&limit=${limit}`)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to search users')
      }
    } catch (error) {
      console.error('Search users error:', error)
      throw error
    }
  }
}

// Export singleton instance
export const userManagementService = new UserManagementService()