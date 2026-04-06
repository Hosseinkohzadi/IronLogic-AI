import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-grid-footer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid-footer flex justify-between items-center px-4 py-3 bg-slate-50 border-t border-slate-200">
      <span class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">
        Total Records: {{ pagination.totalItems }}
      </span>

      <div class="flex items-center gap-4">
        <button (click)="pageChange.emit(pagination.currentPage - 1)"
                [disabled]="pagination.currentPage === 1"
                class="pager-btn">PREV</button>

        <span class="text-xs font-black text-slate-600">
          PAGE {{ pagination.currentPage }} OF {{ pagination.totalPages }}
        </span>

        <button (click)="pageChange.emit(pagination.currentPage + 1)"
                [disabled]="pagination.currentPage === pagination.totalPages"
                class="pager-btn">NEXT</button>
      </div>
    </div>
  `,
  styles: [`
    .pager-btn {
      padding: 4px 12px;
      border-radius: 4px;
      border: 1px solid #e2e8f0;
      background: white;
      font-size: 10px;
      font-weight: 900;
      color: #64748b;
      cursor: pointer;
      transition: all 0.2s;
    }
    .pager-btn:hover:not(:disabled) {
      background-color: #f1f5f9;
      color: #6366f1;
    }
    .pager-btn:disabled {
      opacity: 0.3;
      cursor: not-allowed;
    }
  `]
})
export class GridFooterComponent {
  // این همان بخشی است که خطای NG8002 را رفع می‌کند
  @Input() pagination: any = {
    currentPage: 1,
    pageSize: 10,
    totalItems: 0,
    totalPages: 1
  };

  // مشخص کردن نوع number خطای TS2345 را رفع می‌کند
  @Output() pageChange = new EventEmitter<number>();
}
