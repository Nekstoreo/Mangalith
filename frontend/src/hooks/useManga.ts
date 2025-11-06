'use client'

import { useState, useCallback } from 'react'
import { mangaService, Manga, MangaDetail, CreateMangaRequest, UpdateMangaRequest } from '@/services/manga/client'

export function useManga() {
  const [mangas, setMangas] = useState<Manga[]>([])
  const [currentManga, setCurrentManga] = useState<MangaDetail | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const clearError = useCallback(() => {
    setError(null)
  }, [])

  const getPublicMangas = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await mangaService.getPublicMangas()
      setMangas(data)
      return data
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al cargar mangas públicos'
      setError(errorMessage)
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  const searchPublicMangas = useCallback(async (searchTerm: string) => {
    try {
      setLoading(true)
      setError(null)
      const data = await mangaService.searchPublicMangas(searchTerm)
      setMangas(data)
      return data
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al buscar mangas'
      setError(errorMessage)
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  const getMangaById = useCallback(async (id: string) => {
    try {
      setLoading(true)
      setError(null)
      const data = await mangaService.getMangaById(id)
      setCurrentManga(data)
      return data
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al cargar manga'
      setError(errorMessage)
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  const getMyMangas = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await mangaService.getMyMangas()
      setMangas(data)
      return data
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al cargar tus mangas'
      setError(errorMessage)
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  const createManga = useCallback(async (request: CreateMangaRequest) => {
    try {
      setLoading(true)
      setError(null)
      const result = await mangaService.createManga(request)
      // Refresh the manga list
      await getMyMangas()
      return result
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al crear manga'
      setError(errorMessage)
      throw err
    } finally {
      setLoading(false)
    }
  }, [getMyMangas])

  const updateManga = useCallback(async (id: string, request: UpdateMangaRequest) => {
    try {
      setLoading(true)
      setError(null)
      const result = await mangaService.updateManga(id, request)
      
      // Update the current manga if it's the one being updated
      if (currentManga?.id === id) {
        const updatedManga = await mangaService.getMangaById(id)
        setCurrentManga(updatedManga)
      }
      
      // Update the manga in the list
      setMangas(prev => prev.map(manga => 
        manga.id === id 
          ? { ...manga, title: request.title, updatedAtUtc: result.updatedAtUtc }
          : manga
      ))
      
      return result
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al actualizar manga'
      setError(errorMessage)
      throw err
    } finally {
      setLoading(false)
    }
  }, [currentManga])

  const deleteManga = useCallback(async (id: string) => {
    try {
      setLoading(true)
      setError(null)
      await mangaService.deleteManga(id)
      
      // Remove from the list
      setMangas(prev => prev.filter(manga => manga.id !== id))
      
      // Clear current manga if it's the one being deleted
      if (currentManga?.id === id) {
        setCurrentManga(null)
      }
      
      return true
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al eliminar manga'
      setError(errorMessage)
      throw err
    } finally {
      setLoading(false)
    }
  }, [currentManga])

  return {
    mangas,
    currentManga,
    loading,
    error,
    clearError,
    getPublicMangas,
    searchPublicMangas,
    getMangaById,
    getMyMangas,
    createManga,
    updateManga,
    deleteManga,
  }
}