import React, { useState, useEffect } from 'react';
import { CardComponent } from './CardComponent';
import { CardModal } from './CardModal';
import type { Column, Card } from './types';

interface Props {
  column: Column;
  boardId: string;
  onCardUpdate?: () => void;
}

export const ColumnComponent: React.FC<Props> = ({ column, boardId, onCardUpdate }) => {
  const [cards, setCards] = useState<Card[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCard, setEditingCard] = useState<Card | null>(null);

  const loadCards = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`/api/boards/${boardId}/columns/${column.id}/cards`);
      if (!response.ok) {
        if (response.status === 404) {
          setCards([]);
          return;
        }
        throw new Error(`Failed to load cards (${response.status})`);
      }
      const data = await response.json();
      const cardsList = (data.cards ?? data.Cards ?? []).map((c: any) => ({
        id: c.id ?? c.Id,
        title: c.title ?? c.Title,
        description: c.description ?? c.Description,
        order: c.order ?? c.Order,
        isArchived: c.isArchived ?? c.IsArchived ?? false,
        createdUtc: c.createdUtc ?? c.CreatedUtc
      })) as Card[];
      setCards(cardsList.filter(c => !c.isArchived).sort((a, b) => a.order - b.order));
    } catch (err: any) {
      setError(err.message || 'Failed to load cards');
      setCards([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCards();
  }, [boardId, column.id]);

  const handleCardEdit = (card: Card) => {
    setEditingCard(card);
    setIsModalOpen(true);
  };

  const handleNewCard = () => {
    setEditingCard(null);
    setIsModalOpen(true);
  };

  const handleCardSave = () => {
    loadCards();
    onCardUpdate?.();
  };

  const handleCardDelete = async (cardId: string) => {
    try {
      const response = await fetch(`/api/boards/${boardId}/columns/${column.id}/cards/${cardId}`, {
        method: 'DELETE'
      });
      if (!response.ok) {
        throw new Error(`Failed to delete card (${response.status})`);
      }
      loadCards();
      onCardUpdate?.();
    } catch (err: any) {
      alert(err.message || 'Failed to delete card');
    }
  };

  const handleCardArchive = async (cardId: string) => {
    const card = cards.find(c => c.id === cardId);
    if (!card) return;

    try {
      const response = await fetch(`/api/boards/${boardId}/columns/${column.id}/cards/${cardId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          title: card.title,
          description: card.description,
          order: card.order,
          isArchived: !card.isArchived
        })
      });
      if (!response.ok) {
        throw new Error(`Failed to ${card.isArchived ? 'unarchive' : 'archive'} card (${response.status})`);
      }
      loadCards();
      onCardUpdate?.();
    } catch (err: any) {
      alert(err.message || `Failed to ${card.isArchived ? 'unarchive' : 'archive'} card`);
    }
  };

  const wipLimitReached = column.wipLimit && cards.length >= column.wipLimit;
  const wipLimitNearReached = column.wipLimit && cards.length >= column.wipLimit * 0.8;

  return (
    <>
      <div className="flex-shrink-0 w-80 bg-gray-50 rounded-lg">
        {/* Column Header */}
        <div className="p-4 border-b border-gray-200 bg-white rounded-t-lg">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <h3 className="font-semibold text-gray-900 text-sm">{column.name}</h3>
              <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800">
                {cards.length}
              </span>
            </div>
            
            {column.wipLimit && (
              <div className={`flex items-center gap-1 text-xs font-medium ${
                wipLimitReached 
                  ? 'text-red-600' 
                  : wipLimitNearReached 
                    ? 'text-amber-600' 
                    : 'text-gray-500'
              }`}>
                <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clipRule="evenodd" />
                </svg>
                WIP {column.wipLimit}
              </div>
            )}
          </div>
        </div>

        {/* Cards Container */}
        <div className="p-2 min-h-[200px] max-h-[calc(100vh-300px)] overflow-y-auto custom-scrollbar">
          {loading && (
            <div className="flex items-center justify-center py-8">
              <div className="text-sm text-gray-500">Loading cards...</div>
            </div>
          )}

          {error && (
            <div className="p-3 text-sm text-red-600 bg-red-50 rounded-md mb-2">
              {error}
            </div>
          )}

          {!loading && !error && cards.length === 0 && (
            <div className="flex items-center justify-center py-8">
              <div className="text-sm text-gray-400">No cards yet</div>
            </div>
          )}

          {!loading && !error && cards.map(card => (
            <CardComponent
              key={card.id}
              card={card}
              onEdit={handleCardEdit}
              onDelete={handleCardDelete}
              onArchive={handleCardArchive}
            />
          ))}

          {/* Add Card Button */}
          {!loading && !wipLimitReached && (
            <button
              onClick={handleNewCard}
              className="w-full p-3 text-sm text-gray-500 hover:text-gray-700 hover:bg-white rounded-lg border-2 border-dashed border-gray-200 hover:border-gray-300 transition-colors"
            >
              + Add a card
            </button>
          )}

          {wipLimitReached && (
            <div className="p-3 text-xs text-red-600 bg-red-50 rounded-md">
              WIP limit reached ({column.wipLimit})
            </div>
          )}
        </div>
      </div>

      <CardModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        card={editingCard}
        columnId={column.id}
        boardId={boardId}
        onSave={handleCardSave}
      />
    </>
  );
};