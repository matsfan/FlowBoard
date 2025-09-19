export interface Board {
  id: string;
  name: string;
  createdUtc: string;
}

export interface Column {
  id: string;
  name: string;
  order: number;
  wipLimit: number | null;
}

export interface Card {
  id: string;
  title: string;
  description: string | null;
  order: number;
  isArchived: boolean;
  createdUtc: string;
}
