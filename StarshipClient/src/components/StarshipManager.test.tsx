import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import StarshipManager from './StarshipManager'
import { useAuth } from './AuthProvider'
import { useStarships } from '../hooks/useStarships'
import * as starshipService from '../api/starshipService'
import userEvent from '@testing-library/user-event'

// Mock dependencies
vi.mock('./AuthProvider')
vi.mock('../hooks/useStarships')
vi.mock('../api/starshipService')

const mockUser = {
  id: 1,
  email: 'test@test.com',
  username: 'testuser'
}

const mockLogin = vi.fn();
const mockLogout = vi.fn();
const mockRegister = vi.fn();
const mockRefetch = vi.fn();

const mockStarships = [
  {
    id: 1,
    name: 'Mock Starship',
    model: 'Mock Model',
    manufacturer: 'Mock Manufacturer',
    cost_in_credits: '149999',
    length: '12.5',
    max_atmosphering_speed: '1050',
    crew: '1',
    passengers: '0',
    cargo_capacity: '110',
    consumables: '1 week',
    hyperdrive_rating: '1.0',
    MGLT: '100',
    starship_class: 'Starfighter',
    pilots: [],
    films: [],
    created: new Date(),
    edited: new Date(),
    url: ''
  }
]

const renderStarshipManager = () => (
  render(
      <StarshipManager />
    )
)

describe('StarshipManager', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    
    // Default mock implementations
    vi.mocked(useAuth).mockReturnValue({
      user: mockUser,
      logout: mockLogout,
      isLoading: false,
      login: mockLogin,
      register: mockRegister
    })

    vi.mocked(useStarships).mockReturnValue({
      starships: mockStarships,
      loading: false,
      error: null,
      refetch: mockRefetch
    })

    // vi.mocked(starshipService.addStarship).mockReturnValue({})
  })

  it('should render starship manager with user info', () => {
    renderStarshipManager()

    expect(screen.getByText('Starship Manager')).toBeInTheDocument()
  })

  it('should call logout when logout button is clicked', async () => {
    renderStarshipManager()

    const logoutButton = screen.getByRole('button', {name: /logout/i})
    await userEvent.click(logoutButton)

    expect(mockLogout).toHaveBeenCalledOnce()
  })

  it('should open modal when add button is clicked', async () => {
    renderStarshipManager()

    const addButton = screen.getByRole('button', {name: /add a starship/i})
    await userEvent.click(addButton)

    expect(screen.getByText('Add New Starship')).toBeInTheDocument()
  })

  it('should close modal and clear editing state on modal close', async () => {
    renderStarshipManager()

    const addButton = screen.getByRole('button', {name: /add a starship/i})
    await userEvent.click(addButton)
    
    expect(screen.getByText('Add New Starship')).toBeInTheDocument()
    
    const closeButton = screen.getByRole('button', {name: /cancel/i})
    await userEvent.click(closeButton)
    
    expect(screen.queryByText('Add New Starship')).not.toBeInTheDocument()
  })

  it('should call addStarship and refetch when adding new starship', async () => {
    vi.mocked(starshipService.addStarship).mockResolvedValueOnce({})

    renderStarshipManager()

    const addButton = screen.getByRole('button', {name: /add a starship/i})
    await userEvent.click(addButton)

    await userEvent.type(screen.getByLabelText(/name/i), 'Test Name')
    await userEvent.type(screen.getByLabelText(/model/i), 'Test Model')
    await userEvent.type(screen.getByLabelText(/manufacturer/i), 'Test Manufacturer')

    const submitButton = screen.getByRole('button', {name: /add starship/i})
    await userEvent.click(submitButton)

    await waitFor(() => {
        expect(starshipService.addStarship).toHaveBeenCalledOnce()
        expect(mockRefetch).toHaveBeenCalledOnce()
    })

    expect(screen.queryByText('Add New Starship')).not.toBeInTheDocument()
  })

  it('should call editStarship and refetch when editing starship', async () => {
    vi.mocked(starshipService.editStarship).mockResolvedValueOnce({})

    renderStarshipManager()

    const editButton = screen.getByRole('button', { name: /edit/i })
    await userEvent.click(editButton)

    expect(screen.getByRole('heading', { name: 'Edit Starship' })).toBeInTheDocument()

    const nameInput = screen.getByLabelText(/name/i)
    await userEvent.clear(nameInput)
    await userEvent.type(nameInput, 'Test Model Modified')

    const submitButton = screen.getByRole('button', { name: /edit starship/i })
    await userEvent.click(submitButton)

    await waitFor(() => {
      expect(starshipService.editStarship).toHaveBeenCalledOnce()
      expect(mockRefetch).toHaveBeenCalledOnce()
    })

    expect(screen.queryByText('Edit Starship')).not.toBeInTheDocument()
  })

  it('should call deleteStarship and refetch when deleting starship', async () => {
    vi.mocked(starshipService.deleteStarship).mockResolvedValueOnce()

    renderStarshipManager()

    const deleteButton = screen.getByRole('button', { name: /delete/i })
    await userEvent.click(deleteButton)

    expect(screen.getByText(/are you sure/i)).toBeInTheDocument()

    const confirmButton = screen.getByRole('button', { name: /delete/i })
    await userEvent.click(confirmButton)

    await waitFor(() => {
      expect(starshipService.deleteStarship).toHaveBeenCalledWith(1)
      expect(mockRefetch).toHaveBeenCalledOnce()
    })
  })
})
