import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common'; // ضروری برای استفاده در تمپلیت اصلی
import { ColumnConfig } from './models/column-config';
import { GridDataService } from './services/grid-data';
// وارد کردن کامپوننت‌های داخلی
import { GridHeaderComponent } from './components/grid-header/grid-header';
import { GridBodyComponent } from './components/grid-body/grid-body';
import { GridFooterComponent } from './components/grid-footer/grid-footer';

@Component({
  selector: 'app-grid',
  standalone: true, // اطمینان حاصل کنید این مقدار true است
  imports: [
    CommonModule,
    GridHeaderComponent,
    GridBodyComponent,
    GridFooterComponent
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

  constructor(public gridDataService: GridDataService) {}
}
