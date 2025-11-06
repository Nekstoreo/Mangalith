'use client'

import { useState, useEffect } from 'react'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Alert } from '@/components/ui/alert'
import { 
  Bell, 
  X, 
  CheckCircle, 
  AlertTriangle, 
  Info, 
  Clock,
  Users,
  FileText,
  Flag,
  Settings
} from 'lucide-react'

export interface ModerationNotification {
  id: string
  type: 'queue_update' | 'new_report' | 'bulk_complete' | 'system_alert' | 'assignment'
  title: string
  message: string
  priority: 'high' | 'medium' | 'low'
  timestamp: string
  isRead: boolean
  actionUrl?: string
  metadata?: Record<string, any>
}

interface ModerationNotificationsProps {
  notifications: ModerationNotification[]
  onMarkAsRead: (id: string) => void
  onMarkAllAsRead: () => void
  onDismiss: (id: string) => void
  onAction?: (notification: ModerationNotification) => void
  maxVisible?: number
  showSettings?: boolean
}

export function ModerationNotifications({
  notifications,
  onMarkAsRead,
  onMarkAllAsRead,
  onDismiss,
  onAction,
  maxVisible = 5,
  showSettings = false,
}: ModerationNotificationsProps) {
  const [isExpanded, setIsExpanded] = useState(false)
  const [filter, setFilter] = useState<'all' | 'unread' | 'high'>('unread')

  const unreadCount = notifications.filter(n => !n.isRead).length
  const highPriorityCount = notifications.filter(n => n.priority === 'high' && !n.isRead).length

  const filteredNotifications = notifications.filter(notification => {
    switch (filter) {
      case 'unread':
        return !notification.isRead
      case 'high':
        return notification.priority === 'high'
      default:
        return true
    }
  })

  const visibleNotifications = isExpanded 
    ? filteredNotifications 
    : filteredNotifications.slice(0, maxVisible)

  const getNotificationIcon = (type: ModerationNotification['type']) => {
    switch (type) {
      case 'queue_update':
        return <FileText className="w-4 h-4 text-blue-600" />
      case 'new_report':
        return <Flag className="w-4 h-4 text-red-600" />
      case 'bulk_complete':
        return <CheckCircle className="w-4 h-4 text-green-600" />
      case 'system_alert':
        return <AlertTriangle className="w-4 h-4 text-yellow-600" />
      case 'assignment':
        return <Users className="w-4 h-4 text-purple-600" />
      default:
        return <Info className="w-4 h-4 text-gray-600" />
    }
  }

  const getPriorityColor = (priority: ModerationNotification['priority']) => {
    switch (priority) {
      case 'high':
        return 'border-l-red-500 bg-red-50'
      case 'medium':
        return 'border-l-yellow-500 bg-yellow-50'
      default:
        return 'border-l-blue-500 bg-blue-50'
    }
  }

  const formatTimeAgo = (timestamp: string) => {
    const now = new Date()
    const time = new Date(timestamp)
    const diffInMinutes = Math.floor((now.getTime() - time.getTime()) / (1000 * 60))
    
    if (diffInMinutes < 1) return 'Ahora'
    if (diffInMinutes < 60) return `${diffInMinutes}m`
    if (diffInMinutes < 1440) return `${Math.floor(diffInMinutes / 60)}h`
    return `${Math.floor(diffInMinutes / 1440)}d`
  }

  if (notifications.length === 0) {
    return (
      <Card className="p-4 text-center">
        <Bell className="w-8 h-8 mx-auto text-muted-foreground mb-2" />
        <p className="text-sm text-muted-foreground">No hay notificaciones</p>
      </Card>
    )
  }

  return (
    <Card className="overflow-hidden">
      {/* Header */}
      <div className="p-4 border-b bg-muted/30">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="relative">
              <Bell className="w-5 h-5" />
              {unreadCount > 0 && (
                <Badge className="absolute -top-2 -right-2 w-5 h-5 p-0 flex items-center justify-center text-xs bg-red-500 text-white">
                  {unreadCount > 99 ? '99+' : unreadCount}
                </Badge>
              )}
            </div>
            <div>
              <h3 className="font-semibold">Notificaciones</h3>
              <p className="text-xs text-muted-foreground">
                {unreadCount} sin leer
                {highPriorityCount > 0 && `, ${highPriorityCount} urgentes`}
              </p>
            </div>
          </div>
          <div className="flex items-center gap-2">
            {showSettings && (
              <Button variant="ghost" size="sm">
                <Settings className="w-4 h-4" />
              </Button>
            )}
            {unreadCount > 0 && (
              <Button variant="ghost" size="sm" onClick={onMarkAllAsRead}>
                Marcar todas
              </Button>
            )}
          </div>
        </div>

        {/* Filters */}
        <div className="flex gap-2 mt-3">
          {(['all', 'unread', 'high'] as const).map((filterType) => (
            <button
              key={filterType}
              onClick={() => setFilter(filterType)}
              className={`px-3 py-1 rounded-full text-xs transition-colors ${
                filter === filterType
                  ? 'bg-primary text-primary-foreground'
                  : 'bg-muted hover:bg-muted/80'
              }`}
            >
              {filterType === 'all' ? 'Todas' : filterType === 'unread' ? 'Sin leer' : 'Urgentes'}
            </button>
          ))}
        </div>
      </div>

      {/* Notifications List */}
      <div className="max-h-96 overflow-y-auto">
        {visibleNotifications.length === 0 ? (
          <div className="p-4 text-center text-sm text-muted-foreground">
            No hay notificaciones para este filtro
          </div>
        ) : (
          <div className="space-y-1">
            {visibleNotifications.map((notification) => (
              <div
                key={notification.id}
                className={`p-3 border-l-4 transition-colors hover:bg-accent cursor-pointer ${
                  getPriorityColor(notification.priority)
                } ${notification.isRead ? 'opacity-60' : ''}`}
                onClick={() => !notification.isRead && onMarkAsRead(notification.id)}
              >
                <div className="flex items-start gap-3">
                  <div className="mt-0.5">
                    {getNotificationIcon(notification.type)}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-start justify-between gap-2">
                      <div className="flex-1">
                        <p className={`text-sm font-medium ${!notification.isRead ? 'font-semibold' : ''}`}>
                          {notification.title}
                        </p>
                        <p className="text-xs text-muted-foreground mt-1">
                          {notification.message}
                        </p>
                      </div>
                      <div className="flex items-center gap-2 flex-shrink-0">
                        <span className="text-xs text-muted-foreground">
                          {formatTimeAgo(notification.timestamp)}
                        </span>
                        <button
                          onClick={(e) => {
                            e.stopPropagation()
                            onDismiss(notification.id)
                          }}
                          className="text-muted-foreground hover:text-foreground"
                        >
                          <X className="w-3 h-3" />
                        </button>
                      </div>
                    </div>
                    
                    {/* Action Button */}
                    {notification.actionUrl && onAction && (
                      <button
                        onClick={(e) => {
                          e.stopPropagation()
                          onAction(notification)
                        }}
                        className="text-xs text-primary hover:text-primary/80 mt-2"
                      >
                        Ver detalles →
                      </button>
                    )}

                    {/* Metadata */}
                    {notification.metadata && (
                      <div className="flex flex-wrap gap-1 mt-2">
                        {Object.entries(notification.metadata).map(([key, value]) => (
                          <Badge key={key} variant="outline" className="text-xs">
                            {key}: {String(value)}
                          </Badge>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Footer */}
      {filteredNotifications.length > maxVisible && (
        <div className="p-3 border-t bg-muted/30">
          <button
            onClick={() => setIsExpanded(!isExpanded)}
            className="w-full text-sm text-primary hover:text-primary/80"
          >
            {isExpanded 
              ? 'Mostrar menos' 
              : `Ver ${filteredNotifications.length - maxVisible} más`
            }
          </button>
        </div>
      )}
    </Card>
  )
}

interface NotificationToastProps {
  notification: ModerationNotification
  onDismiss: () => void
  autoHide?: boolean
  duration?: number
}

export function NotificationToast({
  notification,
  onDismiss,
  autoHide = true,
  duration = 5000,
}: NotificationToastProps) {
  useEffect(() => {
    if (autoHide) {
      const timer = setTimeout(onDismiss, duration)
      return () => clearTimeout(timer)
    }
  }, [autoHide, duration, onDismiss])

  const getAlertVariant = () => {
    switch (notification.priority) {
      case 'high':
        return 'destructive'
      default:
        return 'default'
    }
  }

  return (
    <Alert className={`${getPriorityColor(notification.priority)} border shadow-lg`}>
      <div className="flex items-start gap-3">
        {getNotificationIcon(notification.type)}
        <div className="flex-1">
          <h4 className="font-medium">{notification.title}</h4>
          <p className="text-sm text-muted-foreground mt-1">{notification.message}</p>
        </div>
        <button
          onClick={onDismiss}
          className="text-muted-foreground hover:text-foreground"
        >
          <X className="w-4 h-4" />
        </button>
      </div>
    </Alert>
  )
}

// Hook for managing notifications
export function useModerationNotifications() {
  const [notifications, setNotifications] = useState<ModerationNotification[]>([])
  const [toasts, setToasts] = useState<ModerationNotification[]>([])

  const addNotification = (notification: Omit<ModerationNotification, 'id' | 'timestamp' | 'isRead'>) => {
    const newNotification: ModerationNotification = {
      ...notification,
      id: Date.now().toString(),
      timestamp: new Date().toISOString(),
      isRead: false,
    }

    setNotifications(prev => [newNotification, ...prev])
    
    // Show as toast for high priority
    if (notification.priority === 'high') {
      setToasts(prev => [newNotification, ...prev])
    }
  }

  const markAsRead = (id: string) => {
    setNotifications(prev => 
      prev.map(n => n.id === id ? { ...n, isRead: true } : n)
    )
  }

  const markAllAsRead = () => {
    setNotifications(prev => 
      prev.map(n => ({ ...n, isRead: true }))
    )
  }

  const dismiss = (id: string) => {
    setNotifications(prev => prev.filter(n => n.id !== id))
    setToasts(prev => prev.filter(n => n.id !== id))
  }

  const dismissToast = (id: string) => {
    setToasts(prev => prev.filter(n => n.id !== id))
  }

  return {
    notifications,
    toasts,
    addNotification,
    markAsRead,
    markAllAsRead,
    dismiss,
    dismissToast,
  }
}

function getPriorityColor(priority: ModerationNotification['priority']) {
  switch (priority) {
    case 'high':
      return 'border-l-red-500 bg-red-50'
    case 'medium':
      return 'border-l-yellow-500 bg-yellow-50'
    default:
      return 'border-l-blue-500 bg-blue-50'
  }
}