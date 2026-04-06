import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { ColumnConfig } from '../../models/column-config';

@Component({
  selector: 'app-grid-body',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngFor="let row of data$ | async" class="grid-row">
      <div *ngFor="let col of columns" class="grid-cell" [style.width]="col.width">
        {{ row[col.field] }}
      </div>
    </div>
  `,
  styleUrls: ['./grid-body.css']
})
export class GridBodyComponent {
  @Input() columns: ColumnConfig[] = [];
  @Input() data$!: Observable<any[]>;
}
