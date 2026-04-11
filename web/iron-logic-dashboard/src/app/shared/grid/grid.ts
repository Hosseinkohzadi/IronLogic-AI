import {
  ChangeDetectorRef,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  ViewChild,
  input,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { DragDropModule, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
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
  imports: [
    CommonModule,
    FormsModule,
    LucideAngularModule,
    GridHeaderComponent,
    GridBodyComponent,
    GridFooterComponent,
    ScrollingModule,
    DragDropModule,
  ],
  templateUrl: './grid.html',
  styleUrls: ['./grid.css'],
  providers: [GridDataService],
})
export class GridComponent implements OnInit, OnChanges {
  @Input() columns: ColumnConfig[] = [];
  @Input() set data(value: any[]) {
    this.gridDataService.setData(value);
  }
  @Input() isLoading: boolean = false;
  @Input() set searchTerm(value: string) {
    this._searchTerm = String(value ?? '');
    this.gridDataService.setSearchTerm(this._searchTerm);
  }
  get searchTerm(): string {
    return this._searchTerm;
  }
  @Input() editSettings: GridEditSettings = { mode: 'None', allowEditing: false };

  // --- پرچم‌های کنترلی جدید ---
  @Input() showExport: boolean = true; // نمایش دکمه‌های اکسل/PDF
  @Input() showFilters: boolean = false; // نمایش فیلترهای زیر هدر
  @Input() showSearch: boolean = true; // نمایش نوار جستجوی بالا
  @Input() pagerPosition: 'top' | 'bottom' | 'both' = 'bottom';
  @Input() resizableRows: boolean = true;
  reorderable = input<boolean>(false);

  filterResetKey = 0;
  isColumnMenuOpen = false;
  isFitViewportMode = false;
  private _searchTerm = '';
  private readonly initialColumnWidths = new Map<string, string | undefined>();

  @ViewChild('columnMenuContainer') private columnMenuContainerRef?: ElementRef<HTMLElement>;
  @ViewChild('gridScrollContainer') private gridScrollContainerRef?: ElementRef<HTMLElement>;

  @Output() actionTriggered = new EventEmitter<{ type: string; row: any }>();
  @Output() selectionChanged = new EventEmitter<any[]>();
  @Output() refresh = new EventEmitter<void>();
  @Output() search = new EventEmitter<string>();
  @Output() saveChanges = new EventEmitter<any>();
  @Output() inlineSave = new EventEmitter<any>();

  constructor(
    public gridDataService: GridDataService,
    private exportService: GridExportService,
    private cdr: ChangeDetectorRef,
    private elementRef: ElementRef<HTMLElement>,
  ) {}

  get pagedData$() {
    return this.gridDataService.pagedData$;
  }

  get pagination$() {
    return this.gridDataService.pagination$;
  }

  get visibleColumns(): ColumnConfig[] {
    return this.columns.filter((column) => column.hidden !== true);
  }

  get toggleableColumns(): ColumnConfig[] {
    return this.columns.filter(
      (column) => column.field !== 'selection' && column.field !== 'actions',
    );
  }

