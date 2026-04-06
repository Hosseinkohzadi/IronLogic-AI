import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { ColumnConfig } from '../../models/column-config';
import { ScrollingModule } from '@angular/cdk/scrolling'; // وارد کردن ماژول اسکرول

@Component({
  selector: 'app-grid-body',
  standalone: true,
  imports: [CommonModule,ScrollingModule],
  template: `
    <cdk-virtual-scroll-viewport itemSize="48" class="viewport">
      <div *cdkVirtualFor="let row of data$ | async" class="grid-row">
        <div *ngFor="let col of columns" class="grid-cell" [style.width]="col.width">
          {{ row[col.field] }}
        </div>
      </div>
    </cdk-virtual-scroll-viewport>
  `,
  styleUrls: ['./grid-body.css']
})
export class GridBodyComponent {
  @Input() columns: ColumnConfig[] = [];
  @Input() data$!: Observable<any[]>;
}
