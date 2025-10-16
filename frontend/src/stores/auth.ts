import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { User, UserRole, PermissionString } from '@/lib/types'

interface AuthState {
  user: User | null
  token: string | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
}

interface AuthActions {
  login: (user: User, token: string) => void
  logout: () => void
  setLoading: (loading: boolean) => void
  setError: (error: string | null) => void
  updateUser: (updates: Partial<User>) => void
  clearError: () => void
  hasPermission: (permission: PermissionString) => boolean
  hasRole: (role: UserRole) => boolean
  hasAnyPermission: (permissions: PermissionString[]) => boolean
  hasAllPermissions: (permissions: PermissionString[]) => boolean
  updatePermissions: (permissions: string[]) => void
}

export const useAuthStore = create<AuthState & AuthActions>()(
  persist(
    (set, get) => ({
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,

      login: (user: User, token: string) => {
        set({
          user,
          token,
          isAuthenticated: true,
          error: null,
        })
      },

      logout: () => {
        set({
          user: null,
          token: null,
          isAuthenticated: false,
          error: null,
        })
      },

      setLoading: (isLoading: boolean) => {
        set({ isLoading })
      },

      setError: (error: string | null) => {
        set({ error })
      },

      updateUser: (updates: Partial<User>) => {
        const currentUser = get().user
        if (currentUser) {
          set({
            user: { ...currentUser, ...updates }
          })
        }
      },

      clearError: () => {
        set({ error: null })
      },

      hasPermission: (permission: PermissionString) => {
        const currentUser = get().user
        return currentUser?.permissions?.includes(permission) ?? false
      },

      hasRole: (role: UserRole) => {
        const currentUser = get().user
        return currentUser ? currentUser.role >= role : false
      },

      hasAnyPermission: (permissions: PermissionString[]) => {
        const currentUser = get().user
        if (!currentUser?.permissions) return false
        return permissions.some(permission => currentUser.permissions.includes(permission))
      },

      hasAllPermissions: (permissions: PermissionString[]) => {
        const currentUser = get().user
        if (!currentUser?.permissions) return false
        return permissions.every(permission => currentUser.permissions.includes(permission))
      },

      updatePermissions: (permissions: string[]) => {
        const currentUser = get().user
        if (currentUser) {
          set({
            user: { ...currentUser, permissions }
          })
        }
      },
    }),
    {
      name: 'mangalith-auth',
      partialize: (state) => ({
        user: state.user,
        token: state.token,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
)
