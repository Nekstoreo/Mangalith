'use client'

import { useEffect } from 'react'
import { useAuthStore } from '@/stores/auth'
import { ReportStatus } from '@/components/content-report/ReportStatus'
import { ProtectedRoute } from '@/components/auth/ProtectedRoute'

export default function MyReportsPage() {
  const { user } = useAuthStore()

  return (
    <ProtectedRoute>
      <div className="container mx-auto px-4 py-8">
        <ReportStatus />
      </div>
    </ProtectedRoute>
  )
}