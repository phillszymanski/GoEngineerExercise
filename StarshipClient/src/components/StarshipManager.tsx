import { useState } from 'react'
import { useAuth } from '../components/AuthProvider'
import StarshipTable from '../components/StarshipTable'
import StarshipModal from '../components/StarshipModal'
import { addStarship, editStarship, deleteStarship } from '../api/starshipService'
import { useStarships } from "../hooks/useStarships"
import { Starship } from '../models/Starship'

export default function StarshipManager() {
  const { user, logout, isLoading } = useAuth()
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingStarship, setEditingStarship] = useState<Starship | undefined>(undefined)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const { starships, loading, error, refetch } = useStarships(!isLoading && !!user)

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
    await addStarship(newStarship)
    await refetch()
    setIsModalOpen(false)
    setIsSubmitting(false)
  }

  const handleOpenEditModal = (starship: Starship) => {
    setEditingStarship(starship)
    setIsModalOpen(true)
  }

  const handleEditStarship = async (starship: Omit<Starship, 'id' | 'created' | 'edited' | 'url' | 'pilots' | 'films'>) => {
    setIsSubmitting(true)
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
    setIsSubmitting(false)
  }

  const handleDeleteStarship = async (starshipId: number) => {
    setIsSubmitting(true)
    await deleteStarship(starshipId)
    await refetch()
    setIsSubmitting(false)
  }

  const handleModalSubmit = editingStarship ? handleEditStarship : handleAddStarship

  const handleModalClose = () => {
    setIsModalOpen(false)
    setEditingStarship(undefined)
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
              starships={starships} 
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