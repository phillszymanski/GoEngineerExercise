import { useState } from "react";
import { Starship, StarshipTableProps } from "../models/Starship";
import ConfirmDialog from "./ConfirmDialog";


export default function StarshipTable({starships, loading, error, onEdit, onDelete}: StarshipTableProps) {

    const [deleteConfirm, setDeleteConfirm] = useState<{ isOpen: boolean; starship: Starship | null }>({
        isOpen: false,
        starship: null
    });
    
    const handleDeleteClick =(starship:Starship) => {
        setDeleteConfirm({ isOpen: true, starship: starship});
    };

    const handleConfirmDelete = () => {
        if(deleteConfirm.starship) {
            onDelete(deleteConfirm.starship.id);
        }
        setDeleteConfirm({ isOpen: false, starship: null});
    };

    const handleCancelDelete = () => {
        setDeleteConfirm({ isOpen: false, starship: null});
    }

    return (
        <>
            <div aria-busy={loading} className="overflow-x-auto rounded border">
                <table className="w-full border-collapse text-sm">
                    <caption className="sr-only">Starship Table</caption>
                    <thead className="bg-slate-50 text-slate-700">
                        <tr>
                            <th className="px-4 py-2 text-left font-semibold">Name</th>
                            <th className="px-4 py-2 text-left font-semibold">Model</th>
                            <th className="px-4 py-2 text-left font-semibold">Manufacturer</th>
                            <th className="px-4 py-2 text-left font-semibold">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-200">
                        {loading && (
                            <tr>
                                <td colSpan={4} className="px-4 py-2 text-center">Loading...</td>
                            </tr>
                        )}
                        {error && (
                            <tr>
                                <td colSpan={4} className="px-4 py-2 text-center text-red-600">Error: {error.message}</td>
                            </tr>
                        )}
                        {starships.map((starship: Starship) => (
                            <tr key={starship.id} className="hover:bg-slate-50">
                                <td className="px-4 py-2">{starship.name}</td>
                                <td className="px-4 py-2">{starship.model}</td>
                                <td className="px-4 py-2">{starship.manufacturer}</td>
                                <td className="px-4 py-2">
                                    <div className="flex gap-2">
                                        <button
                                            className="px-2 py-1 bg-blue-600 text-white rounded hover:bg-blue-700"
                                            aria-label={`Edit ${starship.name}`}
                                            onClick={() => onEdit(starship)}
                                        >
                                            <span>Edit</span>
                                        </button>
                                        <button
                                            className="px-2 py-1 bg-red-600 text-white rounded hover:bg-red-700"
                                            aria-label={`Delete ${starship.name}`}
                                            onClick={() => handleDeleteClick(starship)}
                                        >
                                            <span>Delete</span>
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            <ConfirmDialog
                    isOpen={deleteConfirm.isOpen}
                    title="Delete Starship"
                    message={`Are you sure you want to delete "${deleteConfirm.starship?.name}"? This action cannot be undone.`}
                    variant="danger"
                    onConfirm={handleConfirmDelete}
                    onCancel={handleCancelDelete}
                    confirmText="Delete"
                    cancelText="Cancel"
                />
        </>
    );
}