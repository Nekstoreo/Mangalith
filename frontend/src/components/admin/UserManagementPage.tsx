'use client'

import React, { useState, useEffect } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { LoadingSpinner } from '@/components/ui/loading-spinner'
import { UserListTable } from './UserListTable'
import { UserFilters } from './UserFilters'
import { UserBulkActions } from './UserBulkActions'
import { userManagementService } from '@/services/admin/user-management.service'
import { UserSummary, UserFilter, PaginatedResponse } from '@/lib/types'
import { Search, Plus, Download, RefreshCw } from 'lucide-react'
import { useRouter } from 'next/navigation'

export const UserManagementPage: React.FC = () => {
  const [users, setUsers] = useState<UserSummary[]>([])
  const [selectedUsers, setSelectedUsers] = useState<string[]>([])
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [filters, setFilters] = useState<UserFilter>({
    page: 1,
    pageSize: 20,
    sortBy: 'createdAt',
    sortDirection: 'desc'
  })
  const [pagination, setPagination] = useState({
    currentPage: 1,
    totalPages: 1,
    totalItems: 0,
    pageSize: 20
  })
  const [error, setError] = useState<string | null>(null)
  const router = useRouter()

  useEffect(() => {
    loadUsers()
  }, [filters])

  useEffect(() => {
    const delayedSearch = setTimeout(() => {
      if (searchQuery !== filters.email && searchQuery !== filters.fullName) {
        setFilters(prev => ({
          ...prev,
          email: searchQuery.includes('@') ? searchQuery : undefined,
          fullName: !searchQuery.includes('@') && searchQuery ? searchQuery : undefined,
          page: 1
        }))
      }
    }, 500)

    return () => clearTimeout(delayedSearch)
  }, [searchQuery])

  const loadUsers = async () => {
    try {
      setLoading(true)
      setError(null)
      
      const response = await userManagementService.getUsers(filters)
      
      if ('data' in response) {
        setUsers(response.data)
        setPagination({
          currentPage: response.currentPage,
          totalPages: response.totalPages,
          totalItems: response.totalItems,
          pageSize: response.pageSize
        })
      } else {
        // Handle case where response is directly an array
        setUsers(response as UserSummary[])
      }
    } catch (err) {
      console.error('Error loading users:', err)
      setError('Error al cargar los usuarios')
    } finally {
      setLoading(false)
    }
  }

  const handleFilterChange = (newFilters: Partial<UserFilter>) => {
    setFilters(prev => ({
      ...prev,
      ...newFilters,
      page: 1 // Reset to first page when filters change
    }))
  }

  const handlePageChange = (page: number) => {
    setFilters(prev => ({ ...prev, page }))
  }

  const handleUserSelect = (userId: string, selected: boolean) => {
    setSelectedUsers(prev => 
      selected 
        ? [...prev, userId]
        : prev.filter(id => id !== userId)
    )
  }

  const handleSelectAll = (selected: boolean) => {
    setSelectedUsers(selected ? users.map(user => user.id) : [])
  }

  const handleBulkActionComplete = () => {
    setSelectedUsers([])
    loadUsers()
  }

  const handleUserClick = (userId: string) => {
    router.push(`/admin/users/${userId}`)
  }

  const exportUsers = async () => {
    try {
      // This would typically generate a CSV or Excel file
      console.log('Exporting users with filters:', filters)
      // Implementation would depend on backend API
    } catch (err) {
      console.error('Error exporting users:', err)
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h2 className="text-2xl font-bold">Gestión de Usuarios</h2>
          <p className="text-muted-foreground">
            Administra usuarios, roles y permisos del sistema
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={exportUsers}>
            <Download className="h-4 w-4 mr-2" />
            Exportar
          </Button>
          <Button onClick={() => router.push('/admin/invitations')}>
            <Plus className="h-4 w-4 mr-2" />
            Invitar Usuario
          </Button>
        </div>
      </div>

      {/* Search and Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col lg:flex-row gap-4">
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Buscar por nombre o email..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-10"
                />
              </div>
            </div>
            <div className="flex gap-2">
              <UserFilters 
                filters={filters}
                onFiltersChange={handleFilterChange}
              />
              <Button 
                variant="outline" 
                size="icon"
                onClick={loadUsers}
                disabled={loading}
              >
                <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Bulk Actions */}
      {selectedUsers.length > 0 && (
        <UserBulkActions
          selectedUserIds={selectedUsers}
          onActionComplete={handleBulkActionComplete}
        />
      )}

      {/* Users Table */}
      <Card>
        <CardHeader>
          <CardTitle>
            Usuarios ({pagination.totalItems})
          </CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="flex items-center justify-center py-12">
              <LoadingSpinner size="lg" />
            </div>
          ) : error ? (
            <div className="text-center py-12">
              <p className="text-red-600 mb-4">{error}</p>
              <Button onClick={loadUsers}>Reintentar</Button>
            </div>
          ) : (
            <UserListTable
              users={users}
              selectedUsers={selectedUsers}
              onUserSelect={handleUserSelect}
              onSelectAll={handleSelectAll}
              onUserClick={handleUserClick}
              pagination={pagination}
              onPageChange={handlePageChange}
              sortBy={filters.sortBy}
              sortDirection={filters.sortDirection}
              onSort={(sortBy, sortDirection) => 
                setFilters(prev => ({ ...prev, sortBy, sortDirection }))
              }
            />
          )}
        </CardContent>
      </Card>
    </div>
  )
}