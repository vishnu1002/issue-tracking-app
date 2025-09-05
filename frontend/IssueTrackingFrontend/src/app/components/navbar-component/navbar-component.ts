import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [],
  templateUrl: './navbar-component.html',
  styleUrl: './navbar-component.css',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class NavbarComponent {}
