'use client'

import React, { useEffect, useState } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Progress } from '@/components/ui/progress'
import { Badge } from '@/components/ui/badge'
import { LoadingSpinner } from '@/components/ui/loading-spinner'
import { 
  Upload, 
  FileText, 
  TrendingUp, 
  Clock, 
  HardDrive,
  Eye,
  Download,
  AlertCircle,
  CheckCircle,
  Plus
} from 'lucide-react'
import Link from 'next/link'
import { useApiData } from '@/hooks'

interface UploaderStats {
  totalUploads: number
  totalViews: number
  totalDownloads: number
  storageUsed: number
  storageLimit: number
  pendingProcessing: number
  recentUploads: Array<{
    id: string
    title: string
    status: 'processing' | 'completed' | 'failed'
    uploadedAt: string
    views: number
    size: number
  }>
  popularContent: Array<{
    id: string
    title: string
    views: number
    downloads: number
    rating: number
  }>
  monthlyStats: {
    uploads: number
    views: number
    downloads: number
  }
}

export const UploaderDashboard: React.FC = () => {
  const { data: stats, isLoading, error } = useApiData<UploaderStats>('/dashboard/uploader')

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'completed':
        return 'bg-green-100 text-green-800'
      case 'processing':
        return 'bg-yellow-100 text-yellow-800'
      case 'failed':
        return 'bg-red-100 text-red-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'completed':
        return <CheckCircle className="h-4 w-4" />
      case 'processing':
        return <Clock className="h-4 w-4" />
      case 'failed':
        return <AlertCircle className="h-4 w-4" />
      default:
        return <FileText className="h-4 w-4" />
    }
  }

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <LoadingSpinner size="lg" />
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600 mb-4">Error al cargar las estadísticas del uploader</p>
        <Button onClick={() => window.location.reload()}>Reintentar</Button>
      </div>
    )
  }

  const storagePercentage = stats ? (stats.storageUsed / stats.storageLimit) * 100 : 0

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Panel de Uploader</h1>
          <p className="text-muted-foreground">Gestiona tu contenido y revisa tus estadísticas</p>
        </div>
        <Link href="/upload">
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            Subir Nuevo Manga
          </Button>
        </Link>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Subidas</CardTitle>
            <Upload className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.totalUploads || 0}</div>
            <p className="text-xs text-muted-foreground">
              +{stats?.monthlyStats.uploads || 0} este mes
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Visualizaciones</CardTitle>
            <Eye className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.totalViews || 0}</div>
            <p className="text-xs text-muted-foreground">
              +{stats?.monthlyStats.views || 0} este mes
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Descargas</CardTitle>
            <Download className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.totalDownloads || 0}</div>
            <p className="text-xs text-muted-foreground">
              +{stats?.monthlyStats.downloads || 0} este mes
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Procesando</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.pendingProcessing || 0}</div>
            <p className="text-xs text-muted-foreground">
              Archivos en cola
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Storage Usage */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <HardDrive className="h-5 w-5" />
            Uso de Almacenamiento
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div className="flex justify-between text-sm">
              <span>Usado: {formatFileSize(stats?.storageUsed || 0)}</span>
              <span>Límite: {formatFileSize(stats?.storageLimit || 0)}</span>
            </div>
            <Progress value={storagePercentage} className="h-2" />
            <div className="flex justify-between text-xs text-muted-foreground">
              <span>{storagePercentage.toFixed(1)}% utilizado</span>
              {storagePercentage > 80 && (
                <span className="text-orange-600">Cerca del límite</span>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Recent Uploads */}
        <Card>
          <CardHeader>
            <CardTitle>Subidas Recientes</CardTitle>
          </CardHeader>
          <CardContent>
            {stats?.recentUploads && stats.recentUploads.length > 0 ? (
              <div className="space-y-4">
                {stats.recentUploads.slice(0, 5).map((upload) => (
                  <div key={upload.id} className="flex items-center justify-between p-3 border rounded-lg">
                    <div className="flex items-center space-x-3">
                      {getStatusIcon(upload.status)}
                      <div>
                        <p className="font-medium truncate max-w-[200px]">{upload.title}</p>
                        <p className="text-xs text-muted-foreground">
                          {formatFileSize(upload.size)} • {upload.views} vistas
                        </p>
                      </div>
                    </div>
                    <div className="text-right">
                      <Badge className={getStatusColor(upload.status)}>
                        {upload.status === 'completed' ? 'Completado' : 
                         upload.status === 'processing' ? 'Procesando' : 'Error'}
                      </Badge>
                      <p className="text-xs text-muted-foreground mt-1">
                        {new Date(upload.uploadedAt).toLocaleDateString()}
                      </p>
                    </div>
                  </div>
                ))}
                <Link href="/library?filter=my-uploads">
                  <Button variant="ghost" className="w-full">
                    Ver Todas Mis Subidas
                  </Button>
                </Link>
              </div>
            ) : (
              <div className="text-center py-6">
                <Upload className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                <p className="text-muted-foreground mb-4">
                  Aún no has subido ningún manga
                </p>
                <Link href="/upload">
                  <Button>
                    <Plus className="mr-2 h-4 w-4" />
                    Subir tu primer manga
                  </Button>
                </Link>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Popular Content */}
        <Card>
          <CardHeader>
            <CardTitle>Contenido Popular</CardTitle>
          </CardHeader>
          <CardContent>
            {stats?.popularContent && stats.popularContent.length > 0 ? (
              <div className="space-y-4">
                {stats.popularContent.slice(0, 5).map((content, index) => (
                  <div key={content.id} className="flex items-center justify-between p-3 border rounded-lg">
                    <div className="flex items-center space-x-3">
                      <div className="flex items-center justify-center w-8 h-8 bg-primary/10 rounded-full">
                        <span className="text-sm font-bold text-primary">#{index + 1}</span>
                      </div>
                      <div>
                        <p className="font-medium truncate max-w-[200px]">{content.title}</p>
                        <p className="text-xs text-muted-foreground">
                          {content.views} vistas • {content.downloads} descargas
                        </p>
                      </div>
                    </div>
                    <div className="text-right">
                      <div className="flex items-center gap-1">
                        <TrendingUp className="h-4 w-4 text-green-600" />
                        <span className="text-sm font-medium">{content.rating}/5</span>
                      </div>
                    </div>
                  </div>
                ))}
                <Link href="/library?filter=my-uploads&sort=popular">
                  <Button variant="ghost" className="w-full">
                    Ver Todo Mi Contenido
                  </Button>
                </Link>
              </div>
            ) : (
              <div className="text-center py-6">
                <TrendingUp className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                <p className="text-muted-foreground mb-4">
                  Sube contenido para ver estadísticas de popularidad
                </p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Acciones Rápidas</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-4">
            <Link href="/upload">
              <Button variant="outline" className="w-full h-20 flex-col">
                <Upload className="h-6 w-6 mb-2" />
                Subir Manga
              </Button>
            </Link>
            <Link href="/library?filter=my-uploads">
              <Button variant="outline" className="w-full h-20 flex-col">
                <FileText className="h-6 w-6 mb-2" />
                Mis Subidas
              </Button>
            </Link>
            <Link href="/library?filter=processing">
              <Button variant="outline" className="w-full h-20 flex-col">
                <Clock className="h-6 w-6 mb-2" />
                En Proceso
              </Button>
            </Link>
            <Link href="/profile?tab=statistics">
              <Button variant="outline" className="w-full h-20 flex-col">
                <TrendingUp className="h-6 w-6 mb-2" />
                Estadísticas
              </Button>
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}