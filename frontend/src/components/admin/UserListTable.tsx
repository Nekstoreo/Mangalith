'use client'

import React from 'react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { UserSummary, UserRole } from '@/lib/types'
import { UserStatusBadge } from './UserStatusBadge'
import { UserRoleSelect } from './UserRoleSelect'
import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight, ArrowUpDown, ArrowUp, ArrowDown } from 'lucide-react'

interface UserListTableProps {
  users: UserSummary[]
  selectedUsers: string[]
  onUserSelect: (userId: string, selected: boolean) => void
  onSelectAll: (selected: boolean) => void
  onUserClick: (userId: string) => void
  pagination: {
    currentPage: number
    totalPages: number
    totalItems: number
    pageSize: number
  }
  onPageChange: (page: number) => void
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
  onSort: (sortBy: string, sortDirection: 'asc' | 'desc') => void
}

export const UserListTable: React.FC<UserListTableProps> = ({
  users,
  selectedUsers,
  onUserSelect,
  onSelectAll,
  onUserClick,
  pagination,
  onPageChange,
  sortBy,
  sortDirection,
  onSort
}) => {
  const allSelected = users.length > 0 && users.every(user => selectedUsers.includes(user.id))
  const someSelected = users.some(user => selectedUsers.includes(user.id))

  const handleSort = (field: string) => {
    if (sortBy === field) {
      onSort(field, sortDirection === 'asc' ? 'desc' : 'asc')
    } else {
      onSort(field, 'asc')
    }
  }

  const getSortIcon = (field: string) => {
    if (sortBy !== field) return <ArrowUpDown className="h-4 w-4" />
    return sortDirection === 'asc' ? <ArrowUp className="h-4 w-4" /> : <ArrowDown className="h-4 w-4" />
  }

  const getRoleName = (role: UserRole): string => {
    switch (role) {
      case UserRole.Reader:
        return 'Lector'
      case UserRole.Uploader:
        return 'Subidor'
      case UserRole.Moderator:
        return 'Moderador'
      case UserRole.Administrator:
        return 'Administrador'
      default:
        return 'Desconocido'
    }
  }

  const getRoleColor = (role: UserRole): string => {
    switch (role) {
      case UserRole.Reader:
        return 'bg-blue-100 text-blue-800'
      case UserRole.Uploader:
        return 'bg-green-100 text-green-800'
      case UserRole.Moderator:
        return 'bg-yellow-100 text-yellow-800'
      case UserRole.Administrator:
        return 'bg-red-100 text-red-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  return (
    <div className="space-y-4">
      {/* Table */}
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="border-b">
              <th className="text-left p-2">
                <input
                  type="checkbox"
                  checked={allSelected}
                  ref={input => {
                    if (input) input.indeterminate = someSelected && !allSelected
                  }}
                  onChange={(e) => onSelectAll(e.target.checked)}
                  className="rounded border-gray-300"
                />
              </th>
              <th className="text-left p-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleSort('fullName')}
                  className="h-auto p-0 font-semibold"
                >
                  Nombre
                  {getSortIcon('fullName')}
                </Button>
              </th>
              <th className="text-left p-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleSort('email')}
                  className="h-auto p-0 font-semibold"
                >
                  Email
                  {getSortIcon('email')}
                </Button>
              </th>
              <th className="text-left p-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleSort('role')}
                  className="h-auto p-0 font-semibold"
                >
                  Rol
                  {getSortIcon('role')}
                </Button>
              </th>
              <th className="text-left p-2">Estado</th>
              <th className="text-left p-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleSort('totalUploads')}
                  className="h-auto p-0 font-semibold"
                >
                  Subidas
                  {getSortIcon('totalUploads')}
                </Button>
              </th>
              <th className="text-left p-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleSort('createdAt')}
                  className="h-auto p-0 font-semibold"
                >
                  Registro
                  {getSortIcon('createdAt')}
                </Button>
              </th>
              <th className="text-left p-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleSort('lastLoginAt')}
                  className="h-auto p-0 font-semibold"
                >
                  Último Acceso
                  {getSortIcon('lastLoginAt')}
                </Button>
              </th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr 
                key={user.id} 
                className="border-b hover:bg-gray-50 cursor-pointer"
                onClick={() => onUserClick(user.id)}
              >
                <td className="p-2" onClick={(e) => e.stopPropagation()}>
                  <input
                    type="checkbox"
                    checked={selectedUsers.includes(user.id)}
                    onChange={(e) => onUserSelect(user.id, e.target.checked)}
                    className="rounded border-gray-300"
                  />
                </td>
                <td className="p-2">
                  <div>
                    <div className="font-medium">{user.fullName}</div>
                    {user.username && (
                      <div className="text-sm text-muted-foreground">@{user.username}</div>
                    )}
                  </div>
                </td>
                <td className="p-2 text-sm">{user.email}</td>
                <td className="p-2">
                  <Badge className={getRoleColor(user.role)}>
                    {getRoleName(user.role)}
                  </Badge>
                </td>
                <td className="p-2">
                  <UserStatusBadge isActive={user.isActive} />
                </td>
                <td className="p-2 text-sm">{user.totalUploads}</td>
                <td className="p-2 text-sm">
                  {new Date(user.createdAt).toLocaleDateString()}
                </td>
                <td className="p-2 text-sm">
                  {user.lastLoginAt 
                    ? new Date(user.lastLoginAt).toLocaleDateString()
                    : 'Nunca'
                  }
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {pagination.totalPages > 1 && (
        <div className="flex items-center justify-between">
          <div className="text-sm text-muted-foreground">
            Mostrando {((pagination.currentPage - 1) * pagination.pageSize) + 1} a{' '}
            {Math.min(pagination.currentPage * pagination.pageSize, pagination.totalItems)} de{' '}
            {pagination.totalItems} usuarios
          </div>
          
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => onPageChange(1)}
              disabled={pagination.currentPage === 1}
            >
              <ChevronsLeft className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => onPageChange(pagination.currentPage - 1)}
              disabled={pagination.currentPage === 1}
            >
              <ChevronLeft className="h-4 w-4" />
            </Button>
            
            <span className="text-sm">
              Página {pagination.currentPage} de {pagination.totalPages}
            </span>
            
            <Button
              variant="outline"
              size="sm"
              onClick={() => onPageChange(pagination.currentPage + 1)}
              disabled={pagination.currentPage === pagination.totalPages}
            >
              <ChevronRight className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => onPageChange(pagination.totalPages)}
              disabled={pagination.currentPage === pagination.totalPages}
            >
              <ChevronsRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}

      {users.length === 0 && (
        <div className="text-center py-12">
          <p className="text-muted-foreground">No se encontraron usuarios</p>
        </div>
      )}
    </div>
  )
}