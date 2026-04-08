import { Component, EventEmitter, Input, Output, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ColumnConfig } from '../../models/column-config';
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
export class GridHeaderComponent {
  @Input() columns: ColumnConfig[] = [];
  @Output() sortChange = new EventEmitter<ColumnConfig>();
  @Output() filterChange = new EventEmitter<{ field: string, value: string }>();
  @Output() toggleAll = new EventEmitter<boolean>();

  private filterSubject = new Subject<{ field: string, value: string }>();

  // وضعیت‌های Resizing
  private resizingColumn?: ColumnConfig;
  private startX = 0;
  private startWidth = 0;

  constructor() {
    this.filterSubject.pipe(
      debounceTime(300),
      distinctUntilChanged((prev, curr) => prev.value === curr.value)
    ).subscribe(filter => {
      this.filterChange.emit(filter);
    });
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
    this.startWidth = parseInt(column.width || '150', 10);

    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';
  }

  @HostListener('document:mousemove', ['$event'])
  onMouseMove(event: MouseEvent) {
    if (!this.resizingColumn) return;
    // محاسبه RTL: کشیدن به سمت چپ باعث افزایش عرض می‌شود
    const delta = this.startX - event.pageX;
    const newWidth = Math.max(60, this.startWidth + delta); // حداقل عرض ۶۰ پیکسل
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

  onTextInputFilter(event: any, field: string) {
    this.filterSubject.next({ field, value: event.target.value });
  }

  // متد مربوط به فیلترهای Select و Date
  onSelectFilter(event: any, field: string) {
    this.filterChange.emit({ field, value: event.target.value });
  }
}
