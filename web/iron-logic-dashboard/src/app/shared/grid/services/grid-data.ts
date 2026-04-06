import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';

@Injectable()
export class GridDataService {
  // ۱. مخازن داده‌های خام و وضعیت‌ها (Private)
  private _rawData$ = new BehaviorSubject<any[]>([]);
  private _sortConfig$ = new BehaviorSubject<{ field: string; order: 'asc' | 'desc' | null }>({ field: '', order: null });
  private _filters$ = new BehaviorSubject<{ [key: string]: string }>({});
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
        return Object.keys(filters).every(field => {
          const filterValue = filters[field];
          if (!filterValue) return true;
          const rowValue = row[field];

          if (rowValue instanceof Date) {
            const yyyy = rowValue.getFullYear();
            const mm = String(rowValue.getMonth() + 1).padStart(2, '0');
            const dd = String(rowValue.getDate()).padStart(2, '0');
            return `${yyyy}-${mm}-${dd}` === filterValue;
          }

          const term = String(filterValue).toLowerCase();
          return String(rowValue || '').toLowerCase().includes(term);
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
    this._selectedItems$.next([]); // ریست کردن انتخاب‌ها هنگام تغییر فیلتر
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


}
