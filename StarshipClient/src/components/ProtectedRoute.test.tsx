import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import ProtectedRoute from './ProtectedRoute'
import { AuthContext } from './AuthProvider'

const mockAuthContext = {
  user: null,
  isLoading: false,
  login: vi.fn(),
  register: vi.fn(),
  logout: vi.fn(),
}

describe('ProtectedRoute', () => {
  it('should render children when user is authenticated', () => {
    const authenticatedContext = { ...mockAuthContext, user: { id: 1, email: 'test@test.com', username: 'test' } }
    
    render(
      <AuthContext.Provider value={authenticatedContext}>
        <ProtectedRoute>
          <div>Protected Content</div>
        </ProtectedRoute>
      </AuthContext.Provider>
    )
    
    expect(screen.getByText('Protected Content')).toBeInTheDocument()
  })

  it('should render LoginForm when user is null', () => {
    const authenticatedContext = { ...mockAuthContext, user: null }

    render(
      <AuthContext.Provider value={authenticatedContext}>
        <ProtectedRoute>
          <div>Protected Content</div>
        </ProtectedRoute>
      </AuthContext.Provider>
    )

    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument()
  })

  it('should show loading spinner when isLoading is true', () => {
    const authenticatedContext = { ...mockAuthContext, isLoading: true }

    render(
      <AuthContext.Provider value={authenticatedContext}>
        <ProtectedRoute>
          <div>Protected Content</div>
        </ProtectedRoute>
      </AuthContext.Provider>
    )

    expect(screen.getByText(/loading/i)).toBeInTheDocument()
  })
})
