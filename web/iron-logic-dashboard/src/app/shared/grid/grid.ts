import { ChangeDetectorRef, Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { ColumnConfig } from './models/column-config';
import { GridDataService } from './services/grid-data';
import { GridHeaderComponent } from './components/grid-header/grid-header';
import { GridBodyComponent } from './components/grid-body/grid-body';
import { GridFooterComponent } from './components/grid-footer/grid-footer';
import { take } from 'rxjs';
import { GridExportService } from '@shared/grid/services/grid-export-service';
import { LucideAngularModule } from 'lucide-angular';

type GridEditMode = 'Inline' | 'Popup' | 'Batch' | 'None';

interface GridEditSettings {
  mode: GridEditMode;
  allowEditing: boolean;
}

@Component({
  selector: 'app-grid',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, GridHeaderComponent, GridBodyComponent, GridFooterComponent, ScrollingModule],
  templateUrl: './grid.html',
  styleUrls: ['./grid.css'],
  providers: [GridDataService]
})
export class GridComponent implements OnInit {
  @Input() columns: ColumnConfig[] = [];
  @Input() set data(value: any[]) { this.gridDataService.setData(value); }
  @Input() isLoading: boolean = false;
  @Input() searchTerm: string = '';
  @Input() editSettings: GridEditSettings = { mode: 'None', allowEditing: false };

  // --- پرچم‌های کنترلی جدید ---
  @Input() showExport: boolean = true;   // نمایش دکمه‌های اکسل/PDF
  @Input() showFilters: boolean = false; // نمایش فیلترهای زیر هدر
  @Input() showSearch: boolean = true;   // نمایش نوار جستجوی بالا
  @Input() pagerPosition: 'top' | 'bottom' | 'both' = 'bottom';

  filterResetKey = 0;

  @Output() actionTriggered = new EventEmitter<{type: string, row: any}>();
  @Output() selectionChanged = new EventEmitter<any[]>();
  @Output() refresh = new EventEmitter<void>();
  @Output() search = new EventEmitter<string>();
  @Output() saveChanges = new EventEmitter<any>();
  @Output() inlineSave = new EventEmitter<any>();

  constructor(
    public gridDataService: GridDataService,
    private exportService: GridExportService,
    private cdr: ChangeDetectorRef
  ) {}

  get pagedData$() {
    return this.gridDataService.pagedData$;
  }

  get pagination$() {
    return this.gridDataService.pagination$;
  }

  ngOnInit() {
    this.gridDataService.selectedItems$.subscribe(items => this.selectionChanged.emit(items));
  }

  export(type: 'excel' | 'pdf') {
    this.gridDataService.processedData$.pipe(take(1)).subscribe(data => {
      const exportColumns = this.columns.filter(c => c.type !== 'action' && c.type !== 'selection');
      const titles = exportColumns.map(c => c.title);
      const fields = exportColumns.map(c => c.field);
      if (type === 'excel') this.exportService.exportToExcel(data, 'Grid_Export');
      else if (type === 'pdf') this.exportService.exportToPdf(data, titles, fields, 'Grid_Export');
    });
  }

  onPageSizeChange(size: number) {
    this.gridDataService.setPageSize(size);
    this.cdr.markForCheck();
  }

  onRefreshClick() {
    this.gridDataService.clearSorts();
    this.gridDataService.clearFilters();
    this.columns.forEach((column) => {
      column.sortOrder = null;
    });
    this.filterResetKey += 1;
    this.search.emit('');
    this.refresh.emit();
  }
}
