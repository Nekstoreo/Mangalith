'use client'

import React from 'react'
import Link from 'next/link'
import { usePermissions } from '@/hooks'
import { UserRole, PermissionString } from '@/lib/types'

interface NavigationItemProps {
  href: string
  children: React.ReactNode
  permission?: PermissionString
  role?: UserRole
  className?: string
}

/**
 * Navigation link that only renders if user has required permissions
 */
export const ProtectedNavLink: React.FC<NavigationItemProps> = ({
  href,
  children,
  permission,
  role,
  className = ''
}) => {
  const { hasPermission, hasRole, isAuthenticated } = usePermissions()
  
  if (!isAuthenticated) return null
  
  if (permission && !hasPermission(permission)) return null
  
  if (role !== undefined && !hasRole(role)) return null
  
  return (
    <Link href={href} className={className}>
      {children}
    </Link>
  )
}

/**
 * Navigation menu that filters items based on permissions
 */
export const ProtectedNavMenu: React.FC<{
  items: Array<{
    href: string
    label: string
    permission?: PermissionString
    role?: UserRole
    icon?: React.ReactNode
    children?: Array<{
      href: string
      label: string
      permission?: PermissionString
      role?: UserRole
    }>
  }>
  className?: string
  itemClassName?: string
}> = ({ items, className, itemClassName }) => {
  const { hasPermission, hasRole, isAuthenticated } = usePermissions()
  
  if (!isAuthenticated) return null
  
  const filteredItems = items.filter(item => {
    if (item.permission && !hasPermission(item.permission)) return false
    if (item.role !== undefined && !hasRole(item.role)) return false
    return true
  })
  
  return (
    <nav className={className}>
      {filteredItems.map((item, index) => (
        <div key={index}>
          <Link href={item.href} className={itemClassName}>
            {item.icon}
            {item.label}
          </Link>
          {item.children && (
            <div className="ml-4">
              {item.children
                .filter(child => {
                  if (child.permission && !hasPermission(child.permission)) return false
                  if (child.role !== undefined && !hasRole(child.role)) return false
                  return true
                })
                .map((child, childIndex) => (
                  <Link key={childIndex} href={child.href} className={itemClassName}>
                    {child.label}
                  </Link>
                ))}
            </div>
          )}
        </div>
      ))}
    </nav>
  )
}

/**
 * Admin navigation component
 */
export const AdminNavigation: React.FC<{ className?: string }> = ({ className }) => {
  const adminItems = [
    {
      href: '/admin/dashboard',
      label: 'Dashboard',
      role: UserRole.Administrator,
      icon: <span>📊</span>
    },
    {
      href: '/admin/users',
      label: 'Gestión de Usuarios',
      permission: 'user.manage' as PermissionString,
      icon: <span>👥</span>
    },
    {
      href: '/admin/invitations',
      label: 'Invitaciones',
      permission: 'user.invite' as PermissionString,
      icon: <span>✉️</span>
    },
    {
      href: '/admin/audit',
      label: 'Auditoría',
      permission: 'system.audit' as PermissionString,
      icon: <span>🔍</span>
    },
    {
      href: '/admin/settings',
      label: 'Configuración',
      permission: 'system.configure' as PermissionString,
      icon: <span>⚙️</span>
    }
  ]
  
  return <ProtectedNavMenu items={adminItems} className={className} />
}

/**
 * Moderator navigation component
 */
export const ModeratorNavigation: React.FC<{ className?: string }> = ({ className }) => {
  const moderatorItems = [
    {
      href: '/moderator/dashboard',
      label: 'Panel de Moderación',
      role: UserRole.Moderator,
      icon: <span>🛡️</span>
    },
    {
      href: '/moderator/content',
      label: 'Moderar Contenido',
      permission: 'manga.moderate' as PermissionString,
      icon: <span>📚</span>
    },
    {
      href: '/moderator/users',
      label: 'Usuarios',
      permission: 'user.read' as PermissionString,
      icon: <span>👤</span>
    },
    {
      href: '/moderator/reports',
      label: 'Reportes',
      permission: 'comment.moderate' as PermissionString,
      icon: <span>🚨</span>
    }
  ]
  
  return <ProtectedNavMenu items={moderatorItems} className={className} />
}

/**
 * User navigation component
 */
export const UserNavigation: React.FC<{ className?: string }> = ({ className }) => {
  const userItems = [
    {
      href: '/dashboard',
      label: 'Mi Dashboard',
      role: UserRole.Reader,
      icon: <span>🏠</span>
    },
    {
      href: '/library',
      label: 'Mi Biblioteca',
      role: UserRole.Reader,
      icon: <span>📖</span>
    },
    {
      href: '/upload',
      label: 'Subir Contenido',
      permission: 'file.upload' as PermissionString,
      icon: <span>⬆️</span>
    },
    {
      href: '/profile',
      label: 'Mi Perfil',
      role: UserRole.Reader,
      icon: <span>👤</span>
    }
  ]
  
  return <ProtectedNavMenu items={userItems} className={className} />
}

/**
 * Breadcrumb component with permission checking
 */
export const ProtectedBreadcrumb: React.FC<{
  items: Array<{
    href?: string
    label: string
    permission?: PermissionString
    role?: UserRole
    current?: boolean
  }>
  className?: string
  separator?: React.ReactNode
}> = ({ items, className, separator = '/' }) => {
  const { hasPermission, hasRole, isAuthenticated } = usePermissions()
  
  if (!isAuthenticated) return null
  
  const filteredItems = items.filter(item => {
    if (item.permission && !hasPermission(item.permission)) return false
    if (item.role !== undefined && !hasRole(item.role)) return false
    return true
  })
  
  return (
    <nav className={className}>
      {filteredItems.map((item, index) => (
        <span key={index}>
          {index > 0 && <span className="mx-2">{separator}</span>}
          {item.href && !item.current ? (
            <Link href={item.href} className="text-blue-600 hover:text-blue-800">
              {item.label}
            </Link>
          ) : (
            <span className={item.current ? 'font-semibold' : ''}>
              {item.label}
            </span>
          )}
        </span>
      ))}
    </nav>
  )
}