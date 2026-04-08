import { Component, EventEmitter, Input, Output, HostListener, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ColumnConfig, GridFilterPayload, GridNumberOperator } from '../../models/column-config';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

@Component({
  selector: 'app-grid-header',
  standalone: true,
  imports: [CommonModule, DragDropModule],
  templateUrl: './grid-header.html',
  styleUrls: ['./grid-header.css']
})
export class GridHeaderComponent implements OnChanges {
  @Input() columns: ColumnConfig[] = [];

  // این خط اضافه شد تا ارورهای TS2339 و NG8002 برطرف شوند
  @Input() showFilters: boolean = false;
  @Input() filterResetKey: number = 0;

  @Output() sortChange = new EventEmitter<ColumnConfig>();
  @Output() filterChange = new EventEmitter<GridFilterPayload>();
  @Output() toggleAll = new EventEmitter<boolean>();

  private textFilterSubject = new Subject<GridFilterPayload>();

  dateModes: Record<string, 'exact' | 'range'> = {};
  numberModes: Record<string, 'compare' | 'range'> = {};
  numberOperators: Record<string, GridNumberOperator> = {};
  textFilterValues: Record<string, string> = {};
  selectFilterValues: Record<string, string> = {};

  dateExact: Record<string, string> = {};
  dateFrom: Record<string, string> = {};
  dateTo: Record<string, string> = {};
  numberValue: Record<string, number | null> = {};
  numberMin: Record<string, number | null> = {};
  numberMax: Record<string, number | null> = {};

  // وضعیت‌های Resizing
  private resizingColumn?: ColumnConfig;
  private startX = 0;
  private startWidth = 0;

  constructor() {
    this.textFilterSubject.pipe(
      debounceTime(300),
      distinctUntilChanged((prev, curr) => prev.field === curr.field && prev.value === curr.value)
    ).subscribe(filter => {
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

  // --- مدیریت Drag & Drop ---
  onColumnDrop(event: CdkDragDrop<ColumnConfig[]>) {
    moveItemInArray(this.columns, event.previousIndex, event.currentIndex);
  }

  // --- مدیریت Resizing ---
  onResizeStart(event: MouseEvent, column: ColumnConfig) {
    event.preventDefault();
    event.stopPropagation(); // جلوگیری از تداخل با Drag

    this.resizingColumn = column;
    this.startX = event.pageX;
    this.startWidth = this.getSafeWidth(column.width);

    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';
  }

  @HostListener('document:mousemove', ['$event'])
  onMouseMove(event: MouseEvent) {
    if (!this.resizingColumn) return;
    // محاسبه RTL: کشیدن به سمت چپ باعث افزایش عرض می‌شود
    const delta = this.startX - event.pageX;
    const minWidth = this.getMinWidth(this.resizingColumn);
    const newWidth = Math.max(minWidth, this.startWidth + delta);
    this.resizingColumn.width = `${newWidth}px`;
  }

  @HostListener('document:mouseup')
  onMouseUp() {
    if (this.resizingColumn) {
      this.resizingColumn = undefined;
      document.body.style.cursor = 'default';
      document.body.style.userSelect = 'auto';
    }
  }

  // --- سایر متدها ---
  onToggleAll(event: any) {
    this.toggleAll.emit(event.target.checked);
  }

  onSort(column: ColumnConfig) {
    if (['action', 'selection', 'image'].includes(column.type!) || column.field === 'avatar') return;
    column.sortOrder = column.sortOrder === 'asc' ? 'desc' : (column.sortOrder === 'desc' ? null : 'asc');
    this.columns.forEach(c => { if (c.field !== column.field) c.sortOrder = null; });
    this.sortChange.emit(column);
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

  onTextInputFilter(event: Event, field: string) {
    const value = (event.target as HTMLInputElement).value;
    this.textFilterValues[field] = value;
    this.textFilterSubject.next({
      field,
      filterType: 'text',
      mode: 'contains',
      value
    });
  }

  onSelectFilter(event: any, field: string) {
    this.selectFilterValues[field] = event.target.value;
    this.filterChange.emit({
      field,
      filterType: 'select',
      mode: 'equals',
      value: event.target.value
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
        value: this.dateExact[field] || ''
      });
      return;
    }

    this.filterChange.emit({
      field,
      filterType: 'date',
      mode: 'range',
      from: this.dateFrom[field] || '',
      to: this.dateTo[field] || ''
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
        value: this.numberValue[field] ?? undefined
      });
      return;
    }

    this.filterChange.emit({
      field,
      filterType: 'number',
      mode: 'range',
      min: this.numberMin[field] ?? undefined,
      max: this.numberMax[field] ?? undefined
    });
  }

  private parseNumber(value: string): number | null {
    if (value.trim() === '') return null;
    const parsed = Number(value);
    return Number.isNaN(parsed) ? null : parsed;
  }

  private resetFilterUiState() {
    this.textFilterValues = {};
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

  private getSafeWidth(width?: string): number {
    const parsed = parseInt(width || '150', 10);
    return Number.isNaN(parsed) ? 150 : parsed;
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
        return 140;
    }
  }
}
