"use client"

import { MainLayout } from "@/components/layout"
import { LoadingSpinner } from "@/components/ui/loading-spinner"
import { Button } from "@/components/ui/button"
import { useAuthStore } from "@/stores/auth"
import { UserRole } from "@/lib/types"
import { 
  ReaderDashboard, 
  UploaderDashboard, 
  ModeratorDashboard, 
  AdminDashboard 
} from "@/components/dashboard"

export default function DashboardPage() {
  const { user, isAuthenticated, isLoading } = useAuthStore()

  if (isLoading) {
    return (
      <MainLayout>
        <div className="flex items-center justify-center py-12">
          <LoadingSpinner size="lg" />
        </div>
      </MainLayout>
    )
  }

  if (!isAuthenticated || !user) {
    return (
      <MainLayout>
        <div className="flex items-center justify-center h-64">
          <div className="text-center">
            <h2 className="text-xl font-semibold mb-2">Acceso requerido</h2>
            <p className="text-muted-foreground mb-4">
              Debes iniciar sesión para ver tu dashboard
            </p>
            <Button onClick={() => window.location.href = '/auth/login'}>
              Iniciar Sesión
            </Button>
          </div>
        </div>
      </MainLayout>
    )
  }

  const renderRoleSpecificDashboard = () => {
    switch (user.role) {
      case UserRole.Administrator:
        return <AdminDashboard />
      case UserRole.Moderator:
        return <ModeratorDashboard />
      case UserRole.Uploader:
        return <UploaderDashboard />
      case UserRole.Reader:
      default:
        return <ReaderDashboard />
    }
  }

  return (
    <MainLayout>
      {renderRoleSpecificDashboard()}
    </MainLayout>
  )
}
