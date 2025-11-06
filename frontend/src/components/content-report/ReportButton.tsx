'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { ContentReportForm } from './ContentReportForm'
import { useContentReport } from '@/hooks/useContentReport'
import { ContentReportCategory } from '@/services/content-report/client'
import { Flag, CheckCircle } from 'lucide-react'
import { Card } from '@/components/ui/card'

interface ReportButtonProps {
  publicationId: string
  variant?: 'button' | 'icon'
  size?: 'sm' | 'default' | 'lg'
  className?: string
}

export function ReportButton({ 
  publicationId, 
  variant = 'button',
  size = 'default',
  className = '' 
}: ReportButtonProps) {
  const [showForm, setShowForm] = useState(false)
  const [submitted, setSubmitted] = useState(false)
  const { createReport, loading } = useContentReport()

  const handleSubmit = async (category: ContentReportCategory, description: string) => {
    try {
      await createReport({
        publicationId,
        category,
        description
      })
      setSubmitted(true)
      setTimeout(() => {
        setShowForm(false)
        setSubmitted(false)
      }, 2000)
    } catch (error) {
      console.error('Error submitting report:', error)
      throw error
    }
  }

  if (showForm) {
    return (
      <>
        {/* Backdrop */}
        <div 
          className="fixed inset-0 bg-black/50 z-40" 
          onClick={() => setShowForm(false)} 
        />
        
        {/* Modal */}
        <div className="fixed left-1/2 top-1/2 transform -translate-x-1/2 -translate-y-1/2 w-full max-w-2xl max-h-[90vh] overflow-y-auto z-50">
          {submitted ? (
            <Card className="p-8 text-center space-y-4 mx-4">
              <CheckCircle className="w-16 h-16 text-green-500 mx-auto" />
              <h2 className="text-xl font-semibold">¡Reporte Enviado!</h2>
              <p className="text-muted-foreground">
                Gracias por ayudarnos a mantener la comunidad segura. 
                Revisaremos tu reporte pronto.
              </p>
            </Card>
          ) : (
            <ContentReportForm
              publicationId={publicationId}
              onSubmit={handleSubmit}
              onCancel={() => setShowForm(false)}
              isSubmitting={loading}
              className="mx-4"
            />
          )}
        </div>
      </>
    )
  }

  if (variant === 'icon') {
    return (
      <Button
        variant="ghost"
        size={size}
        onClick={() => setShowForm(true)}
        className={`text-muted-foreground hover:text-red-600 ${className}`}
        title="Reportar contenido"
      >
        <Flag className="w-4 h-4" />
      </Button>
    )
  }

  return (
    <Button
      variant="outline"
      size={size}
      onClick={() => setShowForm(true)}
      className={`text-muted-foreground hover:text-red-600 hover:border-red-300 ${className}`}
    >
      <Flag className="w-4 h-4 mr-2" />
      Reportar
    </Button>
  )
}