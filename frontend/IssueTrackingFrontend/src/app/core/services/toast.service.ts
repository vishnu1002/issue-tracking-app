import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

export type ToastType = 'success' | 'error' | 'info';

export interface ToastMessage {
  id: number;
  type: ToastType;
  message: string;
  durationMs: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private toastsSubject = new Subject<ToastMessage>();
  private idCounter = 0;

  get toasts$(): Observable<ToastMessage> {
    return this.toastsSubject.asObservable();
  }

  show(message: string, type: ToastType = 'info', durationMs = 3500): void {
    const toast: ToastMessage = {
      id: ++this.idCounter,
      type,
      message,
      durationMs,
    };
    this.toastsSubject.next(toast);
  }

  success(message: string, durationMs = 3000): void {
    this.show(message, 'success', durationMs);
  }

  error(message: string, durationMs = 4000): void {
    this.show(message, 'error', durationMs);
  }

  info(message: string, durationMs = 3000): void {
    this.show(message, 'info', durationMs);
  }
}
