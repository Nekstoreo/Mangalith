import { apiClient } from '@/services/api/client'
import { 
  User, 
  LoginRequest, 
  RegisterRequest, 
  AuthResponse, 
  ProfileUpdateRequest, 
  ChangePasswordRequest,
  ApiResponse 
} from '@/lib/types'

export class AuthService {
  // Login user
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    try {
      const response = await apiClient.post<AuthResponse>('/auth/login', credentials)

      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Login failed')
      }
    } catch (error) {
      console.error('Login error:', error)
      throw error
    }
  }

  // Register user
  async register(userData: RegisterRequest): Promise<AuthResponse> {
    try {
      const response = await apiClient.post<AuthResponse>('/auth/register', userData)

      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Registration failed')
      }
    } catch (error) {
      console.error('Registration error:', error)
      throw error
    }
  }

  // Get current user profile
  async getProfile(): Promise<User> {
    try {
      const response = await apiClient.get<User>('/auth/profile')

      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to get profile')
      }
    } catch (error) {
      console.error('Get profile error:', error)
      throw error
    }
  }

  // Update user profile
  async updateProfile(updates: ProfileUpdateRequest): Promise<User> {
    try {
      const response = await apiClient.put<User>('/auth/profile', updates)

      if (response.data.success) {
        return response.data.data
      } else {
        throw new Error(response.data.message || 'Failed to update profile')
      }
    } catch (error) {
      console.error('Update profile error:', error)
      throw error
    }
  }

  // Change password
  async changePassword(data: ChangePasswordRequest): Promise<void> {
    try {
      const response = await apiClient.post('/auth/change-password', data)

      if (!response.data.success) {
        throw new Error(response.data.message || 'Failed to change password')
      }
    } catch (error) {
      console.error('Change password error:', error)
      throw error
    }
  }

  // Logout (client-side only)
  logout(): void {
    // This just clears the client-side state
    // The server-side logout should be handled by the backend
    // when the token expires or is explicitly invalidated
  }
}

// Export singleton instance
export const authService = new AuthService()
