import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { ColumnConfig } from '../../models/column-config';
import { GridDataService } from '../../services/grid-data';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';

type GridEditMode = 'Inline' | 'Popup' | 'Batch' | 'None';

interface GridEditSettings {
  mode: GridEditMode;
  allowEditing: boolean;
}

@Component({
  selector: 'app-grid-body',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './grid-body.html',
  styleUrls: ['./grid-body.css']
})
export class GridBodyComponent {
  @Input() columns: ColumnConfig[] = [];
  @Input() fitViewportMode: boolean = false;

  get visibleColumns(): ColumnConfig[] {
    return this.columns.filter((col) => !col.hidden);
  }

  @Input() data$!: Observable<any[]>;
  @Input() editSettings: GridEditSettings = { mode: 'None', allowEditing: false };
  @Output() action = new EventEmitter<{ type: string, row: any }>();
  @Output() saveChanges = new EventEmitter<any>();
  @Output() inlineSave = new EventEmitter<any>();

  editingRowId = signal<string | null>(null);
  private inlineOriginalData: any | null = null;
  popupEditData: any | null = null;
  popupOriginalData: any | null = null;
  batchChanges = new Map<string, Record<string, any>>();

  constructor(private gridDataService: GridDataService) {}

  onAction(type: string, row: any, event?: Event) {
    if(event) event.stopPropagation(); // جلوگیری از تداخل کلیک دکمه با کلیک سطر

    if (this.editSettings.allowEditing && type === 'edit') {
      if (this.editSettings.mode === 'Inline') {
        this.startInlineEdit(row);
        return;
      }

      if (this.editSettings.mode === 'Popup') {
        this.openPopupEdit(row);
        return;
      }
    }

    this.action.emit({type, row});
  }

  onRowSelect(row: any) {
    this.gridDataService.toggleRowSelection(row);
  }

  // متد جدید برای کلیک روی کل سطر (باز کردن Drawer)
  onRowClick(row: any) {
    this.action.emit({ type: 'row-click', row });
  }

  getBadgeClass(col: ColumnConfig, row: any): string {
    const value = String(row[col.field] ?? '');

    if (col.badgeStyle === 'mechanics') {
      return value === 'Compound'
        ? 'badge-mechanics-compound'
        : 'badge-mechanics-isolation';
    }

    if (col.badgeStyle === 'aiTag') {
      return 'bg-slate-100 text-slate-700 ring-1 ring-inset ring-slate-200 rounded-full';
    }

    if (col.badgeStyle === 'financePlan') {
      return value === 'Gold'
        ? 'badge-finance-plan-gold'
        : 'badge-finance-plan-silver';
    }

    if (col.badgeStyle === 'financeStatus') {
      return value === 'Paid'
        ? 'badge-finance-status-paid'
        : 'badge-finance-status-pending';
    }

    if (col.badgeStyle === 'verified') {
      return 'badge-verified';
    }

    if (col.badgeStyle === 'userTier') {
      return value === 'Elite'
        ? 'badge-difficulty-advanced'
        : value === 'Pro'
          ? 'badge-difficulty-intermediate'
          : 'badge-difficulty-beginner';
    }

    if (col.badgeStyle === 'difficulty') {
      return value === 'Advanced'
        ? 'badge-difficulty-advanced'
        : value === 'Intermediate'
          ? 'badge-difficulty-intermediate'
          : 'badge-difficulty-beginner';
    }

    if (col.badgeStyle === 'userStatus') {
      return value === 'Active'
        ? 'badge-user-status-active'
        : value === 'Review'
          ? 'badge-user-status-review'
          : 'badge-user-status-banned';
    }

    return value === 'Active'
      ? 'bg-emerald-100 text-emerald-700'
      : value === 'Review'
        ? 'bg-amber-100 text-amber-700'
        : 'bg-rose-100 text-rose-700';
  }

  getActionType(col: ColumnConfig): string {
    return col.actionType ?? 'edit';
  }

  getActionTitle(col: ColumnConfig): string {
    return col.actionLabel ?? 'Edit';
  }

  getActionIcon(col: ColumnConfig): string {
    return col.actionIcon ?? 'more-horizontal';
  }

