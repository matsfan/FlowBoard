import React, { useState } from 'react';

interface Props {
  onCreated(): void;
}

export const CreateBoardForm: React.FC<Props> = ({ onCreated }) => {
  const [name, setName] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;
    setSubmitting(true);
    try {
      const res = await fetch('/api/boards', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name })
      });
      if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err?.errors?.join?.(', ') || `Error ${res.status}`);
      }
      setName('');
      setError(null);
      onCreated();
    } catch (e: any) {
      setError(e.message || 'Failed');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={submit} className="flex gap-2 items-start">
      <div className="flex-1">
        <input
          type="text"
          value={name}
          onChange={e => setName(e.target.value)}
          placeholder="Board name"
          className="w-full rounded border px-3 py-2 text-sm focus:outline-none focus:ring focus:ring-blue-200"
          disabled={submitting}
        />
        {error && <p className="mt-1 text-xs text-red-600">{error}</p>}
      </div>
      <button
        type="submit"
        disabled={submitting || !name.trim()}
        className="bg-blue-600 text-white text-sm font-medium px-4 py-2 rounded disabled:opacity-50 hover:bg-blue-500"
      >
        {submitting ? 'Creating...' : 'Create'}
      </button>
    </form>
  );
};
