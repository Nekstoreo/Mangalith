import { z } from 'zod'

// Login validation schema
export const loginSchema = z.object({
  email: z
    .string()
    .min(1, 'El email es requerido')
    .email('El email no es válido'),
  password: z
    .string()
    .min(1, 'La contraseña es requerida')
    .min(6, 'La contraseña debe tener al menos 6 caracteres'),
})

export type LoginFormData = z.infer<typeof loginSchema>

// Register validation schema
export const registerSchema = z.object({
  username: z
    .string()
    .min(1, 'El nombre de usuario es requerido')
    .min(3, 'El nombre de usuario debe tener al menos 3 caracteres')
    .max(20, 'El nombre de usuario no puede tener más de 20 caracteres')
    .regex(/^[a-zA-Z0-9_]+$/, 'El nombre de usuario solo puede contener letras, números y guiones bajos'),
  email: z
    .string()
    .min(1, 'El email es requerido')
    .email('El email no es válido'),
  password: z
    .string()
    .min(1, 'La contraseña es requerida')
    .min(8, 'La contraseña debe tener al menos 8 caracteres')
    .regex(
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/,
      'La contraseña debe contener al menos una letra minúscula, una mayúscula y un número'
    ),
  confirmPassword: z
    .string()
    .min(1, 'La confirmación de contraseña es requerida'),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Las contraseñas no coinciden',
  path: ['confirmPassword'],
})

export type RegisterFormData = z.infer<typeof registerSchema>

// Change password validation schema
export const changePasswordSchema = z.object({
  currentPassword: z
    .string()
    .min(1, 'La contraseña actual es requerida'),
  newPassword: z
    .string()
    .min(1, 'La nueva contraseña es requerida')
    .min(8, 'La nueva contraseña debe tener al menos 8 caracteres')
    .regex(
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/,
      'La nueva contraseña debe contener al menos una letra minúscula, una mayúscula y un número'
    ),
  confirmNewPassword: z
    .string()
    .min(1, 'La confirmación de contraseña es requerida'),
}).refine((data) => data.newPassword === data.confirmNewPassword, {
  message: 'Las contraseñas no coinciden',
  path: ['confirmNewPassword'],
})

export type ChangePasswordFormData = z.infer<typeof changePasswordSchema>

// Profile update validation schema
export const profileSchema = z.object({
  username: z
    .string()
    .min(1, 'El nombre de usuario es requerido')
    .min(3, 'El nombre de usuario debe tener al menos 3 caracteres')
    .max(20, 'El nombre de usuario no puede tener más de 20 caracteres')
    .regex(/^[a-zA-Z0-9_]+$/, 'El nombre de usuario solo puede contener letras, números y guiones bajos'),
  email: z
    .string()
    .min(1, 'El email es requerido')
    .email('El email no es válido'),
})

export type ProfileFormData = z.infer<typeof profileSchema>
