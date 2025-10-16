"use client"

import { useState } from "react"
import { MainLayout } from "@/components/layout"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Progress } from "@/components/ui/progress"
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form"
import {
  Settings,
  Camera,
  Loader2,
  BookOpen,
  Star
} from "lucide-react"

import { useAuthStore, usePreferencesStore } from "@/stores"
import { useApiData, useApiMutation } from "@/hooks"
import { useProfileForm } from "@/hooks"
import { RoleSpecificSettings, ReadingGoals } from "@/components/profile"

interface UserProfile {
  id: string
  username: string
  email: string
  avatar?: string
  bio?: string
  createdAt: string
  updatedAt: string
  stats: {
    totalManga: number
    totalReadChapters: number
    favoriteSeries: number
    readingStreak: number
  }
}

export default function ProfilePage() {
  const { user } = useAuthStore()
  const { preferences } = usePreferencesStore()
  const [isEditing, setIsEditing] = useState(false)

  const { data: profile, isLoading, error } = useApiData<UserProfile>('/auth/profile')
  const updateProfileMutation = useApiMutation('/auth/profile', {
    onSuccess: () => {
      setIsEditing(false)
    },
  })

  const form = useProfileForm()

  const onSubmit = async (data: unknown) => {
    try {
      await updateProfileMutation.trigger(data)
    } catch (error) {
      console.error('Error updating profile:', error)
    }
  }

  if (isLoading) {
    return (
      <MainLayout>
        <div className="max-w-4xl mx-auto space-y-6">
          <div className="flex items-center space-x-4">
            <div className="h-20 w-20 bg-muted rounded-full animate-pulse" />
            <div className="space-y-2">
              <div className="h-6 bg-muted rounded w-32 animate-pulse" />
              <div className="h-4 bg-muted rounded w-48 animate-pulse" />
            </div>
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
            <h2 className="text-xl font-semibold mb-2">Error al cargar el perfil</h2>
            <p className="text-muted-foreground mb-4">{error.message}</p>
            <Button onClick={() => window.location.reload()}>
              Reintentar
            </Button>
          </div>
        </div>
      </MainLayout>
    )
  }

  const profileData = profile || user
  const stats = profile?.stats || {
    totalManga: 0,
    totalReadChapters: 0,
    favoriteSeries: 0,
    readingStreak: 0,
  }

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto space-y-6">
        {/* Profile Header */}
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center space-x-6">
              <div className="relative">
                <Avatar className="h-20 w-20">
                  <AvatarImage src={profileData?.avatar} alt={profileData?.username} />
                  <AvatarFallback className="text-lg">
                    {profileData?.username?.charAt(0).toUpperCase()}
                  </AvatarFallback>
                </Avatar>
                <Button
                  size="sm"
                  variant="outline"
                  className="absolute -bottom-2 -right-2 h-8 w-8 rounded-full p-0"
                >
                  <Camera className="h-4 w-4" />
                </Button>
              </div>

              <div className="flex-1">
                <h1 className="text-2xl font-bold">{profileData?.username}</h1>
                <p className="text-muted-foreground">{profileData?.email}</p>
                <p className="text-sm text-muted-foreground mt-1">
                  Miembro desde {new Date(profileData?.createdAt || '').toLocaleDateString('es-ES')}
                </p>
              </div>

              <Button
                onClick={() => setIsEditing(!isEditing)}
                variant={isEditing ? "outline" : "default"}
              >
                <Settings className="mr-2 h-4 w-4" />
                {isEditing ? 'Cancelar' : 'Editar Perfil'}
              </Button>
            </div>
          </CardContent>
        </Card>

        <Tabs defaultValue="profile" className="space-y-6">
          <TabsList>
            <TabsTrigger value="profile">Perfil</TabsTrigger>
            <TabsTrigger value="stats">Estadísticas</TabsTrigger>
            <TabsTrigger value="goals">Metas y Logros</TabsTrigger>
            <TabsTrigger value="role">Configuración de Rol</TabsTrigger>
            <TabsTrigger value="preferences">Preferencias</TabsTrigger>
          </TabsList>

          {/* Profile Tab */}
          <TabsContent value="profile" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Información Personal</CardTitle>
                <CardDescription>
                  Actualiza tu información personal y biografía
                </CardDescription>
              </CardHeader>
              <CardContent>
                {isEditing ? (
                  <Form {...form}>
                    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                      <div className="grid gap-4 md:grid-cols-2">
                        <FormField
                          control={form.control}
                          name="username"
                          render={({ field }) => (
                            <FormItem>
                              <FormLabel>Nombre de Usuario</FormLabel>
                              <FormControl>
                                <Input {...field} />
                              </FormControl>
                              <FormMessage />
                            </FormItem>
                          )}
                        />

                        <FormField
                          control={form.control}
                          name="email"
                          render={({ field }) => (
                            <FormItem>
                              <FormLabel>Email</FormLabel>
                              <FormControl>
                                <Input type="email" {...field} />
                              </FormControl>
                              <FormMessage />
                            </FormItem>
                          )}
                        />
                      </div>

                      <div className="space-y-2">
                        <Label htmlFor="bio">Biografía</Label>
                        <Textarea
                          id="bio"
                          placeholder="Cuéntanos sobre ti..."
                          className="min-h-[100px]"
                        />
                      </div>

                      <div className="flex justify-end space-x-2">
                        <Button
                          type="button"
                          variant="outline"
                          onClick={() => setIsEditing(false)}
                        >
                          Cancelar
                        </Button>
                        <Button
                          type="submit"
                          disabled={updateProfileMutation.isMutating}
                        >
                          {updateProfileMutation.isMutating ? (
                            <>
                              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                              Guardando...
                            </>
                          ) : (
                            'Guardar Cambios'
                          )}
                        </Button>
                      </div>
                    </form>
                  </Form>
                ) : (
                  <div className="space-y-4">
                    <div className="grid gap-4 md:grid-cols-2">
                      <div className="space-y-2">
                        <Label className="text-sm font-medium">Nombre de Usuario</Label>
                        <p className="text-sm text-muted-foreground">{profileData?.username}</p>
                      </div>

                      <div className="space-y-2">
                        <Label className="text-sm font-medium">Email</Label>
                        <p className="text-sm text-muted-foreground">{profileData?.email}</p>
                      </div>
                    </div>

                    <div className="space-y-2">
                      <Label className="text-sm font-medium">Biografía</Label>
                      <p className="text-sm text-muted-foreground">
                        {profileData?.bio || 'No hay biografía establecida'}
                      </p>
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          {/* Stats Tab */}
          <TabsContent value="stats" className="space-y-6">
            <div className="grid gap-6 md:grid-cols-2">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center">
                    <BookOpen className="mr-2 h-5 w-5" />
                    Estadísticas de Lectura
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-muted-foreground">Mangas en Biblioteca</span>
                    <span className="font-semibold">{stats.totalManga}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-muted-foreground">Capítulos Leídos</span>
                    <span className="font-semibold">{stats.totalReadChapters}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-muted-foreground">Series Favoritas</span>
                    <span className="font-semibold">{stats.favoriteSeries}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-muted-foreground">Racha de Lectura</span>
                    <span className="font-semibold">{stats.readingStreak} días</span>
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center">
                    <Star className="mr-2 h-5 w-5" />
                    Logros
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <span className="text-sm">Lector Principiante</span>
                      <span className="text-xs text-muted-foreground">✓ Completado</span>
                    </div>
                    <Progress value={100} />
                  </div>

                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <span className="text-sm">Coleccionista</span>
                      <span className="text-xs text-muted-foreground">
                        {stats.totalManga}/10
                      </span>
                    </div>
                    <Progress value={(stats.totalManga / 10) * 100} />
                  </div>

                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <span className="text-sm">Lector Ávido</span>
                      <span className="text-xs text-muted-foreground">
                        {stats.totalReadChapters}/100
                      </span>
                    </div>
                    <Progress value={(stats.totalReadChapters / 100) * 100} />
                  </div>
                </CardContent>
              </Card>
            </div>
          </TabsContent>

          {/* Reading Goals Tab */}
          <TabsContent value="goals" className="space-y-6">
            <ReadingGoals />
          </TabsContent>

          {/* Role-Specific Settings Tab */}
          <TabsContent value="role" className="space-y-6">
            <RoleSpecificSettings />
          </TabsContent>

          {/* Preferences Tab */}
          <TabsContent value="preferences" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Preferencias de Lectura</CardTitle>
                <CardDescription>
                  Configura tus preferencias de lectura y visualización
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="space-y-2">
                    <Label>Modo de Lectura</Label>
                    <p className="text-sm text-muted-foreground capitalize">
                      {preferences.readingMode}
                    </p>
                  </div>

                  <div className="space-y-2">
                    <Label>Modo de Biblioteca</Label>
                    <p className="text-sm text-muted-foreground capitalize">
                      {preferences.libraryView}
                    </p>
                  </div>

                  <div className="space-y-2">
                    <Label>Ordenar por</Label>
                    <p className="text-sm text-muted-foreground capitalize">
                      {preferences.sortBy}
                    </p>
                  </div>

                  <div className="space-y-2">
                    <Label>Elementos por página</Label>
                    <p className="text-sm text-muted-foreground">
                      {preferences.itemsPerPage}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </div>
    </MainLayout>
  )
}
