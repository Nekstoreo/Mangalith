'use client'

import React, { useEffect, useState } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Progress } from '@/components/ui/progress'
import { Badge } from '@/components/ui/badge'
import { LoadingSpinner } from '@/components/ui/loading-spinner'
import { 
  BookOpen, 
  Star, 
  Clock, 
  TrendingUp,
  Heart,
  Bookmark,
  Calendar,
  Award,
  ArrowRight,
  Plus,
  Search,
  Filter
} from 'lucide-react'
import Link from 'next/link'
import { useApiData } from '@/hooks'

interface ReaderStats {
  totalMangaRead: number
  totalChaptersRead: number
  readingStreak: number
  favoriteGenres: string[]
  recentlyRead: Array<{
    id: string
    title: string
    lastChapter: string
    progress: number
    coverImage?: string
    lastReadAt: string
    totalChapters: number
  }>
  favorites: Array<{
    id: string
    title: string
    rating: number
    coverImage?: string
    genre: string
    status: string
  }>
  recommendations: Array<{
    id: string
    title: string
    reason: string
    rating: number
    coverImage?: string
    genre: string
  }>
  readingGoals: {
    monthlyTarget: number
    monthlyProgress: number
    yearlyTarget: number
    yearlyProgress: number
  }
  achievements: Array<{
    id: string
    title: string
    description: string
    unlockedAt: string
    icon: string
  }>
  monthlyStats: {
    chaptersRead: number
    hoursRead: number
    newSeries: number
  }
}

