/** Grid filter type options */
export type GridFilterType = 'text' | 'number' | 'date' | 'select';

/** Grid filter mode options */
export type GridFilterMode = 'contains' | 'equals' | 'exact' | 'compare' | 'range';

/** Grid number comparison operators */
export type GridNumberOperator = 'eq' | 'gt' | 'gte' | 'lt' | 'lte';

/** Grid date comparison operators */
export type GridDateOperator =
  | 'equals'
  | 'notEqual'
  | 'after'
  | 'afterEqual'
  | 'before'
  | 'beforeEqual'
  | 'isNull'
  | 'isNotNull';

/** Grid text comparison operators */
export type GridTextOperator = 'contains' | 'notContains' | 'startsWith' | 'endsWith' | 'equals';

/** Multi-column sort descriptor */
export interface GridSortDescriptor {
  field: string;
  order: 'asc' | 'desc';
  priority: number;
}

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
  /** Date operator (for date/calendar filters) */
  dateOperator?: GridDateOperator;
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
  /** Optional lucide icon name for column UI */
  icon?: string;
  /** Whether column is hidden in the grid */
  hidden?: boolean;
  /** Column width (CSS value) */
  width?: string;
  /** Cell renderer type (text, profile, image, badge, tier, email, calendar, action, selection, etc.) */
  type?:
    | 'text'
    | 'flag'
    | 'action'
    | 'number'
    | 'date'
    | 'calendar'
    | 'badge'
    | 'image'
    | 'rate'
    | 'selection'
    | 'profile'
    | 'tier'
    | 'email'
    | 'progress'
    | 'boolean'
    | 'link'
    | 'currency'
    | 'tags';
  /** Optional visual style preset for badge cells */
  badgeStyle?:
    | 'default'
    | 'mechanics'
    | 'aiTag'
    | 'financePlan'
    | 'financeStatus'
    | 'verified'
    | 'userTier'
    | 'userStatus'
    | 'difficulty'
    | 'billingStatus';
  /** Optional icon name for action button cells */
  actionIcon?: string;
  /** Optional event type emitted by action button cells */
  actionType?: string;
  /** Optional accessible title for action button cells */
  actionLabel?: string;
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
  /** Whether the column stays frozen on the left side while horizontal scrolling */
  locked?: boolean;
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
