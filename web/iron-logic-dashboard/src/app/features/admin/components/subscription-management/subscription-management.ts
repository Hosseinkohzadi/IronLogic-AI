import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LucideAngularModule } from 'lucide-angular';
import {
  BillingRecord,
  SubscriptionPlan,
  SubscriptionService,
} from '@core/services/subscription.service';
import { GridComponent } from '@shared/grid/grid';
import { ColumnConfig } from '@shared/grid/models/column-config';
import { MetricCardComponent } from '@shared/ui';
import { NotificationService } from '@core/services/notification.service';

interface AdminStats {
  totalRevenue: number;
  activeSubs: number;
  failedPayments: number;
}

@Component({
  selector: 'app-subscription-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LucideAngularModule,
    GridComponent,
    MetricCardComponent,
  ],
  templateUrl: './subscription-management.html',
  styleUrl: './subscription-management.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SubscriptionManagementComponent implements OnInit {
  private readonly subscriptionService = inject(SubscriptionService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly notificationService = inject(NotificationService);

  readonly plans = this.subscriptionService.availablePlans;
  readonly isLoading = signal(false);
  readonly isModalOpen = signal(false);
  readonly editingPlanId = signal<string | null>(null);
  readonly editingRowId = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly billingRecords = signal<BillingRecord[]>([]);
  readonly notification = this.notificationService.message;

  readonly adminStats = computed<AdminStats>(() => {
    const rows = this.billingRecords();
    const totalRevenue = rows.reduce((sum, row) => sum + row.amount, 0);
    const activeSubs = rows.filter((row) => row.status === 'Paid').length;
    const failedPayments = rows.filter((row) => row.status === 'Failed').length;
    return { totalRevenue, activeSubs, failedPayments };
  });

  readonly visibleColumns: Array<
    'userEmail' | 'planName' | 'amount' | 'status' | 'transactionDate' | 'actions'
  > = ['userEmail', 'planName', 'amount', 'status', 'transactionDate', 'actions'];

  readonly billingColumns: ColumnConfig[] = [
    {
      field: 'userEmail',
      title: 'USER EMAIL',
      type: 'email',
      sortable: true,
      width: '280px',
      filterType: 'text',
      hidden: !this.visibleColumns.includes('userEmail'),
    },
    {
      field: 'planName',
      title: 'PLAN NAME',
      type: 'text',
      sortable: true,
      width: '160px',
      filterType: 'text',
      hidden: !this.visibleColumns.includes('planName'),
    },
    {
      field: 'amount',
      title: 'AMOUNT',
      type: 'currency',
      sortable: true,
      width: '130px',
      filterType: 'number',
      filterMode: 'compare',
      hidden: !this.visibleColumns.includes('amount'),
    },
    {
      field: 'status',
      title: 'STATUS',
      type: 'badge',
      badgeStyle: 'billingStatus',
      sortable: true,
      width: '130px',
      filterType: 'select',
      filterOptions: [
        { label: 'Paid', value: 'Paid' },
        { label: 'Pending', value: 'Pending' },
        { label: 'Failed', value: 'Failed' },
      ],
      hidden: !this.visibleColumns.includes('status'),
    },
    {
      field: 'transactionDate',
      title: 'TRANSACTION DATE',
      type: 'date',
      sortable: true,
      width: '170px',
      filterType: 'date',
      filterMode: 'exact',
      hidden: !this.visibleColumns.includes('transactionDate'),
    },
    {
      field: 'actions',
      title: 'ACTIONS',
      type: 'action',
      width: '90px',
      actionIcon: 'more-horizontal',
      actionType: 'actions',
      actionLabel: 'Manage billing record',
      hidden: !this.visibleColumns.includes('actions'),
    },
  ];

  readonly editForm = this.formBuilder.nonNullable.group({
    name: this.formBuilder.nonNullable.control('', [Validators.required]),
    amount: this.formBuilder.nonNullable.control(0, [Validators.required, Validators.min(0)]),
    currency: this.formBuilder.nonNullable.control('USD', [Validators.required]),
  });

  readonly isEditMode = computed(() => !!this.editingPlanId());
  readonly editingPlan = computed(() => this.plans().find((p) => p.id === this.editingPlanId()));

  ngOnInit(): void {
    this.reloadData();
  }

  reloadData(): void {
    this.loadPlans();
    this.loadBillingRecords();
  }

  openAddPlanModal(): void {
    this.editingPlanId.set(null);
    this.editingRowId.set(null);
    this.editForm.reset({ name: '', amount: 0, currency: 'USD' });
    this.isModalOpen.set(true);
    this.errorMessage.set(null);
  }

  onEditPlan(record: BillingRecord): void {
    const matchedPlan = this.resolvePlanForRecord(record);
    this.editingPlanId.set(matchedPlan?.id ?? null);
    this.editingRowId.set(record.id);
    this.editForm.patchValue({
      name: record.planName,
      amount: record.amount,
      currency: record.currency,
    });
    this.isModalOpen.set(true);
    this.errorMessage.set(null);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
    this.editingPlanId.set(null);
    this.editingRowId.set(null);
    this.editForm.reset({ name: '', amount: 0, currency: 'USD' });
    this.errorMessage.set(null);
  }

  savePlan(): void {
    this.errorMessage.set(null);

    if (this.editForm.invalid) {
      this.errorMessage.set('Please fill in all required fields.');
      return;
    }

    const editingPlanId = this.editingPlanId();
    if (!editingPlanId) {
      this.errorMessage.set('No plan found for this billing record.');
      return;
    }

    const formValue = this.editForm.getRawValue();
    this.isLoading.set(true);
    this.subscriptionService
      .updatePlan(editingPlanId, {
        name: formValue.name,
        price: formValue.amount,
        currency: formValue.currency,
      })
      .pipe(
        finalize(() => this.isLoading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: () => {
          const targetRowId = this.editingRowId();
          if (targetRowId) {
            this.billingRecords.update((rows) =>
              rows.map((row) =>
                row.id === targetRowId
                  ? {
                      ...row,
                      planName: formValue.name,
                      amount: formValue.amount,
                      currency: formValue.currency,
                    }
                  : row,
              ),
            );
          }

          this.closeModal();
          this.notificationService.success('Plan successfully updated');
        },
        error: () => {
          this.errorMessage.set('Failed to update plan.');
          this.notificationService.error('Error occurred while updating plan');
        },
      });
  }

  onDeletePlan(record: BillingRecord): void {
    const matchedPlan = this.resolvePlanForRecord(record);
    if (!matchedPlan) {
      this.errorMessage.set('No matching plan found for deletion.');
      this.notificationService.error('Error occurred while deleting plan');
      return;
    }

    const confirmed = window.confirm(
      `Confirm Action: Delete plan "${matchedPlan.name}" for ${record.userEmail}?`,
    );
    if (!confirmed) {
      return;
    }

    this.isLoading.set(true);
    this.subscriptionService
      .deletePlan(matchedPlan.id)
      .pipe(
        finalize(() => this.isLoading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: () => {
          this.billingRecords.update((rows) => rows.filter((row) => row.id !== record.id));
          this.notificationService.success('Plan successfully deleted');
        },
        error: () => {
          this.errorMessage.set('Failed to delete plan.');
          this.notificationService.error('Error occurred while deleting plan');
        },
      });
  }

  handleGridAction(event: { type: string; row: BillingRecord }): void {
    if (event.type !== 'actions') {
      return;
    }

    const shouldEdit = window.confirm('Choose action:\nOK = Edit Plan\nCancel = Delete Plan');

    if (shouldEdit) {
      this.onEditPlan(event.row);
      return;
    }

    this.onDeletePlan(event.row);
  }

  getRevenueValue(): string {
    return this.adminStats().totalRevenue.toLocaleString();
  }

  private resolvePlanForRecord(record: BillingRecord): SubscriptionPlan | undefined {
    if (record.planId) {
      const byId = this.plans().find((plan) => plan.id === record.planId);
      if (byId) {
        return byId;
      }
    }

    return this.plans().find(
      (plan) => plan.name.trim().toLowerCase() === record.planName.trim().toLowerCase(),
    );
  }

  private loadBillingRecords(): void {
    this.isLoading.set(true);
    this.subscriptionService
      .getBillingRecords()
      .pipe(
        finalize(() => this.isLoading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (records) => {
          this.billingRecords.set(records);
        },
        error: () => {
          this.errorMessage.set('Failed to load billing records.');
        },
      });
  }

  private loadPlans(): void {
    this.subscriptionService
      .getPlans()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        error: () => {
          this.errorMessage.set('Failed to load plans.');
        },
      });
  }
}
