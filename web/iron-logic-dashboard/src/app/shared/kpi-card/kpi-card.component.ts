import { Component, input, output, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-kpi-card',
  standalone: true,
  imports: [CommonModule, RouterModule, LucideAngularModule],
  templateUrl: './kpi-card.component.html',
  styleUrls: ['./kpi-card.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KpiCardComponent {
  private router = inject(Router);

  // Input properties
  label = input<string>('');
  value = input<string | number>('');
  trend = input<string>('');
  context = input<string>('');
  mainIcon = input<string>('');
  showInfoIcon = input<boolean>(false);
  infoText = input<string>('');
  actionType = input<'route' | 'modal' | 'none'>('none');
  routeTarget = input<string | any[] | undefined>(undefined);

  // Output property
  cardClick = output<void>();

  /**
   * Handles click events on the card
   * Routes if actionType is 'route' and routeTarget exists
   * Emits cardClick event if actionType is 'modal'
   */
  handleClick(): void {
    const actionType = this.actionType();
    const target = this.routeTarget();
    
    if (actionType === 'route' && target) {
      const navigationPath = Array.isArray(target) ? target : [target];
      this.router.navigate(navigationPath);
    } else if (actionType === 'modal') {
      this.cardClick.emit();
    }
  }
}
