import React, { useState } from 'react';
import { Board } from './types';

interface Props {
  boards: Board[];
  onDeleted?: () => void;
}

export const ListBoards: React.FC<Props> = ({ boards, onDeleted }) => {
  const [deletingId, setDeletingId] = useState<string | null>(null);

  async function deleteBoard(id: string) {
    if (!window.confirm('Delete this board? This action cannot be undone.')) return;
    setDeletingId(id);
    try {
      const res = await fetch(`/api/boards/boards/${id}`, { method: 'DELETE' });
      if (!res.ok) throw new Error(`Error ${res.status}`);
      if (onDeleted) onDeleted();
    } catch (e) {
      alert((e as any).message || 'Failed to delete board');
    } finally {
      setDeletingId(null);
    }
  }

  if (!boards.length) return null;

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {boards.map(board => (
        <div 
          key={board.id} 
          className="group bg-white rounded-lg border border-gray-200 hover:border-gray-300 hover:shadow-md transition-all duration-200 overflow-hidden"
        >
          {/* Board Header */}
          <div className="p-4 border-b border-gray-100">
            <div className="flex items-start justify-between">
              <div className="flex-1 min-w-0">
                <h3 className="text-sm font-semibold text-gray-900 truncate">
                  {board.name}
                </h3>
                <p className="text-xs text-gray-500 mt-1">
                  Created {new Date(board.createdUtc).toLocaleDateString('en-US', {
                    year: 'numeric',
                    month: 'short',
                    day: 'numeric'
                  })}
                </p>
              </div>
              
              {/* Quick Actions */}
              <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                <button
                  onClick={() => deleteBoard(board.id)}
                  disabled={deletingId === board.id}
                  className="p-1 text-gray-400 hover:text-red-500 transition-colors"
                  title="Delete board"
                >
                  {deletingId === board.id ? (
                    <svg className="w-4 h-4 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                    </svg>
                  ) : (
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                  )}
                </button>
              </div>
            </div>
          </div>

          {/* Board Preview/Content */}
          <div className="p-4">
            <div className="flex items-center justify-between text-xs text-gray-500 mb-3">
              <span>Board Overview</span>
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
              </svg>
            </div>
            
            {/* Mini board visualization */}
            <div className="flex gap-2 mb-4">
              <div className="flex-1 bg-gray-50 rounded h-12 flex items-end p-1">
                <div className="w-full bg-gray-200 rounded h-2"></div>
              </div>
              <div className="flex-1 bg-gray-50 rounded h-12 flex items-end p-1">
                <div className="w-2/3 bg-gray-200 rounded h-4"></div>
              </div>
              <div className="flex-1 bg-gray-50 rounded h-12 flex items-end p-1">
                <div className="w-1/3 bg-gray-200 rounded h-6"></div>
              </div>
            </div>

            {/* Action Button */}
            <a 
              href={`#/boards/${board.id}`}
              className="block w-full text-center py-2 px-3 text-sm font-medium text-blue-600 bg-blue-50 rounded-md hover:bg-blue-100 hover:text-blue-700 transition-colors"
            >
              Open Board
            </a>
          </div>
        </div>
      ))}
    </div>
  );
};