export const ReaderDashboard: React.FC = () => {
  const { data: stats, isLoading, error } = useApiData<ReaderStats>('/dashboard/reader')

  const getProgressColor = (progress: number) => {
    if (progress >= 80) return 'bg-green-500'
    if (progress >= 50) return 'bg-yellow-500'
    return 'bg-blue-500'
  }

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'completed':
        return 'bg-green-100 text-green-800'
      case 'ongoing':
        return 'bg-blue-100 text-blue-800'
      case 'hiatus':
        return 'bg-yellow-100 text-yellow-800'
      case 'cancelled':
        return 'bg-red-100 text-red-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
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
        <p className="text-red-600 mb-4">Error al cargar las estadísticas de lectura</p>
        <Button onClick={() => window.location.reload()}>Reintentar</Button>
      </div>
    )
  }

  const monthlyGoalProgress = stats?.readingGoals ? 
    (stats.readingGoals.monthlyProgress / stats.readingGoals.monthlyTarget) * 100 : 0
  const yearlyGoalProgress = stats?.readingGoals ? 
    (stats.readingGoals.yearlyProgress / stats.readingGoals.yearlyTarget) * 100 : 0

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Mi Biblioteca</h1>
          <p className="text-muted-foreground">Descubre, lee y organiza tu colección de manga</p>
        </div>
        <div className="flex items-center space-x-2">
          <Link href="/search">
            <Button variant="outline">
              <Search className="mr-2 h-4 w-4" />
              Buscar Manga
            </Button>
          </Link>
          <Link href="/library">
            <Button>
              <BookOpen className="mr-2 h-4 w-4" />
              Ver Biblioteca
            </Button>
          </Link>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Series Leídas</CardTitle>
            <BookOpen className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.totalMangaRead || 0}</div>
            <p className="text-xs text-muted-foreground">
              +{stats?.monthlyStats.newSeries || 0} este mes
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Capítulos Leídos</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.totalChaptersRead || 0}</div>
            <p className="text-xs text-muted-foreground">
              +{stats?.monthlyStats.chaptersRead || 0} este mes
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Racha de Lectura</CardTitle>
            <Award className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.readingStreak || 0}</div>
            <p className="text-xs text-muted-foreground">
              días consecutivos
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Favoritos</CardTitle>
            <Heart className="h-4 w-4 text-red-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats?.favorites?.length || 0}</div>
            <p className="text-xs text-muted-foreground">
              series marcadas
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Reading Goals */}
      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Calendar className="h-5 w-5" />
              Meta Mensual
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex justify-between text-sm">
                <span>Progreso: {stats?.readingGoals.monthlyProgress || 0} / {stats?.readingGoals.monthlyTarget || 0} capítulos</span>
                <span>{monthlyGoalProgress.toFixed(0)}%</span>
              </div>
              <Progress value={monthlyGoalProgress} className="h-2" />
              <p className="text-xs text-muted-foreground">
                {monthlyGoalProgress >= 100 ? '¡Meta alcanzada!' : 
                 `Faltan ${(stats?.readingGoals.monthlyTarget || 0) - (stats?.readingGoals.monthlyProgress || 0)} capítulos`}
              </p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <TrendingUp className="h-5 w-5" />
              Meta Anual
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex justify-between text-sm">
                <span>Progreso: {stats?.readingGoals.yearlyProgress || 0} / {stats?.readingGoals.yearlyTarget || 0} capítulos</span>
                <span>{yearlyGoalProgress.toFixed(0)}%</span>
              </div>
              <Progress value={yearlyGoalProgress} className="h-2" />
              <p className="text-xs text-muted-foreground">
                {yearlyGoalProgress >= 100 ? '¡Meta anual completada!' : 
                 `Faltan ${(stats?.readingGoals.yearlyTarget || 0) - (stats?.readingGoals.yearlyProgress || 0)} capítulos`}
              </p>
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Continue Reading */}
        <Card>
          <CardHeader>
            <CardTitle>Continuar Leyendo</CardTitle>
          </CardHeader>
          <CardContent>
            {stats?.recentlyRead && stats.recentlyRead.length > 0 ? (
              <div className="space-y-4">
                {stats.recentlyRead.slice(0, 4).map((manga) => (
                  <div key={manga.id} className="flex items-center space-x-4 p-3 border rounded-lg hover:bg-muted/50 transition-colors">
                    <div className="h-12 w-12 bg-muted rounded flex items-center justify-center">
                      <BookOpen className="h-6 w-6 text-muted-foreground" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="font-medium truncate">{manga.title}</p>
                      <p className="text-sm text-muted-foreground">
                        Cap. {manga.lastChapter} de {manga.totalChapters}
                      </p>
                      <div className="flex items-center gap-2 mt-1">
                        <Progress value={manga.progress} className="h-1 flex-1" />
                        <span className="text-xs text-muted-foreground">{manga.progress}%</span>
                      </div>
                    </div>
                    <ArrowRight className="h-4 w-4 text-muted-foreground" />
                  </div>
                ))}
                <Link href="/library?filter=reading">
                  <Button variant="ghost" className="w-full">
                    Ver Todo lo que Estoy Leyendo
                  </Button>
                </Link>
              </div>
            ) : (
              <div className="text-center py-6">
                <BookOpen className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                <p className="text-muted-foreground mb-4">
                  No tienes lecturas en progreso
                </p>
                <Link href="/search">
                  <Button>
                    <Search className="mr-2 h-4 w-4" />
                    Descubrir Manga
                  </Button>
                </Link>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Favorites */}
        <Card>
          <CardHeader>
            <CardTitle>Mis Favoritos</CardTitle>
          </CardHeader>
          <CardContent>
            {stats?.favorites && stats.favorites.length > 0 ? (
              <div className="space-y-4">
                {stats.favorites.slice(0, 4).map((manga) => (
                  <div key={manga.id} className="flex items-center space-x-4 p-3 border rounded-lg hover:bg-muted/50 transition-colors">
                    <div className="h-12 w-12 bg-muted rounded flex items-center justify-center">
                      <Star className="h-6 w-6 text-yellow-500" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="font-medium truncate">{manga.title}</p>
                      <div className="flex items-center gap-2">
                        <Badge variant="outline" className="text-xs">{manga.genre}</Badge>
                        <Badge className={getStatusColor(manga.status)}>
                          {manga.status}
                        </Badge>
                      </div>
                      <div className="flex items-center gap-1 mt-1">
                        <Star className="h-3 w-3 text-yellow-500 fill-current" />
                        <span className="text-xs">{manga.rating}/5</span>
                      </div>
                    </div>
                    <ArrowRight className="h-4 w-4 text-muted-foreground" />
                  </div>
                ))}
                <Link href="/library?filter=favorites">
                  <Button variant="ghost" className="w-full">
                    Ver Todos los Favoritos
                  </Button>
                </Link>
              </div>
            ) : (
              <div className="text-center py-6">
                <Heart className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                <p className="text-muted-foreground mb-4">
                  No tienes favoritos aún
                </p>
                <Link href="/library">
                  <Button variant="outline">
                    <BookOpen className="mr-2 h-4 w-4" />
                    Explorar Biblioteca
                  </Button>
                </Link>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Recommendations */}
      <Card>
        <CardHeader>
          <CardTitle>Recomendaciones para Ti</CardTitle>
        </CardHeader>
        <CardContent>
          {stats?.recommendations && stats.recommendations.length > 0 ? (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {stats.recommendations.slice(0, 6).map((manga) => (
                <div key={manga.id} className="p-4 border rounded-lg hover:bg-muted/50 transition-colors">
                  <div className="flex items-start space-x-3">
                    <div className="h-16 w-12 bg-muted rounded flex items-center justify-center">
                      <BookOpen className="h-6 w-6 text-muted-foreground" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="font-medium truncate">{manga.title}</p>
                      <Badge variant="outline" className="text-xs mb-2">{manga.genre}</Badge>
                      <p className="text-xs text-muted-foreground mb-2">{manga.reason}</p>
                      <div className="flex items-center gap-1">
                        <Star className="h-3 w-3 text-yellow-500 fill-current" />
                        <span className="text-xs">{manga.rating}/5</span>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-6">
              <TrendingUp className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground mb-4">
                Lee más manga para recibir recomendaciones personalizadas
              </p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Recent Achievements */}
      {stats?.achievements && stats.achievements.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Award className="h-5 w-5" />
              Logros Recientes
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {stats.achievements.slice(0, 3).map((achievement) => (
                <div key={achievement.id} className="flex items-center space-x-3 p-3 border rounded-lg bg-gradient-to-r from-yellow-50 to-orange-50">
                  <div className="h-12 w-12 bg-yellow-100 rounded-full flex items-center justify-center">
                    <Award className="h-6 w-6 text-yellow-600" />
                  </div>
                  <div>
                    <p className="font-medium">{achievement.title}</p>
                    <p className="text-sm text-muted-foreground">{achievement.description}</p>
                    <p className="text-xs text-muted-foreground">
                      {new Date(achievement.unlockedAt).toLocaleDateString()}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Acciones Rápidas</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-4">
            <Link href="/search">
              <Button variant="outline" className="w-full h-20 flex-col">
                <Search className="h-6 w-6 mb-2" />
                Buscar Manga
              </Button>
            </Link>
            <Link href="/library?filter=reading">
              <Button variant="outline" className="w-full h-20 flex-col">
                <BookOpen className="h-6 w-6 mb-2" />
                Continuar Leyendo
              </Button>
            </Link>
            <Link href="/library?filter=favorites">
              <Button variant="outline" className="w-full h-20 flex-col">
                <Heart className="h-6 w-6 mb-2" />
                Mis Favoritos
              </Button>
            </Link>
            <Link href="/profile?tab=reading-goals">
              <Button variant="outline" className="w-full h-20 flex-col">
                <TrendingUp className="h-6 w-6 mb-2" />
                Metas de Lectura
              </Button>
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}