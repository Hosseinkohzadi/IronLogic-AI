import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  Output,
  HostListener,
  OnChanges,
  SimpleChanges,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ColumnConfig,
  GridDateOperator,
  GridFilterPayload,
  GridNumberOperator,
  GridSortDescriptor,
  GridTextOperator,
} from '../../models/column-config';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, take } from 'rxjs/operators';
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule } from '@angular/forms';
import { GridDataService } from '../../services/grid-data';

@Component({
  selector: 'app-grid-header',
  standalone: true,
  imports: [CommonModule, FormsModule, DragDropModule, LucideAngularModule],
  templateUrl: './grid-header.html',
  styleUrls: ['./grid-header.css'],
})
export class GridHeaderComponent implements OnChanges, OnDestroy {
  @Input() columns: ColumnConfig[] = [];
  @Input() fitViewportMode: boolean = false;
  @Input() reorderable: boolean = false;

  get visibleColumns(): ColumnConfig[] {
    return this.columns.filter((col) => !col.hidden);
  }

  // این خط اضافه شد تا ارورهای TS2339 و NG8002 برطرف شوند
  @Input() showFilters: boolean = false;
  @Input() filterResetKey: number = 0;

  @Output() sortChange = new EventEmitter<GridSortDescriptor[]>();
  @Output() filterChange = new EventEmitter<GridFilterPayload>();
  @Output() toggleAll = new EventEmitter<boolean>();
  @Output() columnDrop = new EventEmitter<CdkDragDrop<ColumnConfig[]>>();

  activeFilterMenu = signal<string | null>(null);
  activeSorts = signal<GridSortDescriptor[]>([]);
  tempFilterValue = signal<any>('');
  tempFilterOperator = signal<string>('contains');
  tempSelectedOptions = signal<string[]>([]);
  tempRangeStart = signal<string>('');
  tempRangeEnd = signal<string>('');
  tempDate = signal<string>('');
  tempTime = signal<string>('00:00');
  uniqueColumnValues = signal<Record<string, string[]>>({});

  private textFilterSubject = new Subject<GridFilterPayload>();

  dateModes: Record<string, 'exact' | 'range'> = {};
  numberModes: Record<string, 'compare' | 'range'> = {};
  numberOperators: Record<string, GridNumberOperator> = {};
  textFilterValues: Record<string, string> = {};
  textOperators: Record<string, GridTextOperator> = {};
  selectFilterValues: Record<string, string> = {};

  dateExact: Record<string, string> = {};
  dateFrom: Record<string, string> = {};
  dateTo: Record<string, string> = {};
  numberValue: Record<string, number | null> = {};
  numberMin: Record<string, number | null> = {};
  numberMax: Record<string, number | null> = {};

  // Resize state
  resizingColumn: ColumnConfig | null = null;
  startMouseX = 0;
  startColWidth = 0;

  private readonly resizeMouseMoveListener = (event: MouseEvent) => this.onMouseMove(event);
  private readonly resizeMouseUpListener = () => this.onMouseUp();

