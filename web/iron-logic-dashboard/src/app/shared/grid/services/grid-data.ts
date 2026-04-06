import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable()
export class GridDataService {
  private _data = new BehaviorSubject<any[]>([]);
  public data$ = this._data.asObservable();

  setData(data: any[]) {
    this._data.next(data);
  }
}
