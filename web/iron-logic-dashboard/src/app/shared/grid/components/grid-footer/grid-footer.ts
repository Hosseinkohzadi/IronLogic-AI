import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-grid-footer',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  template: `
    <div class="flex flex-col md:flex-row justify-between items-center py-4 px-2">

      <div class="flex items-center gap-4">
        <div class="flex items-center gap-2">
          <div class="relative">
            <select
              [value]="pagination.pageSize"
              (change)="onPageSizeChange($event)"
              class="appearance-none h-8 pl-3 pr-8 text-[12px] font-medium border border-slate-200 rounded-lg bg-white hover:bg-slate-50 cursor-pointer outline-none transition-colors focus:border-slate-400">
              <option [value]="5">5</option>
              <option [value]="10">10</option>
              <option [value]="20">20</option>
              <option [value]="50">50</option>
              <option [value]="100">100</option>
            </select>
            <lucide-icon name="chevron-down" class="absolute right-2 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400 pointer-events-none"></lucide-icon>
          </div>
          <span class="text-[12px] font-medium text-slate-500">items per page</span>
        </div>
        <span class="hidden md:inline text-[12px] font-medium text-slate-400">
          Showing page {{ pagination.currentPage }} of {{ pagination.totalPages }} &middot; {{ pagination.totalItems | number }} total
        </span>
      </div>

      <div class="flex items-center gap-1.5 mt-3 md:mt-0">
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
  @Output() pageSizeChange = new EventEmitter<number>();

  onPageSizeChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.pageSizeChange.emit(Number(value));
  }
}
