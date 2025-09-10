import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

export type ToastType = 'success' | 'error' | 'info';

export interface ToastAction {
  label: string;
  variant?: 'primary' | 'secondary' | 'danger';
  onClick?: () => void;
}

export interface ToastMessage {
  id: number;
  type: ToastType;
  message: string;
  durationMs: number; // Set to 0 for persistent/interactive toasts
  title?: string;
  actions?: ToastAction[];
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

  showInteractive(params: {
    title?: string;
    message: string;
    type?: ToastType;
    actions: ToastAction[];
    // If durationMs omitted, keep until user acts or closes
    durationMs?: number;
  }): void {
    const toast: ToastMessage = {
      id: ++this.idCounter,
      type: params.type || 'info',
      title: params.title,
      message: params.message,
      actions: params.actions,
      durationMs: params.durationMs ?? 0,
    };
    this.toastsSubject.next(toast);
  }

  confirm(
    message: string,
    options?: { title?: string; yesLabel?: string; noLabel?: string }
  ): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      const yesLabel = options?.yesLabel || 'Yes';
      const noLabel = options?.noLabel || 'No';
      const id = ++this.idCounter;
      const emit = (actions: ToastAction[]) => {
        const toast: ToastMessage = {
          id,
          type: 'info',
          title: options?.title,
          message,
          actions,
          durationMs: 0,
        };
        this.toastsSubject.next(toast);
      };

      const resolveAndDismiss = (value: boolean) => {
        resolve(value);
        // Emit a hidden toast by id to allow container to remove it via dismiss button; the container handles dismissing by id via callback
      };

      emit([
        {
          label: yesLabel,
          variant: 'primary',
          onClick: () => resolveAndDismiss(true),
        },
        {
          label: noLabel,
          variant: 'secondary',
          onClick: () => resolveAndDismiss(false),
        },
      ]);
    });
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
