import React from 'react';
import { Board } from './types';

interface Props { boards: Board[]; }

export const ListBoards: React.FC<Props> = ({ boards }) => {
  if (!boards.length) return null;
  return (
    <ul className="divide-y rounded border bg-white">{
      boards.map(b => (
        <li key={b.id} className="p-3 flex items-center justify-between">
          <div>
            <p className="font-medium">{b.name}</p>
            <p className="text-xs text-slate-500">{new Date(b.createdUtc).toLocaleString()}</p>
          </div>
          <a href={`#/boards/${b.id}`} className="text-xs text-blue-600 hover:underline">Open</a>
        </li>
      ))
    }</ul>
  );
};
