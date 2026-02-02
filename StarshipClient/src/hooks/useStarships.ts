import { useCallback, useEffect, useState } from "react";
import { fetchStarships } from "../api/starshipService";
import { Starship } from "../models/Starship";

export function useStarships(enabled: boolean = true) {
    const [starships, setStarships] = useState<Starship[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<Error | null>(null);

    const loadStarships = useCallback(async () => {
        if (!enabled) return;
        
        setLoading(true);

        try {
            const response = await fetchStarships();
            setStarships(response);
        } catch (err) {
            setError(err as Error);
        } finally { 
            setLoading(false);
        }
    }, [enabled]);

    useEffect(() => {
        loadStarships();
    }, [loadStarships]);

    return { starships, loading, error, refetch: loadStarships };
}