import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';
import { UserRow } from '@core/models/user.model';
import { LucideAngularModule } from 'lucide-angular';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, FormsModule],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css'
})
export class UserManagementComponent implements OnInit {
  private apiService = inject(IronLogicApiService);

  users = signal<UserRow[]>([]);
  searchTerm = signal('');
  selectedUserId = signal<string | null>(null);
  selectedUserIndices = signal<number[]>([]);
  isDrawerOpen = signal(false);

  // آمار کارت‌های بالا مطابق تصاویر
  stats = signal({
    active: { count: 2011, growth: '+4.4%' },
    suspended: { count: 38, growth: '-2.1%' },
    confirmed: { count: '97.2%', growth: '+0.6%' },
    resets: { count: 14, growth: '+1.1%' }
  });

  filteredUsers = computed(() => {
    const term = this.searchTerm().toLowerCase();
    return this.users().filter(u =>
      u.name.toLowerCase().includes(term) || u.email.toLowerCase().includes(term)
    );
  });

  activeUser = computed(() => this.users().find(u => u.id === this.selectedUserId()));

  ngOnInit() {
    this.apiService.getUsers().subscribe(data => {
      if (data) this.users.set(data);
    });
  }

  selectUser(user: UserRow) {
    this.selectedUserId.set(user.id);
    this.isDrawerOpen.set(true);
  }

  toggleSelection(event: Event, index: number) {
    event.stopPropagation();
    this.selectedUserIndices.update(indices =>
      indices.includes(index) ? indices.filter(i => i !== index) : [...indices, index]
    );
  }

  closeDrawer() {
    this.isDrawerOpen.set(false);
    this.selectedUserId.set(null);
  }
}
