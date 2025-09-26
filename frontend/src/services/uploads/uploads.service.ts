import { apiClient } from '@/services/api/client'

export interface UploadFileResponse {
  id: string
  filename: string
  originalFilename: string
  fileSize: number
  mimeType: string
  path: string
  status: string
  createdAt: string
}

export interface UploadProgressEvent {
  loaded: number
  total: number
  percentage: number
}

export interface UploadOptions {
  title?: string
  description?: string
  onProgress?: (progress: UploadProgressEvent) => void
}

class UploadsService {
  private readonly allowedTypes = [
    'application/zip',
    'application/x-zip-compressed',
    'application/x-cbr',
    'application/x-cbz',
    'application/x-rar-compressed',
  ]

  private readonly allowedExtensions = ['.cbz', '.cbr', '.zip', '.rar']
  private readonly maxFileSize = 104857600 // 100MB

  async uploadFile(file: File, options: UploadOptions = {}): Promise<UploadFileResponse> {
    // Validaciones del lado cliente
    this.validateFile(file)

    const formData = new FormData()
    formData.append('file', file)
    
    if (options.title) {
      formData.append('title', options.title)
    }
    
    if (options.description) {
      formData.append('description', options.description)
    }

    try {
      const response = await apiClient.post(
        '/uploads',
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
          onUploadProgress: (progressEvent) => {
            if (options.onProgress && progressEvent.total) {
              const percentage = Math.round((progressEvent.loaded * 100) / progressEvent.total)
              options.onProgress({
                loaded: progressEvent.loaded,
                total: progressEvent.total,
                percentage,
              })
            }
          },
        }
      )

      if (response.data.success) {
        return response.data.data as UploadFileResponse
      } else {
        throw new Error(response.data.message || 'Upload failed')
      }
    } catch (error) {
      console.error('Upload error:', error)
      throw error
    }
  }

  async getFile(fileId: string): Promise<UploadFileResponse> {
    try {
      const response = await apiClient.get(`/uploads/${fileId}`)
      
      if (response.data.success) {
        return response.data.data as UploadFileResponse
      } else {
        throw new Error(response.data.message || 'Failed to get file')
      }
    } catch (error) {
      console.error('Get file error:', error)
      throw error
    }
  }

  async deleteFile(fileId: string): Promise<void> {
    try {
      await apiClient.delete(`/uploads/${fileId}`)
    } catch (error) {
      console.error('Delete file error:', error)
      throw error
    }
  }

  validateFile(file: File): void {
    // Validar tamaño
    if (file.size > this.maxFileSize) {
      throw new Error(`El archivo es demasiado grande. El tamaño máximo es ${this.maxFileSize / 1024 / 1024}MB`)
    }

    // Validar tipo MIME
    if (!this.allowedTypes.includes(file.type)) {
      throw new Error('Tipo de archivo no válido. Solo se permiten archivos CBZ, CBR, ZIP y RAR')
    }

    // Validar extensión
    const fileExt = file.name.toLowerCase().substring(file.name.lastIndexOf('.'))
    if (!this.allowedExtensions.includes(fileExt)) {
      throw new Error(`Extensión de archivo no válida. Extensiones permitidas: ${this.allowedExtensions.join(', ')}`)
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes'
    
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  getFileIcon(filename: string): string {
    const ext = filename.toLowerCase().substring(filename.lastIndexOf('.'))
    
    switch (ext) {
      case '.cbz':
      case '.zip':
        return '📦'
      case '.cbr':
      case '.rar':
        return '📚'
      default:
        return '📄'
    }
  }
}

export const uploadsService = new UploadsService()
export default uploadsService