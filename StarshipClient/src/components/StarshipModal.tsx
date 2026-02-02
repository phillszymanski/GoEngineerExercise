import { useEffect, useState } from "react";
import { Dialog, DialogPanel, DialogTitle } from '@headlessui/react';
import { AddStarshipModalProps, Starship } from "../models/Starship";

export default function StarshipModal({ isOpen, onClose, onSubmit, starship, isSubmitting = false }: AddStarshipModalProps) {
    const isEdit = !!starship;
    const [formData, setFormData] = useState<Omit<Starship, 'id' | 'created' | 'edited' | 'url' | 'pilots' | 'films'>>({
        name: "",
        model: "",
        manufacturer: "",
        cost_in_credits: "",
        length: "",
        max_atmosphering_speed: "",
        crew: "",
        passengers: "",
        cargo_capacity: "",
        consumables: "",
        hyperdrive_rating: "",
        MGLT: "",
        starship_class: ""
    });

    useEffect(() => {
        if(starship) {
            setFormData({
                name: starship.name,
                model: starship.model,
                manufacturer: starship.manufacturer,
                cost_in_credits: starship.cost_in_credits,
                length: starship.length,
                max_atmosphering_speed: starship.max_atmosphering_speed,
                crew: starship.crew,
                passengers: starship.passengers,
                cargo_capacity: starship.cargo_capacity,
                consumables: starship.consumables,
                hyperdrive_rating: starship.hyperdrive_rating,
                MGLT: starship.MGLT,
                starship_class: starship.starship_class
            });
        } else {
            setFormData({
                name: "",
                model: "",
                manufacturer: "",
                cost_in_credits: "",
                length: "",
                max_atmosphering_speed: "",
                crew: "",
                passengers: "",
                cargo_capacity: "",
                consumables: "",
                hyperdrive_rating: "",
                MGLT: "",
                starship_class: ""
            });
        }
    }, [starship, isOpen]);


    const fields = [
        { label: "Name", name: "name", required: true },
        { label: "Model", name: "model", required: true },
        { label: "Manufacturer", name: "manufacturer", required: true },
        { label: "Cost in Credits", name: "cost_in_credits", required: false },
        { label: "Length", name: "length", required: false },
        { label: "Max Atmosphering Speed", name: "max_atmosphering_speed", required: false },
        { label: "Crew", name: "crew", required: false },
        { label: "Passengers", name: "passengers", required: false },
        { label: "Cargo Capacity", name: "cargo_capacity", required: false },
        { label: "Consumables", name: "consumables", required: false },
        { label: "Hyperdrive Rating", name: "hyperdrive_rating", required: false },
        { label: "MGLT", name: "MGLT", required: false },
        { label: "Starship Class", name: "starship_class", required: false }
    ];

    const handleSubmit = (e: React.SubmitEvent) => {
        e.preventDefault();
        onSubmit(formData);
        onClose();
    }

    return (
        <Dialog open={isOpen} onClose={onClose}>
            <div className="fixed inset-0 bg-black bg-opacity-30 flex items-center justify-center">
                <DialogPanel className="bg-white p-6 rounded shadow-md w-full max-w-md">
                    <DialogTitle className="text-lg font-medium mb-4">{isEdit ? "Edit Starship" : "Add New Starship"}</DialogTitle>

                    <form onSubmit={handleSubmit}>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {fields.map(({ label, name, required }) => (
                                <div key={name} className="flex flex-col">
                                    <label htmlFor={name} className="mb-1 font-semibold">{label}{required && '*'}</label>
                                    <input
                                        type="text"
                                        id={name}
                                        value={formData[name as keyof typeof formData]}
                                        onChange={(e) => setFormData({ ...formData, [name]: e.target.value })}
                                        required={required}
                                        className="border border-slate-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                                    />
                                </div>
                            ))}
                        </div>

                        <div className="mt-6 flex justify-end space-x-4">
                            <button
                                type="button"
                                onClick={onClose}
                                disabled={isSubmitting}
                                className="px-4 py-2 bg-gray-300 text-gray-700 rounded hover:bg-gray-400"
                            >Cancel</button>
                            <button
                                type="submit"
                                disabled={isSubmitting}
                                className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                            >
                                {isSubmitting && (
                                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                                )}
                                {isEdit ? "Edit Starship" : "Add Starship"}
                            </button>
                        </div>
                    </form>
                </DialogPanel>
            </div>
        </Dialog>
    );
}