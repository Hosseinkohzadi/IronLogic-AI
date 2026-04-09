import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { GridFilterPayload, GridNumberOperator, GridTextOperator } from '../models/column-config';

@Injectable()
export class GridDataService {
  // ۱. مخازن داده‌های خام و وضعیت‌ها (Private)
  private _rawData$ = new BehaviorSubject<any[]>([]);
  private _sortConfig$ = new BehaviorSubject<{ field: string; order: 'asc' | 'desc' | null }>({ field: '', order: null });
  private _filters$ = new BehaviorSubject<Record<string, GridFilterPayload>>({});
  private _currentPage$ = new BehaviorSubject<number>(1);
  private _pageSize$ = new BehaviorSubject<number>(10);

  // اضافه شدن مخزن انتخاب‌ها (Selection Storage)
  private _selectedItems$ = new BehaviorSubject<any[]>([]);
  public selectedItems$ = this._selectedItems$.asObservable();

  // ۲. پردازش داده‌ها (Filtering -> Sorting)
  private _processedData$: Observable<any[]> = combineLatest([
    this._rawData$,
    this._sortConfig$,
    this._filters$
  ]).pipe(
    map(([data, sort, filters]) => {
      let result = data.filter(row => {
        return Object.values(filters).every(filter => {
          const rowValue = row[filter.field];
          return this.matchesFilter(rowValue, filter);
        });
      });

      if (sort.order) {
        result = [...result].sort((a, b) => {
          let valA = a[sort.field];
          let valB = b[sort.field];
          if (valA instanceof Date) valA = valA.getTime();
          if (valB instanceof Date) valB = valB.getTime();
          if (valA == null) return 1;
          if (valB == null) return -1;
          if (valA < valB) return sort.order === 'asc' ? -1 : 1;
          if (valA > valB) return sort.order === 'asc' ? 1 : -1;
          return 0;
        });
      }
      return result;
    })
  );

  public processedData$ = this._processedData$;

  public pagedData$ = combineLatest([
    this._processedData$,
    this._currentPage$,
    this._pageSize$
  ]).pipe(
    map(([data, page, size]) => {
      const start = (page - 1) * size;
      return data.slice(start, start + size);
    })
  );

  public pagination$ = combineLatest([
    this._processedData$,
    this._currentPage$,
    this._pageSize$
  ]).pipe(
    map(([data, page, size]) => ({
      currentPage: page,
      pageSize: size,
      totalItems: data.length,
      totalPages: Math.ceil(data.length / size) || 1
    }))
  );

  setData(data: any[]) {
    this._rawData$.next(data || []); // ایمن‌سازی دیتا
    this._currentPage$.next(1);
    this._selectedItems$.next([]); // ریست کردن تیک‌ها هنگام لود جدید
  }

  applySort(field: string, order: 'asc' | 'desc' | null) {
    this._sortConfig$.next({ field, order });
  }

  updateFilter(filter: GridFilterPayload) {
    const current = this._filters$.value;
    const next = { ...current };

    if (this.isFilterEmpty(filter)) {
      delete next[filter.field];
    } else {
      next[filter.field] = filter;
    }

    this._filters$.next(next);
    this._currentPage$.next(1);
    this._selectedItems$.next([]); // ریست کردن انتخاب‌ها هنگام تغییر فیلتر
  }

  clearFilters() {
    this._filters$.next({});
    this._currentPage$.next(1);
    this._selectedItems$.next([]);
  }

  goToPage(page: number) {
    this._currentPage$.next(page);
  }

  // متدهای جدید برای مدیریت انتخاب‌ها
  toggleAllSelection(isSelected: boolean) {
    this.processedData$.pipe(take(1)).subscribe(data => {
      data.forEach(row => row.isSelected = isSelected);
      this._selectedItems$.next(isSelected ? [...data] : []);
    });
  }

