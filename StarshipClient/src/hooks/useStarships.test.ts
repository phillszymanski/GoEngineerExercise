import { describe, it, expect, vi, beforeEach } from 'vitest'
import { act, renderHook, waitFor } from '@testing-library/react'
import { useStarships } from './useStarships'
import * as starshipService from '../api/starshipService'

vi.mock('../api/starshipService')

describe('useStarships', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should call fetch when enabled is true', async () => {
    vi.mocked(starshipService.fetchStarships).mockResolvedValue([])

    renderHook(() => useStarships())

    await waitFor(() => { 
      expect(starshipService.fetchStarships).toHaveBeenCalledOnce()
    })
  })

  it('should NOT call fetch when enabled is false', async () => {
    vi.mocked(starshipService.fetchStarships).mockResolvedValue([])

    renderHook(() => useStarships(false))

    await waitFor(() => { 
      expect(starshipService.fetchStarships).not.toHaveBeenCalled()
    })
  })

  it('should handle error when fetch fails', async () => {
    const testError = 'Test Error';
    vi.mocked(starshipService.fetchStarships).mockRejectedValueOnce(new Error(testError))

    var result = renderHook(() => useStarships())

    await waitFor(() => {
      expect(result.result.current.error?.message).toEqual(testError)
    })
  })
})
