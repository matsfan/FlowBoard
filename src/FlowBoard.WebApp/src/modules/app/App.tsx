import React, { useEffect, useMemo, useState } from 'react';
import { BoardsPage } from '../boards/BoardsPage';
import { BoardPage } from '../boards/BoardPage';

export const App: React.FC = () => {
  const [hash, setHash] = useState<string>(() => window.location.hash || '#/');

  useEffect(() => {
    const onHashChange = () => setHash(window.location.hash || '#/');
    window.addEventListener('hashchange', onHashChange);
    return () => window.removeEventListener('hashchange', onHashChange);
  }, []);

  const route = useMemo(() => {
    // Very small hash router: #/ => boards list, #/boards/:id => board page
    const cleaned = hash.startsWith('#') ? hash.slice(1) : hash; // '/...' or ''
    const parts = cleaned.split('/').filter(Boolean); // [ 'boards', ':id' ]
    if (parts.length === 0) return { name: 'home' as const };
    if (parts[0] === 'boards' && parts[1]) return { name: 'board' as const, id: parts[1] };
    return { name: 'home' as const };
  }, [hash]);

  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {route.name === 'home' && (
        <header className="bg-white border-b border-gray-200 px-6 py-4 shadow-sm">
          <div className="flex items-center">
            <div className="flex items-center gap-3">
              <div className="w-8 h-8 bg-gradient-to-br from-blue-600 to-indigo-600 rounded-lg flex items-center justify-center">
                <svg className="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clipRule="evenodd" />
                </svg>
              </div>
              <h1 className="text-xl font-bold tracking-tight text-gray-900">
                <a href="#/" className="hover:text-blue-600 transition-colors">FlowBoard</a>
              </h1>
            </div>
          </div>
        </header>
      )}
      
      <main className="flex-1 overflow-hidden">
        {route.name === 'home' && <BoardsPage />}
        {route.name === 'board' && <BoardPage boardId={route.id!} />}
      </main>
    </div>
  );
};
