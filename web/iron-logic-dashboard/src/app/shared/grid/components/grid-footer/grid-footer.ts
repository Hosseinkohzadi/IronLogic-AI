import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-grid-footer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex flex-col md:flex-row justify-between items-center py-6 border-t border-slate-200 mt-4">
      <span class="text-[13px] font-medium text-slate-500">
        Showing page {{ pagination.currentPage }} of {{ pagination.totalPages }} &middot; {{ pagination.totalItems | number }} total records
      </span>

      <div class="flex items-center gap-1.5 mt-4 md:mt-0">
        <button (click)="pageChange.emit(pagination.currentPage - 1)"
                [disabled]="pagination.currentPage === 1"
                class="w-8 h-8 flex items-center justify-center rounded-lg bg-slate-100 text-slate-500 hover:bg-slate-200 disabled:opacity-40 transition-colors">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/></svg>
        </button>

        <button class="w-8 h-8 rounded-lg bg-black text-white text-xs font-bold shadow-md">{{ pagination.currentPage }}</button>

        <button *ngIf="pagination.currentPage < pagination.totalPages"
                (click)="pageChange.emit(pagination.currentPage + 1)"
                class="w-8 h-8 rounded-lg bg-slate-100 text-slate-600 text-xs font-bold hover:bg-slate-200 transition-colors">
          {{ pagination.currentPage + 1 }}
        </button>

        <button (click)="pageChange.emit(pagination.currentPage + 1)"
                [disabled]="pagination.currentPage === pagination.totalPages"
                class="w-8 h-8 flex items-center justify-center rounded-lg bg-slate-100 text-slate-500 hover:bg-slate-200 disabled:opacity-40 transition-colors">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/></svg>
        </button>
      </div>
    </div>
  `
})
export class GridFooterComponent {
  @Input() pagination: any = { currentPage: 1, pageSize: 10, totalItems: 0, totalPages: 1 };
  @Output() pageChange = new EventEmitter<number>();
}
