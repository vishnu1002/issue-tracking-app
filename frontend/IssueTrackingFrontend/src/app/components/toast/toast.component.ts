import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription, timer } from 'rxjs';
import { ToastMessage, ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrls: ['./toast.component.css'],
})
export class ToastContainerComponent implements OnInit, OnDestroy {
  toasts: ToastMessage[] = [];
  private sub?: Subscription;

  constructor(private toast: ToastService) {}

  ngOnInit(): void {
    this.sub = this.toast.toasts$.subscribe((t) => {
      // Replace any existing toast; only one visible at a time
      this.toasts = [t];
      if (t.durationMs && t.durationMs > 0) {
        timer(t.durationMs).subscribe(() => {
          // Only dismiss if this toast is still the visible one
          if (this.toasts.length && this.toasts[0].id === t.id) {
            this.dismiss(t.id);
          }
        });
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  dismiss(id: number): void {
    this.toasts = this.toasts.filter((t) => t.id !== id);
  }
}
