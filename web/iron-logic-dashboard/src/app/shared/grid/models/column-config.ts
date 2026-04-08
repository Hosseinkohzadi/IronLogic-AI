export interface ColumnConfig {
  field: string;
  title: string;
  width?: string;
  type?: 'text' | 'flag' | 'action' | 'number' | 'date' | 'badge' | 'image' | 'rate' | 'calendar' | 'selection' | 'profile' | 'tier' | 'email';
  sortable?: boolean;
  sortOrder?: 'asc' | 'desc' | null;
}

export interface GridPagination {
  currentPage: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}
