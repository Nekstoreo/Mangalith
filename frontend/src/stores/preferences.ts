import { create } from 'zustand'
import { persist } from 'zustand/middleware'

export interface UserPreferences {
  theme: 'light' | 'dark' | 'system'
  language: string
  readingMode: 'single' | 'double' | 'webtoon' | 'continuous'
  autoAdvance: boolean
  pageFit: 'width' | 'height' | 'original' | 'auto'
  backgroundColor: string
  showProgress: boolean
  enableNotifications: boolean
  libraryView: 'grid' | 'list'
  sortBy: 'title' | 'lastRead' | 'dateAdded' | 'rating'
  sortOrder: 'asc' | 'desc'
  itemsPerPage: number
}

interface PreferencesState {
  preferences: UserPreferences
}

interface PreferencesActions {
  updatePreferences: (updates: Partial<UserPreferences>) => void
  resetPreferences: () => void
  setTheme: (theme: UserPreferences['theme']) => void
  setReadingMode: (mode: UserPreferences['readingMode']) => void
  setLibraryView: (view: UserPreferences['libraryView']) => void
}

const defaultPreferences: UserPreferences = {
  theme: 'system',
  language: 'es',
  readingMode: 'single',
  autoAdvance: false,
  pageFit: 'width',
  backgroundColor: '#ffffff',
  showProgress: true,
  enableNotifications: true,
  libraryView: 'grid',
  sortBy: 'title',
  sortOrder: 'asc',
  itemsPerPage: 20,
}

export const usePreferencesStore = create<PreferencesState & PreferencesActions>()(
  persist(
    (set, get) => ({
      preferences: defaultPreferences,

      updatePreferences: (updates: Partial<UserPreferences>) => {
        const currentPreferences = get().preferences
        set({
          preferences: { ...currentPreferences, ...updates }
        })
      },

      resetPreferences: () => {
        set({ preferences: defaultPreferences })
      },

      setTheme: (theme: UserPreferences['theme']) => {
        const currentPreferences = get().preferences
        set({
          preferences: { ...currentPreferences, theme }
        })
      },

      setReadingMode: (readingMode: UserPreferences['readingMode']) => {
        const currentPreferences = get().preferences
        set({
          preferences: { ...currentPreferences, readingMode }
        })
      },

      setLibraryView: (libraryView: UserPreferences['libraryView']) => {
        const currentPreferences = get().preferences
        set({
          preferences: { ...currentPreferences, libraryView }
        })
      },
    }),
    {
      name: 'mangalith-preferences',
    }
  )
)
