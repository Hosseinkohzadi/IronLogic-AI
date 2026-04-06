import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // برای استفاده از ngModel

@Component({
  selector: 'app-edit-user-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: `./edit-user-modal.html`,
  styleUrl: './edit-user-modal.css'
})
export class EditUserModalComponent implements OnChanges {
  @Input() isOpen = false;
  @Input() user: any = null; // اطلاعات کاربری که قرار است ویرایش شود

  @Output() save = new EventEmitter<any>();
  @Output() cancel = new EventEmitter<void>();

  // یک کپی از اطلاعات کاربر می‌سازیم تا تغییرات مستقیماً قبل از ذخیره روی گرید اعمال نشود
  editData: any = {};

  ngOnChanges(changes: SimpleChanges) {
    if (changes['user'] && this.user) {
      // استفاده از Spread Operator برای کپی کردن آبجکت
      this.editData = { ...this.user };
    }
  }

  onSave() {
    this.save.emit(this.editData);
  }

  onCancel() {
    this.cancel.emit();
  }
}