  ngOnInit() {
    this.captureInitialColumnWidths();
    this.gridDataService.selectedItems$.subscribe((items) => this.selectionChanged.emit(items));
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['columns']) {
      this.captureInitialColumnWidths();
    }
  }

  export(type: 'excel' | 'pdf') {
    this.gridDataService.processedData$.pipe(take(1)).subscribe((data) => {
      const exportColumns = this.columns.filter(
        (c) => c.type !== 'action' && c.type !== 'selection',
      );
      const titles = exportColumns.map((c) => c.title);
      const fields = exportColumns.map((c) => c.field);
      if (type === 'excel') this.exportService.exportToExcel(data, 'Grid_Export');
      else if (type === 'pdf') this.exportService.exportToPdf(data, titles, fields, 'Grid_Export');
    });
  }

  onPageSizeChange(size: number) {
    this.gridDataService.setPageSize(size);
    this.cdr.markForCheck();
  }

  toggleColumn(col: ColumnConfig) {
    col.hidden = !col.hidden;
    this.columns = [...this.columns];
    this.cdr.markForCheck();
  }

  onSearchTermChange(term: string) {
    this._searchTerm = String(term ?? '');
    this.gridDataService.setSearchTerm(this._searchTerm);
    this.search.emit(this._searchTerm);
  }

  onRowResize(): void {
    const viewport = this.elementRef.nativeElement.querySelector('cdk-virtual-scroll-viewport') as {
      checkViewportSize?: () => void;
    } | null;

    if (viewport?.checkViewportSize) {
      viewport.checkViewportSize();
    }
  }

  drop(event: CdkDragDrop<ColumnConfig[]>) {
    const visible = this.visibleColumns;
    const actionIdx = visible.findIndex((c) => c.type === 'action');

    let toVisible = event.currentIndex;
    if (actionIdx !== -1 && toVisible >= actionIdx) {
      toVisible = actionIdx - 1;
    }

    if (event.previousIndex === toVisible) return;

    const fromField = visible[event.previousIndex].field;
    const toField = visible[toVisible].field;
    const fromIdx = this.columns.findIndex((c) => c.field === fromField);
    const toIdx = this.columns.findIndex((c) => c.field === toField);

    moveItemInArray(this.columns, fromIdx, toIdx);
    this.columns = [...this.columns];
    this.cdr.markForCheck();
  }

  shouldShowColumnField(col: ColumnConfig): boolean {
    if (!col.field) {
      return false;
    }

    const headerName = (col.title || '').trim().toLowerCase();
    const fieldName = col.field.trim().toLowerCase();

    if (!headerName) {
      return true;
    }

    return fieldName !== headerName;
  }

  isColumnFiltered(field: string): boolean {
    return !!this.gridDataService.getFilter(field);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const clickTarget = event.target as Node | null;
    const menuContainer = this.columnMenuContainerRef?.nativeElement;

    if (this.isColumnMenuOpen) {
      if (menuContainer && clickTarget && !menuContainer.contains(clickTarget)) {
        this.isColumnMenuOpen = false;
        this.cdr.markForCheck();
      }

      if (!menuContainer && clickTarget && !this.elementRef.nativeElement.contains(clickTarget)) {
        this.isColumnMenuOpen = false;
        this.cdr.markForCheck();
      }
    }

  }

  private captureInitialColumnWidths(): void {
    for (const column of this.columns) {
      if (!this.initialColumnWidths.has(column.field)) {
        this.initialColumnWidths.set(column.field, column.width);
      }
    }
  }

  private getPreferredViewportPercentage(column: ColumnConfig): number | null {
    if (column.type === 'selection' || column.field === 'selection') {
      return 5;
    }

    if (column.field === 'name' || column.type === 'profile') {
      return 25;
    }

    if (column.field === 'email' || column.type === 'email') {
      return 25;
    }

    if (column.field === 'status' || column.type === 'badge') {
      return 15;
    }

    if (column.type === 'action' || column.field === 'actions') {
      return 8;
    }

    return null;
  }

  private buildViewportPercentages(columns: ColumnConfig[]): Map<string, string> {
    const visibleColumns = columns.filter((column) => column.hidden !== true);
    const percentageMap = new Map<string, string>();

    if (visibleColumns.length === 0) {
      return percentageMap;
    }

    const prioritizedColumns: Array<{ field: string; percentage: number }> = [];
    const remainingColumns: ColumnConfig[] = [];

    for (const column of visibleColumns) {
      const preferredPercentage = this.getPreferredViewportPercentage(column);
      if (preferredPercentage === null) {
        remainingColumns.push(column);
        continue;
      }

      prioritizedColumns.push({ field: column.field, percentage: preferredPercentage });
    }

    const prioritizedTotal = prioritizedColumns.reduce((sum, column) => sum + column.percentage, 0);

    if (prioritizedTotal >= 100) {
      let usedPercentage = 0;
      prioritizedColumns.forEach((column, index) => {
        const isLastColumn = index === prioritizedColumns.length - 1;
        if (isLastColumn) {
          percentageMap.set(
            column.field,
            `${Math.max(0, Number((100 - usedPercentage).toFixed(2)))}%`,
          );
          return;
        }

        const scaled = Number(((column.percentage / prioritizedTotal) * 100).toFixed(2));
        usedPercentage += scaled;
        percentageMap.set(column.field, `${scaled}%`);
      });

      return percentageMap;
    }

    const remainingPercentage = Math.max(0, 100 - prioritizedTotal);
    const defaultPercentage =
      remainingColumns.length > 0
        ? Number((remainingPercentage / remainingColumns.length).toFixed(2))
        : 0;

    prioritizedColumns.forEach((column) => {
      percentageMap.set(column.field, `${column.percentage}%`);
    });

    let usedPercentage = prioritizedTotal;
    remainingColumns.forEach((column, index) => {
      const isLastColumn = index === remainingColumns.length - 1;
      if (isLastColumn) {
        percentageMap.set(
          column.field,
          `${Math.max(0, Number((100 - usedPercentage).toFixed(2)))}%`,
        );
        return;
      }

      usedPercentage += defaultPercentage;
      percentageMap.set(column.field, `${defaultPercentage}%`);
    });

    return percentageMap;
  }

  autoSizeColumnsContentBased(): void {
    this.isColumnMenuOpen = false;
    this.isFitViewportMode = true;
    this.cdr.detectChanges();

    setTimeout(() => {
      const widths = this.buildViewportPercentages(this.columns);

      this.columns = this.columns.map((column) => {
        if (column.hidden) {
          return column;
        }

        return {
          ...column,
          width: widths.get(column.field) ?? column.width,
        };
      });

      this.cdr.markForCheck();
    }, 0);
  }

  handleRefresh() {
    this.isFitViewportMode = false;
    this.columns = this.columns.map((column) => ({
      ...column,
      hidden: false,
      width: this.initialColumnWidths.has(column.field)
        ? this.initialColumnWidths.get(column.field)
        : column.width,
    }));
    this.gridDataService.clearAllStates();
    this.filterResetKey += 1;
    this._searchTerm = '';
    this.search.emit('');
    this.refresh.emit();
    this.cdr.markForCheck();
  }
}
