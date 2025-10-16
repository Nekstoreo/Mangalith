import { NextRequest, NextResponse } from 'next/server'
import { UserRole, PermissionString } from '@/lib/types'

/**
 * Route protection configuration
 */
interface RouteProtection {
  path: string
  permission?: PermissionString
  role?: UserRole
  redirectTo?: string
  exact?: boolean
}

/**
 * Default route protections
 */
export const defaultRouteProtections: RouteProtection[] = [
  // Admin routes
  {
    path: '/admin',
    role: UserRole.Administrator,
    redirectTo: '/dashboard'
  },
  {
    path: '/admin/users',
    permission: 'user.manage',
    redirectTo: '/dashboard'
  },
  {
    path: '/admin/audit',
    permission: 'system.audit',
    redirectTo: '/dashboard'
  },
  {
    path: '/admin/invitations',
    permission: 'user.invite',
    redirectTo: '/dashboard'
  },
  {
    path: '/admin/settings',
    permission: 'system.configure',
    redirectTo: '/dashboard'
  },
  
  // Moderator routes
  {
    path: '/moderator',
    role: UserRole.Moderator,
    redirectTo: '/dashboard'
  },
  
  // Uploader routes
  {
    path: '/upload',
    permission: 'file.upload',
    redirectTo: '/dashboard'
  },
  
  // General authenticated routes
  {
    path: '/dashboard',
    role: UserRole.Reader,
    redirectTo: '/auth/login'
  },
  {
    path: '/library',
    role: UserRole.Reader,
    redirectTo: '/auth/login'
  },
  {
    path: '/profile',
    role: UserRole.Reader,
    redirectTo: '/auth/login'
  }
]

/**
 * Extract user information from JWT token
 */
function parseJwtToken(token: string): {
  userId: string
  role: UserRole
  permissions: string[]
} | null {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return {
      userId: payload.sub,
      role: payload.role,
      permissions: payload.permissions || []
    }
  } catch {
    return null
  }
}

/**
 * Check if user has required permission
 */
function hasPermission(userPermissions: string[], permission: PermissionString): boolean {
  return userPermissions.includes(permission)
}

/**
 * Check if user has required role
 */
function hasRole(userRole: UserRole, requiredRole: UserRole): boolean {
  return userRole >= requiredRole
}

/**
 * Find matching route protection
 */
function findRouteProtection(
  pathname: string,
  protections: RouteProtection[]
): RouteProtection | null {
  return protections.find(protection => {
    if (protection.exact) {
      return pathname === protection.path
    }
    return pathname.startsWith(protection.path)
  }) || null
}

/**
 * Auth middleware function
 */
export function createAuthMiddleware(
  customProtections: RouteProtection[] = []
) {
  const allProtections = [...defaultRouteProtections, ...customProtections]
  
  return function authMiddleware(request: NextRequest) {
    const { pathname } = request.nextUrl
    
    // Skip middleware for public routes
    const publicRoutes = ['/auth', '/api/auth', '/', '/about', '/contact']
    if (publicRoutes.some(route => pathname.startsWith(route))) {
      return NextResponse.next()
    }
    
    // Find route protection
    const protection = findRouteProtection(pathname, allProtections)
    if (!protection) {
      return NextResponse.next()
    }
    
    // Get token from cookies or headers
    const token = request.cookies.get('token')?.value || 
                  request.headers.get('authorization')?.replace('Bearer ', '')
    
    if (!token) {
      return NextResponse.redirect(
        new URL(protection.redirectTo || '/auth/login', request.url)
      )
    }
    
    // Parse token
    const user = parseJwtToken(token)
    if (!user) {
      return NextResponse.redirect(
        new URL(protection.redirectTo || '/auth/login', request.url)
      )
    }
    
    // Check role requirement
    if (protection.role !== undefined && !hasRole(user.role, protection.role)) {
      return NextResponse.redirect(
        new URL(protection.redirectTo || '/dashboard', request.url)
      )
    }
    
    // Check permission requirement
    if (protection.permission && !hasPermission(user.permissions, protection.permission)) {
      return NextResponse.redirect(
        new URL(protection.redirectTo || '/dashboard', request.url)
      )
    }
    
    return NextResponse.next()
  }
}

/**
 * Default auth middleware instance
 */
export const authMiddleware = createAuthMiddleware()

/**
 * Middleware configuration
 */
export const config = {
  matcher: [
    /*
     * Match all request paths except for the ones starting with:
     * - api (API routes)
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     */
    '/((?!api|_next/static|_next/image|favicon.ico).*)',
  ],
}