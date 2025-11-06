'use client'

import { useState, useEffect } from 'react'
import { useAuthStore } from '@/stores/auth'
import { PublicationDashboard } from '@/components/publication/PublicationDashboard'
import { PublicationMobileLayout } from '@/components/publication/PublicationMobileLayout'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Publication } from '@/services/publication/client'
import { usePublication } from '@/hooks/usePublication'
import { 
  FileText, 
  Plus, 
  Smartphone, 
  Monitor,
  ArrowLeft
} from 'lucide-react'

export default function CreatorPublicationsPage() {
  const { user } = useAuthStore()
  const { createPublication, loading } = usePublication()
  const [selectedPublication, setSelectedPublication] = useState<Publication | null>(null)
  const [viewMode, setViewMode] = useState<'desktop' | 'mobile'>('desktop')
  const [isCreating, setIsCreating] = useState(false)

  // Simular detección de dispositivo móvil
  useEffect(() => {
    const checkMobile = () => {
      setViewMode(window.innerWidth < 768 ? 'mobile' : 'desktop')
    }
    
    checkMobile()
    window.addEventListener('resize', checkMobile)
    return () => window.removeEventListener('resize', checkMobile)
  }, [])

  const handleCreatePublication = async () => {
    if (!user) return
    
    setIsCreating(true)
    try {
      // En un caso real, esto vendría de un formulario o selección de manga
      const mockMangaId = '12345678-1234-1234-1234-123456789012'
      const newPublication = await createPublication(mockMangaId)
      console.log('Created publication:', newPublication)
    } catch (error) {
      console.error('Error creating publication:', error)
    } finally {
      setIsCreating(false)
    }
  }

  const handleActionComplete = (publication: Publication, action: string) => {
    console.log('Action completed:', action, publication)
    
    // Manejar navegación según la acción
    switch (action) {
      case 'edit':
        // TODO: Navegar al editor de manga
        console.log('Navigate to manga editor')
        break
      case 'view':
        // TODO: Navegar a la vista pública
        console.log('Navigate to public view')
        break
      default:
        // Actualizar datos si es necesario
        break
    }
  }

  if (!user) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Alert>
          <AlertDescription>
            Debes iniciar sesión para acceder a tus publicaciones.
          </AlertDescription>
        </Alert>
      </div>
    )
  }

  return (
    <div className="container mx-auto px-4 py-8 max-w-7xl">
      {/* Header */}
      <div className="mb-8">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-4">
          <div>
            <h1 className="text-3xl font-bold flex items-center gap-2">
              <FileText className="w-8 h-8" />
              Gestión de Publicaciones
            </h1>
            <p className="text-muted-foreground mt-1">
              Administra tus publicaciones de manga y su proceso de revisión
            </p>
          </div>

          <div className="flex items-center gap-2">
            {/* Toggle de vista (solo para demostración) */}
            <div className="hidden sm:flex border rounded-lg p-1">
              <Button
                variant={viewMode === 'desktop' ? 'default' : 'ghost'}
                size="sm"
                onClick={() => setViewMode('desktop')}
              >
                <Monitor className="w-4 h-4" />
              </Button>
              <Button
                variant={viewMode === 'mobile' ? 'default' : 'ghost'}
                size="sm"
                onClick={() => setViewMode('mobile')}
              >
                <Smartphone className="w-4 h-4" />
              </Button>
            </div>

            <Button
              onClick={handleCreatePublication}
              disabled={isCreating || loading}
            >
              {isCreating ? (
                <>
                  <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
                  Creando...
                </>
              ) : (
                <>
                  <Plus className="w-4 h-4 mr-2" />
                  Nueva Publicación
                </>
              )}
            </Button>
          </div>
        </div>

        {/* Información del usuario */}
        <Card>
          <CardContent className="pt-4">
            <div className="flex items-center gap-4">
              <div className="w-12 h-12 bg-primary/10 rounded-full flex items-center justify-center">
                <FileText className="w-6 h-6 text-primary" />
              </div>
              <div>
                <h3 className="font-semibold">{user.username}</h3>
                <p className="text-sm text-muted-foreground">
                  Creador de Contenido
                </p>
                <div className="flex items-center gap-2 mt-1">
                  <Badge variant="outline" className="text-xs">
                    {user.role}
                  </Badge>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Vista de publicación individual en móvil */}
      {selectedPublication && viewMode === 'mobile' ? (
        <div className="space-y-4">
          <Button
            variant="outline"
            onClick={() => setSelectedPublication(null)}
            className="mb-4"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Volver a la Lista
          </Button>
          
          <PublicationMobileLayout
            publication={selectedPublication}
            onActionComplete={handleActionComplete}
          />
        </div>
      ) : (
        /* Dashboard principal */
        <PublicationDashboard
          userId={user.id}
          showCreateButton={true}
          className={viewMode === 'mobile' ? 'mobile-optimized' : ''}
        />
      )}

      {/* Información adicional */}
      <div className="mt-12 grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">Proceso de Revisión</CardTitle>
          </CardHeader>
          <CardContent className="text-sm text-muted-foreground space-y-2">
            <p>• Las publicaciones son revisadas en 1-3 días hábiles</p>
            <p>• Recibirás notificaciones sobre cambios de estado</p>
            <p>• Puedes realizar cambios en borradores y publicaciones que requieren revisión</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">Estados de Publicación</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="flex items-center gap-2 text-sm">
              <Badge variant="secondary">Borrador</Badge>
              <span className="text-muted-foreground">En edición</span>
            </div>
            <div className="flex items-center gap-2 text-sm">
              <Badge variant="outline">En Revisión</Badge>
              <span className="text-muted-foreground">Siendo moderado</span>
            </div>
            <div className="flex items-center gap-2 text-sm">
              <Badge variant="default">Publicado</Badge>
              <span className="text-muted-foreground">Disponible públicamente</span>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">Consejos</CardTitle>
          </CardHeader>
          <CardContent className="text-sm text-muted-foreground space-y-2">
            <p>• Asegúrate de que las imágenes tengan buena calidad</p>
            <p>• Completa todos los metadatos antes de enviar</p>
            <p>• Revisa las normas de la comunidad</p>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}