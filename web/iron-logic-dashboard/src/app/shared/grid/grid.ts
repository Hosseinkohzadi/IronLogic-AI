import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { ColumnConfig } from './models/column-config';
import { GridDataService } from './services/grid-data';
import { GridHeaderComponent } from './components/grid-header/grid-header';
import { GridBodyComponent } from './components/grid-body/grid-body';
import { GridFooterComponent } from './components/grid-footer/grid-footer';

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

  constructor(public gridDataService: GridDataService) {}
}
