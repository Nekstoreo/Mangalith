import { apiClient } from '@/services/api/client'

export interface ModerationQueueItem {
  id: string
  mangaId: string
  creatorId: string
  submittedAtUtc: string
  priority: 'high' | 'normal' | 'low'
  reportCount?: number
}

export interface ModerationHistory {
  id: string
  publicationId: string
  moderatorId: string
  actionType: string
  reason?: string
  comments: string
  actionAtUtc: string
}

export interface BulkModerationAction {
  publicationIds: string[]
  actionType: 'approve' | 'reject' | 'under-review'
  reason?: string
  comments: string
}

export interface PagedModerationQueue {
  items: ModerationQueueItem[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface PagedModerationHistory {
  items: ModerationHistory[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export class ModerationService {
  async getModerationQueue(page: number = 1, pageSize: number = 10): Promise<PagedModerationQueue> {
    const response = await apiClient.get<PagedModerationQueue>(
      `/api/moderation/queue?page=${page}&pageSize=${pageSize}`
    )
    return response.data.data
  }

  async getModerationHistory(
    moderatorId?: string,
    page: number = 1,
    pageSize: number = 10
  ): Promise<PagedModerationHistory> {
    const url = moderatorId
      ? `/api/moderation/history/${moderatorId}?page=${page}&pageSize=${pageSize}`
      : `/api/moderation/history?page=${page}&pageSize=${pageSize}`
    const response = await apiClient.get<PagedModerationHistory>(url)
    return response.data.data
  }

  async getModeratorActions(
    moderatorId: string,
    page: number = 1,
    pageSize: number = 10
  ): Promise<PagedModerationHistory> {
    const response = await apiClient.get<PagedModerationHistory>(
      `/api/moderation/moderator-actions/${moderatorId}?page=${page}&pageSize=${pageSize}`
    )
    return response.data.data
  }

  async bulkAction(action: BulkModerationAction): Promise<{ successCount: number; failureCount: number }> {
    const response = await apiClient.post<{ successCount: number; failureCount: number }>(
      `/api/moderation/bulk-action`,
      action
    )
    return response.data.data
  }
}

export const moderationService = new ModerationService()
