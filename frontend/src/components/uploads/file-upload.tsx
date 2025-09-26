"use client"

import { useState, useCallback } from 'react'
import { useDropzone } from 'react-dropzone'
import { Upload, File, X, CheckCircle2, AlertCircle, Loader2 } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Progress } from '@/components/ui/progress'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'

import { uploadsService, type UploadFileResponse, type UploadProgressEvent } from '@/services/uploads/uploads.service'
import { cn } from '@/lib/utils'

interface UploadedFile {
  file: File
  id?: string
  status: 'pending' | 'uploading' | 'success' | 'error'
  progress: number
  error?: string
  response?: UploadFileResponse
}

interface FileUploadProps {
  onUploadComplete?: (files: UploadFileResponse[]) => void
  onUploadError?: (error: string) => void
  maxFiles?: number
  className?: string
}

export function FileUpload({
  onUploadComplete,
  onUploadError,
  maxFiles = 5,
  className
}: FileUploadProps) {
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([])
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [isUploading, setIsUploading] = useState(false)

  const onDrop = useCallback((acceptedFiles: File[]) => {
    const newFiles: UploadedFile[] = acceptedFiles.map(file => ({
      file,
      status: 'pending',
      progress: 0
    }))

    setUploadedFiles(prev => [...prev, ...newFiles].slice(0, maxFiles))
  }, [maxFiles])

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'application/zip': ['.zip'],
      'application/x-zip-compressed': ['.zip'],
      'application/x-cbr': ['.cbr'],
      'application/x-cbz': ['.cbz'],
      'application/x-rar-compressed': ['.rar'],
    },
    maxFiles,
    maxSize: 104857600, // 100MB
    disabled: isUploading
  })

  const uploadFile = async (fileIndex: number) => {
    const uploadedFile = uploadedFiles[fileIndex]
    if (!uploadedFile || uploadedFile.status !== 'pending') return

    setUploadedFiles(prev => prev.map((f, i) => 
      i === fileIndex ? { ...f, status: 'uploading' } : f
    ))

    try {
      const response = await uploadsService.uploadFile(uploadedFile.file, {
        title: title || uploadedFile.file.name,
        description,
        onProgress: (progress: UploadProgressEvent) => {
          setUploadedFiles(prev => prev.map((f, i) => 
            i === fileIndex ? { ...f, progress: progress.percentage } : f
          ))
        }
      })

      setUploadedFiles(prev => prev.map((f, i) => 
        i === fileIndex ? { 
          ...f, 
          status: 'success', 
          progress: 100,
          id: response.id,
          response 
        } : f
      ))

    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Upload failed'
      
      setUploadedFiles(prev => prev.map((f, i) => 
        i === fileIndex ? { 
          ...f, 
          status: 'error', 
          error: errorMessage 
        } : f
      ))

      onUploadError?.(errorMessage)
    }
  }

  const uploadAllFiles = async () => {
    setIsUploading(true)
    
    try {
      const pendingFiles = uploadedFiles
        .map((file, index) => ({ file, index }))
        .filter(({ file }) => file.status === 'pending')

      await Promise.all(pendingFiles.map(({ index }) => uploadFile(index)))

      const successfulFiles = uploadedFiles
        .filter(f => f.status === 'success' && f.response)
        .map(f => f.response!)

      if (successfulFiles.length > 0) {
        onUploadComplete?.(successfulFiles)
      }
    } finally {
      setIsUploading(false)
    }
  }

  const removeFile = (index: number) => {
    setUploadedFiles(prev => prev.filter((_, i) => i !== index))
  }

  const clearAll = () => {
    setUploadedFiles([])
    setTitle('')
    setDescription('')
  }

  const hasFiles = uploadedFiles.length > 0
  const hasPendingFiles = uploadedFiles.some(f => f.status === 'pending')
  const hasSuccessfulFiles = uploadedFiles.some(f => f.status === 'success')

  return (
    <Card className={cn("w-full max-w-2xl", className)}>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Upload className="h-5 w-5" />
          Subir Archivos de Manga
        </CardTitle>
        <CardDescription>
          Sube archivos CBZ, CBR, ZIP o RAR con tu contenido de manga. Máximo {maxFiles} archivos, 100MB cada uno.
        </CardDescription>
      </CardHeader>
      
      <CardContent className="space-y-6">
        {/* Metadata inputs */}
        <div className="space-y-4">
          <div>
            <Label htmlFor="title">Título (opcional)</Label>
            <Input
              id="title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Nombre del manga o serie"
              disabled={isUploading}
            />
          </div>
          
          <div>
            <Label htmlFor="description">Descripción (opcional)</Label>
            <Textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Descripción del contenido..."
              rows={3}
              disabled={isUploading}
            />
          </div>
        </div>

        {/* Dropzone */}
        <div
          {...getRootProps()}
          className={cn(
            "border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors",
            isDragActive ? "border-primary bg-primary/10" : "border-border hover:border-primary/50",
            isUploading && "cursor-not-allowed opacity-50"
          )}
        >
          <input {...getInputProps()} />
          <Upload className="h-10 w-10 mx-auto mb-4 text-muted-foreground" />
          
          {isDragActive ? (
            <p className="text-primary font-medium">Suelta los archivos aquí...</p>
          ) : (
            <div>
              <p className="font-medium mb-2">
                Arrastra archivos aquí o haz clic para seleccionar
              </p>
              <p className="text-sm text-muted-foreground">
                Formatos soportados: CBZ, CBR, ZIP, RAR (máx. 100MB)
              </p>
            </div>
          )}
        </div>

        {/* File list */}
        {hasFiles && (
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <h3 className="font-medium">Archivos ({uploadedFiles.length})</h3>
              <Button
                variant="ghost"
                size="sm"
                onClick={clearAll}
                disabled={isUploading}
              >
                Limpiar todo
              </Button>
            </div>

            <div className="space-y-2">
              {uploadedFiles.map((uploadedFile, index) => (
                <div
                  key={`${uploadedFile.file.name}-${index}`}
                  className="flex items-center gap-3 p-3 border rounded-lg"
                >
                  <div className="flex-shrink-0">
                    {uploadedFile.status === 'success' && (
                      <CheckCircle2 className="h-5 w-5 text-green-500" />
                    )}
                    {uploadedFile.status === 'error' && (
                      <AlertCircle className="h-5 w-5 text-red-500" />
                    )}
                    {uploadedFile.status === 'uploading' && (
                      <Loader2 className="h-5 w-5 animate-spin text-primary" />
                    )}
                    {uploadedFile.status === 'pending' && (
                      <File className="h-5 w-5 text-muted-foreground" />
                    )}
                  </div>

                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between mb-1">
                      <p className="font-medium truncate">{uploadedFile.file.name}</p>
                      <span className="text-sm text-muted-foreground">
                        {uploadsService.formatFileSize(uploadedFile.file.size)}
                      </span>
                    </div>

                    {uploadedFile.status === 'uploading' && (
                      <Progress value={uploadedFile.progress} className="h-2" />
                    )}

                    {uploadedFile.status === 'error' && uploadedFile.error && (
                      <Alert variant="destructive" className="mt-2">
                        <AlertDescription className="text-sm">
                          {uploadedFile.error}
                        </AlertDescription>
                      </Alert>
                    )}
                  </div>

                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => removeFile(index)}
                    disabled={isUploading}
                  >
                    <X className="h-4 w-4" />
                  </Button>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Upload button */}
        {hasPendingFiles && (
          <div className="flex gap-2">
            <Button
              onClick={uploadAllFiles}
              disabled={isUploading}
              className="flex-1"
            >
              {isUploading ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Subiendo...
                </>
              ) : (
                <>
                  <Upload className="h-4 w-4 mr-2" />
                  Subir {uploadedFiles.filter(f => f.status === 'pending').length} archivo(s)
                </>
              )}
            </Button>
          </div>
        )}

        {/* Success message */}
        {hasSuccessfulFiles && !hasPendingFiles && !isUploading && (
          <Alert>
            <CheckCircle2 className="h-4 w-4" />
            <AlertDescription>
              ¡Archivos subidos exitosamente! Los archivos están siendo procesados.
            </AlertDescription>
          </Alert>
        )}
      </CardContent>
    </Card>
  )
}