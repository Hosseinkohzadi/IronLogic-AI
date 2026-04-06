import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridComponent } from '@shared/grid/grid';
import { ColumnConfig } from '@shared/grid/models/column-config';
import { ConfirmModalComponent } from '@shared/confirm-modal/confirm-modal';
import { EditUserModalComponent } from '@shared/edit-user-modal/edit-user-modal';

@Component({
  selector: 'app-user-management',
  standalone: true,
  // ایمپورت‌های تکراری اصلاح شدند
  imports: [CommonModule, GridComponent, ConfirmModalComponent, EditUserModalComponent],
  templateUrl: './user-management.html',
  styleUrl:'./user-management.css'
})
export class UserManagement {
  // ۱. تنظیمات ستون‌های گرید
  userColumns: ColumnConfig[] = [
    { field: 'avatar', title: 'آواتار', width: '6%', type: 'image' }, // نوع تصویر
    { field: 'name', title: 'نام کاربر', width: '18%' },
    { field: 'country', title: 'کشور', width: '10%', type: 'flag' },
    { field: 'role', title: 'نقش', width: '12%' },
    { field: 'rating', title: 'عملکرد', width: '12%', type: 'rate' }, // نوع ستاره
    { field: 'joinDate', title: 'تاریخ عضویت', width: '14%', type: 'calendar' }, // نوع تاریخ
    { field: 'status', title: 'وضعیت', width: '10%', type: 'badge' },
    { field: 'actions', title: 'عملیات', width: '8%', type: 'action' }
  ];

  // ۲. داده‌های فیک
  users = signal(
    Array.from({ length: 1000 }).map((_, i) => {
      const countries = ['ir', 'ca', 'us', 'gb', 'fr', 'de', 'br', 'jp'];
      const randomCountry = countries[i % countries.length];

      // تولید یک تاریخ تصادفی در گذشته (بین امروز تا ۲ سال پیش)
      const randomDate = new Date(Date.now() - Math.floor(Math.random() * 60000000000));

      return {
        id: i + 1,
        // استفاده از سرویس ui-avatars برای تولید عکس‌های پروفایل تصادفی اما واقعی
        avatar: `https://ui-avatars.com/api/?name=User+${i + 1}&background=random&color=fff`,
        name: `User ${i + 1}`,
        country: randomCountry,
        role: i % 3 === 0 ? 'Admin' : 'Athlete',
        // یک عدد تصادفی بین ۱ تا ۵ برای امتیاز
        rating: Math.floor(Math.random() * 5) + 1,
        joinDate: randomDate, // تاریخ اضافه شد
        status: i % 2 === 0 ? 'Online' : 'Offline',
        lastActive: `${i % 24} hours ago`,
      };
    })
  );

  // ۳. وضعیت مدال‌ها (حذف و ویرایش)
  isDeleteModalOpen = signal(false);
  selectedUserForDelete = signal<any>(null);
  deleteMessage = signal('');

  isEditModalOpen = signal(false);
  selectedUserForEdit = signal<any>(null);

  // ۴. کنترل‌کننده مرکزی تمام اکشن‌های گرید (ادغام شده)
  handleUserAction(event: { type: string, row: any }) {
    if (event.type === 'delete') {
      this.selectedUserForDelete.set(event.row);
      this.deleteMessage.set(`آیا از حذف کاربر "${event.row.name}" اطمینان دارید؟ این عملیات غیرقابل بازگشت است.`);
      this.isDeleteModalOpen.set(true);
    }
    else if (event.type === 'edit') {
      this.selectedUserForEdit.set(event.row);
      this.isEditModalOpen.set(true);
    }
  }

  // =====================================
  // بخش منطق حذف (Delete Logic)
  // =====================================
  executeDelete() {
    const user = this.selectedUserForDelete();
    if (user) {
      this.users.update(currentUsers => currentUsers.filter(u => u.id !== user.id));
    }
    this.closeDeleteModal();
  }

  cancelDelete() {
    this.closeDeleteModal();
  }

  private closeDeleteModal() {
    this.isDeleteModalOpen.set(false);
    this.selectedUserForDelete.set(null);
  }

  // =====================================
  // بخش منطق ویرایش (Edit Logic)
  // =====================================
  executeEdit(updatedData: any) {
    this.users.update(currentUsers => {
      return currentUsers.map(u => u.id === updatedData.id ? updatedData : u);
    });
    this.closeEditModal();
  }

  cancelEdit() {
    this.closeEditModal();
  }

  private closeEditModal() {
    this.isEditModalOpen.set(false);
    this.selectedUserForEdit.set(null);
  }
}
