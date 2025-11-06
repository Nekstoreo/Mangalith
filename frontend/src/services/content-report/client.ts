import { apiClient } from '@/services/api/client'

export enum ContentReportCategory {
  Copyright = 0,
  InappropriateContent = 1,
  Spam = 2,
  Harassment = 3,
  Violence = 4,
  AdultContent = 5,
  Other = 6,
}

export enum ContentReportStatus {
  Pending = 0,
  Reviewed = 1,
  Resolved = 2,
  Dismissed = 3,
}

export interface ContentReport {
  id: string
  publicationId: string
  reporterId: string
  category: ContentReportCategory
  description: string
  status: ContentReportStatus
  createdAtUtc: string
  reviewedAtUtc?: string
  resolutionNotes?: string
}

export interface PagedContentReports {
  items: ContentReport[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface CreateReportRequest {
  publicationId: string
  category: ContentReportCategory
  description: string
}

export class ContentReportService {
  async createReport(request: CreateReportRequest): Promise<ContentReport> {
    const response = await apiClient.post<ContentReport>(`/api/content-report/create`, request)
    return response.data.data
  }

  async reviewReport(
    reportId: string,
    data: { status: ContentReportStatus; response?: string }
  ): Promise<ContentReport> {
    const response = await apiClient.post<ContentReport>(`/api/content-report/${reportId}/review`, data)
    return response.data.data
  }

  async getPendingReports(page: number = 1, pageSize: number = 10): Promise<PagedContentReports> {
    const response = await apiClient.get<PagedContentReports>(
      `/api/content-report/pending?page=${page}&pageSize=${pageSize}`
    )
    return response.data.data
  }

  async getReportsByPublication(
    publicationId: string,
    page: number = 1,
    pageSize: number = 10
  ): Promise<PagedContentReports> {
    const response = await apiClient.get<PagedContentReports>(
      `/api/content-report/publications/${publicationId}?page=${page}&pageSize=${pageSize}`
    )
    return response.data.data
  }

  async getUserReports(
    page: number = 1,
    pageSize: number = 10
  ): Promise<PagedContentReports> {
    const response = await apiClient.get<PagedContentReports>(
      `/api/content-report/my-reports?page=${page}&pageSize=${pageSize}`
    )
    return response.data.data
  }

  async getReportCountByPublication(publicationId: string): Promise<{ reportCount: number }> {
    const response = await apiClient.get<{ reportCount: number }>(
      `/api/content-report/publications/${publicationId}/count`
    )
    return response.data.data
  }
}

export const contentReportService = new ContentReportService()
