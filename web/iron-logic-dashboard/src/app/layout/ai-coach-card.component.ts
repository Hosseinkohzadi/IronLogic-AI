import { Component, input } from '@angular/core';

@Component({
  selector: 'app-ai-coach-card',
  standalone: true,
  template: `
    <div class="rounded-3xl bg-indigo-50/50 p-8 w-full">
      <div class="mb-4 flex items-center gap-2">
        <span class="text-xl">🤖</span>
        <h4 class="text-xs font-black uppercase tracking-widest text-indigo-900">Coach Intelligence Insight</h4>
      </div>
      <p class="text-sm leading-relaxed text-indigo-950/80 font-medium whitespace-pre-line max-w-4xl">
        "{{ adviceText() }}"
      </p>
    </div>
  `
})
export class AiCoachCardComponent {
  adviceText = input.required<string>();
}
