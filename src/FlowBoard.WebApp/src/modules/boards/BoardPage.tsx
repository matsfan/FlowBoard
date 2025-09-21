import React, { useEffect, useState } from 'react';
import { ColumnComponent } from './ColumnComponent';
import type { Board, Column } from './types';

interface Props {
  boardId: string;
}

export const BoardPage: React.FC<Props> = ({ boardId }) => {
  const [board, setBoard] = useState<Board | null>(null);
  const [columns, setColumns] = useState<Column[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadBoardData = async () => {
    setLoading(true);
    setError(null);
    try {
      const [bRes, cRes] = await Promise.all([
        fetch(`/api/boards/${boardId}`),
        fetch(`/api/boards/${boardId}/columns`),
      ]);
      if (!bRes.ok) throw new Error(`Board not found (${bRes.status})`);
      if (!cRes.ok) throw new Error(`Failed to load columns (${cRes.status})`);
      const b = await bRes.json();
      const c = await cRes.json();
      setBoard({ id: b.id ?? b.Id, name: b.name ?? b.Name, createdUtc: (b.createdUtc ?? b.CreatedUtc) });
      const cols = (c.columns ?? c.Columns ?? []).map((x: any) => ({
        id: x.id ?? x.Id,
        name: x.name ?? x.Name,
        order: x.order ?? x.Order,
        wipLimit: x.wipLimit ?? x.WipLimit ?? null,
      })) as Column[];
      setColumns(cols);
    } catch (e: any) {
      setError(e.message || 'Failed to load board');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadBoardData();
  }, [boardId]);

  return (
    <div className="h-full flex flex-col bg-gradient-to-br from-blue-50 to-indigo-100">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-6 py-4 shadow-sm">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <a 
              href="#/" 
              className="inline-flex items-center text-sm font-medium text-gray-600 hover:text-gray-900 transition-colors"
            >
              <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
              </svg>
              All boards
            </a>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">
                {board ? board.name : 'Board'}
              </h1>
              {board && (
                <p className="text-sm text-gray-500 mt-1">
                  Created {new Date(board.createdUtc).toLocaleDateString('en-US', {
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric'
                  })}
                </p>
              )}
            </div>
          </div>
          
          <div className="flex items-center gap-3">
            {/* Board Actions */}
            <button className="inline-flex items-center px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2">
              <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 100 4m0-4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 100 4m0-4v2m0-6V4" />
              </svg>
              Settings
            </button>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 p-6 overflow-hidden">
        {loading && (
          <div className="flex items-center justify-center h-64">
            <div className="flex flex-col items-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              <p className="text-sm text-gray-500 mt-2">Loading board...</p>
            </div>
          </div>
        )}
        
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-md p-4">
            <div className="flex">
              <svg className="w-5 h-5 text-red-400" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
              <div className="ml-3">
                <h3 className="text-sm font-medium text-red-800">Error loading board</h3>
                <p className="text-sm text-red-700 mt-1">{error}</p>
              </div>
            </div>
          </div>
        )}

        {!loading && !error && (
          <div className="h-full">
            {columns.length === 0 ? (
              <div className="flex flex-col items-center justify-center h-64 bg-white rounded-lg border-2 border-dashed border-gray-300">
                <svg className="w-12 h-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
                <h3 className="text-sm font-medium text-gray-900 mb-1">No columns yet</h3>
                <p className="text-sm text-gray-500">Create columns to start organizing your cards</p>
              </div>
            ) : (
              <div className="flex gap-6 h-full overflow-x-auto pb-4">
                {columns.sort((a, b) => a.order - b.order).map(column => (
                  <ColumnComponent
                    key={column.id}
                    column={column}
                    boardId={boardId}
                    onCardUpdate={loadBoardData}
                  />
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