  getLockedOffset(field: string): string {
    let offset = 0;

    for (const column of this.columns) {
      if (column.field === field) {
        break;
      }

      if (column.locked) {
        offset += this.getSafeWidth(column.width);
      }
    }

    return `${offset}px`;
  }

  isLastLocked(field: string): boolean {
    const lockedColumns = this.columns.filter((column) => column.locked);
    if (lockedColumns.length === 0) {
      return false;
    }

    return lockedColumns[lockedColumns.length - 1].field === field;
  }

  isEditableColumn(col: ColumnConfig): boolean {
    return col.type === 'text' || col.type === 'email' || col.type === 'number';
  }

  isPopupPrimaryEditableColumn(col: ColumnConfig): boolean {
    return this.isEditableColumn(col) || col.type === 'profile';
  }

  getPopupSubfieldLabel(col: ColumnConfig): string {
    return `${col.title} DETAIL`;
  }

  isBatchMode(): boolean {
    return this.editSettings.allowEditing && this.editSettings.mode === 'Batch';
  }

  isPopupMode(): boolean {
    return this.editSettings.allowEditing && this.editSettings.mode === 'Popup';
  }

  isCellInEditMode(row: any, col: ColumnConfig): boolean {
    if (!this.isEditableColumn(col)) {
      return false;
    }

    if (this.isBatchMode()) {
      return true;
    }

    return this.isInlineEditingRow(row);
  }

  isInlineEditingRow(row: any): boolean {
    return this.editSettings.allowEditing && this.editSettings.mode === 'Inline' && this.editingRowId() === row?.id;
  }

  startInlineEdit(row: any): void {
    this.editingRowId.set(row?.id ?? null);
    this.inlineOriginalData = { ...row };
  }

  startEdit(id: string | null): void {
    this.editingRowId.set(id ?? null);
  }

  saveInlineEdit(row: any, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }

    this.inlineSave.emit({ ...row });
    this.saveChanges.emit({
      mode: 'Inline',
      row: { ...row },
      id: row?.id
    });
    this.editingRowId.set(null);
    this.inlineOriginalData = null;
  }

  saveEdit(row: any): void {
    this.inlineSave.emit({ ...row });
    this.editingRowId.set(null);
    this.inlineOriginalData = null;
  }

  cancelInlineEdit(row: any, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }

    if (this.inlineOriginalData) {
      Object.keys(this.inlineOriginalData).forEach((key) => {
        row[key] = this.inlineOriginalData[key];
      });
    }

    this.editingRowId.set(null);
    this.inlineOriginalData = null;
  }

  cancelEdit(): void {
    this.editingRowId.set(null);
    this.inlineOriginalData = null;
  }

  openPopupEdit(row: any): void {
    this.popupEditData = { ...row };
    this.popupOriginalData = { ...row };
  }

  cancelPopupEdit(): void {
    this.popupEditData = null;
    this.popupOriginalData = null;
  }

  savePopupEdit(): void {
    if (!this.popupEditData) {
      return;
    }

    this.saveChanges.emit({
      mode: 'Popup',
      row: { ...this.popupEditData },
      id: this.popupEditData.id
    });

    this.popupEditData = null;
    this.popupOriginalData = null;
  }

  onBatchValueChange(row: any, col: ColumnConfig, value: any): void {
    const rowId = row?.id;
    if (!rowId) {
      return;
    }

    row[col.field] = value;
    const rowChanges = this.batchChanges.get(rowId) ?? {};
    rowChanges[col.field] = value;
    this.batchChanges.set(rowId, rowChanges);
  }

  isBatchFieldDirty(row: any, field: string): boolean {
    const rowId = row?.id;
    if (!rowId) {
      return false;
    }

    const rowChanges = this.batchChanges.get(rowId);
    return !!rowChanges && Object.prototype.hasOwnProperty.call(rowChanges, field);
  }

  saveBatchEdit(): void {
    if (this.batchChanges.size === 0) {
      return;
    }

    const changes = Array.from(this.batchChanges.entries()).map(([id, values]) => ({ id, values: { ...values } }));
    this.saveChanges.emit({ mode: 'Batch', changes });
    this.batchChanges.clear();
  }

  cancelBatchEdit(): void {
    this.batchChanges.clear();
  }

  private getSafeWidth(width?: string): number {
    const parsed = Number.parseInt(String(width ?? '150').replace('px', ''), 10);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : 150;
  }
}
