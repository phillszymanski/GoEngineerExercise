import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import StarshipTable from './StarshipTable'
import { Starship } from '../models/Starship'

describe('StarshipTable', () => {
  const mockStarships: Starship[] = [
    {
      id: 1,
      name: 'Test Starship',
      model: 'Test Model',
      manufacturer: 'Test Manufacturer',
      cost_in_credits: '100000',
      length: '34.37',
      max_atmosphering_speed: '1050',
      crew: '4',
      passengers: '6',
      cargo_capacity: '100000',
      consumables: '2 months',
      hyperdrive_rating: '0.5',
      MGLT: '75',
      starship_class: 'Light freighter',
      pilots: [],
      films: [],
      created: new Date('2014-12-10T16:59:45.094000Z'),
      edited: new Date('2014-12-20T21:23:49.880000Z'),
    },
    {
      id: 2,
      name: 'Test Starship 2',
      model: 'Test Model 2',
      manufacturer: 'Test Manufacturer 2',
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
      created: new Date('2014-12-12T11:19:05.340000Z'),
      edited: new Date('2014-12-20T21:23:49.886000Z'),
    },
  ]

  const mockOnEdit = vi.fn()
  const mockOnDelete = vi.fn()

  beforeEach(() => {
    mockOnEdit.mockClear()
    mockOnDelete.mockClear()
  })

  describe('Rendering', () => {
    it('renders table with correct structure', () => {
      render(
        <StarshipTable
          starships={mockStarships}
          loading={false}
          error={null}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      )

      expect(screen.getByRole('columnheader', { name: /name/i })).toBeInTheDocument()
      expect(screen.getByRole('columnheader', { name: /model/i })).toBeInTheDocument()
      expect(screen.getByRole('columnheader', { name: /manufacturer/i })).toBeInTheDocument()
      expect(screen.getByRole('columnheader', { name: /actions/i })).toBeInTheDocument()
    })

    it('renders starship data correctly', () => {
      render(
        <StarshipTable
          starships={mockStarships}
          loading={false}
          error={null}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      )

      expect(screen.getByText('Test Starship')).toBeInTheDocument()
      expect(screen.getByText('Test Model')).toBeInTheDocument()
      expect(screen.getByText('Test Manufacturer')).toBeInTheDocument()
      expect(screen.getByText('Test Starship 2')).toBeInTheDocument()
      expect(screen.getByText('Test Model 2')).toBeInTheDocument()
      expect(screen.getByText('Test Manufacturer 2')).toBeInTheDocument()
    })

    it('renders action buttons for each starship', () => {
      render(
        <StarshipTable
          starships={mockStarships}
          loading={false}
          error={null}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      )

      expect(screen.getByLabelText('Edit Test Starship')).toBeInTheDocument()
      expect(screen.getByLabelText('Edit Test Starship 2')).toBeInTheDocument()

      expect(screen.getByLabelText('Delete Test Starship')).toBeInTheDocument()
      expect(screen.getByLabelText('Delete Test Starship 2')).toBeInTheDocument()
    })
  })

  describe('Loading State', () => {
    it('displays loading message when loading is true', () => {
      render(
        <StarshipTable
          starships={[]}
          loading={true}
          error={null}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      )

      expect(screen.getByText('Loading...')).toBeInTheDocument()
    })

    it('sets aria-busy attribute when loading', () => {
      const { container } = render(
        <StarshipTable
          starships={[]}
          loading={true}
          error={null}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      )

      const tableContainer = container.querySelector('[aria-busy="true"]')
      expect(tableContainer).toBeInTheDocument()
    })
  })

  describe('Error State', () => {
    it('displays error message when error is present', () => {
      const mockError = new Error('Failed to fetch starships')

      render(
        <StarshipTable
          starships={[]}
          loading={false}
          error={mockError}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      )

      expect(screen.getByText(/Error: Failed to fetch starships/i)).toBeInTheDocument()
    })
  })

  describe('Empty State', () => {
    it('renders table structure with no data when starships array is empty', () => {
      render(
        <StarshipTable
          starships={[]}
          loading={false}
          error={null}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      )

      // Headers should still be present
      expect(screen.getByRole('columnheader', { name: /name/i })).toBeInTheDocument()
      
      // But no data rows
      const rows = screen.getAllByRole('row')
      expect(rows).toHaveLength(1) // Only header row
    })
  })

  describe('User Interactions', () => {
    it('calls onEdit with correct starship when Edit button is clicked', async () => {
      const user = userEvent.setup()

      render(
        <StarshipTable
          starships={mockStarships}
          loading={false}
          error={null}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      )

      const editButton = screen.getByLabelText('Edit Test Starship')
      await user.click(editButton)

      expect(mockOnEdit).toHaveBeenCalledTimes(1)
      expect(mockOnEdit).toHaveBeenCalledWith(mockStarships[0])
    })

    it('calls onDelete with correct id when Delete button is clicked and confirmed', async () => {
      const user = userEvent.setup()

      render(
        <StarshipTable
          starships={mockStarships}
          loading={false}
          error={null}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      )

      const deleteButton = screen.getByLabelText('Delete Test Starship 2')
      await user.click(deleteButton)

      expect(screen.getByRole('dialog')).toBeInTheDocument()
      expect(screen.getByText(/Are you sure you want to delete "Test Starship 2"/i)).toBeInTheDocument()

      const confirmButton = screen.getByRole('button', { name: 'Delete' })
      await user.click(confirmButton)

      expect(mockOnDelete).toHaveBeenCalledTimes(1)
      expect(mockOnDelete).toHaveBeenCalledWith(2)
    })
  })

  describe('Accessibility', () => {
    it('has accessible table caption', () => {
      render(
        <StarshipTable
          starships={mockStarships}
          loading={false}
          error={null}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      )

      const caption = screen.getByText('Starship Table')
      expect(caption).toBeInTheDocument()
    })
  })
})
