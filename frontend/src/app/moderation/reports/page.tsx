'use client'

import { ReportManagement } from '@/components/content-report/ReportManagement'
import { ProtectedRoute } from '@/components/auth/ProtectedRoute'
import { RoleGuard } from '@/components/auth/RoleGuard'

export default function ModerationReportsPage() {
  return (
    <ProtectedRoute>
      <RoleGuard allowedRoles={['Moderator', 'Administrator']}>
        <div className="container mx-auto px-4 py-8">
          <ReportManagement />
        </div>
      </RoleGuard>
    </ProtectedRoute>
  )
}