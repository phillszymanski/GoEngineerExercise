export interface Starship {
    id: number;
    name: string;
    model: string;
    manufacturer: string;
    cost_in_credits: string;
    length: string;
    max_atmosphering_speed: string;
    crew: string;
    passengers: string;
    cargo_capacity: string;
    consumables: string;
    hyperdrive_rating: string;
    MGLT: string;
    starship_class: string;
    pilots: string[];
    films: string[];
    created: Date;
    edited: Date;
    url?: string;
}

export interface AddStarshipModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (starship: Omit<Starship, 'id' | 'created' | 'edited' | 'url' | 'pilots' | 'films'>) => void;
    starship?: Starship;
    isSubmitting?: boolean;
}

export interface StarshipTableProps {
    starships: Starship[];
    loading: boolean;
    error: Error | null;
    onEdit: (starship: Starship) => void;
    onDelete: (id: number) => void;
}

export interface SearchResult {
  results: Starship[]
  usedFallback: boolean
  fallbackReason?: string
}