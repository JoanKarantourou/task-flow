// ============================================
// Modal Component
// ============================================
// Accessible dialog/popup overlay component
// Used for forms, confirmations, detailed views

import { ReactNode, useEffect } from "react";

// ============================================
// Modal Props
// ============================================

/**
 * Props for the Modal component
 */
interface ModalProps {
  isOpen: boolean; // Controls modal visibility
  onClose: () => void; // Called when modal should close
  title?: string; // Modal title
  children: ReactNode; // Modal content
  size?: "sm" | "md" | "lg" | "xl"; // Modal width
  showCloseButton?: boolean; // Show X button in corner
}

// ============================================
// Modal Component
// ============================================

/**
 * Modal Component
 * An accessible overlay dialog
 *
 * Usage:
 * const [isOpen, setIsOpen] = useState(false);
 *
 * <Modal isOpen={isOpen} onClose={() => setIsOpen(false)} title="Edit Task">
 *   <p>Modal content here</p>
 * </Modal>
 */
export default function Modal({
  isOpen,
  onClose,
  title,
  children,
  size = "md",
  showCloseButton = true,
}: ModalProps) {
  // ==========================================
  // Close on Escape Key
  // ==========================================
  useEffect(() => {
    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === "Escape" && isOpen) {
        onClose();
      }
    };

    document.addEventListener("keydown", handleEscape);
    return () => document.removeEventListener("keydown", handleEscape);
  }, [isOpen, onClose]);

  // ==========================================
  // Prevent Body Scroll When Modal is Open
  // ==========================================
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "unset";
    }

    return () => {
      document.body.style.overflow = "unset";
    };
  }, [isOpen]);

  // ==========================================
  // Don't Render if Not Open
  // ==========================================
  if (!isOpen) return null;

  // ==========================================
  // Size Styles
  // ==========================================
  const sizeStyles = {
    sm: "max-w-md",
    md: "max-w-lg",
    lg: "max-w-2xl",
    xl: "max-w-4xl",
  };

  return (
    <>
      {/* Backdrop Overlay */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity"
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Modal Container */}
      <div className="fixed inset-0 z-50 overflow-y-auto">
        <div className="flex min-h-full items-center justify-center p-4">
          {/* Modal Panel */}
          <div
            className={`relative bg-white rounded-lg shadow-xl w-full ${sizeStyles[size]} transform transition-all`}
            role="dialog"
            aria-modal="true"
            aria-labelledby={title ? "modal-title" : undefined}
            onClick={(e) => e.stopPropagation()} // Prevent closing when clicking inside modal
          >
            {/* Modal Header */}
            {(title || showCloseButton) && (
              <div className="flex items-center justify-between p-6 border-b border-gray-200">
                {/* Title */}
                {title && (
                  <h2
                    id="modal-title"
                    className="text-xl font-semibold text-gray-900"
                  >
                    {title}
                  </h2>
                )}

                {/* Close Button */}
                {showCloseButton && (
                  <button
                    onClick={onClose}
                    className="text-gray-400 hover:text-gray-600 transition-colors p-1 rounded-lg hover:bg-gray-100"
                    aria-label="Close modal"
                  >
                    <svg
                      className="w-6 h-6"
                      fill="none"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth="2"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                )}
              </div>
            )}

            {/* Modal Body */}
            <div className="p-6">{children}</div>
          </div>
        </div>
      </div>
    </>
  );
}