  constructor(
    private elementRef: ElementRef<HTMLElement>,
    private gridDataService: GridDataService,
  ) {
    this.textFilterSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(
          (prev, curr) =>
            prev.field === curr.field &&
            prev.value === curr.value &&
            prev.textOperator === curr.textOperator,
        ),
      )
      .subscribe((filter) => {
        this.filterChange.emit(filter);
      });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['columns'] || changes['showFilters']) {
      this.applyColumnMinWidths();
    }

    if (changes['filterResetKey'] && !changes['filterResetKey'].firstChange) {
      this.resetFilterUiState();
    }
  }

  ngOnDestroy(): void {
    this.onMouseUp();
  }

  // --- مدیریت Drag & Drop ---
  onColumnDrop(event: CdkDragDrop<ColumnConfig[]>) {
    this.columnDrop.emit(event);
  }

  // --- مدیریت Resizing ---
  onResizeStart(event: MouseEvent, column: ColumnConfig): void {
    if (!this.isColumnResizable(column)) {
      return;
    }

    event.preventDefault();
    event.stopPropagation(); // جلوگیری از تداخل با Drag

    this.resizingColumn = column;
    this.startMouseX = event.clientX;
    this.startColWidth = this.getSafeWidth(column.width);

    document.addEventListener('mousemove', this.resizeMouseMoveListener);
    document.addEventListener('mouseup', this.resizeMouseUpListener);

    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';
  }

  onMouseMove(event: MouseEvent): void {
    if (!this.resizingColumn) return;

    const deltaX = event.clientX - this.startMouseX;
    const minWidth = this.getColumnResizeMinWidth(this.resizingColumn);
    const nextWidth = Math.max(minWidth, this.startColWidth + deltaX);
    this.resizingColumn.width = `${Math.round(nextWidth)}px`;
  }

  onMouseUp(): void {
    document.removeEventListener('mousemove', this.resizeMouseMoveListener);
    document.removeEventListener('mouseup', this.resizeMouseUpListener);

    if (this.resizingColumn) {
      this.resizingColumn = null;
    }

    document.body.style.cursor = 'default';
    document.body.style.userSelect = 'auto';
  }

  autoFitColumn(column: ColumnConfig): void {
    if (!this.isColumnResizable(column)) {
      return;
    }

    this.gridDataService.processedData$.pipe(take(1)).subscribe((rows) => {
      const headerText = String(column.title || column.field || '').trim();
      const sampleCell = this.elementRef.nativeElement.querySelector(
        '.header-cell',
      ) as HTMLElement | null;
      const computedFont = sampleCell
        ? `${getComputedStyle(sampleCell).fontWeight} ${getComputedStyle(sampleCell).fontSize} ${getComputedStyle(sampleCell).fontFamily}`
        : '500 12px Inter, sans-serif';

      let maxTextWidth = this.measureTextWidth(headerText, computedFont);

      for (const row of rows) {
        const rawValue = row?.[column.field];
        const text = rawValue == null ? '' : String(rawValue);
        const textWidth = this.measureTextWidth(text, computedFont);
        if (textWidth > maxTextWidth) {
          maxTextWidth = textWidth;
        }
      }

      const paddedWidth = maxTextWidth + 24;
      const minWidth = this.getColumnResizeMinWidth(column);
      const maxWidth = this.getColumnResizeMaxWidth(column);

      let nextWidth = Math.max(minWidth, paddedWidth);
      if (maxWidth != null) {
        nextWidth = Math.min(nextWidth, maxWidth);
      }

      column.width = `${Math.ceil(nextWidth)}px`;
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.elementRef.nativeElement.contains(event.target as Node)) {
      this.activeFilterMenu.set(null);
    }
  }

  // --- سایر متدها ---
  onToggleAll(event: any) {
    this.toggleAll.emit(event.target.checked);
  }

  handleSort(column: ColumnConfig, event: MouseEvent) {
    if (
      ['action', 'selection', 'image'].includes(column.type!) ||
      column.field === 'avatar' ||
      !column.sortable
    )
      return;

    const currentSorts = [...this.activeSorts()];
    const existingIndex = currentSorts.findIndex((item) => item.field === column.field);

    if (!event.ctrlKey) {
      const nextOrder = this.getNextSortOrder(currentSorts[existingIndex]?.order);
      const nextSorts: GridSortDescriptor[] = [
        {
          field: column.field,
          order: nextOrder,
          priority: 1,
        },
      ];

      this.activeSorts.set(nextSorts);
      this.syncColumnSortOrders(nextSorts);
      this.sortChange.emit(nextSorts);
      return;
    }

    if (existingIndex >= 0) {
      const existing = currentSorts[existingIndex];
      currentSorts[existingIndex] = {
        ...existing,
        order: this.getNextSortOrder(existing.order),
      };
    } else {
      currentSorts.push({
        field: column.field,
        order: 'asc',
        priority: currentSorts.length + 1,
      });
    }

    const normalized = currentSorts.map((item, index) => ({ ...item, priority: index + 1 }));

    this.activeSorts.set(normalized);
    this.syncColumnSortOrders(normalized);
    this.sortChange.emit(normalized);
  }

  getSortPriority(field: string): number | null {
    const sort = this.activeSorts().find((item) => item.field === field);
    return sort ? sort.priority : null;
  }

  getSortOrder(field: string): 'asc' | 'desc' | null {
    const sort = this.activeSorts().find((item) => item.field === field);
    return sort ? sort.order : null;
  }

  hasMultiSort(): boolean {
    return this.activeSorts().length > 1;
  }

  private getNextSortOrder(current?: 'asc' | 'desc' | null): 'asc' | 'desc' {
    return current === 'asc' ? 'desc' : 'asc';
  }

  private syncColumnSortOrders(sorts: GridSortDescriptor[]): void {
    this.columns.forEach((column) => {
      const sort = sorts.find((item) => item.field === column.field);
      column.sortOrder = sort ? sort.order : null;
    });
  }

  toggleFilterMenu(col: ColumnConfig, event?: MouseEvent) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    if (this.activeFilterMenu() === col.field) {
      this.activeFilterMenu.set(null);
      return;
    }

    this.resetTempFilterState();

    if (this.isCalendarType(col.type)) {
      this.tempFilterOperator.set('equals');
      this.parseExistingCalendarFilter(col.field);
    } else if (this.isNumericType(col.type)) {
      this.tempFilterOperator.set('eq');
    } else if (this.isDateType(col.type)) {
      this.tempFilterOperator.set('equals');
    } else if (this.isSelectionType(col.type)) {
      this.tempFilterOperator.set('equals');
      this.loadUniqueColumnValues(col.field);
    } else if (this.isTextType(col.type)) {
      this.tempFilterOperator.set('contains');
    } else {
      this.tempFilterOperator.set('contains');
    }

    this.activeFilterMenu.set(col.field);
  }

  applyFilter(field: string): void {
    const col = this.columns.find((item) => item.field === field);
    if (!col) {
      this.activeFilterMenu.set(null);
      return;
    }

    if (this.isCalendarType(col.type)) {
      const operator = this.toDateOperator(this.tempFilterOperator());

      if (operator === 'isNull' || operator === 'isNotNull') {
        sessionStorage.removeItem(`calendar_${field}`);
        sessionStorage.setItem(`calendar_operator_${field}`, operator);

        this.filterChange.emit({
          field,
          filterType: 'date',
          mode: 'exact',
          dateOperator: operator,
        });
        this.activeFilterMenu.set(null);
        return;
      }

      const dateStr = this.tempDate();
      const timeStr = this.tempTime() || '00:00';

      if (!dateStr) {
        this.activeFilterMenu.set(null);
        return;
      }

      const combinedValue = `${dateStr}T${timeStr}:00`;

      sessionStorage.setItem(`calendar_${field}`, combinedValue);
      sessionStorage.setItem(`calendar_operator_${field}`, operator);

      this.filterChange.emit({
        field,
        filterType: 'date',
        mode: 'exact',
        value: combinedValue,
        dateOperator: operator,
      });
      this.activeFilterMenu.set(null);
      return;
    }

    if (this.isNumericType(col.type)) {
      const op = this.tempFilterOperator();
      if (op === 'between') {
        const min = this.tempRangeStart() === '' ? undefined : Number(this.tempRangeStart());
        const max = this.tempRangeEnd() === '' ? undefined : Number(this.tempRangeEnd());
        this.filterChange.emit({
          field,
          filterType: 'number',
          mode: 'range',
          min,
          max,
        });
      } else {
        const operator: GridNumberOperator = op === 'gt' ? 'gt' : op === 'lt' ? 'lt' : 'eq';
        const value = this.tempFilterValue() === '' ? undefined : Number(this.tempFilterValue());
        this.filterChange.emit({
          field,
          filterType: 'number',
          mode: 'compare',
          operator,
          value,
        });
      }
      this.activeFilterMenu.set(null);
      return;
    }

    if (this.isDateType(col.type)) {
      const op = this.tempFilterOperator();
      if (op === 'range') {
        this.filterChange.emit({
          field,
          filterType: 'date',
          mode: 'range',
          from: this.tempRangeStart(),
          to: this.tempRangeEnd(),
        });
      } else if (op === 'before') {
        this.filterChange.emit({
          field,
          filterType: 'date',
          mode: 'range',
          to: String(this.tempFilterValue() || ''),
        });
      } else if (op === 'after') {
        this.filterChange.emit({
          field,
          filterType: 'date',
          mode: 'range',
          from: String(this.tempFilterValue() || ''),
        });
      } else {
        this.filterChange.emit({
          field,
          filterType: 'date',
          mode: 'exact',
          value: String(this.tempFilterValue() || ''),
        });
      }
      this.activeFilterMenu.set(null);
      return;
    }

    if (this.isSelectionType(col.type)) {
      const selected = this.tempSelectedOptions();
      this.filterChange.emit({
        field,
        filterType: 'select',
        mode: 'equals',
        value: selected.length > 0 ? selected[0] : '',
      });
      this.activeFilterMenu.set(null);
      return;
    }

    const textOperator = this.toTextOperator(this.tempFilterOperator());
    this.filterChange.emit({
      field,
      filterType: 'text',
      mode: 'contains',
      value: String(this.tempFilterValue() || ''),
      textOperator,
    });

    this.activeFilterMenu.set(null);
  }

  clearFilter(field: string): void {
    const col = this.columns.find((item) => item.field === field);
    this.resetTempFilterState();

    if (this.isCalendarType(col?.type)) {
      sessionStorage.removeItem(`calendar_${field}`);
      sessionStorage.removeItem(`calendar_operator_${field}`);
      this.filterChange.emit({
        field,
        filterType: 'date',
        mode: 'exact',
        value: '',
        dateOperator: 'equals',
      });
    } else if (this.isNumericType(col?.type)) {
      this.filterChange.emit({
        field,
        filterType: 'number',
        mode: 'compare',
        operator: 'eq',
        value: undefined,
      });
    } else if (this.isDateType(col?.type)) {
      this.filterChange.emit({
        field,
        filterType: 'date',
        mode: 'exact',
        value: '',
      });
    } else if (this.isSelectionType(col?.type)) {
      this.filterChange.emit({
        field,
        filterType: 'select',
        mode: 'equals',
        value: '',
      });
    } else {
      this.filterChange.emit({
        field,
        filterType: 'text',
        mode: 'contains',
        value: '',
        textOperator: 'contains',
      });
    }

    this.activeFilterMenu.set(null);
  }

  toggleOption(value: string): void {
    this.tempSelectedOptions.update((current) =>
      current.includes(value) ? current.filter((item) => item !== value) : [...current, value],
    );
  }

  getUniqueColumnValues(field: string): string[] {
    return this.uniqueColumnValues()[field] || [];
  }

  private loadUniqueColumnValues(field: string): void {
    this.gridDataService.processedData$.pipe(take(1)).subscribe((rows) => {
      const values = [
        ...new Set(
          rows.map((row) => String(row[field] ?? '').trim()).filter((value) => value.length > 0),
        ),
      ];

      this.uniqueColumnValues.update((current) => ({
        ...current,
        [field]: values,
      }));
    });
  }

  private toTextOperator(operator: string): GridTextOperator {
    if (
      operator === 'startsWith' ||
      operator === 'endsWith' ||
      operator === 'equals' ||
      operator === 'contains'
    ) {
      return operator;
    }

    return 'contains';
  }

  getDateMode(field: string, defaultMode?: string): 'exact' | 'range' {
    if (this.dateModes[field]) return this.dateModes[field];
    return defaultMode === 'range' ? 'range' : 'exact';
  }

  getNumberMode(field: string, defaultMode?: string): 'compare' | 'range' {
    if (this.numberModes[field]) return this.numberModes[field];
    return defaultMode === 'range' ? 'range' : 'compare';
  }

  getNumberOperator(field: string): GridNumberOperator {
    if (!this.numberOperators[field]) {
      this.numberOperators[field] = 'eq';
    }
    return this.numberOperators[field];
  }

  getTextOperator(field: string): GridTextOperator {
    if (!this.textOperators[field]) {
      this.textOperators[field] = 'contains';
    }
    return this.textOperators[field];
  }

  // Type categorization helpers
  isTextType(type?: string): boolean {
    return ['text', 'profile', 'email', 'link'].includes(type || '');
  }

  isCalendarType(type?: string): boolean {
    return type === 'calendar';
  }

  isDateType(type?: string): boolean {
    return type === 'date';
  }

  isNumericType(type?: string): boolean {
    return ['number', 'rate', 'progress', 'currency'].includes(type || '');
  }

  isSelectionType(type?: string): boolean {
    return ['badge', 'tier', 'status', 'flag', 'boolean', 'tags'].includes(type || '');
  }

  getLockedOffset(field: string): string {
    let offset = 0;

    for (const column of this.columns) {
      if (column.field === field) {
        break;
      }

      if (column.locked) {
        offset += this.getSafeWidth(column.width);
      }
    }

    return `${offset}px`;
  }

  isLastLocked(field: string): boolean {
    const lockedColumns = this.columns.filter((column) => column.locked);
    if (lockedColumns.length === 0) {
      return false;
    }

    return lockedColumns[lockedColumns.length - 1].field === field;
  }

  private getSafeWidth(width?: string): number {
    const parsed = Number.parseInt(String(width ?? '150').replace('px', ''), 10);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : 150;
  }

  private getColumnResizeMinWidth(column: ColumnConfig): number {
    const rawMinWidth = (column as ColumnConfig & { minWidth?: string | number }).minWidth;

    if (typeof rawMinWidth === 'number' && Number.isFinite(rawMinWidth)) {
      return Math.max(50, rawMinWidth);
    }

    if (typeof rawMinWidth === 'string') {
      const parsed = Number.parseInt(rawMinWidth.replace('px', '').trim(), 10);
      if (Number.isFinite(parsed)) {
        return Math.max(50, parsed);
      }
    }

    return 50;
  }

  private getColumnResizeMaxWidth(column: ColumnConfig): number | null {
    const rawMaxWidth = (column as ColumnConfig & { maxWidth?: string | number }).maxWidth;

    if (typeof rawMaxWidth === 'number' && Number.isFinite(rawMaxWidth)) {
      return Math.max(50, rawMaxWidth);
    }

    if (typeof rawMaxWidth === 'string') {
      const parsed = Number.parseInt(rawMaxWidth.replace('px', '').trim(), 10);
      if (Number.isFinite(parsed)) {
        return Math.max(50, parsed);
      }
    }

    return null;
  }

  private measureTextWidth(text: string, font: string): number {
    const canvas = document.createElement('canvas');
    const context = canvas.getContext('2d');
    if (!context) {
      return Math.max(0, text.length * 7);
    }

    context.font = font;
    return context.measureText(text).width;
  }

  isColumnResizable(column: ColumnConfig): boolean {
    const resizable = (column as ColumnConfig & { resizable?: boolean }).resizable;
    return resizable !== false;
  }

  isColumnFiltered(field: string): boolean {
    return !!this.gridDataService.getFilter(field);
  }

  isPopupAlignedRight(columnIndex: number): boolean {
    return columnIndex >= Math.max(this.columns.length - 2, 0);
  }

  shouldShowCalendarInputs(): boolean {
    const operator = this.toDateOperator(this.tempFilterOperator());
    return operator !== 'isNull' && operator !== 'isNotNull';
  }

  private resetTempFilterState(): void {
    this.tempFilterValue.set('');
    this.tempSelectedOptions.set([]);
    this.tempRangeStart.set('');
    this.tempRangeEnd.set('');
    this.tempDate.set('');
    this.tempTime.set('00:00');
  }

  /**
   * Parses an existing calendar filter value (ISO 8601 string) and populates tempDate and tempTime signals.
   * Expected format: "2026-04-15T13:00:00" or "2026-04-15T13:00"
   */
  private parseExistingCalendarFilter(field: string): void {
    const storedOperator = sessionStorage.getItem(`calendar_operator_${field}`);
    if (storedOperator) {
      this.tempFilterOperator.set(storedOperator);
    }

    const storedValue = sessionStorage.getItem(`calendar_${field}`);
    if (!storedValue) {
      return;
    }

    try {
      const dateTimeRegex = /^(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2})/;
      const match = storedValue.match(dateTimeRegex);

      if (match) {
        this.tempDate.set(match[1]);
        this.tempTime.set(match[2]);
      }
    } catch (error) {
      console.warn('Failed to parse calendar filter:', error);
      this.tempDate.set('');
      this.tempTime.set('00:00');
    }
  }

  private toDateOperator(operator: string): GridDateOperator {
    switch (operator) {
      case 'notEqual':
      case 'after':
      case 'afterEqual':
      case 'before':
      case 'beforeEqual':
      case 'isNull':
      case 'isNotNull':
      case 'equals':
        return operator;
      default:
        return 'equals';
    }
  }

  onTextOperatorChange(field: string, operator: GridTextOperator) {
    this.textOperators[field] = operator;
    const value = this.textFilterValues[field] || '';
    this.textFilterSubject.next({
      field,
      filterType: 'text',
      mode: 'contains',
      value,
      textOperator: operator,
    });
  }

  getTextOperatorIcon(operator: GridTextOperator): string {
    switch (operator) {
      case 'contains':
        return 'search';
      case 'notContains':
        return 'x';
      case 'startsWith':
        return 'chevron-right';
      case 'endsWith':
        return 'chevron-left';
      case 'equals':
        return 'check';
      default:
        return 'search';
    }
  }

  getTextOperatorLabel(operator: GridTextOperator): string {
    switch (operator) {
      case 'contains':
        return 'Contains';
      case 'notContains':
        return 'Not Contains';
      case 'startsWith':
        return 'Starts With';
      case 'endsWith':
        return 'Ends With';
      case 'equals':
        return 'Equals';
      default:
        return 'Contains';
    }
  }

  onTextInputFilter(event: Event, field: string) {
    const value = (event.target as HTMLInputElement).value;
    this.textFilterValues[field] = value;
    const operator = this.getTextOperator(field);
    this.textFilterSubject.next({
      field,
      filterType: 'text',
      mode: 'contains',
      value,
      textOperator: operator,
    });
  }

  onSelectFilter(event: any, field: string) {
    this.selectFilterValues[field] = event.target.value;
    this.filterChange.emit({
      field,
      filterType: 'select',
      mode: 'equals',
      value: event.target.value,
    });
  }

  onDateModeChange(event: Event, field: string) {
    const mode = (event.target as HTMLSelectElement).value === 'range' ? 'range' : 'exact';
    this.dateModes[field] = mode;
    if (mode === 'exact') {
      this.dateFrom[field] = '';
      this.dateTo[field] = '';
    } else {
      this.dateExact[field] = '';
    }
    this.emitDateFilter(field);
  }

  onDateExactChange(event: Event, field: string) {
    this.dateExact[field] = (event.target as HTMLInputElement).value;
    this.emitDateFilter(field);
  }

  onDateFromChange(event: Event, field: string) {
    this.dateFrom[field] = (event.target as HTMLInputElement).value;
    this.emitDateFilter(field);
  }

  onDateToChange(event: Event, field: string) {
    this.dateTo[field] = (event.target as HTMLInputElement).value;
    this.emitDateFilter(field);
  }

  onNumberModeChange(event: Event, field: string) {
    const mode = (event.target as HTMLSelectElement).value === 'range' ? 'range' : 'compare';
    this.numberModes[field] = mode;
    if (mode === 'compare') {
      this.numberMin[field] = null;
      this.numberMax[field] = null;
    } else {
      this.numberValue[field] = null;
    }
    this.emitNumberFilter(field);
  }

  onNumberOperatorChange(event: Event, field: string) {
    const operator = (event.target as HTMLSelectElement).value as GridNumberOperator;
    this.numberOperators[field] = operator;
    this.emitNumberFilter(field);
  }

  onNumberValueChange(event: Event, field: string) {
    this.numberValue[field] = this.parseNumber((event.target as HTMLInputElement).value);
    this.emitNumberFilter(field);
  }

  onNumberMinChange(event: Event, field: string) {
    this.numberMin[field] = this.parseNumber((event.target as HTMLInputElement).value);
    this.emitNumberFilter(field);
  }

  onNumberMaxChange(event: Event, field: string) {
    this.numberMax[field] = this.parseNumber((event.target as HTMLInputElement).value);
    this.emitNumberFilter(field);
  }

  private emitDateFilter(field: string) {
    const mode = this.getDateMode(field);

    if (mode === 'exact') {
      this.filterChange.emit({
        field,
        filterType: 'date',
        mode: 'exact',
        value: this.dateExact[field] || '',
      });
      return;
    }

    this.filterChange.emit({
      field,
      filterType: 'date',
      mode: 'range',
      from: this.dateFrom[field] || '',
      to: this.dateTo[field] || '',
    });
  }

  private emitNumberFilter(field: string) {
    const mode = this.getNumberMode(field);

    if (mode === 'compare') {
      this.filterChange.emit({
        field,
        filterType: 'number',
        mode: 'compare',
        operator: this.getNumberOperator(field),
        value: this.numberValue[field] ?? undefined,
      });
      return;
    }

    this.filterChange.emit({
      field,
      filterType: 'number',
      mode: 'range',
      min: this.numberMin[field] ?? undefined,
      max: this.numberMax[field] ?? undefined,
    });
  }

  private parseNumber(value: string): number | null {
    if (value.trim() === '') return null;
    const parsed = Number(value);
    return Number.isNaN(parsed) ? null : parsed;
  }

  private resetFilterUiState() {
    this.activeSorts.set([]);
    this.columns.forEach((column) => {
      column.sortOrder = null;
    });

    this.textFilterValues = {};
    this.textOperators = {};
    this.selectFilterValues = {};
    this.dateModes = {};
    this.numberModes = {};
    this.numberOperators = {};
    this.dateExact = {};
    this.dateFrom = {};
    this.dateTo = {};
    this.numberValue = {};
    this.numberMin = {};
    this.numberMax = {};
  }

  private applyColumnMinWidths() {
    for (const column of this.columns) {
      const safeWidth = this.getSafeWidth(column.width);
      const minWidth = this.getMinWidth(column);
      if (safeWidth < minWidth) {
        column.width = `${minWidth}px`;
      }
    }
  }

  private getMinWidth(column: ColumnConfig): number {
    if (column.type === 'selection') return 70;
    if (column.type === 'action') return 90;
    if (column.type === 'image') return 100;

    if (!this.showFilters) return 120;

    switch (column.filterType) {
      case 'number':
        return 190;
      case 'date':
        return 200;
      case 'select':
        return 150;
      case 'text':
      default:
        return 190;
    }
  }
}
