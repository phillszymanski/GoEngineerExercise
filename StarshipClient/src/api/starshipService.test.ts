import { describe, it, expect, vi, beforeEach } from 'vitest'
import { fetchStarships, addStarship, editStarship, deleteStarship } from './starshipService'
import { apiClient } from './apiClient'
import type { Starship } from '../models/Starship'

vi.mock('./apiClient', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}))

describe('starshipService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('fetchStarships', () => {
    it('should fetch starships successfully', async () => {
      const mockStarships: Starship[] = [
        {
          id: 1,
          name: 'Test Starship',
          model: 'Test Starship',
          manufacturer: 'Test Manufacturer',
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
          created: new Date('2024-01-01'),
          edited: new Date('2024-01-01'),
          url: 'https://swapi.dev/api/starships/1/',
        },
      ]

      vi.mocked(apiClient.get).mockResolvedValue({ data: mockStarships })

      const result = await fetchStarships()

      expect(apiClient.get).toHaveBeenCalledWith('/starship/GetAllStarships')
      expect(result).toEqual(mockStarships)
    })

    it('should throw error when fetch fails', async () => {
      const mockError = new Error('Network error')
      vi.mocked(apiClient.get).mockRejectedValue(mockError)

      await expect(fetchStarships()).rejects.toThrow('Network error')
    })
  })

  describe('addStarship', () => {
    it('should add starship successfully', async () => {
      const newStarship: Starship = {
        id: 0,
        name: 'New Ship',
        model: 'Model X',
        manufacturer: 'Test Corp',
        cost_in_credits: '1000000',
        length: '100',
        max_atmosphering_speed: '1200',
        crew: '10',
        passengers: '50',
        cargo_capacity: '5000',
        consumables: '1 month',
        hyperdrive_rating: '2.0',
        MGLT: '80',
        starship_class: 'Cruiser',
        pilots: [],
        films: [],
        created: new Date(),
        edited: new Date(),
        url: '',
      }

      const mockResponse = { ...newStarship, id: 42 }
      vi.mocked(apiClient.post).mockResolvedValue({ data: mockResponse })

      const result = await addStarship(newStarship)

      expect(apiClient.post).toHaveBeenCalledWith('/starship/AddStarship', newStarship)
      expect(result).toEqual(mockResponse)
    })

    it('should throw error when add fails', async () => {
      const starship: Starship = {
        id: 0,
        name: 'Test',
        model: 'Test',
        manufacturer: 'Test',
        cost_in_credits: '1000',
        length: '10',
        max_atmosphering_speed: '1000',
        crew: '1',
        passengers: '0',
        cargo_capacity: '100',
        consumables: '1 week',
        hyperdrive_rating: '1.0',
        MGLT: '100',
        starship_class: 'Test',
        pilots: [],
        films: [],
        created: new Date(),
        edited: new Date(),
        url: '',
      }

      const mockError = new Error('Validation error')
      vi.mocked(apiClient.post).mockRejectedValue(mockError)

      await expect(addStarship(starship)).rejects.toThrow('Validation error')
    })
  })

  describe('editStarship', () => {
    it('should edit starship successfully', async () => {
      const updatedStarship: Starship = {
        id: 1,
        name: 'Updated Test Starship',
        model: 'Test Model Modified',
        manufacturer: 'Test Manufaturer',
        cost_in_credits: '199999',
        length: '12.5',
        max_atmosphering_speed: '1100',
        crew: '1',
        passengers: '0',
        cargo_capacity: '120',
        consumables: '2 weeks',
        hyperdrive_rating: '0.9',
        MGLT: '110',
        starship_class: 'Starfighter',
        pilots: [],
        films: [],
        created: new Date('2024-01-01'),
        edited: new Date('2024-02-01'),
        url: 'https://swapi.dev/api/starships/1/',
      }

      vi.mocked(apiClient.put).mockResolvedValue({ data: updatedStarship })

      const result = await editStarship(updatedStarship)

      expect(apiClient.put).toHaveBeenCalledWith('/starship/UpdateStarship', updatedStarship)
      expect(result).toEqual(updatedStarship)
    })

    it('should throw error when edit fails', async () => {
      const starship: Starship = {
        id: 999,
        name: 'Nonexistent',
        model: 'Test',
        manufacturer: 'Test',
        cost_in_credits: '1000',
        length: '10',
        max_atmosphering_speed: '1000',
        crew: '1',
        passengers: '0',
        cargo_capacity: '100',
        consumables: '1 week',
        hyperdrive_rating: '1.0',
        MGLT: '100',
        starship_class: 'Test',
        pilots: [],
        films: [],
        created: new Date(),
        edited: new Date(),
        url: '',
      }

      const mockError = new Error('Not found')
      vi.mocked(apiClient.put).mockRejectedValue(mockError)

      await expect(editStarship(starship)).rejects.toThrow('Not found')
    })
  })

  describe('deleteStarship', () => {
    it('should delete starship successfully', async () => {
      const starshipId = 1
      const mockResponse = { success: true }
      vi.mocked(apiClient.delete).mockResolvedValue({ data: mockResponse })

      const result = await deleteStarship(starshipId)

      expect(apiClient.delete).toHaveBeenCalledWith('/starship/DeleteStarship/1')
      expect(result).toEqual(mockResponse)
    })

    it('should throw error when delete fails', async () => {
      const starshipId = 999
      const mockError = new Error('Delete failed')
      vi.mocked(apiClient.delete).mockRejectedValue(mockError)

      await expect(deleteStarship(starshipId)).rejects.toThrow('Delete failed')
    })
  })
})
