"use client"

import { useState } from 'react'
import { FileUpload } from '@/components/uploads'
import { type UploadFileResponse } from '@/services/uploads/uploads.service'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { CheckCircle2, AlertCircle } from 'lucide-react'

export default function UploadPage() {
  const [uploadedFiles, setUploadedFiles] = useState<UploadFileResponse[]>([])
  const [error, setError] = useState<string | null>(null)

  const handleUploadComplete = (files: UploadFileResponse[]) => {
    setUploadedFiles(prev => [...prev, ...files])
    setError(null)
  }

  const handleUploadError = (errorMessage: string) => {
    setError(errorMessage)
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="max-w-4xl mx-auto">
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-2">Subir Archivos de Manga</h1>
          <p className="text-muted-foreground">
            Sube tus archivos de manga en formato CBZ, CBR, ZIP o RAR para empezar a construir tu biblioteca personal.
          </p>
        </div>

        <div className="grid gap-8 md:grid-cols-2">
          {/* Upload component */}
          <div>
            <FileUpload
              onUploadComplete={handleUploadComplete}
              onUploadError={handleUploadError}
              maxFiles={10}
            />
          </div>

          {/* Results panel */}
          <div className="space-y-4">
            {error && (
              <Alert variant="destructive">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            )}

            {uploadedFiles.length > 0 && (
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <CheckCircle2 className="h-5 w-5 text-green-500" />
                    Archivos Subidos ({uploadedFiles.length})
                  </CardTitle>
                  <CardDescription>
                    Archivos que se han subido exitosamente al servidor
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="space-y-3">
                    {uploadedFiles.map((file) => (
                      <div
                        key={file.id}
                        className="flex items-center justify-between p-3 border rounded-lg"
                      >
                        <div className="flex-1">
                          <p className="font-medium">{file.originalFilename}</p>
                          <p className="text-sm text-muted-foreground">
                            {(file.fileSize / 1024 / 1024).toFixed(2)} MB • {file.mimeType}
                          </p>
                          <p className="text-xs text-green-600">
                            Estado: {file.status}
                          </p>
                        </div>
                        <div className="text-right text-sm text-muted-foreground">
                          <p>ID: {file.id.slice(0, 8)}...</p>
                          <p>{new Date(file.createdAt).toLocaleString()}</p>
                        </div>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            )}

            {uploadedFiles.length === 0 && !error && (
              <Card>
                <CardContent className="pt-6">
                  <div className="text-center text-muted-foreground">
                    <p>No se han subido archivos aún.</p>
                    <p className="text-sm">Los archivos subidos aparecerán aquí.</p>
                  </div>
                </CardContent>
              </Card>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}