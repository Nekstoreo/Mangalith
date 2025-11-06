'use client'

import { Card } from '@/components/ui/card'

interface AnalyticsStat {
  label: string
  value: string | number
  change?: { value: number; isPositive: boolean }
  icon?: string
}

interface AnalyticsGridProps {
  stats: AnalyticsStat[]
}

export function AnalyticsGrid({ stats }: AnalyticsGridProps) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
      {stats.map((stat, i) => (
        <Card key={i} className="p-4 space-y-2">
          <div className="flex items-start justify-between">
            <p className="text-sm text-muted-foreground">{stat.label}</p>
            {stat.icon && <span className="text-2xl">{stat.icon}</span>}
          </div>
          <p className="text-2xl font-bold">{stat.value}</p>
          {stat.change && (
            <p
              className={`text-xs font-medium ${
                stat.change.isPositive ? 'text-green-600' : 'text-red-600'
              }`}
            >
              {stat.change.isPositive ? '↑' : '↓'} {Math.abs(stat.change.value)}% desde el mes pasado
            </p>
          )}
        </Card>
      ))}
    </div>
  )
}

interface AnalyticsChartProps {
  title: string
  data: Array<{ label: string; value: number }>
}

export function AnalyticsChart({ title, data }: AnalyticsChartProps) {
  const maxValue = Math.max(...data.map((d) => d.value))

  return (
    <Card className="p-4 space-y-4">
      <h3 className="font-semibold">{title}</h3>
      <div className="space-y-3">
        {data.map((item) => (
          <div key={item.label} className="space-y-1">
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">{item.label}</span>
              <span className="font-medium">{item.value}</span>
            </div>
            <div className="w-full bg-muted rounded-full h-2">
              <div
                className="bg-primary rounded-full h-2 transition-all"
                style={{ width: `${(item.value / maxValue) * 100}%` }}
              />
            </div>
          </div>
        ))}
      </div>
    </Card>
  )
}

interface ModeratorStatsProps {
  moderators: Array<{ name: string; actionsCompleted: number; averageTime: string }>
}

export function ModeratorStats({ moderators }: ModeratorStatsProps) {
  return (
    <Card className="p-4 space-y-4">
      <h3 className="font-semibold">Rendimiento de Moderadores</h3>
      <div className="space-y-3">
        {moderators.map((mod) => (
          <div key={mod.name} className="flex items-center justify-between p-3 bg-muted rounded">
            <div>
              <p className="font-medium text-sm">{mod.name}</p>
              <p className="text-xs text-muted-foreground">{mod.actionsCompleted} acciones</p>
            </div>
            <div className="text-right">
              <p className="font-medium text-sm">{mod.averageTime}</p>
              <p className="text-xs text-muted-foreground">tiempo promedio</p>
            </div>
          </div>
        ))}
      </div>
    </Card>
  )
}

export function AdminAnalyticsDashboard() {
  // Mock data - En producción, esto vendría de endpoints reales
  const stats: AnalyticsStat[] = [
    {
      label: 'Publicaciones En Revisión',
      value: 24,
      change: { value: 12, isPositive: false },
      icon: '📋',
    },
    {
      label: 'Reportes Pendientes',
      value: 7,
      change: { value: 3, isPositive: true },
      icon: '🚩',
    },
    {
      label: 'Publicadas Hoy',
      value: 12,
      change: { value: 8, isPositive: true },
      icon: '✅',
    },
    {
      label: 'Usuarios Activos',
      value: '156',
      change: { value: 15, isPositive: true },
      icon: '👥',
    },
  ]

  const publicationStats = [
    { label: 'Borrador', value: 45 },
    { label: 'En Revisión', value: 24 },
    { label: 'Publicadas', value: 312 },
    { label: 'Rechazadas', value: 8 },
    { label: 'Archivadas', value: 23 },
  ]

  const reportStats = [
    { label: 'Spam', value: 12 },
    { label: 'Derechos de Autor', value: 5 },
    { label: 'Acoso', value: 3 },
    { label: 'Inapropiado', value: 8 },
    { label: 'Otro', value: 2 },
  ]

  const moderators = [
    { name: 'Moderador A', actionsCompleted: 156, averageTime: '2.5 min' },
    { name: 'Moderador B', actionsCompleted: 143, averageTime: '3.1 min' },
    { name: 'Moderador C', actionsCompleted: 129, averageTime: '2.8 min' },
  ]

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Panel de Análisis</h1>
        <p className="text-muted-foreground mt-1">
          Resumen del sistema de publicaciones y moderación
        </p>
      </div>

      <AnalyticsGrid stats={stats} />

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <AnalyticsChart title="Publicaciones por Estado" data={publicationStats} />
        <AnalyticsChart title="Reportes por Categoría" data={reportStats} />
      </div>

      <ModeratorStats moderators={moderators} />

      <Card className="p-4 space-y-4 bg-blue-50 border-blue-200">
        <h3 className="font-semibold text-blue-900">Actividad Reciente</h3>
        <div className="space-y-2 text-sm text-blue-800">
          <p>• Publicación #abc123 fue aprobada hace 5 minutos</p>
          <p>• Reporte de usuario sobre publicación #def456 hace 10 minutos</p>
          <p>• Publicación #ghi789 rechazada hace 15 minutos</p>
          <p>• Nuevo usuario registrado hace 20 minutos</p>
        </div>
      </Card>
    </div>
  )
}
