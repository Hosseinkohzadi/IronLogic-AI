export interface ColumnConfig {
  field: string;
  title: string;
  width?: string;
  type?: 'text' | 'number' | 'date';
  sortable?: boolean; // آیا این ستون قابلیت سورت دارد؟
  sortOrder?: 'asc' | 'desc' | null; // وضعیت فعلی سورت
}

export interface GridPagination {
  currentPage: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}
