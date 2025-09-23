import { useForm as useHookForm, UseFormProps, UseFormReturn, FieldValues } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import {
  loginSchema,
  registerSchema,
  changePasswordSchema,
  profileSchema,
  type LoginFormData,
  type RegisterFormData,
  type ChangePasswordFormData,
  type ProfileFormData,
} from '@/lib/validations/auth'

// Generic hook for forms with Zod validation
export function useForm<T extends z.ZodTypeAny>(
  schema: T,
  options?: Omit<UseFormProps<FieldValues>, 'resolver'>
): UseFormReturn<FieldValues> {
  // @ts-expect-error useHookForm return type conflict with FieldValues
  return useHookForm({
    // @ts-expect-error zodResolver expects specific Zod schema types that conflict with our generic T
    resolver: zodResolver(schema),
    mode: 'onChange',
    ...options,
  })
}

// Hook for login form
export function useLoginForm() {
  return useHookForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    mode: 'onChange',
  })
}

// Hook for register form
export function useRegisterForm() {
  return useHookForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    mode: 'onChange',
  })
}

// Hook for change password form
export function useChangePasswordForm() {
  return useHookForm<ChangePasswordFormData>({
    resolver: zodResolver(changePasswordSchema),
    mode: 'onChange',
  })
}

// Hook for profile form
export function useProfileForm() {
  return useHookForm<ProfileFormData>({
    resolver: zodResolver(profileSchema),
    mode: 'onChange',
  })
}
