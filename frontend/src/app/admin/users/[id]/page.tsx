'use client'

import React from 'react'
import { UserDetailPage } from '@/components/admin/UserDetailPage'

interface UserDetailPageProps {
  params: {
    id: string
  }
}

export default function AdminUserDetailPage({ params }: UserDetailPageProps) {
  return <UserDetailPage userId={params.id} />
}