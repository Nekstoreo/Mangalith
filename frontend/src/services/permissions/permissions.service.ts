import { apiClient } from '@/services/api/client'
import { 
  Permission, 
  PermissionString, 
  UserRole, 
  ApiResponse,
  ResourcePermissionCheck,
  PermissionCheckResult
} from '@/lib/types'

export class PermissionService {
  // Get all available permissions
  async getPermissions(): Promise<Permission[]> {
    try {
      const response = await apiClient.get<Permission[]>('/permissions')
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get permissions')
      }
    } catch (error) {
      console.error('Get permissions error:', error)
      throw error
    }
  }

  // Get permissions for a specific user
  async getUserPermissions(userId: string): Promise<string[]> {
    try {
      const response = await apiClient.get<string[]>(`/permissions/user/${userId}`)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get user permissions')
      }
    } catch (error) {
      console.error('Get user permissions error:', error)
      throw error
    }
  }

  // Get permissions for a specific role
  async getRolePermissions(role: UserRole): Promise<string[]> {
    try {
      const response = await apiClient.get<string[]>(`/permissions/role/${role}`)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get role permissions')
      }
    } catch (error) {
      console.error('Get role permissions error:', error)
      throw error
    }
  }

  // Check if user has specific permission
  async checkPermission(
    userId: string, 
    permission: PermissionString,
    resourceId?: string
  ): Promise<PermissionCheckResult> {
    try {
      const response = await apiClient.post<PermissionCheckResult>('/permissions/check', {
        userId,
        permission,
        resourceId
      })
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to check permission')
      }
    } catch (error) {
      console.error('Check permission error:', error)
      throw error
    }
  }

  // Check multiple permissions at once
  async checkMultiplePermissions(
    userId: string,
    checks: ResourcePermissionCheck[]
  ): Promise<Record<string, PermissionCheckResult>> {
    try {
      const response = await apiClient.post<Record<string, PermissionCheckResult>>('/permissions/check-multiple', {
        userId,
        checks
      })
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to check permissions')
      }
    } catch (error) {
      console.error('Check multiple permissions error:', error)
      throw error
    }
  }

  // Validate permissions for a form or action
  async validatePermissions(
    userId: string,
    requiredPermissions: PermissionString[],
    resourceChecks?: ResourcePermissionCheck[]
  ): Promise<{
    isValid: boolean
    missingPermissions: string[]
    errors: string[]
  }> {
    try {
      const response = await apiClient.post('/permissions/validate', {
        userId,
        requiredPermissions,
        resourceChecks
      })
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to validate permissions')
      }
    } catch (error) {
      console.error('Validate permissions error:', error)
      throw error
    }
  }

  // Get permission hierarchy for role
  async getPermissionHierarchy(role: UserRole): Promise<{
    role: UserRole
    permissions: string[]
    inheritedFrom: UserRole[]
  }> {
    try {
      const response = await apiClient.get(`/permissions/hierarchy/${role}`)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get permission hierarchy')
      }
    } catch (error) {
      console.error('Get permission hierarchy error:', error)
      throw error
    }
  }
}

// Export singleton instance
export const permissionService = new PermissionService()