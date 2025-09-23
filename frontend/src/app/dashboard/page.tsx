"use client"

import { MainLayout } from "@/components/layout"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Progress } from "@/components/ui/progress"
import {
  BookOpen,
  Upload,
  TrendingUp,
  Clock,
  Star,
  Plus,
  ArrowRight
} from "lucide-react"
import Link from "next/link"
import { useApiData } from "@/hooks"

interface DashboardStats {
  totalManga: number
  totalChapters: number
  readingProgress: number
  recentlyRead: Array<{
    id: string
    title: string
    lastChapter: string
    progress: number
    coverImage?: string
  }>
  favoriteSeries: Array<{
    id: string
    title: string
    rating: number
    coverImage?: string
  }>
}

export default function DashboardPage() {
  const { data: stats, isLoading, error } = useApiData<DashboardStats>('/dashboard')

  if (isLoading) {
    return (
      <MainLayout>
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <h1 className="text-3xl font-bold">Dashboard</h1>
          </div>

          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
            {[...Array(4)].map((_, i) => (
              <Card key={i}>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <div className="h-4 bg-muted rounded w-20 animate-pulse" />
                  <div className="h-4 w-4 bg-muted rounded animate-pulse" />
                </CardHeader>
                <CardContent>
                  <div className="h-8 bg-muted rounded w-16 mb-2 animate-pulse" />
                  <div className="h-3 bg-muted rounded w-24 animate-pulse" />
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </MainLayout>
    )
  }

  if (error) {
    return (
      <MainLayout>
        <div className="flex items-center justify-center h-64">
          <div className="text-center">
            <h2 className="text-xl font-semibold mb-2">Error al cargar el dashboard</h2>
            <p className="text-muted-foreground mb-4">{error.message}</p>
            <Button onClick={() => window.location.reload()}>
              Reintentar
            </Button>
          </div>
        </div>
      </MainLayout>
    )
  }

  return (
    <MainLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <h1 className="text-3xl font-bold">Dashboard</h1>
          <div className="flex items-center space-x-2">
            <Link href="/library">
              <Button variant="outline">
                <BookOpen className="mr-2 h-4 w-4" />
                Ver Biblioteca
              </Button>
            </Link>
            <Link href="/upload">
              <Button>
                <Plus className="mr-2 h-4 w-4" />
                Subir Manga
              </Button>
            </Link>
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">
                Total de Mangas
              </CardTitle>
              <BookOpen className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stats?.totalManga || 0}</div>
              <p className="text-xs text-muted-foreground">
                Series en tu biblioteca
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">
                Capítulos Leídos
              </CardTitle>
              <Clock className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stats?.totalChapters || 0}</div>
              <p className="text-xs text-muted-foreground">
                Progreso total de lectura
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">
                Progreso General
              </CardTitle>
              <TrendingUp className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stats?.readingProgress || 0}%</div>
              <Progress value={stats?.readingProgress || 0} className="mt-2" />
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">
                Favoritos
              </CardTitle>
              <Star className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stats?.favoriteSeries?.length || 0}</div>
              <p className="text-xs text-muted-foreground">
                Series marcadas como favoritas
              </p>
            </CardContent>
          </Card>
        </div>

        {/* Recent Activity */}
        <div className="grid gap-6 md:grid-cols-2">
          {/* Recently Read */}
          <Card>
            <CardHeader>
              <CardTitle>Lectura Reciente</CardTitle>
              <CardDescription>
                Los mangas que has estado leyendo últimamente
              </CardDescription>
            </CardHeader>
            <CardContent>
              {stats?.recentlyRead && stats.recentlyRead.length > 0 ? (
                <div className="space-y-4">
                  {stats.recentlyRead.slice(0, 3).map((item) => (
                    <div key={item.id} className="flex items-center space-x-4">
                      <div className="h-12 w-12 bg-muted rounded flex items-center justify-center">
                        <BookOpen className="h-6 w-6 text-muted-foreground" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium truncate">
                          {item.title}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          Cap. {item.lastChapter} • {item.progress}%
                        </p>
                      </div>
                      <ArrowRight className="h-4 w-4 text-muted-foreground" />
                    </div>
                  ))}
                  <Link href="/library">
                    <Button variant="ghost" className="w-full">
                      Ver toda la biblioteca
                    </Button>
                  </Link>
                </div>
              ) : (
                <div className="text-center py-6">
                  <BookOpen className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                  <p className="text-muted-foreground mb-4">
                    Aún no has leído ningún manga
                  </p>
                  <Link href="/library">
                    <Button>
                      <Upload className="mr-2 h-4 w-4" />
                      Subir tu primer manga
                    </Button>
                  </Link>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Favorite Series */}
          <Card>
            <CardHeader>
              <CardTitle>Series Favoritas</CardTitle>
              <CardDescription>
                Tus mangas favoritos para acceso rápido
              </CardDescription>
            </CardHeader>
            <CardContent>
              {stats?.favoriteSeries && stats.favoriteSeries.length > 0 ? (
                <div className="space-y-4">
                  {stats.favoriteSeries.slice(0, 3).map((item) => (
                    <div key={item.id} className="flex items-center space-x-4">
                      <div className="h-12 w-12 bg-muted rounded flex items-center justify-center">
                        <Star className="h-6 w-6 text-yellow-500" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium truncate">
                          {item.title}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          ★ {item.rating}/5
                        </p>
                      </div>
                      <ArrowRight className="h-4 w-4 text-muted-foreground" />
                    </div>
                  ))}
                  <Link href="/library?favorites=true">
                    <Button variant="ghost" className="w-full">
                      Ver todos los favoritos
                    </Button>
                  </Link>
                </div>
              ) : (
                <div className="text-center py-6">
                  <Star className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                  <p className="text-muted-foreground mb-4">
                    No tienes series favoritas aún
                  </p>
                  <Link href="/library">
                    <Button variant="outline">
                      Explorar biblioteca
                    </Button>
                  </Link>
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Quick Actions */}
        <Card>
          <CardHeader>
            <CardTitle>Acciones Rápidas</CardTitle>
            <CardDescription>
              Accede rápidamente a las funciones más usadas
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-3">
              <Link href="/upload">
                <Button variant="outline" className="w-full h-20 flex-col">
                  <Upload className="h-6 w-6 mb-2" />
                  Subir Manga
                </Button>
              </Link>
              <Link href="/library">
                <Button variant="outline" className="w-full h-20 flex-col">
                  <BookOpen className="h-6 w-6 mb-2" />
                  Ver Biblioteca
                </Button>
              </Link>
              <Link href="/library?favorites=true">
                <Button variant="outline" className="w-full h-20 flex-col">
                  <Star className="h-6 w-6 mb-2" />
                  Mis Favoritos
                </Button>
              </Link>
            </div>
          </CardContent>
        </Card>
      </div>
    </MainLayout>
  )
}
