import { describe, it, expect, vi, beforeEach } from 'vitest'
import { act, render, renderHook, screen } from '@testing-library/react'
import { AuthProvider, useAuth } from './AuthProvider'
import { apiClient } from '../api/apiClient'

vi.mock('../api/apiClient', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
  },
}))

describe('AuthProvider', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(apiClient.get).mockRejectedValue(new Error('No session'))
  })

  it('should render children', async () => {
    const { findByText } = render(
      <AuthProvider>
        <div>Test Child</div>
      </AuthProvider>
    )
    
    expect(await findByText('Test Child')).toBeInTheDocument()
  })

  it('should set user data on login', async () => {
    const mockUserData = { email: 'test@test.com', password: 'pass' }
    vi.mocked(apiClient.post).mockResolvedValue({data: mockUserData})

    const { result } = renderHook(() => useAuth(), {
      wrapper:AuthProvider
    })

    await act(async () => {
      await result.current.login(mockUserData.email, mockUserData.password)
    })

    expect(result.current.user).toEqual(mockUserData)
    expect(apiClient.post).toHaveBeenCalledWith('/auth/login', {
      email: mockUserData.email,
      password: mockUserData.password
    })
  })

  it('should set user data on register', async () => {
    const mockUserData = { email: 'test@test.com', username: 'testuser', password: 'pass' }
    vi.mocked(apiClient.post).mockResolvedValue({data: mockUserData})

    const { result } = renderHook(() => useAuth(), {
      wrapper:AuthProvider
    })

    await act(async () => {
      await result.current.register(mockUserData.email, mockUserData.username, mockUserData.password)
    })

    expect(result.current.user).toEqual(mockUserData)
    expect(apiClient.post).toHaveBeenCalledWith('/auth/register', {
      email: mockUserData.email,
      username: mockUserData.username,
      password: mockUserData.password
    })
  })

  it('should clear user data on logout', async () => {
    const mockUserData = { email: 'test@test.com', username: 'testuser' }
    
    vi.mocked(apiClient.post).mockResolvedValueOnce({ data: mockUserData })
    vi.mocked(apiClient.post).mockResolvedValueOnce({})
    vi.mocked(apiClient.get).mockRejectedValueOnce(new Error())

    const { result } = renderHook(() => useAuth(), {
      wrapper: AuthProvider
    })

    await act(async () => {
      await result.current.login('test@test.com', 'password')
    })
    
    expect(result.current.user).toEqual(mockUserData)

    await act(async () => {
      await result.current.logout()
    })

    expect(result.current.user).toBeNull()
    expect(apiClient.post).toHaveBeenCalledWith('/auth/logout')
  })

  it('should throw error when useAuth is called outside AuthProvider', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

    expect(() => {
      renderHook(() => useAuth())
    }).toThrow('useAuth must be used within an AuthProvider')

    consoleSpy.mockRestore()
  })
})