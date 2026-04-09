import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { ColumnConfig } from '../../models/column-config';
import { GridDataService } from '../../services/grid-data';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-grid-body',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './grid-body.html',
  styleUrls: ['./grid-body.css']
})
export class GridBodyComponent {
  @Input() columns: ColumnConfig[] = [];
  @Input() data$!: Observable<any[]>;
  @Output() action = new EventEmitter<{ type: 'edit' | 'delete' | 'row-click', row: any }>();

  constructor(private gridDataService: GridDataService) {}

  onAction(type: 'edit' | 'delete', row: any, event?: Event) {
    if(event) event.stopPropagation(); // جلوگیری از تداخل کلیک دکمه با کلیک سطر
    this.action.emit({type, row});
  }

  onRowSelect(row: any) {
    this.gridDataService.toggleRowSelection(row);
  }

  // متد جدید برای کلیک روی کل سطر (باز کردن Drawer)
  onRowClick(row: any) {
    this.action.emit({ type: 'row-click', row });
  }
}
