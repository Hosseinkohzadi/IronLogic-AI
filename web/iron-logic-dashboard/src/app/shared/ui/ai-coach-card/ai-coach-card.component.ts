import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-ai-coach-card',
  standalone: true,
  imports: [],
  templateUrl: './ai-coach-card.component.html',
})
export class AiCoachCardComponent {
  @Input() advice: string = '';
}
