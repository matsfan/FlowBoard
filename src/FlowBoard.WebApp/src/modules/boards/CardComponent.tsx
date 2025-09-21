import React, { useState } from 'react';
import type { Card } from './types';

interface Props {
  card: Card;
  onEdit?: (card: Card) => void;
  onDelete?: (cardId: string) => void;
  onArchive?: (cardId: string) => void;
}

export const CardComponent: React.FC<Props> = ({ card, onEdit, onDelete, onArchive }) => {
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const handleCardClick = (e: React.MouseEvent) => {
    // Don't trigger when clicking on action buttons
    if ((e.target as Element).closest('.card-actions')) return;
    onEdit?.(card);
  };

  return (
    <div 
      className="group bg-white rounded-lg shadow-sm border border-gray-200 p-3 mb-2 cursor-pointer hover:shadow-md transition-shadow duration-200 relative"
      onClick={handleCardClick}
    >
      {/* Card Content */}
      <div className="space-y-2">
        <h4 className="text-sm font-medium text-gray-900 leading-snug">
          {card.title}
        </h4>
        
        {card.description && (
          <p className="text-xs text-gray-600 line-clamp-3">
            {card.description}
          </p>
        )}
        
        {/* Card Meta */}
        <div className="flex items-center justify-between text-xs text-gray-500">
          <span>
            {new Date(card.createdUtc).toLocaleDateString()}
          </span>
          
          {card.isArchived && (
            <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800">
              Archived
            </span>
          )}
        </div>
      </div>

      {/* Action Menu */}
      <div className="card-actions absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
        <div className="relative">
          <button
            onClick={(e) => {
              e.stopPropagation();
              setIsMenuOpen(!isMenuOpen);
            }}
            className="p-1 rounded hover:bg-gray-100 transition-colors"
          >
            <svg className="w-4 h-4 text-gray-500" fill="currentColor" viewBox="0 0 20 20">
              <path d="M10 6a2 2 0 110-4 2 2 0 010 4zM10 12a2 2 0 110-4 2 2 0 010 4zM10 18a2 2 0 110-4 2 2 0 010 4z" />
            </svg>
          </button>
          
          {isMenuOpen && (
            <div className="absolute right-0 top-full mt-1 w-32 bg-white rounded-md shadow-lg border border-gray-200 py-1 z-10">
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  onEdit?.(card);
                  setIsMenuOpen(false);
                }}
                className="w-full text-left px-3 py-1 text-xs text-gray-700 hover:bg-gray-50"
              >
                Edit
              </button>
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  onArchive?.(card.id);
                  setIsMenuOpen(false);
                }}
                className="w-full text-left px-3 py-1 text-xs text-gray-700 hover:bg-gray-50"
              >
                {card.isArchived ? 'Unarchive' : 'Archive'}
              </button>
              <hr className="my-1 border-gray-100" />
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  if (window.confirm('Delete this card?')) {
                    onDelete?.(card.id);
                  }
                  setIsMenuOpen(false);
                }}
                className="w-full text-left px-3 py-1 text-xs text-red-600 hover:bg-red-50"
              >
                Delete
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};