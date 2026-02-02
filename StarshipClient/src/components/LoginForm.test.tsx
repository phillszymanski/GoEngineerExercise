import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import LoginForm from './LoginForm'
import { AuthContext } from './AuthProvider'

const mockLogin = vi.fn()
const mockRegister = vi.fn()

const mockAuthContext = {
  user: null,
  isLoading: false,
  login: mockLogin,
  register: mockRegister,
  logout: vi.fn(),
}

const renderLoginForm = () => (
  render(
      <AuthContext.Provider value={mockAuthContext}>
        <LoginForm />
      </AuthContext.Provider>
    )
)

describe('LoginForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should render login form by default', () => {
    renderLoginForm()
    
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument()
    expect(screen.getByPlaceholderText(/email/i)).toBeInTheDocument()
    expect(screen.getByPlaceholderText(/password/i)).toBeInTheDocument()
  })

  it('should call login with correct data', async () => {
    renderLoginForm()

    var emailInput = screen.getByPlaceholderText(/email/i)
    var passwordInput = screen.getByPlaceholderText(/password/i)
    var submitButton = screen.getByRole('button', { name: /sign in/i})
    await userEvent.type(emailInput, 'test@test.com')
    await userEvent.type(passwordInput, 'password')
    await userEvent.click(submitButton)
    expect(mockLogin).toHaveBeenCalledWith('test@test.com', 'password')
  })

  it('should switch to register mode', async () => {
    renderLoginForm()

    const toggleButton = screen.getByRole('button', { name: /don't have an account\? register/i});
    await userEvent.click(toggleButton);

    expect(screen.getByRole('button', { name: /^register$/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /already have an account\? sign in/i })).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/username/i)).toBeInTheDocument();
  });

  it('should call register with correct data', async () => {
    renderLoginForm()

    const toggleButton = screen.getByRole('button', { name: /don't have an account\? register/i});
    await userEvent.click(toggleButton);

    var usernameInput = screen.getByPlaceholderText(/username/i)
    var emailInput = screen.getByPlaceholderText(/email/i)
    var passwordInput = screen.getByPlaceholderText(/password/i)
    var submitButton = screen.getByRole('button', { name: /register/i})
    await userEvent.type(usernameInput, 'testuser')
    await userEvent.type(emailInput, 'test@test.com')
    await userEvent.type(passwordInput, 'password')
    await userEvent.click(submitButton)

    expect(mockRegister).toHaveBeenCalledWith('test@test.com', 'testuser', 'password')
  });

  it('should display error message when submit fails', async () => {
    mockLogin.mockRejectedValueOnce(new Error('Authentication failed'))
    renderLoginForm()

    var emailInput = screen.getByPlaceholderText(/email/i)
    var passwordInput = screen.getByPlaceholderText(/password/i)
    var submitButton = screen.getByRole('button', { name: /sign in/i})
    await userEvent.type(emailInput, 'test@test.com')
    await userEvent.type(passwordInput, 'password')
    await userEvent.click(submitButton)

    expect(await screen.findByText(/failed/i)).toBeInTheDocument()
  });

  it('should display be disabled while loading', async () => {
    const loadingContext = { ...mockAuthContext, isLoading: true}
    render (
    <AuthContext.Provider value={loadingContext}>
        <LoginForm />
      </AuthContext.Provider>
    )

    var submitButton = screen.getByRole('button', { name: /loading/i})
    expect(submitButton).toBeDisabled()
  });

  it('should validate empty fields', async () => {
    renderLoginForm()

    var submitButton = screen.getByRole('button', { name: /sign in/i})
    await userEvent.click(submitButton)

    expect(mockLogin).not.toHaveBeenCalled()
  });
})
