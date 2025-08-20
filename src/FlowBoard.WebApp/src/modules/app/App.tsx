import React from 'react';
import { BoardsPage } from '../boards/BoardsPage';

export const App: React.FC = () => {
  return (
    <div className="min-h-full flex flex-col">
      <header className="border-b bg-white px-4 py-3 shadow-sm">
        <h1 className="text-xl font-semibold tracking-tight">FlowBoard</h1>
      </header>
      <main className="flex-1 p-4">
        <BoardsPage />
      </main>
    </div>
  );
};
