export interface ColumnConfig {
  field: string;
  title: string;
  width?: string;
  type?: 'text' | 'flag' | 'action' | 'number' | 'date' | 'badge' | 'image' | 'rate'|'calendar';
  sortable?: boolean; // آیا این ستون قابلیت سورت دارد؟
  sortOrder?: 'asc' | 'desc' | null; // وضعیت فعلی سورت
}

export interface GridPagination {
  currentPage: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}
