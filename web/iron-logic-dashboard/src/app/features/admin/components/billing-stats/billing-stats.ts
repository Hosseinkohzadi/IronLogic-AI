import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-billing-stats',
  imports: [CommonModule],
  templateUrl: './billing-stats.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BillingStatsComponent {}
