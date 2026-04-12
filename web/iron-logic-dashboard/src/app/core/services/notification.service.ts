import { Injectable, computed, signal } from '@angular/core';

export interface Toast {
  id: string;
  type: 'success' | 'error';
  message: string;
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  readonly toasts = signal<Toast[]>([]);
  readonly message = computed(() => {
    const first = this.toasts()[0];
    if (!first) {
      return null;
    }

    return { type: first.type, text: first.message };
  });

  showSuccess(message: string): void {
    this.show('success', message);
  }

  showError(message: string): void {
    this.show('error', message);
  }

  remove(id: string): void {
    this.toasts.update((items) => items.filter((toast) => toast.id !== id));
  }

  // Backward-compatible API for existing callers.
  success(text: string): void {
    this.showSuccess(text);
  }

  // Backward-compatible API for existing callers.
  error(text: string): void {
    this.showError(text);
  }

  clear(): void {
    this.toasts.set([]);
  }

  private show(type: Toast['type'], message: string): void {
    const id = `${Date.now()}-${Math.random().toString(36).slice(2, 9)}`;
    const toast: Toast = { id, type, message };

    this.toasts.update((items) => [...items, toast]);

    setTimeout(() => {
      this.remove(id);
    }, 3000);
  }
}
