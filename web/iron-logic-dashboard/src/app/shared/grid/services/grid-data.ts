import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable()
export class GridDataService {
  // ۱. مخازن داده‌های خام و وضعیت‌ها
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
          const term = filters[field].toLowerCase();
          return String(row[field] || '').toLowerCase().includes(term);
        });
      });

      // ب) مرتب‌سازی
      if (sort.order) {
        result = [...result].sort((a, b) => {
          const valA = a[sort.field];
          const valB = b[sort.field];
          if (valA < valB) return sort.order === 'asc' ? -1 : 1;
          if (valA > valB) return sort.order === 'asc' ? 1 : -1;
          return 0;
        });
      }
      return result;
    })
  );

  // ۳. خروجی نهایی برای نمایش در صفحه (Paging)
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

  // ۴. اطلاعات صفحه‌بندی برای فوتر
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

  // --- متدهای عمومی برای تغییر وضعیت ---

  setData(data: any[]) {
    this._rawData$.next(data);
    this._currentPage$.next(1); // بازگشت به صفحه اول با دیتای جدید
  }

  applySort(field: string, order: 'asc' | 'desc' | null) {
    this._sortConfig$.next({ field, order });
  }

  updateFilter(field: string, value: string) {
    const current = this._filters$.value;
    this._filters$.next({ ...current, [field]: value });
    this._currentPage$.next(1); // با فیلتر جدید به صفحه اول برگرد
  }

  goToPage(page: number) {
    this._currentPage$.next(page);
  }

  // این متد را اضافه یا اصلاح کنید
  applyChanges(field: string, order: 'asc' | 'desc' | null) {
    this._sortConfig$.next({ field, order });
  }
}
