'use client'

import React from 'react'
import { ProtectedRoute } from '@/components/auth'
import { UserRole } from '@/lib/types'
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs'
import { usePathname, useRouter } from 'next/navigation'
import { Users, FileText, UserPlus, Settings, BarChart3 } from 'lucide-react'

interface AdminLayoutProps {
  children: React.ReactNode
}

export default function AdminLayout({ children }: AdminLayoutProps) {
  const pathname = usePathname()
  const router = useRouter()

  const getActiveTab = () => {
    if (pathname.includes('/admin/users')) return 'users'
    if (pathname.includes('/admin/audit')) return 'audit'
    if (pathname.includes('/admin/invitations')) return 'invitations'
    if (pathname.includes('/admin/settings')) return 'settings'
    return 'dashboard'
  }

  const handleTabChange = (value: string) => {
    switch (value) {
      case 'dashboard':
        router.push('/admin')
        break
      case 'users':
        router.push('/admin/users')
        break
      case 'audit':
        router.push('/admin/audit')
        break
      case 'invitations':
        router.push('/admin/invitations')
        break
      case 'settings':
        router.push('/admin/settings')
        break
    }
  }

  return (
    <ProtectedRoute role={UserRole.Moderator}>
      <div className="container mx-auto py-6">
        <div className="mb-6">
          <h1 className="text-3xl font-bold">Panel de Administración</h1>
          <p className="text-muted-foreground">
            Gestiona usuarios, revisa logs de auditoría y configura el sistema
          </p>
        </div>

        <Tabs value={getActiveTab()} onValueChange={handleTabChange} className="w-full">
          <TabsList className="grid w-full grid-cols-5">
            <TabsTrigger value="dashboard" className="flex items-center gap-2">
              <BarChart3 className="h-4 w-4" />
              Dashboard
            </TabsTrigger>
            <TabsTrigger value="users" className="flex items-center gap-2">
              <Users className="h-4 w-4" />
              Usuarios
            </TabsTrigger>
            <TabsTrigger value="audit" className="flex items-center gap-2">
              <FileText className="h-4 w-4" />
              Auditoría
            </TabsTrigger>
            <TabsTrigger value="invitations" className="flex items-center gap-2">
              <UserPlus className="h-4 w-4" />
              Invitaciones
            </TabsTrigger>
            <TabsTrigger value="settings" className="flex items-center gap-2">
              <Settings className="h-4 w-4" />
              Configuración
            </TabsTrigger>
          </TabsList>

          <div className="mt-6">
            {children}
          </div>
        </Tabs>
      </div>
    </ProtectedRoute>
  )
}