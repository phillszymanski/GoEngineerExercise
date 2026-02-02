import { useState } from 'react'
import { useAuth } from '../components/AuthProvider'
import StarshipTable from '../components/StarshipTable'
import StarshipModal from '../components/StarshipModal'
import { addStarship, editStarship, deleteStarship } from '../api/starshipService'
import { useStarships } from "../hooks/useStarships"
import { Starship } from '../models/Starship'
import { searchStarshipsWithAI } from '../api/aiSearchService'
import toast from 'react-hot-toast'

export default function StarshipManager() {
  const { user, logout, isLoading } = useAuth()
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingStarship, setEditingStarship] = useState<Starship | undefined>(undefined)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const { starships, loading, error, refetch } = useStarships(!isLoading && !!user)

  const [searchQuery, setSearchQuery] = useState('')
  const [isSearching, setIsSearching] = useState(false)
  const [searchResults, setSearchResults] = useState<Starship[] | null>(null)

  const handleAddStarship = async (starship: Omit<Starship, 'id' | 'created' | 'edited' | 'url' | 'pilots' | 'films'>) => {
    setIsSubmitting(true)
    const newStarship: Starship = {
      ...starship,
      id: 0,
      pilots: [],
      films: [],
      created: new Date(),
      edited: new Date(),
      url: ''
    }
    try{
        await addStarship(newStarship)
        await refetch()
        setIsModalOpen(false)
        toast.success('Starship added successfully!')
    } catch(err:unknown) {
        const error = err as Error;
        const errorMessage = error.message || 'Failed to add starship.'
        toast.error('An error occurred while adding the starship: ' + errorMessage)
    } finally {
        setIsSubmitting(false)
    }
  }

  const handleOpenEditModal = (starship: Starship) => {
    setEditingStarship(starship)
    setIsModalOpen(true)
  }

  const handleEditStarship = async (starship: Omit<Starship, 'id' | 'created' | 'edited' | 'url' | 'pilots' | 'films'>) => {
    setIsSubmitting(true)
    try{
        const starshipWithId: Starship = {
            ...starship,
            id: editingStarship!.id,
            pilots: editingStarship!.pilots,
            films: editingStarship!.films,
            created: editingStarship!.created,
            edited: editingStarship!.edited,
            url: editingStarship!.url
        }
        await editStarship(starshipWithId)
        await refetch()
        setIsModalOpen(false)
        setEditingStarship(undefined)
    } catch(err:unknown) {
        const error = err as Error;
        const errorMessage = error.message || 'Failed to edit starship'
        toast.error('An error occurred while editing the starship: ' + errorMessage)
    }  finally {
        setIsSubmitting(false)
    }
  }

  const handleDeleteStarship = async (starshipId: number) => {
    setIsSubmitting(true)
    try {
        await deleteStarship(starshipId)
        await refetch()
    } catch(err:unknown) {
        const error = err as Error;
        const errorMessage = error.message || 'Failed to delete starship'
        toast.error('An error occurred while deleting the starship: ' + errorMessage)
    } finally {
        setIsSubmitting(false)
    }
  }

  const handleModalSubmit = editingStarship ? handleEditStarship : handleAddStarship

  const handleModalClose = () => {
    setIsModalOpen(false)
    setEditingStarship(undefined)
  }

  const handleAiSearch = async () => {
    if(!searchQuery.trim()) {
        setSearchResults(null)
        return
    }

    setIsSearching(true)
    try {
        const results = await searchStarshipsWithAI(searchQuery)
        setSearchResults(results.results)

        if(results.usedFallback) {
            toast('Using basic text search: ' + results.fallbackReason, {
              icon: 'ℹ️',
              duration: 4000,
          })
        }
    } catch(err:unknown) {
        const error = err as Error;
        const errorMessage = error.message || 'Failed to complete search.'
        toast.error('An error occurred while performing the search: ' + errorMessage)
    } finally {
        setIsSearching(false)
    }
  }

  const handleClearSearch = () => {
    setSearchQuery('')
    setSearchResults(null)
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-blue-600 text-white p-4 shadow-md">
        <div className="container mx-auto flex justify-between items-center">
          <h1 className="text-2xl font-bold">Starship Manager</h1>
          {user && (
            <div className="flex items-center gap-4">
              <span>Welcome, {user.username}</span>
              <button
                onClick={logout}
                className="px-4 py-2 bg-blue-700 hover:bg-blue-800 rounded"
              >
                Logout
              </button>
            </div>
          )}
        </div>
      </header>

      <main className="container mx-auto p-4">
        <div className='relative min-h-screen'>
            <div className="mb-6 max-w-2x1">
                <div className="flex gap-2">
                    <div className="flex-1 relative">
                        <input
                            type='text'
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            onKeyDown={(e) => e.key === 'Enter' && handleAiSearch()}
                            placeholder="AI Search: Try smart queries like 'fast ships', 'cheap but large ships', etc."
                            className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                        />
                    </div>
                    <button
                        onClick={handleAiSearch}
                        disabled={isSearching || !searchQuery.trim()}
                        className="px-6 py-3 bg-gradient-to-r from-purple-600 to-pink-600 text-white rounded-lg hover:from-purple-700 hover:to-pink-700 disabled:opacity-50 disabled:cursor-not-allowed font-medium"
                    >
                        {isSearching ? '🔍 Searching...' : '🔍 Search'}
                    </button>
                    {searchResults && (
                        <button
                            onClick={handleClearSearch}
                            className="px-4 py-3 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
                        >
                            Clear
                        </button>
                    )}
                </div>
            </div>
            {isSubmitting && (
              <div className="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50">
                <div className="bg-white p-6 rounded-lg shadow-xl">
                  <div className="flex flex-col items-center gap-4">
                    <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-200 border-t-blue-600"></div>
                    <p className="text-lg font-medium text-gray-700">Processing...</p>
                  </div>
                </div>
              </div>
            )}
            
            <button 
              type="button" 
              className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 mb-4"
              onClick={() => setIsModalOpen(true)}
              disabled={isSubmitting}
            >
                Add a Starship
            </button>
            <StarshipTable 
              starships={searchResults ?? starships} 
              loading={loading} 
              error={error} 
              onEdit={handleOpenEditModal} 
              onDelete={handleDeleteStarship} />

            <StarshipModal
              isOpen={isModalOpen}
              onClose={handleModalClose}
              onSubmit={handleModalSubmit}
              starship={editingStarship}
              isSubmitting={isSubmitting}
            />
          </div>
        </main>
    </div>
  )
}