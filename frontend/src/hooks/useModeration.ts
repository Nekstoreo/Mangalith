'use client'

import { useState, useCallback } from 'react'
import {
  moderationService,
  ModerationQueueItem,
  ModerationHistory,
  PagedModerationQueue,
  PagedModerationHistory,
} from '@/services/moderation/client'

export interface UseModerationState {
  queue: ModerationQueueItem[]
  history: ModerationHistory[]
  loading: boolean
  error: string | null
  totalCount: number
  currentPage: number
}

export const useModeration = () => {
  const [state, setState] = useState<UseModerationState>({
    queue: [],
    history: [],
    loading: false,
    error: null,
    totalCount: 0,
    currentPage: 1,
  })

  const setError = useCallback((error: string | null) => {
    setState((prev) => ({ ...prev, error }))
  }, [])

  const setLoading = useCallback((loading: boolean) => {
    setState((prev) => ({ ...prev, loading }))
  }, [])

  const getModerationQueue = useCallback(
    async (page: number = 1, pageSize: number = 10) => {
      setLoading(true)
      setError(null)
      try {
        const result = await moderationService.getModerationQueue(page, pageSize)
        setState((prev) => ({
          ...prev,
          queue: result.items,
          totalCount: result.totalCount,
          currentPage: page,
          error: null,
        }))
        return result
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to fetch moderation queue'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const getModerationHistory = useCallback(
    async (moderatorId?: string, page: number = 1, pageSize: number = 10) => {
      setLoading(true)
      setError(null)
      try {
        const result = await moderationService.getModerationHistory(moderatorId, page, pageSize)
        setState((prev) => ({
          ...prev,
          history: result.items,
          totalCount: result.totalCount,
          currentPage: page,
          error: null,
        }))
        return result
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to fetch moderation history'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const getModeratorActions = useCallback(
    async (moderatorId: string, page: number = 1, pageSize: number = 10) => {
      setLoading(true)
      setError(null)
      try {
        const result = await moderationService.getModeratorActions(moderatorId, page, pageSize)
        setState((prev) => ({
          ...prev,
          history: result.items,
          totalCount: result.totalCount,
          currentPage: page,
          error: null,
        }))
        return result
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to fetch moderator actions'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const bulkAction = useCallback(
    async (publicationIds: string[], actionType: 'approve' | 'reject' | 'under-review', comments: string, reason?: string) => {
      setLoading(true)
      setError(null)
      try {
        const result = await moderationService.bulkAction({
          publicationIds,
          actionType,
          reason,
          comments,
        })
        setState((prev) => ({
          ...prev,
          error: null,
        }))
        return result
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to perform bulk action'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const clearError = useCallback(() => {
    setError(null)
  }, [setError])

  return {
    ...state,
    getModerationQueue,
    getModerationHistory,
    getModeratorActions,
    bulkAction,
    clearError,
  }
}
