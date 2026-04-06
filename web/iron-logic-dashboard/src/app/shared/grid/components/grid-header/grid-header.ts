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
  @Output() toggleAll = new EventEmitter<boolean>(); // رویداد جدید

  private filterSubject = new Subject<{ field: string, value: string }>();

  constructor() {
    this.filterSubject.pipe(
      debounceTime(300),
      distinctUntilChanged((prev, curr) => prev.value === curr.value)
    ).subscribe(filter => {
      this.filterChange.emit(filter);
    });
  }

  onToggleAll(event: any) {
    this.toggleAll.emit(event.target.checked);
  }

  onSort(column: ColumnConfig) {
    if (column.type === 'action' || column.type === 'selection' || column.field === 'avatar') return;

    if (!column.sortOrder) {
      column.sortOrder = 'asc';
    } else if (column.sortOrder === 'asc') {
      column.sortOrder = 'desc';
    } else {
      column.sortOrder = null;
    }

    this.columns.forEach(c => {
      if (c.field !== column.field) c.sortOrder = null;
    });

    this.sortChange.emit(column);
  }

  onTextInputFilter(event: any, field: string) {
    this.filterSubject.next({field, value: event.target.value});
  }

  onSelectFilter(event: any, field: string) {
    this.filterChange.emit({ field, value: event.target.value });
  }
}
