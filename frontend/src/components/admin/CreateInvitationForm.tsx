'use client'

import React, { useState } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { UserRoleSelect } from './UserRoleSelect'
import { invitationService } from '@/services/admin/invitations.service'
import { CreateInvitationRequest, UserRole } from '@/lib/types'
import { Mail, X, Plus, Trash2 } from 'lucide-react'

interface CreateInvitationFormProps {
  onInvitationCreated: () => void
  onCancel: () => void
}

export const CreateInvitationForm: React.FC<CreateInvitationFormProps> = ({
  onInvitationCreated,
  onCancel
}) => {
  const [formData, setFormData] = useState<CreateInvitationRequest>({
    email: '',
    targetRole: UserRole.Reader,
    message: '',
    expirationHours: 168 // 7 days default
  })
  const [bulkEmails, setBulkEmails] = useState<string[]>([''])
  const [isBulkMode, setIsBulkMode] = useState(false)
  const [loading, setLoading] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  const validateEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(email)
  }

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {}

    if (isBulkMode) {
      const validEmails = bulkEmails.filter(email => email.trim() && validateEmail(email.trim()))
      if (validEmails.length === 0) {
        newErrors.bulkEmails = 'Debe proporcionar al menos un email válido'
      }
      
      const invalidEmails = bulkEmails.filter(email => email.trim() && !validateEmail(email.trim()))
      if (invalidEmails.length > 0) {
        newErrors.bulkEmails = `Emails inválidos: ${invalidEmails.join(', ')}`
      }
    } else {
      if (!formData.email.trim()) {
        newErrors.email = 'El email es requerido'
      } else if (!validateEmail(formData.email)) {
        newErrors.email = 'El email no es válido'
      }
    }

    if (formData.expirationHours && (formData.expirationHours < 1 || formData.expirationHours > 8760)) {
      newErrors.expirationHours = 'Las horas de expiración deben estar entre 1 y 8760 (1 año)'
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!validateForm()) return

    try {
      setLoading(true)

      if (isBulkMode) {
        const validEmails = bulkEmails.filter(email => email.trim() && validateEmail(email.trim()))
        const invitations = validEmails.map(email => ({
          email: email.trim(),
          targetRole: formData.targetRole,
          message: formData.message,
          expirationHours: formData.expirationHours
        }))

        await invitationService.createBulkInvitations({
          invitations,
          sendEmails: true
        })

        alert(`${invitations.length} invitaciones creadas exitosamente`)
      } else {
        await invitationService.createInvitation(formData)
        alert('Invitación creada exitosamente')
      }

      onInvitationCreated()
    } catch (error) {
      console.error('Error creating invitation:', error)
      alert('Error al crear la invitación')
    } finally {
      setLoading(false)
    }
  }

  const addBulkEmailField = () => {
    setBulkEmails([...bulkEmails, ''])
  }

  const removeBulkEmailField = (index: number) => {
    setBulkEmails(bulkEmails.filter((_, i) => i !== index))
  }

  const updateBulkEmail = (index: number, value: string) => {
    const newEmails = [...bulkEmails]
    newEmails[index] = value
    setBulkEmails(newEmails)
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

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle className="flex items-center gap-2">
          <Mail className="h-5 w-5" />
          {isBulkMode ? 'Crear Invitaciones Masivas' : 'Nueva Invitación'}
        </CardTitle>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setIsBulkMode(!isBulkMode)}
          >
            {isBulkMode ? 'Modo Individual' : 'Modo Masivo'}
          </Button>
          <Button variant="ghost" size="sm" onClick={onCancel}>
            <X className="h-4 w-4" />
          </Button>
        </div>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Email Fields */}
          {isBulkMode ? (
            <div>
              <Label>Emails de destinatarios</Label>
              <div className="space-y-2 mt-2">
                {bulkEmails.map((email, index) => (
                  <div key={index} className="flex gap-2">
                    <Input
                      type="email"
                      value={email}
                      onChange={(e) => updateBulkEmail(index, e.target.value)}
                      placeholder="email@ejemplo.com"
                      className="flex-1"
                    />
                    {bulkEmails.length > 1 && (
                      <Button
                        type="button"
                        variant="outline"
                        size="icon"
                        onClick={() => removeBulkEmailField(index)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    )}
                  </div>
                ))}
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={addBulkEmailField}
                  className="flex items-center gap-2"
                >
                  <Plus className="h-4 w-4" />
                  Agregar Email
                </Button>
              </div>
              {errors.bulkEmails && (
                <p className="text-sm text-red-600 mt-1">{errors.bulkEmails}</p>
              )}
            </div>
          ) : (
            <div>
              <Label htmlFor="email">Email del destinatario</Label>
              <Input
                id="email"
                type="email"
                value={formData.email}
                onChange={(e) => setFormData(prev => ({ ...prev, email: e.target.value }))}
                placeholder="email@ejemplo.com"
                required
              />
              {errors.email && (
                <p className="text-sm text-red-600 mt-1">{errors.email}</p>
              )}
            </div>
          )}

          {/* Target Role */}
          <div>
            <Label htmlFor="targetRole">Rol objetivo</Label>
            <UserRoleSelect
              value={formData.targetRole}
              onChange={(role) => setFormData(prev => ({ ...prev, targetRole: role }))}
            />
            <p className="text-sm text-muted-foreground mt-1">
              El usuario recibirá este rol al aceptar la invitación
            </p>
          </div>

          {/* Expiration Hours */}
          <div>
            <Label htmlFor="expirationHours">Expiración (horas)</Label>
            <Select
              value={formData.expirationHours?.toString() || '168'}
              onValueChange={(value) => setFormData(prev => ({ 
                ...prev, 
                expirationHours: parseInt(value) 
              }))}
            >
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="24">24 horas (1 día)</SelectItem>
                <SelectItem value="72">72 horas (3 días)</SelectItem>
                <SelectItem value="168">168 horas (7 días)</SelectItem>
                <SelectItem value="336">336 horas (14 días)</SelectItem>
                <SelectItem value="720">720 horas (30 días)</SelectItem>
              </SelectContent>
            </Select>
            {errors.expirationHours && (
              <p className="text-sm text-red-600 mt-1">{errors.expirationHours}</p>
            )}
          </div>

          {/* Message */}
          <div>
            <Label htmlFor="message">Mensaje personalizado (opcional)</Label>
            <Textarea
              id="message"
              value={formData.message || ''}
              onChange={(e) => setFormData(prev => ({ ...prev, message: e.target.value }))}
              placeholder="Mensaje de bienvenida para el nuevo usuario..."
              rows={3}
            />
            <p className="text-sm text-muted-foreground mt-1">
              Este mensaje se incluirá en el email de invitación
            </p>
          </div>

          {/* Preview */}
          <div className="p-4 bg-gray-50 rounded-lg">
            <h4 className="font-medium mb-2">Vista previa de la invitación:</h4>
            <div className="text-sm space-y-1">
              <p><strong>Rol:</strong> {getRoleName(formData.targetRole)}</p>
              <p><strong>Expira en:</strong> {formData.expirationHours || 168} horas</p>
              {formData.message && (
                <p><strong>Mensaje:</strong> {formData.message}</p>
              )}
              {isBulkMode && (
                <p><strong>Destinatarios:</strong> {bulkEmails.filter(email => email.trim()).length} emails</p>
              )}
            </div>
          </div>

          {/* Actions */}
          <div className="flex gap-2 justify-end">
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancelar
            </Button>
            <Button type="submit" disabled={loading}>
              {loading ? 'Creando...' : isBulkMode ? 'Crear Invitaciones' : 'Crear Invitación'}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}