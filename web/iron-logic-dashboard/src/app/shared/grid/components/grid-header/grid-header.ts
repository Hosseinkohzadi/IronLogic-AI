import {Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ColumnConfig} from '../../models/column-config';

@Component({
  selector: 'app-grid-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid-header-row">
      <div *ngFor="let col of columns"
           class="header-cell"
           [style.width]="col.width"
           (click)="onSort(col)">

        <span>{{ col.title }}</span>

        <span class="sort-icon" [ngClass]="{'active': col.sortOrder}">
          <svg *ngIf="!col.sortOrder" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M7 15l5 5 5-5M7 9l5-5 5 5"/></svg>

          <svg *ngIf="col.sortOrder === 'asc'" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M18 15l-6-6-6 6"/></svg>

          <svg *ngIf="col.sortOrder === 'desc'" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M6 9l6 6 6-6"/></svg>
        </span>

      </div>
    </div>
  `,
  styleUrl: './grid-header.css'
})
export class GridHeaderComponent {
  @Input() columns: ColumnConfig[] = [];
  @Output() sortChange = new EventEmitter<ColumnConfig>();

  onSort(column: ColumnConfig) {
    // تغییر چرخه: null -> asc -> desc -> null
    if (!column.sortOrder) column.sortOrder = 'asc';
    else if (column.sortOrder === 'asc') column.sortOrder = 'desc';
    else column.sortOrder = null;

    // ریست کردن سایر ستون‌ها (سورت تک‌ستونه)
    this.columns.forEach(c => {
      if (c.field !== column.field) c.sortOrder = null;
    });

    this.sortChange.emit(column);
  }
}