  toggleRowSelection(row: any) {
    const currentSelected = [...this._selectedItems$.value];
    const index = currentSelected.findIndex(item => item === row);

    if (row.isSelected && index === -1) {
      currentSelected.push(row);
    } else if (!row.isSelected && index !== -1) {
      currentSelected.splice(index, 1);
    }
    this._selectedItems$.next(currentSelected);
  }

  private matchesFilter(rowValue: any, filter: GridFilterPayload): boolean {
    switch (filter.filterType) {
      case 'number':
        return this.matchesNumberFilter(rowValue, filter);
      case 'date':
        return this.matchesDateFilter(rowValue, filter);
      case 'select':
        return this.matchesSelectFilter(rowValue, filter.value);
      case 'text':
      default:
        return this.matchesTextFilter(rowValue, filter.value, filter.textOperator || 'contains');
    }
  }

  private matchesTextFilter(rowValue: any, value: string | number | undefined, textOperator: GridTextOperator): boolean {
    const term = String(value ?? '').trim().toLowerCase();
    if (!term) return true;

    const normalizedRow = String(rowValue ?? '').toLowerCase();
    
    switch (textOperator) {
      case 'contains':
        return normalizedRow.includes(term);
      case 'notContains':
        return !normalizedRow.includes(term);
      case 'startsWith':
        return normalizedRow.startsWith(term);
      case 'endsWith':
        return normalizedRow.endsWith(term);
      case 'equals':
        return normalizedRow === term;
      default:
        return normalizedRow.includes(term);
    }
  }

  private matchesSelectFilter(rowValue: any, value: string | number | undefined): boolean {
    const selected = String(value ?? '').trim().toLowerCase();
    if (!selected) return true;
    return String(rowValue ?? '').trim().toLowerCase() === selected;
  }

  private matchesNumberFilter(rowValue: any, filter: GridFilterPayload): boolean {
    const rowNumber = Number(rowValue);
    if (Number.isNaN(rowNumber)) return false;

    if (filter.mode === 'range') {
      const hasMin = filter.min != null;
      const hasMax = filter.max != null;
      if (!hasMin && !hasMax) return true;
      if (hasMin && rowNumber < (filter.min as number)) return false;
      if (hasMax && rowNumber > (filter.max as number)) return false;
      return true;
    }

    if (filter.value == null || filter.value === '') return true;
    const value = Number(filter.value);
    if (Number.isNaN(value)) return false;

    return this.compareNumbers(rowNumber, value, filter.operator || 'eq');
  }

  private compareNumbers(row: number, value: number, operator: GridNumberOperator): boolean {
    switch (operator) {
      case 'gt':
        return row > value;
      case 'gte':
        return row >= value;
      case 'lt':
        return row < value;
      case 'lte':
        return row <= value;
      case 'eq':
      default:
        return row === value;
    }
  }

  private matchesDateFilter(rowValue: any, filter: GridFilterPayload): boolean {
    const normalizedRowDate = this.normalizeDate(rowValue);
    if (!normalizedRowDate) return false;

    if (filter.mode === 'range') {
      const from = (filter.from || '').trim();
      const to = (filter.to || '').trim();
      if (!from && !to) return true;
      if (from && normalizedRowDate < from) return false;
      if (to && normalizedRowDate > to) return false;
      return true;
    }

    const exactDate = String(filter.value ?? '').trim();
    if (!exactDate) return true;
    return normalizedRowDate === exactDate;
  }

  private normalizeDate(value: any): string | null {
    if (!value) return null;

    if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value)) {
      return value;
    }

    const date = value instanceof Date ? value : new Date(value);
    if (Number.isNaN(date.getTime())) return null;

    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  private isFilterEmpty(filter: GridFilterPayload): boolean {
    switch (filter.filterType) {
      case 'number':
        if (filter.mode === 'range') {
          return filter.min == null && filter.max == null;
        }
        return filter.value == null || filter.value === '';
      case 'date':
        if (filter.mode === 'range') {
          return !(filter.from || '').trim() && !(filter.to || '').trim();
        }
        return !String(filter.value ?? '').trim();
      case 'select':
      case 'text':
      default:
        return !String(filter.value ?? '').trim();
    }
  }


}
