import { AuthProvider } from './components/AuthProvider'
import ProtectedRoute from './components/ProtectedRoute'
import StarshipManager from './components/StarshipManager'
import { Toaster } from 'react-hot-toast'
import './App.css'



function App() {
  return (
    <>
      <Toaster  />
      <AuthProvider>
        <ProtectedRoute>
          <StarshipManager />
        </ProtectedRoute>
      </AuthProvider>
    </>
  )
}

export default App