import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { finalize } from 'rxjs/operators';
import { NotificationService } from '@core/services/notification.service';
import { SettingsService } from '@core/services/settings.service';
import { MarketingAudience, MarketingService } from '@core/services/marketing.service';

@Component({
  selector: 'app-platform-settings',
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './platform-settings.html',
  styleUrl: './platform-settings.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlatformSettingsComponent implements OnInit {
  private readonly settingsService = inject(SettingsService);
  private readonly marketingService = inject(MarketingService);
  private readonly notificationService = inject(NotificationService);

  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isBroadcasting = signal(false);
  readonly isAudienceCountLoading = signal(false);
  readonly isConfirmModalOpen = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly marketingError = signal<string | null>(null);
  readonly yearlyDiscountPercentage = signal(0);
  readonly targetAudience = signal<MarketingAudience>('AllUsers');
  readonly customMessage = signal('');
  readonly targetAudienceCount = signal(0);

  ngOnInit(): void {
    this.fetchSettings();
    this.fetchAudienceCount();
  }

  onDiscountInput(event: Event): void {
    const value = (event.target as HTMLInputElement | null)?.value ?? '';
    const parsed = Number(value);
    if (Number.isNaN(parsed)) {
      this.yearlyDiscountPercentage.set(0);
      return;
    }

    const clamped = Math.max(0, Math.min(100, parsed));
    this.yearlyDiscountPercentage.set(clamped);
  }

  saveConfiguration(): void {
    if (this.isSaving() || this.isLoading()) {
      return;
    }

    const newValue = this.yearlyDiscountPercentage();
    this.isSaving.set(true);

    this.settingsService
      .updateSetting('YearlyDiscountPercentage', newValue)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.notificationService.showSuccess('Discount percentage updated across the platform.');
        },
        error: () => {
          this.notificationService.showError('Failed to update discount percentage.');
        },
      });
  }

  onTargetAudienceChange(event: Event): void {
    const audience = (event.target as HTMLSelectElement | null)?.value ?? 'AllUsers';
    if (audience === 'BasicPlanUsersOnly' || audience === 'AllUsers') {
      this.targetAudience.set(audience);
      this.fetchAudienceCount();
      return;
    }

    this.targetAudience.set('AllUsers');
    this.fetchAudienceCount();
  }

  onCustomMessageInput(event: Event): void {
    const value = (event.target as HTMLTextAreaElement | null)?.value ?? '';
    this.customMessage.set(value);
  }

  openBroadcastConfirmation(): void {
    this.marketingError.set(null);
    this.isConfirmModalOpen.set(true);
  }

  closeBroadcastConfirmation(): void {
    if (this.isBroadcasting()) {
      return;
    }

    this.isConfirmModalOpen.set(false);
  }

  confirmBroadcast(): void {
    if (this.isBroadcasting()) {
      return;
    }

    const audience = this.targetAudience();
    const currentDiscount = this.yearlyDiscountPercentage();

    this.marketingError.set(null);
    this.isBroadcasting.set(true);

    this.marketingService
      .broadcastDiscount(audience, currentDiscount, this.customMessage())
      .pipe(finalize(() => this.isBroadcasting.set(false)))
      .subscribe({
        next: () => {
          this.isConfirmModalOpen.set(false);
          this.notificationService.showSuccess(
            'Campaign queued successfully. Emails are being sent in the background.',
          );
        },
        error: () => {
          this.marketingError.set('Unable to queue campaign. Please try again.');
          this.notificationService.showError('Failed to queue campaign broadcast.');
        },
      });
  }

  audienceLabel(audience: MarketingAudience): string {
    return audience === 'AllUsers' ? 'All Users' : 'Basic Plan Users Only';
  }

  private fetchSettings(): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.settingsService
      .getPlatformSettings()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (settings) => {
          this.yearlyDiscountPercentage.set(settings.YearlyDiscountPercentage ?? 0);
        },
        error: () => {
          this.loadError.set('Unable to load platform settings.');
          this.notificationService.showError('Failed to load platform settings.');
        },
      });
  }

  private fetchAudienceCount(): void {
    this.isAudienceCountLoading.set(true);

    this.marketingService
      .getAudienceCount(this.targetAudience())
      .pipe(finalize(() => this.isAudienceCountLoading.set(false)))
      .subscribe({
        next: (response) => {
          const count = Number(response.count ?? 0);
          this.targetAudienceCount.set(Number.isNaN(count) ? 0 : Math.max(0, count));
        },
        error: () => {
          this.targetAudienceCount.set(0);
        },
      });
  }
}
