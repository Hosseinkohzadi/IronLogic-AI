import { Component, Input, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-metric-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './metric-card.component.html',
  styleUrl: './metric-card.component.css'
})
export class MetricCardComponent {
  @Input() title: string = '';
  @Input() value: string | null = '';
  @Input() unit?: string;
  @Input() iconClasses: string = '';
  @Input() iconTemplate?: TemplateRef<any>;
  @Input() footerTemplate?: TemplateRef<any>;
  @Input() stats: any;
}
