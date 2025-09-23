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

  // Get token from cookies or headers
  const token = request.cookies.get('mangalith-auth-token')?.value ||
                request.headers.get('authorization')?.replace('Bearer ', '')

  const isAuthenticated = !!token
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

  // Redirect authenticated users away from auth pages
  if (isAuthenticated && isAuthRoute) {
    return NextResponse.redirect(new URL('/dashboard', request.url))
  }

  // Redirect unauthenticated users away from protected pages
  if (!isAuthenticated && isProtectedRoute) {
    const loginUrl = new URL('/auth/login', request.url)
    loginUrl.searchParams.set('redirect', pathname)
    return NextResponse.redirect(loginUrl)
  }

  // Allow access to public routes and assets
  if (isPublicRoute || isAuthRoute) {
    return NextResponse.next()
  }

  // For protected routes, verify token exists (additional validation could be added here)
  if (isProtectedRoute) {
    if (!isAuthenticated) {
      const loginUrl = new URL('/auth/login', request.url)
      loginUrl.searchParams.set('redirect', pathname)
      return NextResponse.redirect(loginUrl)
    }
  }

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
