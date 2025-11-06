'use client'

import React from 'react'
import { useRouter, usePathname } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { 
  BarChart3, 
  FileText, 
  Users, 
  TrendingUp 
} from 'lucide-react'

export const AnalyticsNavigation: React.FC = () => {
  const router = useRouter()
  const pathname = usePathname()

  const navigationItems = [
    {
      path: '/admin/analytics',
      label: 'Moderación',
      icon: BarChart3,
      description: 'Métricas de moderación y alertas del sistema'
    },
    {
      path: '/admin/analytics/publications',
      label: 'Publicaciones',
      icon: FileText,
      description: 'Análisis del pipeline de contenido'
    },
    {
      path: '/admin/analytics/moderators',
      label: 'Moderadores',
      icon: Users,
      description: 'Rendimiento del equipo de moderación'
    }
  ]

  return (
    <Card className="mb-6">
      <CardContent className="p-4">
        <div className="flex flex-wrap gap-2">
          {navigationItems.map((item) => {
            const Icon = item.icon
            const isActive = pathname === item.path
            
            return (
              <Button
                key={item.path}
                variant={isActive ? "default" : "outline"}
                onClick={() => router.push(item.path)}
                className="flex items-center gap-2"
              >
                <Icon className="h-4 w-4" />
                {item.label}
              </Button>
            )
          })}
        </div>
        
        <div className="mt-3 text-sm text-muted-foreground">
          {navigationItems.find(item => item.path === pathname)?.description || 
           'Selecciona una sección de analíticas para ver métricas detalladas'}
        </div>
      </CardContent>
    </Card>
  )
}