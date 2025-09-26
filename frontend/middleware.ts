import { NextResponse } from 'next/server'
import type { NextRequest } from 'next/server'

// Routes that require authentication
const protectedRoutes = [
  '/dashboard',
  '/profile',
  '/library',
  '/manga',
]

// Routes that should redirect to dashboard if already authenticated
const authRoutes = [
  '/auth/login',
  '/auth/register',
]

// Public routes that don't need authentication
const publicRoutes = [
  '/',
]

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl

  // For now, don't check authentication in middleware since tokens are stored in localStorage
  // This will be handled by client-side authentication checks
  const isProtectedRoute = protectedRoutes.some(route =>
    pathname.startsWith(route)
  )
  const isAuthRoute = authRoutes.some(route =>
    pathname.startsWith(route)
  )
  const isPublicRoute = publicRoutes.includes(pathname) ||
                        pathname.startsWith('/_next/') ||
                        pathname.startsWith('/api/') ||
                        pathname.includes('.')

  // Allow access to auth routes (login/register) - authentication will be handled client-side
  if (isAuthRoute) {
    return NextResponse.next()
  }

  // Allow access to public routes and assets
  if (isPublicRoute) {
    return NextResponse.next()
  }

  // For protected routes, we'll let the client handle authentication
  // This avoids middleware redirect loops when localStorage tokens exist
  return NextResponse.next()
}

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
