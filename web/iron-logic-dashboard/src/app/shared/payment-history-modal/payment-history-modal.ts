import {
  ChangeDetectionStrategy,
  Component,
  Input,
  Output,
  EventEmitter,
  signal,
  effect,
  computed,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';
import { Router } from '@angular/router';

export interface Transaction {
  id: string;
  date: Date;
  amount: number;
  planType: string;
  status: 'Paid' | 'Pending' | 'Failed';
  invoiceNumber: string;
}

@Component({
  selector: 'app-payment-history-modal',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './payment-history-modal.html',
  styleUrl: './payment-history-modal.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentHistoryModalComponent {
  private apiService = inject(IronLogicApiService);
  private router = inject(Router);

  @Input() isOpen = false;
  @Input() userName = '';
  @Input() userId: string | null = null;

  @Output() close = new EventEmitter<void>();
  @Output() viewFullDetails = new EventEmitter<void>();

  transactions = signal<Transaction[]>([]);
  isLoading = signal(false);
  error = signal<string | null>(null);

  hasTransactions = computed(() => this.transactions().length > 0);

  constructor() {
    effect(() => {
      if (this.isOpen && this.userId) {
        this.loadTransactions();
      }
    });
  }

  private loadTransactions(): void {
    if (!this.userId) return;

    this.isLoading.set(true);
    this.error.set(null);

    this.apiService.getUserTransactions(this.userId).subscribe({
      next: (data: Transaction[]) => {
        this.transactions.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading transactions:', err);
        this.error.set('Failed to load transactions');
        this.isLoading.set(false);
      },
    });
  }

  onClose(): void {
    this.close.emit();
  }

  onViewFullDetails(): void {
    if (this.userId && this.userName) {
      this.close.emit();
      this.router.navigate(['/admin/financial'], {
        queryParams: {
          search: this.userName,
        },
      });
    }
  }

  getStatusClasses(status: 'Paid' | 'Pending' | 'Failed'): string {
    switch (status) {
      case 'Paid':
        return 'bg-emerald-50 text-emerald-700';
      case 'Pending':
        return 'bg-amber-50 text-amber-700';
      case 'Failed':
        return 'bg-rose-50 text-rose-700';
      default:
        return 'bg-slate-50 text-slate-700';
    }
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 2,
    }).format(amount);
  }
}
