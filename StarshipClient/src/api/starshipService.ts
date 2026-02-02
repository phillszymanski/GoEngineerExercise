import { apiClient } from './apiClient'
import { Starship } from '../models/Starship';

export async function fetchStarships() {
  const response = await apiClient.get(`/starship/GetAllStarships`);
  return response.data;
}

export async function addStarship(starship:Starship) {
  const response = await apiClient.post(`/starship/AddStarship`, starship);
  return response.data;
}

export async function editStarship(starship:Starship) {
  const response = await apiClient.put(`/starship/UpdateStarship`, starship);
  return response.data;
}

export async function deleteStarship(id:number) {
  const response = await apiClient.delete(`/starship/DeleteStarship/${id}`);
  return response.data;
}