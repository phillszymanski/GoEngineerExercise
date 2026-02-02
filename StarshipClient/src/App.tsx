import { AuthProvider } from './components/AuthProvider'
import ProtectedRoute from './components/ProtectedRoute'
import StarshipManager from './components/StarshipManager'
import './App.css'



function App() {
  return (
    <AuthProvider>
      <ProtectedRoute>
        <StarshipManager />
      </ProtectedRoute>
    </AuthProvider>
  )
}

export default App