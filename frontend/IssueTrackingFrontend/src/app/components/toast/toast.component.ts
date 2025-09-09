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
      this.toasts.push(t);
      timer(t.durationMs).subscribe(() => this.dismiss(t.id));
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  dismiss(id: number): void {
    this.toasts = this.toasts.filter((t) => t.id !== id);
  }
}
