import axios from 'axios'

export const apiClient = axios.create({
  baseURL: 'http://localhost:5210/api',
  withCredentials: true,  // CRITICAL: Send cookies with requests
  headers: {
    'Content-Type': 'application/json'
  }
})
