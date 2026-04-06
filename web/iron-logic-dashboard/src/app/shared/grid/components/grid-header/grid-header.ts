import {Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ColumnConfig} from '../../models/column-config';
import {Subject} from 'rxjs';
import {debounceTime, distinctUntilChanged} from 'rxjs/operators';

@Component({
  selector: 'app-grid-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './grid-header.html',
  styleUrls: ['./grid-header.css']
})
export class GridHeaderComponent {
  @Input() columns: ColumnConfig[] = [];
  @Output() sortChange = new EventEmitter<ColumnConfig>();
  @Output() filterChange = new EventEmitter<{ field: string, value: string }>();

  private filterSubject = new Subject<{ field: string, value: string }>();

  constructor() {
    this.filterSubject.pipe(
      debounceTime(300),
      distinctUntilChanged((prev, curr) => prev.value === curr.value)
    ).subscribe(filter => {
      this.filterChange.emit(filter);
    });
  }

  onSort(column: ColumnConfig) {
    // فقط ستون‌های فاقد داده (مثل دکمه‌ها و عکس) سورت نمی‌شوند
    if (column.type === 'action' || column.field === 'avatar') return;

    // چرخه سورت: asc -> desc -> null
    if (!column.sortOrder) {
      column.sortOrder = 'asc';
    } else if (column.sortOrder === 'asc') {
      column.sortOrder = 'desc';
    } else {
      column.sortOrder = null;
    }

    // ریست کردن بقیه ستون‌ها
    this.columns.forEach(c => {
      if (c.field !== column.field) c.sortOrder = null;
    });

    this.sortChange.emit(column);
  }

  onTextInputFilter(event: any, field: string) {
    this.filterSubject.next({field, value: event.target.value});
  }

  onSelectFilter(event: any, field: string) {
    const value = event.target.value;
    this.filterChange.emit({ field, value });
  }
}
