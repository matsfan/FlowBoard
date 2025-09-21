import React from 'react';
import { ColumnComponent } from './ColumnComponent';
import { CardComponent } from './CardComponent';
import type { Column, Card } from './types';

// Mock data for demonstration purposes
const mockColumns: Column[] = [
  { id: '1', name: 'To Do', order: 0, wipLimit: null },
  { id: '2', name: 'In Progress', order: 1, wipLimit: 3 },
  { id: '3', name: 'Review', order: 2, wipLimit: 2 },
  { id: '4', name: 'Done', order: 3, wipLimit: null }
];

const mockCards: Card[] = [
  {
    id: '1',
    title: 'Setup project structure',
    description: 'Initialize the project with proper folder structure and dependencies',
    order: 0,
    isArchived: false,
    createdUtc: '2024-01-15T10:00:00Z'
  },
  {
    id: '2',
    title: 'Design user interface',
    description: 'Create wireframes and mockups for the main application screens',
    order: 1,
    isArchived: false,
    createdUtc: '2024-01-15T11:00:00Z'
  },
  {
    id: '3',
    title: 'Implement authentication',
    description: 'Add user registration, login, and session management',
    order: 0,
    isArchived: false,
    createdUtc: '2024-01-16T09:00:00Z'
  },
  {
    id: '4',
    title: 'API integration',
    description: 'Connect frontend with backend API endpoints',
    order: 1,
    isArchived: false,
    createdUtc: '2024-01-16T14:00:00Z'
  },
  {
    id: '5',
    title: 'Code review for auth module',
    description: null,
    order: 0,
    isArchived: false,
    createdUtc: '2024-01-17T10:00:00Z'
  },
  {
    id: '6',
    title: 'Deploy to staging',
    description: 'Deploy the application to staging environment for testing',
    order: 0,
    isArchived: false,
    createdUtc: '2024-01-18T16:00:00Z'
  }
];

// Mock card distribution
const getCardsForColumn = (columnId: string): Card[] => {
  switch (columnId) {
    case '1': return mockCards.slice(0, 2);
    case '2': return mockCards.slice(2, 4);
    case '3': return mockCards.slice(4, 5);
    case '4': return mockCards.slice(5, 6);
    default: return [];
  }
};

interface Props {
  boardId?: string;
}

export const MockBoardDemo: React.FC<Props> = ({ boardId = 'demo' }) => {
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
                Project Management Demo
              </h1>
              <p className="text-sm text-gray-500 mt-1">
                Created January 15, 2024
              </p>
            </div>
          </div>
          
          <div className="flex items-center gap-3">
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
        <div className="flex gap-6 h-full overflow-x-auto pb-4">
          {mockColumns.map(column => (
            <div key={column.id} className="flex-shrink-0 w-80 bg-gray-50 rounded-lg">
              {/* Column Header */}
              <div className="p-4 border-b border-gray-200 bg-white rounded-t-lg">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <h3 className="font-semibold text-gray-900 text-sm">{column.name}</h3>
                    <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800">
                      {getCardsForColumn(column.id).length}
                    </span>
                  </div>
                  
                  {column.wipLimit && (
                    <div className={`flex items-center gap-1 text-xs font-medium ${
                      getCardsForColumn(column.id).length >= column.wipLimit 
                        ? 'text-red-600' 
                        : getCardsForColumn(column.id).length >= column.wipLimit * 0.8 
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
                {getCardsForColumn(column.id).map(card => (
                  <CardComponent
                    key={card.id}
                    card={card}
                    onEdit={() => console.log('Edit card:', card.title)}
                    onDelete={() => console.log('Delete card:', card.title)}
                    onArchive={() => console.log('Archive card:', card.title)}
                  />
                ))}

                {/* Add Card Button */}
                {getCardsForColumn(column.id).length < (column.wipLimit || 999) && (
                  <button
                    onClick={() => console.log('Add card to:', column.name)}
                    className="w-full p-3 text-sm text-gray-500 hover:text-gray-700 hover:bg-white rounded-lg border-2 border-dashed border-gray-200 hover:border-gray-300 transition-colors"
                  >
                    + Add a card
                  </button>
                )}

                {column.wipLimit && getCardsForColumn(column.id).length >= column.wipLimit && (
                  <div className="p-3 text-xs text-red-600 bg-red-50 rounded-md">
                    WIP limit reached ({column.wipLimit})
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};