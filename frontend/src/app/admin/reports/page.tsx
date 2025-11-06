'use client'

import { ReportStatistics } from '@/components/content-report/ReportStatistics'
import { ProtectedRoute } from '@/components/auth/ProtectedRoute'
import { RoleGuard } from '@/components/auth/RoleGuard'

export default function AdminReportsPage() {
  return (
    <ProtectedRoute>
      <RoleGuard allowedRoles={['Administrator']}>
        <div className="container mx-auto px-4 py-8">
          <ReportStatistics />
        </div>
      </RoleGuard>
    </ProtectedRoute>
  )
}