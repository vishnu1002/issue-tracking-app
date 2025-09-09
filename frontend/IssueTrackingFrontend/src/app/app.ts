import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from './components/navbar-component/navbar-component';
import { ToastContainerComponent } from './components/toast/toast.component';
import { AuthService } from './core/services/auth.service';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, NavbarComponent, ToastContainerComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.css'],
})
export class App {
  private auth = inject(AuthService);
  user = toSignal(this.auth.user$, { initialValue: null });
}
