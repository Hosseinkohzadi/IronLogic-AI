/** Grid filter type options */
export type GridFilterType = 'text' | 'number' | 'date' | 'select';

/** Grid filter mode options */
export type GridFilterMode = 'contains' | 'equals' | 'exact' | 'compare' | 'range';

/** Grid number comparison operators */
export type GridNumberOperator = 'eq' | 'gt' | 'gte' | 'lt' | 'lte';

/** Grid text comparison operators */
export type GridTextOperator = 'contains' | 'notContains' | 'startsWith' | 'endsWith' | 'equals';

/** Filter option configuration */
export interface GridFilterOption {
  /** Display label for the option */
  label: string;
  /** Option value */
  value: string | number;
}

/** Grid filter payload for filter events */
export interface GridFilterPayload {
  /** Field being filtered */
  field: string;
  /** Type of filter */
  filterType: GridFilterType;
  /** Filter comparison mode */
  mode: GridFilterMode;
  /** Filter value */
  value?: string | number;
  /** Number operator (for numeric filters) */
  operator?: GridNumberOperator;
  /** Text operator (for text filters) */
  textOperator?: GridTextOperator;
  /** Range start value */
  from?: string;
  /** Range end value */
  to?: string;
  /** Minimum value */
  min?: number;
  /** Maximum value */
  max?: number;
}

/**
 * Column configuration for grid rendering.
 * Defines how a column should be displayed, filtered, and sorted.
 */
export interface ColumnConfig {
  /** Column data field key */
  field: string;
  /** Column header title */
  title: string;
  /** Column width (CSS value) */
  width?: string;
  /** Cell renderer type (text, profile, image, badge, tier, email, calendar, action, selection, etc.) */
  type?: 'text' | 'flag' | 'action' | 'number' | 'date' | 'badge' | 'image' | 'rate' | 'calendar' | 'selection' | 'profile' | 'tier' | 'email';
  /** Whether column is sortable */
  sortable?: boolean;
  /** Current sort order */
  sortOrder?: 'asc' | 'desc' | null;
  /** Filter type for this column */
  filterType?: GridFilterType;
  /** Filter comparison mode */
  filterMode?: GridFilterMode;
  /** Filter options (for select filters) */
  filterOptions?: GridFilterOption[];
  /** Secondary field to display (e.g., scientific name in profile column) */
  subfield?: string;
}

/** Grid pagination metadata */
export interface GridPagination {
  /** Current page number */
  currentPage: number;
  /** Items per page */
  pageSize: number;
  /** Total number of items */
  totalItems: number;
  /** Total number of pages */
  totalPages: number;
}
