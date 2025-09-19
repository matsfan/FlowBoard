import React, { useState } from 'react';
import { Board } from './types';

interface Props {
  boards: Board[];
  onDeleted?: () => void;
}

export const ListBoards: React.FC<Props> = ({ boards, onDeleted }) => {
  const [deletingId, setDeletingId] = useState<string | null>(null);

  async function deleteBoard(id: string) {
    if (!window.confirm('Delete this board?')) return;
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
    <ul className="divide-y rounded border bg-white">{
      boards.map(b => (
        <li key={b.id} className="p-3 flex items-center justify-between gap-2">
          <div>
            <p className="font-medium">{b.name}</p>
            <p className="text-xs text-slate-500">{new Date(b.createdUtc).toLocaleString()}</p>
          </div>
          <div className="flex gap-2 items-center">
            <a href={`#/boards/${b.id}`} className="text-xs text-blue-600 hover:underline">Open</a>
            <button
              className="text-xs text-red-600 hover:underline px-2 py-1 rounded border border-red-200"
              disabled={deletingId === b.id}
              onClick={() => deleteBoard(b.id)}
            >{deletingId === b.id ? 'Deleting...' : 'Delete'}</button>
          </div>
        </li>
      ))
    }</ul>
  );
};
