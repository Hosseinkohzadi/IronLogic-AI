import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable()
export class GridDataService {
  private _rawData: any[] = []; // نگه داشتن کپی داده‌های اصلی
  private _data = new BehaviorSubject<any[]>([]);
  public data$ = this._data.asObservable();

  setData(data: any[]) {
    this._rawData = [...data];
    this._data.next(data);
  }

  sort(field: string, order: 'asc' | 'desc' | null) {
    if (!order) {
      this._data.next([...this._rawData]); // بازگشت به حالت اولیه
      return;
    }

    const sortedData = [...this._data.value].sort((a, b) => {
      const valA = a[field];
      const valB = b[field];

      if (valA < valB) return order === 'asc' ? -1 : 1;
      if (valA > valB) return order === 'asc' ? 1 : -1;
      return 0;
    });

    this._data.next(sortedData);
  }
}
