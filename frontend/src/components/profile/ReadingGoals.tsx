'use client'

import React, { useState } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Progress } from '@/components/ui/progress'
import { Badge } from '@/components/ui/badge'
import { 
  Target, 
  Calendar, 
  TrendingUp, 
  Award,
  Edit,
  Save,
  X,
  Trophy,
  Star,
  BookOpen,
  Clock,
  Flame
} from 'lucide-react'
import { useApiData, useApiMutation } from '@/hooks'

interface ReadingGoal {
  id: string
  type: 'monthly' | 'yearly' | 'custom'
  target: number
  current: number
  period: string
  isActive: boolean
  createdAt: string
}

interface Achievement {
  id: string
  title: string
  description: string
  icon: string
  category: 'reading' | 'collection' | 'social' | 'special'
  progress: number
  maxProgress: number
  isUnlocked: boolean
  unlockedAt?: string
  rarity: 'common' | 'rare' | 'epic' | 'legendary'
}

interface ReadingStats {
  currentStreak: number
  longestStreak: number
  totalChaptersRead: number
  totalMangaRead: number
  averageChaptersPerDay: number
  favoriteGenres: string[]
  readingTimeToday: number
  readingTimeThisWeek: number
}

export const ReadingGoals: React.FC = () => {
  const [editingGoal, setEditingGoal] = useState<string | null>(null)
  const [goalValues, setGoalValues] = useState<{ [key: string]: number }>({})

  const { data: goals } = useApiData<ReadingGoal[]>('/profile/reading-goals')
  const { data: achievements } = useApiData<Achievement[]>('/profile/achievements')
  const { data: stats } = useApiData<ReadingStats>('/profile/reading-stats')
  const updateGoalMutation = useApiMutation('/profile/reading-goals', {
    onSuccess: () => {
      setEditingGoal(null)
      setGoalValues({})
    }
  })

  const getRarityColor = (rarity: string) => {
    switch (rarity) {
      case 'common':
        return 'bg-gray-100 text-gray-800 border-gray-200'
      case 'rare':
        return 'bg-blue-100 text-blue-800 border-blue-200'
      case 'epic':
        return 'bg-purple-100 text-purple-800 border-purple-200'
      case 'legendary':
        return 'bg-yellow-100 text-yellow-800 border-yellow-200'
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200'
    }
  }

  const getCategoryIcon = (category: string) => {
    switch (category) {
      case 'reading':
        return <BookOpen className="h-4 w-4" />
      case 'collection':
        return <Star className="h-4 w-4" />
      case 'social':
        return <Trophy className="h-4 w-4" />
      case 'special':
        return <Award className="h-4 w-4" />
      default:
        return <Award className="h-4 w-4" />
    }
  }

  const handleSaveGoal = async (goalId: string) => {
    const newTarget = goalValues[goalId]
    if (newTarget && newTarget > 0) {
      await updateGoalMutation.trigger({
        goalId,
        target: newTarget
      })
    }
  }

  const formatTime = (minutes: number) => {
    const hours = Math.floor(minutes / 60)
    const mins = minutes % 60
    if (hours > 0) {
      return `${hours}h ${mins}m`
    }
    return `${mins}m`
  }

  return (
    <div className="space-y-6">
      {/* Reading Stats Overview */}
      {stats && (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center space-x-2">
                <Flame className="h-5 w-5 text-orange-500" />
                <div>
                  <p className="text-2xl font-bold">{stats.currentStreak}</p>
                  <p className="text-xs text-muted-foreground">Racha actual</p>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center space-x-2">
                <BookOpen className="h-5 w-5 text-blue-500" />
                <div>
                  <p className="text-2xl font-bold">{stats.totalChaptersRead}</p>
                  <p className="text-xs text-muted-foreground">Capítulos leídos</p>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center space-x-2">
                <Clock className="h-5 w-5 text-green-500" />
                <div>
                  <p className="text-2xl font-bold">{formatTime(stats.readingTimeToday)}</p>
                  <p className="text-xs text-muted-foreground">Tiempo hoy</p>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center space-x-2">
                <TrendingUp className="h-5 w-5 text-purple-500" />
                <div>
                  <p className="text-2xl font-bold">{stats.averageChaptersPerDay.toFixed(1)}</p>
                  <p className="text-xs text-muted-foreground">Promedio/día</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Reading Goals */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Target className="h-5 w-5" />
            Metas de Lectura
          </CardTitle>
        </CardHeader>
        <CardContent>
          {goals && goals.length > 0 ? (
            <div className="space-y-4">
              {goals.filter(goal => goal.isActive).map((goal) => {
                const progress = (goal.current / goal.target) * 100
                const isEditing = editingGoal === goal.id
                
                return (
                  <div key={goal.id} className="p-4 border rounded-lg">
                    <div className="flex items-center justify-between mb-3">
                      <div className="flex items-center gap-2">
                        <Calendar className="h-4 w-4 text-muted-foreground" />
                        <span className="font-medium capitalize">
                          Meta {goal.type === 'monthly' ? 'Mensual' : 
                                goal.type === 'yearly' ? 'Anual' : 'Personalizada'}
                        </span>
                      </div>
                      <div className="flex items-center gap-2">
                        {!isEditing ? (
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => {
                              setEditingGoal(goal.id)
                              setGoalValues({ ...goalValues, [goal.id]: goal.target })
                            }}
                          >
                            <Edit className="h-4 w-4" />
                          </Button>
                        ) : (
                          <div className="flex gap-1">
                            <Button
                              size="sm"
                              variant="ghost"
                              onClick={() => handleSaveGoal(goal.id)}
                              disabled={updateGoalMutation.isMutating}
                            >
                              <Save className="h-4 w-4" />
                            </Button>
                            <Button
                              size="sm"
                              variant="ghost"
                              onClick={() => {
                                setEditingGoal(null)
                                setGoalValues({})
                              }}
                            >
                              <X className="h-4 w-4" />
                            </Button>
                          </div>
                        )}
                      </div>
                    </div>

                    <div className="space-y-2">
                      <div className="flex justify-between text-sm">
                        <span>Progreso</span>
                        <span>
                          {goal.current} / {isEditing ? (
                            <Input
                              type="number"
                              value={goalValues[goal.id] || goal.target}
                              onChange={(e) => setGoalValues({
                                ...goalValues,
                                [goal.id]: parseInt(e.target.value) || 0
                              })}
                              className="inline-block w-16 h-6 text-xs"
                              min="1"
                            />
                          ) : goal.target} capítulos
                        </span>
                      </div>
                      <Progress value={Math.min(progress, 100)} className="h-2" />
                      <div className="flex justify-between text-xs text-muted-foreground">
                        <span>{progress.toFixed(1)}% completado</span>
                        {progress >= 100 ? (
                          <span className="text-green-600 font-medium">¡Meta alcanzada! 🎉</span>
                        ) : (
                          <span>Faltan {goal.target - goal.current} capítulos</span>
                        )}
                      </div>
                    </div>
                  </div>
                )
              })}
            </div>
          ) : (
            <div className="text-center py-6">
              <Target className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground mb-4">
                No tienes metas de lectura configuradas
              </p>
              <Button>
                <Target className="mr-2 h-4 w-4" />
                Crear Meta
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Achievements */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Award className="h-5 w-5" />
            Logros
          </CardTitle>
        </CardHeader>
        <CardContent>
          {achievements && achievements.length > 0 ? (
            <div className="grid gap-4 md:grid-cols-2">
              {achievements.slice(0, 8).map((achievement) => (
                <div 
                  key={achievement.id} 
                  className={`p-4 border rounded-lg ${
                    achievement.isUnlocked ? 'bg-gradient-to-r from-yellow-50 to-orange-50' : 'bg-gray-50'
                  }`}
                >
                  <div className="flex items-start space-x-3">
                    <div className={`p-2 rounded-full ${
                      achievement.isUnlocked ? 'bg-yellow-100' : 'bg-gray-200'
                    }`}>
                      {getCategoryIcon(achievement.category)}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 mb-1">
                        <h4 className="font-medium truncate">{achievement.title}</h4>
                        <Badge className={getRarityColor(achievement.rarity)}>
                          {achievement.rarity}
                        </Badge>
                      </div>
                      <p className="text-sm text-muted-foreground mb-2">
                        {achievement.description}
                      </p>
                      
                      {achievement.isUnlocked ? (
                        <div className="flex items-center gap-2">
                          <Trophy className="h-4 w-4 text-yellow-600" />
                          <span className="text-sm text-yellow-700 font-medium">
                            Desbloqueado {achievement.unlockedAt && 
                              new Date(achievement.unlockedAt).toLocaleDateString()}
                          </span>
                        </div>
                      ) : (
                        <div className="space-y-1">
                          <div className="flex justify-between text-xs">
                            <span>Progreso</span>
                            <span>{achievement.progress} / {achievement.maxProgress}</span>
                          </div>
                          <Progress 
                            value={(achievement.progress / achievement.maxProgress) * 100} 
                            className="h-1"
                          />
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-6">
              <Award className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <p className="text-muted-foreground">
                Comienza a leer para desbloquear logros
              </p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Favorite Genres */}
      {stats && stats.favoriteGenres.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Star className="h-5 w-5" />
              Géneros Favoritos
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex flex-wrap gap-2">
              {stats.favoriteGenres.map((genre, index) => (
                <Badge key={index} variant="outline" className="text-sm">
                  {genre}
                </Badge>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}