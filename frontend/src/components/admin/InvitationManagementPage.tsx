'use client'

import React, { useState, useEffect } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { LoadingSpinner } from '@/components/ui/loading-spinner'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { InvitationTable } from './InvitationTable'
import { CreateInvitationForm } from './CreateInvitationForm'
import { invitationService } from '@/services/admin/invitations.service'
import { InvitationSummary, InvitationFilter, InvitationStatistics, PaginatedResponse, InvitationStatus } from '@/lib/types'
import { Search, Plus, RefreshCw, Mail, Clock, CheckCircle, XCircle } from 'lucide-react'

export const InvitationManagementPage: React.FC = () => {
  const [invitations, setInvitations] = useState<InvitationSummary[]>([])
  const [statistics, setStatistics] = useState<InvitationStatistics | null>(null)
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [activeTab, setActiveTab] = useState('all')
  const [filters, setFilters] = useState<InvitationFilter>({
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
  const [showCreateForm, setShowCreateForm] = useState(false)

  useEffect(() => {
    loadInvitations()
    loadStatistics()
  }, [filters])

  useEffect(() => {
    const delayedSearch = setTimeout(() => {
      if (searchQuery !== filters.email) {
        setFilters(prev => ({
          ...prev,
          email: searchQuery || undefined,
          page: 1
        }))
      }
    }, 500)

    return () => clearTimeout(delayedSearch)
  }, [searchQuery])

  useEffect(() => {
    // Update filters based on active tab
    const statusFilter = getStatusFromTab(activeTab)
    setFilters(prev => ({
      ...prev,
      status: statusFilter,
      page: 1
    }))
  }, [activeTab])

  const getStatusFromTab = (tab: string): InvitationStatus | undefined => {
    switch (tab) {
      case 'pending':
        return InvitationStatus.Pending
      case 'accepted':
        return InvitationStatus.Accepted
      case 'expired':
        return InvitationStatus.Expired
      case 'cancelled':
        return InvitationStatus.Cancelled
      default:
        return undefined
    }
  }

  const loadInvitations = async () => {
    try {
      setLoading(true)
      setError(null)
      
      const response = await invitationService.getInvitations(filters)
      
      if ('data' in response) {
        setInvitations(response.data)
        setPagination({
          currentPage: response.currentPage,
          totalPages: response.totalPages,
          totalItems: response.totalItems,
          pageSize: response.pageSize
        })
      } else {
        setInvitations(response as InvitationSummary[])
      }
    } catch (err) {
      console.error('Error loading invitations:', err)
      setError('Error al cargar las invitaciones')
    } finally {
      setLoading(false)
    }
  }

  const loadStatistics = async () => {
    try {
      const stats = await invitationService.getStatistics()
      setStatistics(stats)
    } catch (err) {
      console.error('Error loading statistics:', err)
    }
  }

  const handlePageChange = (page: number) => {
    setFilters(prev => ({ ...prev, page }))
  }

  const handleInvitationCreated = () => {
    setShowCreateForm(false)
    loadInvitations()
    loadStatistics()
  }

  const handleInvitationAction = () => {
    loadInvitations()
    loadStatistics()
  }

  const getTabCount = (status: InvitationStatus | 'all') => {
    if (!statistics) return 0
    
    switch (status) {
      case 'all':
        return statistics.totalInvitations
      case InvitationStatus.Pending:
        return statistics.pendingInvitations
      case InvitationStatus.Accepted:
        return statistics.acceptedInvitations
      case InvitationStatus.Expired:
        return statistics.expiredInvitations
      case InvitationStatus.Cancelled:
        return statistics.invitationsByStatus[InvitationStatus.Cancelled] || 0
      default:
        return 0
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h2 className="text-2xl font-bold">Gestión de Invitaciones</h2>
          <p className="text-muted-foreground">
            Invita nuevos usuarios y gestiona invitaciones pendientes
          </p>
        </div>
        <Button onClick={() => setShowCreateForm(true)}>
          <Plus className="h-4 w-4 mr-2" />
          Nueva Invitación
        </Button>
      </div>

      {/* Statistics Cards */}
      {statistics && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Invitaciones</CardTitle>
              <Mail className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{statistics.totalInvitations}</div>
              <p className="text-xs text-muted-foreground">
                Todas las invitaciones
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Pendientes</CardTitle>
              <Clock className="h-4 w-4 text-yellow-600" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-yellow-600">{statistics.pendingInvitations}</div>
              <p className="text-xs text-muted-foreground">
                {statistics.expiringInNext7Days} expiran pronto
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Aceptadas</CardTitle>
              <CheckCircle className="h-4 w-4 text-green-600" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-green-600">{statistics.acceptedInvitations}</div>
              <p className="text-xs text-muted-foreground">
                {statistics.acceptanceRate}% tasa de aceptación
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Expiradas</CardTitle>
              <XCircle className="h-4 w-4 text-red-600" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-red-600">{statistics.expiredInvitations}</div>
              <p className="text-xs text-muted-foreground">
                Tiempo promedio: {Math.round(statistics.averageAcceptanceTimeHours)}h
              </p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Create Invitation Form */}
      {showCreateForm && (
        <CreateInvitationForm
          onInvitationCreated={handleInvitationCreated}
          onCancel={() => setShowCreateForm(false)}
        />
      )}

      {/* Search */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col lg:flex-row gap-4">
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Buscar por email..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-10"
                />
              </div>
            </div>
            <Button 
              variant="outline" 
              size="icon"
              onClick={loadInvitations}
              disabled={loading}
            >
              <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Invitations Table with Tabs */}
      <Card>
        <CardHeader>
          <CardTitle>Invitaciones</CardTitle>
        </CardHeader>
        <CardContent>
          <Tabs value={activeTab} onValueChange={setActiveTab}>
            <TabsList className="grid w-full grid-cols-5">
              <TabsTrigger value="all">
                Todas ({getTabCount('all')})
              </TabsTrigger>
              <TabsTrigger value="pending">
                Pendientes ({getTabCount(InvitationStatus.Pending)})
              </TabsTrigger>
              <TabsTrigger value="accepted">
                Aceptadas ({getTabCount(InvitationStatus.Accepted)})
              </TabsTrigger>
              <TabsTrigger value="expired">
                Expiradas ({getTabCount(InvitationStatus.Expired)})
              </TabsTrigger>
              <TabsTrigger value="cancelled">
                Canceladas ({getTabCount(InvitationStatus.Cancelled)})
              </TabsTrigger>
            </TabsList>

            <TabsContent value={activeTab} className="mt-6">
              {loading ? (
                <div className="flex items-center justify-center py-12">
                  <LoadingSpinner size="lg" />
                </div>
              ) : error ? (
                <div className="text-center py-12">
                  <p className="text-red-600 mb-4">{error}</p>
                  <Button onClick={loadInvitations}>Reintentar</Button>
                </div>
              ) : (
                <InvitationTable
                  invitations={invitations}
                  pagination={pagination}
                  onPageChange={handlePageChange}
                  onInvitationAction={handleInvitationAction}
                  sortBy={filters.sortBy}
                  sortDirection={filters.sortDirection}
                  onSort={(sortBy, sortDirection) => 
                    setFilters(prev => ({ ...prev, sortBy, sortDirection }))
                  }
                />
              )}
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>
    </div>
  )
}