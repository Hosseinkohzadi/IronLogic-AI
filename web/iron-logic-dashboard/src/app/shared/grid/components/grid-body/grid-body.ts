import {Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Observable} from 'rxjs';
import {ColumnConfig} from '../../models/column-config';

@Component({
  selector: 'app-grid-body',
  standalone: true,
  imports: [CommonModule], // ScrollingModule حذف شد چون از Paging استفاده می‌کنیم
  templateUrl: './grid-body.html',
  styleUrls: ['./grid-body.css']
})
export class GridBodyComponent {
  @Input() columns: ColumnConfig[] = [];
  @Input() data$!: Observable<any[]>;

  @Output() action = new EventEmitter<{ type: 'edit' | 'delete', row: any }>();

  onAction(type: 'edit' | 'delete', row: any) {
    this.action.emit({type, row});
  }
}
