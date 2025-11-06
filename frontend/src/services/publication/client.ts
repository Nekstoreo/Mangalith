import { apiClient } from '@/services/api/client'

export interface Publication {
  id: string
  mangaId: string
  status: PublicationStatus
  contentRating: ContentRating
  isNsfw: boolean
  createdAtUtc: string
  submittedAtUtc?: string
  reviewedAtUtc?: string
  moderatorComments?: string
  rejectionReason?: string
}

export interface PagedPublications {
  items: Publication[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export enum PublicationStatus {
  Draft = 0,
  InReview = 1,
  NeedsRevision = 2,
  Published = 3,
  Rejected = 4,
  Archived = 5,
  UnderReview = 6,
}

export enum ContentRating {
  General = 0,
  Teen = 1,
  Mature = 2,
  Adult = 3,
}

export class PublicationService {
  async createPublication(mangaId: string): Promise<Publication> {
    const response = await apiClient.post<Publication>(`/api/publication/${mangaId}/create`, {})
    return response.data.data
  }

  async submitForReview(publicationId: string): Promise<Publication> {
    const response = await apiClient.post<Publication>(`/api/publication/${publicationId}/submit`, {})
    return response.data.data
  }

  async approvePublication(
    publicationId: string,
    data: { contentRating: ContentRating; isNsfw: boolean; comments?: string }
  ): Promise<Publication> {
    const response = await apiClient.post<Publication>(`/api/publication/${publicationId}/approve`, data)
    return response.data.data
  }

  async rejectPublication(
    publicationId: string,
    data: { reason: string; comments: string }
  ): Promise<Publication> {
    const response = await apiClient.post<Publication>(`/api/publication/${publicationId}/reject`, data)
    return response.data.data
  }

  async requestRevision(publicationId: string, comments: string): Promise<Publication> {
    const response = await apiClient.post<Publication>(`/api/publication/${publicationId}/request-revision`, {
      comments,
    })
    return response.data.data
  }

  async archivePublication(publicationId: string): Promise<Publication> {
    const response = await apiClient.post<Publication>(`/api/publication/${publicationId}/archive`, {})
    return response.data.data
  }

  async getPublicationById(publicationId: string): Promise<Publication> {
    const response = await apiClient.get<Publication>(`/api/publication/${publicationId}`)
    return response.data.data
  }

  async getMyPublications(page: number = 1, pageSize: number = 10): Promise<PagedPublications> {
    const response = await apiClient.get<PagedPublications>(
      `/api/publication/my-publications?page=${page}&pageSize=${pageSize}`
    )
    return response.data.data
  }

  async getPublicationsByStatus(
    status: PublicationStatus,
    page: number = 1,
    pageSize: number = 10
  ): Promise<PagedPublications> {
    const response = await apiClient.get<PagedPublications>(
      `/api/publication/by-status/${status}?page=${page}&pageSize=${pageSize}`
    )
    return response.data.data
  }
}

export const publicationService = new PublicationService()
