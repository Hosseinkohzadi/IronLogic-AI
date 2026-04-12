import { Injectable, signal } from '@angular/core';

export interface NotificationMessage {
  type: 'success' | 'error';
  text: string;
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly currentMessage = signal<NotificationMessage | null>(null);

  readonly message = this.currentMessage.asReadonly();

  success(text: string): void {
    this.show({ type: 'success', text });
  }

  error(text: string): void {
    this.show({ type: 'error', text });
  }

  clear(): void {
    this.currentMessage.set(null);
  }

  private show(message: NotificationMessage): void {
    this.currentMessage.set(message);
    setTimeout(() => {
      if (this.currentMessage()?.text === message.text) {
        this.currentMessage.set(null);
      }
    }, 3500);
  }
}
