import { Dialog, DialogPanel, DialogTitle } from '@headlessui/react'

interface ConfirmDialogProps {
  isOpen: boolean
  title: string
  message: string
  confirmText?: string
  cancelText?: string
  onConfirm: () => void
  onCancel: () => void
  variant?: 'danger' | 'warning' | 'info'
}

export default function ConfirmDialog({
  isOpen,
  title,
  message,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  onConfirm,
  onCancel,
  variant = 'danger'
}: ConfirmDialogProps) {
  
  const variantStyles = {
    danger: 'bg-red-600 hover:bg-red-700',
    warning: 'bg-yellow-600 hover:bg-yellow-700',
    info: 'bg-blue-600 hover:bg-blue-700'
  }

  return (
    <Dialog open={isOpen} onClose={onCancel}>
      <div className="fixed inset-0 bg-black/30 flex items-center justify-center p-4">
        <DialogPanel className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
          <DialogTitle className="text-xl font-semibold text-gray-900 mb-2">
            {title}
          </DialogTitle>
          
          <p className="text-gray-600 mb-6">
            {message}
          </p>

          <div className="flex justify-end gap-3">
            <button
              type="button"
              onClick={onCancel}
              className="px-4 py-2 bg-gray-200 text-gray-800 rounded hover:bg-gray-300 font-medium"
            >
              {cancelText}
            </button>
            <button
              type="button"
              onClick={onConfirm}
              className={`px-4 py-2 text-white rounded font-medium ${variantStyles[variant]}`}
            >
              {confirmText}
            </button>
          </div>
        </DialogPanel>
      </div>
    </Dialog>
  )
}
