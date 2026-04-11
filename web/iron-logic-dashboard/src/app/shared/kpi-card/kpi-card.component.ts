import {
  Component,
  input,
  output,
  inject,
  computed,
  ChangeDetectionStrategy,
  HostListener,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-kpi-card',
  standalone: true,
  imports: [CommonModule, RouterModule, LucideAngularModule],
  templateUrl: './kpi-card.component.html',
  styleUrls: ['./kpi-card.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class KpiCardComponent {
  private router = inject(Router);
  menuPosition = signal<'left-aligned' | 'right-aligned' | 'centered'>('centered');

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
  /** Extra Tailwind classes applied to the card container, e.g. 'bg-indigo-50/50 border-indigo-100 shadow-none' */
  customClass = input<string>('');
  /** Color theme: 'slate' (default) | 'indigo' | 'violet' | 'emerald' | 'amber' */
  themeColor = input<string>('slate');
  /** Override label text classes, e.g. 'text-indigo-600' */
  labelClass = input<string>('');
  /** Override value text classes, e.g. 'text-indigo-900' */
  valueClass = input<string>('');
  /** Enable custom content via ng-content instead of default Value/Trend/Context display */
  useCustomContent = input<boolean>(false);

  readonly themeStyles = computed(() => {
    const themes: Record<
      string,
      {
        containerBg: string;
        containerBorder: string;
        iconBg: string;
        iconHover: string;
        iconText: string;
        labelText: string;
        valueText: string;
        contextText: string;
      }
    > = {
      slate: {
        containerBg: 'bg-white',
        containerBorder: 'border-slate-100',
        iconBg: 'bg-slate-50',
        iconHover: 'group-hover:bg-slate-100',
        iconText: 'text-slate-700',
        labelText: 'text-slate-500',
        valueText: 'text-slate-900',
        contextText: 'text-slate-400',
      },
      indigo: {
        containerBg: 'bg-indigo-50/50',
        containerBorder: 'border-indigo-100',
        iconBg: 'bg-indigo-100',
        iconHover: 'group-hover:bg-indigo-200',
        iconText: 'text-indigo-600',
        labelText: 'text-indigo-600',
        valueText: 'text-indigo-900',
        contextText: 'text-indigo-400',
      },
      violet: {
        containerBg: 'bg-violet-50/50',
        containerBorder: 'border-violet-100',
        iconBg: 'bg-violet-100',
        iconHover: 'group-hover:bg-violet-200',
        iconText: 'text-violet-600',
        labelText: 'text-violet-600',
        valueText: 'text-violet-900',
        contextText: 'text-violet-400',
      },
      emerald: {
        containerBg: 'bg-emerald-50/50',
        containerBorder: 'border-emerald-100',
        iconBg: 'bg-emerald-100',
        iconHover: 'group-hover:bg-emerald-200',
        iconText: 'text-emerald-600',
        labelText: 'text-emerald-600',
        valueText: 'text-emerald-900',
        contextText: 'text-emerald-400',
      },
      amber: {
        containerBg: 'bg-amber-50/50',
        containerBorder: 'border-amber-100',
        iconBg: 'bg-amber-100',
        iconHover: 'group-hover:bg-amber-200',
        iconText: 'text-amber-600',
        labelText: 'text-amber-600',
        valueText: 'text-amber-900',
        contextText: 'text-amber-400',
      },
    };
    return themes[this.themeColor()] ?? themes['slate'];
  });

  /** Dynamic inline styles based on theme color: gradient background, colored shadow, and border */
  readonly containerStyles = computed(() => {
    const themeConfig: Record<
      string,
      {
        gradient: string;
        shadow: string;
        borderColor: string;
        infoIconBg: string;
        infoIconText: string;
        infoIconHover: string;
      }
    > = {
      slate: {
        gradient: 'linear-gradient(to bottom, #ffffff, #f8fafc)',
        shadow: '0 10px 25px -5px rgba(100, 116, 139, 0.1)',
        borderColor: '#e2e8f0',
        infoIconBg: 'bg-slate-50/80',
        infoIconText: 'text-slate-300',
        infoIconHover: 'hover:text-slate-600',
      },
      indigo: {
        gradient: 'linear-gradient(to bottom, #ffffff, #f5f3ff)',
        shadow: '0 10px 25px -5px rgba(79, 70, 229, 0.1)',
        borderColor: '#e0e7ff',
        infoIconBg: 'bg-indigo-50/50',
        infoIconText: 'text-indigo-300',
        infoIconHover: 'hover:text-indigo-600',
      },
      violet: {
        gradient: 'linear-gradient(to bottom, #ffffff, #faf5ff)',
        shadow: '0 10px 25px -5px rgba(124, 58, 237, 0.1)',
        borderColor: '#ede9fe',
        infoIconBg: 'bg-violet-50/50',
        infoIconText: 'text-violet-300',
        infoIconHover: 'hover:text-violet-600',
      },
      emerald: {
        gradient: 'linear-gradient(to bottom, #ffffff, #f0fdf4)',
        shadow: '0 10px 25px -5px rgba(16, 185, 129, 0.1)',
        borderColor: '#d1fae5',
        infoIconBg: 'bg-emerald-50/50',
        infoIconText: 'text-emerald-300',
        infoIconHover: 'hover:text-emerald-600',
      },
      amber: {
        gradient: 'linear-gradient(to bottom, #ffffff, #fffbeb)',
        shadow: '0 10px 25px -5px rgba(245, 158, 11, 0.1)',
        borderColor: '#fde68a',
        infoIconBg: 'bg-amber-50/50',
        infoIconText: 'text-amber-300',
        infoIconHover: 'hover:text-amber-600',
      },
    };

    const config = themeConfig[this.themeColor()] ?? themeConfig['slate'];
    return {
      background: config.gradient,
      boxShadow: config.shadow,
      borderColor: config.borderColor,
    };
  });

  /** Info icon theme classes derived from themeColor */
  readonly infoIconTheme = computed(() => {
    const themesMap: Record<string, { bg: string; text: string; hover: string }> = {
      slate: { bg: 'bg-slate-50/80', text: 'text-slate-300', hover: 'hover:text-slate-600' },
      indigo: { bg: 'bg-indigo-50/50', text: 'text-indigo-300', hover: 'hover:text-indigo-600' },
      violet: { bg: 'bg-violet-50/50', text: 'text-violet-300', hover: 'hover:text-violet-600' },
      emerald: {
        bg: 'bg-emerald-50/50',
        text: 'text-emerald-300',
        hover: 'hover:text-emerald-600',
      },
      amber: { bg: 'bg-amber-50/50', text: 'text-amber-300', hover: 'hover:text-amber-600' },
    };
    return themesMap[this.themeColor()] ?? themesMap['slate'];
  });

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

  checkMenuPosition(event: MouseEvent): void {
    const trigger = event.currentTarget as HTMLElement | null;
    if (!trigger) {
      this.menuPosition.set('centered');
      return;
    }

    const rect = trigger.getBoundingClientRect();
    if (rect.left < 200) {
      this.menuPosition.set('left-aligned');
      return;
    }

    if (window.innerWidth - rect.right < 200) {
      this.menuPosition.set('right-aligned');
      return;
    }

    this.menuPosition.set('centered');
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    this.menuPosition.set('centered');
  }
}
