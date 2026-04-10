import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';

type SettingsTab = 'general' | 'billing' | 'ai';
type Currency = 'USD' | 'IRT';

@Component({
  selector: 'app-settings-page',
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './settings-page.html',
  styleUrl: './settings-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsPageComponent {
  readonly activeTab = signal<SettingsTab>('general');

  readonly coachName = signal('Coach Hossein Karimi');
  readonly coachBio = signal('Strength coach focused on progressive overload, fatigue management, and long-term athlete development.');

  readonly monthlyFee = signal(220);
  readonly currency = signal<Currency>('USD');
  readonly autoRemindAthletes = signal(true);

  readonly aiAggressiveness = signal(62);

  setTab(tab: SettingsTab): void {
    this.activeTab.set(tab);
  }

  updateCoachName(value: string): void {
    this.coachName.set(String(value ?? ''));
  }

  updateCoachBio(value: string): void {
    this.coachBio.set(String(value ?? ''));
  }

  updateMonthlyFee(value: number | string): void {
    const normalized = Number(value);
    this.monthlyFee.set(Number.isFinite(normalized) ? normalized : 0);
  }

  updateCurrency(value: Currency): void {
    this.currency.set(value);
  }

  updateAutoRemind(value: boolean): void {
    this.autoRemindAthletes.set(!!value);
  }

  updateAiAggressiveness(value: number | string): void {
    const normalized = Number(value);
    if (!Number.isFinite(normalized)) {
      return;
    }

    const clamped = Math.max(0, Math.min(100, normalized));
    this.aiAggressiveness.set(clamped);
  }

  getAggressivenessLabel(value: number): string {
    if (value < 35) {
      return 'Low Volume';
    }
    if (value < 70) {
      return 'Balanced Volume';
    }
    return 'High Volume';
  }
}
