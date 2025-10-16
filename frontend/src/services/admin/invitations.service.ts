import { apiClient } from '@/services/api/client'
import { 
  InvitationDetail,
  InvitationSummary,
  InvitationFilter,
  CreateInvitationRequest,
  AcceptInvitationRequest,
  AcceptInvitationResponse,
  BulkInvitationOperationResponse,
  InvitationStatistics,
  ValidateInvitationResponse,
  BulkInvitationRequest,
  ResendInvitationRequest,
  CancelInvitationRequest,
  PaginatedResponse,
  ApiResponse
} from '@/lib/types'

export class InvitationService {
  // Get paginated list of invitations
  async getInvitations(filter?: InvitationFilter): Promise<PaginatedResponse<InvitationSummary>> {
    try {
      const params = new URLSearchParams()
      
      if (filter) {
        Object.entries(filter).forEach(([key, value]) => {
          if (value !== undefined && value !== null) {
            params.append(key, value.toString())
          }
        })
      }

      const response = await apiClient.get<InvitationSummary[]>(`/admin/invitations?${params.toString()}`)
      return response.data as PaginatedResponse<InvitationSummary>
    } catch (error) {
      console.error('Get invitations error:', error)
      throw error
    }
  }

  // Get detailed invitation information
  async getInvitationDetail(invitationId: string): Promise<InvitationDetail> {
    try {
      const response = await apiClient.get<InvitationDetail>(`/admin/invitations/${invitationId}`)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get invitation details')
      }
    } catch (error) {
      console.error('Get invitation detail error:', error)
      throw error
    }
  }

  // Create new invitation
  async createInvitation(request: CreateInvitationRequest): Promise<InvitationDetail> {
    try {
      const response = await apiClient.post<InvitationDetail>('/admin/invitations', request)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to create invitation')
      }
    } catch (error) {
      console.error('Create invitation error:', error)
      throw error
    }
  }

  // Create multiple invitations
  async createBulkInvitations(request: BulkInvitationRequest): Promise<BulkInvitationOperationResponse> {
    try {
      const response = await apiClient.post<BulkInvitationOperationResponse>('/admin/invitations/bulk', request)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to create bulk invitations')
      }
    } catch (error) {
      console.error('Create bulk invitations error:', error)
      throw error
    }
  }

  // Accept invitation (public endpoint)
  async acceptInvitation(request: AcceptInvitationRequest): Promise<AcceptInvitationResponse> {
    try {
      const response = await apiClient.post<AcceptInvitationResponse>('/invitations/accept', request)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to accept invitation')
      }
    } catch (error) {
      console.error('Accept invitation error:', error)
      throw error
    }
  }

  // Validate invitation token (public endpoint)
  async validateInvitation(token: string): Promise<ValidateInvitationResponse> {
    try {
      const response = await apiClient.get<ValidateInvitationResponse>(`/invitations/validate/${token}`)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to validate invitation')
      }
    } catch (error) {
      console.error('Validate invitation error:', error)
      throw error
    }
  }

  // Resend invitation email
  async resendInvitation(request: ResendInvitationRequest): Promise<InvitationDetail> {
    try {
      const response = await apiClient.post<InvitationDetail>(`/admin/invitations/${request.invitationId}/resend`, {
        newExpirationHours: request.newExpirationHours
      })
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to resend invitation')
      }
    } catch (error) {
      console.error('Resend invitation error:', error)
      throw error
    }
  }

  // Cancel invitation
  async cancelInvitation(request: CancelInvitationRequest): Promise<void> {
    try {
      const response = await apiClient.post(`/admin/invitations/${request.invitationId}/cancel`, {
        reason: request.reason
      })
      
      if (!response.data.success) {
        throw new Error(response.data.message || 'Failed to cancel invitation')
      }
    } catch (error) {
      console.error('Cancel invitation error:', error)
      throw error
    }
  }

  // Delete invitation
  async deleteInvitation(invitationId: string): Promise<void> {
    try {
      const response = await apiClient.delete(`/admin/invitations/${invitationId}`)
      
      if (!response.data.success) {
        throw new Error(response.data.message || 'Failed to delete invitation')
      }
    } catch (error) {
      console.error('Delete invitation error:', error)
      throw error
    }
  }

  // Get invitation statistics
  async getStatistics(): Promise<InvitationStatistics> {
    try {
      const response = await apiClient.get<InvitationStatistics>('/admin/invitations/statistics')
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get invitation statistics')
      }
    } catch (error) {
      console.error('Get invitation statistics error:', error)
      throw error
    }
  }

  // Get pending invitations for current user
  async getPendingInvitations(): Promise<InvitationSummary[]> {
    try {
      const response = await apiClient.get<InvitationSummary[]>('/admin/invitations/pending')
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get pending invitations')
      }
    } catch (error) {
      console.error('Get pending invitations error:', error)
      throw error
    }
  }

  // Get expiring invitations
  async getExpiringInvitations(days: number = 7): Promise<InvitationSummary[]> {
    try {
      const response = await apiClient.get<InvitationSummary[]>(`/admin/invitations/expiring?days=${days}`)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get expiring invitations')
      }
    } catch (error) {
      console.error('Get expiring invitations error:', error)
      throw error
    }
  }

  // Search invitations by email
  async searchInvitations(query: string, limit: number = 10): Promise<InvitationSummary[]> {
    try {
      const response = await apiClient.get<InvitationSummary[]>(`/admin/invitations/search?q=${encodeURIComponent(query)}&limit=${limit}`)
      
      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to search invitations')
      }
    } catch (error) {
      console.error('Search invitations error:', error)
      throw error
    }
  }
}

// Export singleton instance
export const invitationService = new InvitationService()