import React, { useEffect, useState } from 'react';
interface CreateBoardFormProps {
    onCreated: () => void;
}

const CreateBoardForm: React.FC<CreateBoardFormProps> = ({ onCreated }) => {
    const [name, setName] = useState('');
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    async function submit(e: React.FormEvent) {
        e.preventDefault();
        if (!name.trim()) return;
        setSubmitting(true);
        setError(null);
        try {
            const res = await fetch('/api/boards', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name: name.trim() }),
            });
            if (!res.ok) throw new Error(`Error ${res.status}`);
            setName('');
            onCreated();
        } catch (err: any) {
            setError(err.message || 'Failed to create board');
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <form onSubmit={submit} className="flex flex-col gap-2 max-w-sm">
            <div className="flex gap-2">
                <input
                    className="border rounded px-2 py-1 flex-1"
                    placeholder="Board name"
                    value={name}
                    onChange={e => setName(e.target.value)}
                    disabled={submitting}
                />
                <button
                    type="submit"
                        className="bg-blue-600 text-white px-3 py-1 rounded disabled:opacity-50"
                    disabled={submitting || !name.trim()}
                >
                    {submitting ? 'Creating...' : 'Create'}
                </button>
            </div>
            {error && <p className="text-sm text-red-600">{error}</p>}
        </form>
    );
};
import { ListBoards } from './ListBoards';
import { Board } from './types';

export const BoardsPage: React.FC = () => {
  const [boards, setBoards] = useState<Board[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function loadBoards() {
    setLoading(true);
    try {
      const res = await fetch('/api/boards');
      if (!res.ok) throw new Error(`Error ${res.status}`);
      const data = await res.json();
      // API returns { boards: [...] }
      setBoards(data.boards ?? data.Boards ?? []);
      setError(null);
    } catch (e: any) {
      setError(e.message || 'Failed to load boards');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { loadBoards(); }, []);

  return (
    <div className="space-y-6">
      <section>
        <h2 className="text-lg font-medium mb-2">Create Board</h2>
        <CreateBoardForm onCreated={() => loadBoards()} />
      </section>
      <section>
        <div className="flex items-center gap-2 mb-2">
          <h2 className="text-lg font-medium">Boards</h2>
          <button onClick={loadBoards} className="text-sm text-blue-600 hover:underline">Refresh</button>
        </div>
        {loading && <p className="text-sm text-slate-500">Loading...</p>}
        {error && <p className="text-sm text-red-600">{error}</p>}
        {!loading && !boards.length && <p className="text-sm text-slate-500">No boards yet</p>}
        <ListBoards boards={boards} />
      </section>
    </div>
  );
};
