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
    <div className="min-h-full flex flex-col">
      <header className="border-b bg-white px-4 py-3 shadow-sm">
        <h1 className="text-xl font-semibold tracking-tight">
          <a href="#/" className="hover:underline">FlowBoard</a>
        </h1>
      </header>
      <main className="flex-1 p-4">
        {route.name === 'home' && <BoardsPage />}
        {route.name === 'board' && <BoardPage boardId={route.id!} />}
      </main>
    </div>
  );
};
