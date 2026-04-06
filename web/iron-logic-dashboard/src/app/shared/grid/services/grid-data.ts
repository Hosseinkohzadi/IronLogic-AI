import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable()
export class GridDataService {
  // ۱. مخازن داده‌های خام و وضعیت‌ها (Private)
  private _rawData$ = new BehaviorSubject<any[]>([]);
  private _sortConfig$ = new BehaviorSubject<{ field: string; order: 'asc' | 'desc' | null }>({ field: '', order: null });
  private _filters$ = new BehaviorSubject<{ [key: string]: string }>({});
  private _currentPage$ = new BehaviorSubject<number>(1);
  private _pageSize$ = new BehaviorSubject<number>(10);

  // ۲. پردازش داده‌ها (Filtering -> Sorting)
  private _processedData$: Observable<any[]> = combineLatest([
    this._rawData$,
    this._sortConfig$,
    this._filters$
  ]).pipe(
    map(([data, sort, filters]) => {
      // الف) فیلتر کردن
      let result = data.filter(row => {
        return Object.keys(filters).every(field => {
          const filterValue = filters[field];
          if (!filterValue) return true;

          const rowValue = row[field];

          // فیلتر تاریخ
          if (rowValue instanceof Date) {
            const yyyy = rowValue.getFullYear();
            const mm = String(rowValue.getMonth() + 1).padStart(2, '0');
            const dd = String(rowValue.getDate()).padStart(2, '0');
            const formattedDate = `${yyyy}-${mm}-${dd}`;
            return formattedDate === filterValue;
          }

          const term = String(filterValue).toLowerCase();
          return String(rowValue || '').toLowerCase().includes(term);
        });
      });

      // ب) مرتب‌سازی
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

  // ۳. خروجی‌های عمومی (Public) - بعد از تعریف Observable اصلی تعریف شده‌اند تا ارور رفع شود
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
    this._rawData$.next(data);
    this._currentPage$.next(1);
  }

  applySort(field: string, order: 'asc' | 'desc' | null) {
    this._sortConfig$.next({ field, order });
  }

  updateFilter(field: string, value: string) {
    const current = this._filters$.value;
    this._filters$.next({ ...current, [field]: value });
    this._currentPage$.next(1);
  }

  goToPage(page: number) {
    this._currentPage$.next(page);
  }
}
