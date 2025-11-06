'use client'

import { useState, useCallback } from 'react'
import { publicationService, Publication, PagedPublications, PublicationStatus, ContentRating } from '@/services/publication/client'

export interface UsePublicationState {
  publication: Publication | null
  publications: Publication[]
  loading: boolean
  error: string | null
  totalCount: number
  currentPage: number
}

export const usePublication = () => {
  const [state, setState] = useState<UsePublicationState>({
    publication: null,
    publications: [],
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

  const createPublication = useCallback(
    async (mangaId: string) => {
      setLoading(true)
      setError(null)
      try {
        const publication = await publicationService.createPublication(mangaId)
        setState((prev) => ({ ...prev, publication, error: null }))
        return publication
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to create publication'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const submitForReview = useCallback(
    async (publicationId: string) => {
      setLoading(true)
      setError(null)
      try {
        const publication = await publicationService.submitForReview(publicationId)
        setState((prev) => ({ ...prev, publication, error: null }))
        return publication
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to submit for review'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const approvePublication = useCallback(
    async (publicationId: string, data: { contentRating: ContentRating; isNsfw: boolean; comments?: string }) => {
      setLoading(true)
      setError(null)
      try {
        const publication = await publicationService.approvePublication(publicationId, data)
        setState((prev) => ({ ...prev, publication, error: null }))
        return publication
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to approve publication'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const rejectPublication = useCallback(
    async (publicationId: string, data: { reason: string; comments: string }) => {
      setLoading(true)
      setError(null)
      try {
        const publication = await publicationService.rejectPublication(publicationId, data)
        setState((prev) => ({ ...prev, publication, error: null }))
        return publication
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to reject publication'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const requestRevision = useCallback(
    async (publicationId: string, comments: string) => {
      setLoading(true)
      setError(null)
      try {
        const publication = await publicationService.requestRevision(publicationId, comments)
        setState((prev) => ({ ...prev, publication, error: null }))
        return publication
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to request revision'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const archivePublication = useCallback(
    async (publicationId: string) => {
      setLoading(true)
      setError(null)
      try {
        const publication = await publicationService.archivePublication(publicationId)
        setState((prev) => ({ ...prev, publication, error: null }))
        return publication
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to archive publication'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const getPublicationById = useCallback(
    async (publicationId: string) => {
      setLoading(true)
      setError(null)
      try {
        const publication = await publicationService.getPublicationById(publicationId)
        setState((prev) => ({ ...prev, publication, error: null }))
        return publication
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to fetch publication'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const getMyPublications = useCallback(
    async (page: number = 1, pageSize: number = 10) => {
      setLoading(true)
      setError(null)
      try {
        const result = await publicationService.getMyPublications(page, pageSize)
        setState((prev) => ({
          ...prev,
          publications: result.items,
          totalCount: result.totalCount,
          currentPage: page,
          error: null,
        }))
        return result
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to fetch your publications'
        setError(errorMsg)
        throw err
      } finally {
        setLoading(false)
      }
    },
    []
  )

  const getPublicationsByStatus = useCallback(
    async (status: PublicationStatus, page: number = 1, pageSize: number = 10) => {
      setLoading(true)
      setError(null)
      try {
        const result = await publicationService.getPublicationsByStatus(status, page, pageSize)
        setState((prev) => ({
          ...prev,
          publications: result.items,
          totalCount: result.totalCount,
          currentPage: page,
          error: null,
        }))
        return result
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : 'Failed to fetch publications'
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
    createPublication,
    submitForReview,
    approvePublication,
    rejectPublication,
    requestRevision,
    archivePublication,
    getPublicationById,
    getMyPublications,
    getPublicationsByStatus,
    clearError,
  }
}
