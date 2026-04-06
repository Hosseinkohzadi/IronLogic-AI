import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common'; // برای استفاده از ngFor
import { ColumnConfig } from '../../models/column-config';

@Component({
  selector: 'app-grid-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid-header-row">
      <div *ngFor="let col of columns" class="header-cell" [style.width]="col.width">
        {{ col.title }}
      </div>
    </div>
  `,
  styleUrls: ['./grid-header.css']
})
export class GridHeaderComponent {
  @Input() columns: ColumnConfig[] = [];
}
