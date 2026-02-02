import { SearchResult, Starship } from "../models/Starship";
import { apiClient } from "./apiClient";

export async function searchStarshipsWithAI(query:string): Promise<SearchResult> {
    const response = await apiClient.post<SearchResult>('/starship/search', { query })
    return response.data;
}