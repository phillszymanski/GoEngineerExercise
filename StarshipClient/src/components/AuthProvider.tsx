import { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { apiClient } from '../api/apiClient'

interface User {
  id: number
  email: string
  username: string
}

interface AuthContextType {
  user: User | null
  login: (email: string, password: string) => Promise<void>
  register: (email: string, username: string, password: string) => Promise<void>
  logout: () => void
  isLoading: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export { AuthContext }

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const validateSession = async () => {
      try {
        const response = await apiClient.get('/auth/me')
        setUser(response.data)
      } catch (error) {
        setUser(null)
      } finally {
        setIsLoading(false)
      }
    }

    validateSession()
  }, [])

  const login = async (email: string, password: string) => {
    const response = await apiClient.post('/auth/login', { email, password })
    setUser(response.data)
  }

  const register = async (email: string, username: string, password: string) => {
    const response = await apiClient.post('/auth/register', { email, username, password })
    setUser(response.data)
  }

  const logout = async () => {
    try {
      await apiClient.post('/auth/logout')
    } catch (error) {
      console.log('Logout failed');
      throw error;
    } finally {
      setUser(null)
    }
  }

  return (
    <AuthContext.Provider value={{ user, login, register, logout, isLoading }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}