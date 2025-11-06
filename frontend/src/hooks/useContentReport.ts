'use client'

import { useState, useCallback } from 'react'
import {
  contentReportService,
  ContentReport,
  PagedContentReports,
  CreateReportRequest,
  ContentReportStatus,
} from '@/services/content-report/client'

export interface UseContentReportState {
  reports: ContentReport[]
  currentReport: ContentReport | null
  loading: boolean
  error: string | null
  totalCount: number
  currentPage: number
  pendingCount: number
}

export const useContentReport = () => {
  const [state, setState] = useState<UseContentReportState>({
    reports: [],
    currentReport: null,
    loading: false,
    error: null,
    totalCount: 0,
    currentPage: 1,
    pendingCount: 0,
  })

  const setError = useCallback((error: string | null) => {
    setState((prev) => ({ ...prev, error }))
  }, [])

  const setLoading = useCallback((loading: boolean) => {
    setState((prev) => ({ ...prev, loading }))
  }, [])

  const createReport = useCallback(
    async (request: CreateReportRequest) => {
      setLoading(true)
      setError(null)
      try {
        const report = await contentReportService.createReport(request)
        setState((prev) => ({ ...prev, currentReport: report, error: null }))
        return report
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to create report'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const reviewReport = useCallback(
    async (reportId: string, status: ContentReportStatus, response?: string) => {
      setLoading(true)
      setError(null)
      try {
        const report = await contentReportService.reviewReport(reportId, {
          status,
          response,
        })
        setState((prev) => ({
          ...prev,
          currentReport: report,
          error: null,
          reports: prev.reports.map((r) => (r.id === reportId ? report : r)),
        }))
        return report
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to review report'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const getPendingReports = useCallback(
    async (page: number = 1, pageSize: number = 10) => {
      setLoading(true)
      setError(null)
      try {
        const result = await contentReportService.getPendingReports(page, pageSize)
        setState((prev) => ({
          ...prev,
          reports: result.items,
          totalCount: result.totalCount,
          currentPage: page,
          error: null,
        }))
        return result
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to fetch pending reports'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const getReportsByPublication = useCallback(
    async (publicationId: string, page: number = 1, pageSize: number = 10) => {
      setLoading(true)
      setError(null)
      try {
        const result = await contentReportService.getReportsByPublication(publicationId, page, pageSize)
        setState((prev) => ({
          ...prev,
          reports: result.items,
          totalCount: result.totalCount,
          currentPage: page,
          error: null,
        }))
        return result
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to fetch reports'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const getUserReports = useCallback(
    async (page: number = 1, pageSize: number = 10) => {
      setLoading(true)
      setError(null)
      try {
        const result = await contentReportService.getUserReports(page, pageSize)
        setState((prev) => ({
          ...prev,
          reports: result.items,
          totalCount: result.totalCount,
          currentPage: page,
          error: null,
        }))
        return result
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to fetch user reports'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const getReportCountByPublication = useCallback(async (publicationId: string) => {
    try {
      const result = await contentReportService.getReportCountByPublication(publicationId)
      return result.reportCount
    } catch (err) {
      console.error('Failed to fetch report count:', err)
      throw err
    }
  }, [])

  const clearError = useCallback(() => {
    setError(null)
  }, [setError])

  return {
    ...state,
    createReport,
    reviewReport,
    getPendingReports,
    getReportsByPublication,
    getUserReports,
    getReportCountByPublication,
    clearError,
  }
}
