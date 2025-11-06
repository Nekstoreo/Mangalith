import { apiClient } from '@/services/api/client'

export interface Manga {
  id: string
  title: string
  alternativeTitle?: string
  description?: string
  author?: string
  artist?: string
  year?: number
  status: string
  coverImagePath?: string
  tags?: string
  genres?: string
  chapterCount: number
  viewCount: number
  rating: number
  ratingCount: number
  createdAtUtc: string
  updatedAtUtc: string
  publicationStatus?: string
  contentRating?: string
  isNsfw: boolean
}

export interface MangaDetail extends Manga {
  createdByUserId: string
  chapters: Chapter[]
  publication?: Publication
}

export interface Chapter {
  id: string
  title: string
  number: number
  volumeNumber?: number
  pageCount: number
  createdAtUtc: string
}

export interface Publication {
  id: string
  status: string
  contentRating: string
  isNsfw: boolean
  moderatorComments?: string
  rejectionReason?: string
  submittedAtUtc?: string
  reviewedAtUtc?: string
  createdAtUtc: string
}

export interface CreateMangaRequest {
  title: string
  description?: string
}

export interface UpdateMangaRequest {
  title: string
  alternativeTitle?: string
  description?: string
  author?: string
  artist?: string
  year?: number
}

export class MangaService {
  /**
   * Obtiene todos los mangas públicos
   */
  async getPublicMangas(): Promise<Manga[]> {
    const response = await apiClient.get<Manga[]>('/manga/public')
    return response.data as Manga[]
  }

  /**
   * Busca mangas públicos
   */
  async searchPublicMangas(searchTerm: string): Promise<Manga[]> {
    const response = await apiClient.get<Manga[]>('/manga/search', {
      params: { q: searchTerm }
    })
    return response.data as Manga[]
  }

  /**
   * Obtiene un manga por ID
   */
  async getMangaById(id: string): Promise<MangaDetail> {
    const response = await apiClient.get<MangaDetail>(`/manga/${id}`)
    return response.data as MangaDetail
  }

  /**
   * Obtiene los mangas del usuario actual
   */
  async getMyMangas(): Promise<Manga[]> {
    const response = await apiClient.get<Manga[]>('/manga/my-mangas')
    return response.data as Manga[]
  }

  /**
   * Crea un nuevo manga
   */
  async createManga(request: CreateMangaRequest): Promise<{ id: string; title: string; status: string }> {
    const response = await apiClient.post<{ id: string; title: string; status: string }>('/manga', request)
    return response.data as { id: string; title: string; status: string }
  }

  /**
   * Actualiza un manga existente
   */
  async updateManga(id: string, request: UpdateMangaRequest): Promise<{ id: string; title: string; updatedAtUtc: string }> {
    const response = await apiClient.put<{ id: string; title: string; updatedAtUtc: string }>(`/manga/${id}`, request)
    return response.data as { id: string; title: string; updatedAtUtc: string }
  }

  /**
   * Elimina un manga
   */
  async deleteManga(id: string): Promise<void> {
    await apiClient.delete(`/manga/${id}`)
  }
}

export const mangaService = new MangaService()