export type GridFilterType = 'text' | 'number' | 'date' | 'select';

export type GridFilterMode = 'contains' | 'equals' | 'exact' | 'compare' | 'range';

export type GridNumberOperator = 'eq' | 'gt' | 'gte' | 'lt' | 'lte';

export type GridTextOperator = 'contains' | 'notContains' | 'startsWith' | 'endsWith' | 'equals';

export interface GridFilterOption {
  label: string;
  value: string | number;
}

export interface GridFilterPayload {
  field: string;
  filterType: GridFilterType;
  mode: GridFilterMode;
  value?: string | number;
  operator?: GridNumberOperator;
  textOperator?: GridTextOperator;
  from?: string;
  to?: string;
  min?: number;
  max?: number;
}

export interface ColumnConfig {
  field: string;
  title: string;
  width?: string;
  type?: 'text' | 'flag' | 'action' | 'number' | 'date' | 'badge' | 'image' | 'rate' | 'calendar' | 'selection' | 'profile' | 'tier' | 'email';
  sortable?: boolean;
  sortOrder?: 'asc' | 'desc' | null;
  filterType?: GridFilterType;
  filterMode?: GridFilterMode;
  filterOptions?: GridFilterOption[];
}

export interface GridPagination {
  currentPage: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}
