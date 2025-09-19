import React, { useEffect, useState } from 'react';
import type { Board, Column } from './types';

interface Props {
  boardId: string;
}

export const BoardPage: React.FC<Props> = ({ boardId }) => {
  const [board, setBoard] = useState<Board | null>(null);
  const [columns, setColumns] = useState<Column[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    async function load() {
      setLoading(true);
      setError(null);
      try {
        const [bRes, cRes] = await Promise.all([
          fetch(`/api/boards/boards/${boardId}`),
          fetch(`/api/boards/boards/${boardId}/columns`),
        ]);
        if (!bRes.ok) throw new Error(`Board not found (${bRes.status})`);
        if (!cRes.ok) throw new Error(`Failed to load columns (${cRes.status})`);
        const b = await bRes.json();
        const c = await cRes.json();
        if (cancelled) return;
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
        if (!cancelled) setLoading(false);
      }
    }
    load();
    return () => { cancelled = true; };
  }, [boardId]);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <a href="#/" className="text-sm text-blue-600 hover:underline">← All boards</a>
          <h2 className="text-xl font-semibold mt-1">{board ? board.name : 'Board'}</h2>
          {board && <p className="text-xs text-slate-500">Created {new Date(board.createdUtc).toLocaleString()}</p>}
        </div>
      </div>

      {loading && <p className="text-sm text-slate-500">Loading board…</p>}
      {error && <p className="text-sm text-red-600">{error}</p>}

      {!loading && !error && (
        <section>
          {columns.length === 0 ? (
            <p className="text-sm text-slate-500">No columns yet.</p>
          ) : (
            <div className="flex gap-4 overflow-auto">
              {columns.sort((a, b) => a.order - b.order).map(col => (
                <div key={col.id} className="min-w-[220px] rounded border bg-white">
                  <div className="px-3 py-2 border-b font-medium flex items-center justify-between">
                    <span>{col.name}</span>
                    {col.wipLimit != null && <span className="text-xs text-slate-500">WIP {col.wipLimit}</span>}
                  </div>
                  <div className="p-3 text-xs text-slate-500">Cards will appear here…</div>
                </div>
              ))}
            </div>
          )}
        </section>
      )}
    </div>
  );
};
