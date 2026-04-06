import {Component, EventEmitter, Input, Output} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { ColumnConfig } from './models/column-config';
import { GridDataService } from './services/grid-data';
import { GridHeaderComponent } from './components/grid-header/grid-header';
import { GridBodyComponent } from './components/grid-body/grid-body';
import { GridFooterComponent } from './components/grid-footer/grid-footer';
import {take} from 'rxjs';
import {GridExportService} from '@shared/grid/services/grid-export-service';

@Component({
  selector: 'app-grid',
  standalone: true,
  imports: [
    CommonModule,
    GridHeaderComponent,
    GridBodyComponent,
    GridFooterComponent,
    ScrollingModule
  ],
  templateUrl: './grid.html',
  styleUrls: ['./grid.css'],
  providers: [GridDataService]
})
export class GridComponent {
  @Input() columns: ColumnConfig[] = [];
  @Input() set data(value: any[]) {
    this.gridDataService.setData(value);
  }

  // اضافه کردن خروجی برای ارسال اکشن به کامپوننت والد (UserManagement)
  @Output() actionTriggered = new EventEmitter<{type: string, row: any}>();

  constructor(
    public gridDataService: GridDataService,
    private exportService: GridExportService // تزریق سرویس اکسپورت
  ) {}

  // متدی که در HTML صدا زده شده و خطا می‌داد:
  onGridAction(event: {type: string, row: any}) {
    // ارسال مستقیم رویداد به لایه بالاتر
    this.actionTriggered.emit(event);
  }
  export(type: 'excel' | 'pdf') {
    // دریافت آخرین نسخه داده‌های فیلتر شده (بدون صفحه‌بندی)
    this.gridDataService.processedData$.pipe(take(1)).subscribe(data => {
      // استخراج فیلدها و عناوین (به جز ستون عملیات)
      const exportColumns = this.columns.filter(c => c.type !== 'action');
      const titles = exportColumns.map(c => c.title);
      const fields = exportColumns.map(c => c.field);

      if (type === 'excel') {
        this.exportService.exportToExcel(data, 'گزارش_کاربران');
      } else if (type === 'pdf') {
        this.exportService.exportToPdf(data, titles, fields, 'گزارش_کاربران');
      }
    });
  }

  applyAiFilter($event: any) {
    
  }
}
